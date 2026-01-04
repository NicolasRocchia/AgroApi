using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace APIAgroConnect.Application.Services
{
    public sealed class PdfLotsExtractor
    {
        public sealed record LotRow(
            string? Nombre,
            decimal? SuperficieHa,
            string? Localidad,
            string? Departamento,
            int? Orden,
            decimal? Latitud,
            decimal? Longitud
        );

        public List<LotRow> ExtractLots(Stream pdfStream)
        {
            using var doc = PdfDocument.Open(pdfStream);

            foreach (var page in doc.GetPages())
            {
                var words = page.GetWords().ToList();
                if (words.Count == 0) continue;

                // 1) Detectar tabla por header
                var headerWords = words
                    .Where(w =>
                    {
                        var t = Normalize(w.Text);
                        return t is "NOMBRE" or "SUPERFICIE" or "LOCALIDAD" or "DEPARTAMENTO" or "ORDEN" or "LATITUD" or "LONGITUD";
                    })
                    .ToList();

                if (headerWords.Count < 5)
                    continue;

                // 2) “línea” del header por Y (la que más keywords tenga)
                var headerBand = headerWords
                    .GroupBy(w => BandY(w.BoundingBox.Bottom))
                    .OrderByDescending(g => g.Count())
                    .First();

                var headerY = headerBand.Key;

                var headerLine = headerBand
                    .OrderBy(w => w.BoundingBox.Left)
                    .ToList();

                // 3) X de columnas (usamos centro X del word para ser más estable)
                double xNombre = FindX(headerLine, "NOMBRE");
                double xSup = FindX(headerLine, "SUPERFICIE");
                double xLoc = FindX(headerLine, "LOCALIDAD");
                double xDept = FindX(headerLine, "DEPARTAMENTO");
                double xOrden = FindX(headerLine, "ORDEN");
                double xLat = FindX(headerLine, "LATITUD");
                double xLon = FindX(headerLine, "LONGITUD");

                if (xNombre < 0 || xSup < 0 || xLoc < 0 || xDept < 0 || xOrden < 0 || xLat < 0 || xLon < 0)
                    continue;

                // 👇 FIX: ordenar y calcular "End" por la siguiente columna real
                var cols = new List<(string key, double x)>
                {
                    ("NOMBRE", xNombre),
                    ("SUPERFICIE", xSup),
                    ("LOCALIDAD", xLoc),
                    ("DEPARTAMENTO", xDept),
                    ("ORDEN", xOrden),
                    ("LATITUD", xLat),
                    ("LONGITUD", xLon),
                }
                .OrderBy(c => c.x)
                .ToList();

                double Start(string k) => cols.First(c => c.key == k).x;
                double End(string k)
                {
                    var idx = cols.FindIndex(c => c.key == k);
                    return (idx >= 0 && idx < cols.Count - 1) ? cols[idx + 1].x : double.MaxValue;
                }

                // 4) Construir rangos por “siguiente columna”
                var x = new[] { xNombre, xSup, xLoc, xDept, xOrden, xLat, xLon }
                    .Where(v => v >= 0)
                    .OrderBy(v => v)
                    .ToArray();

                double Next(double v) => x.FirstOrDefault(xx => xx > v, double.MaxValue);

                var rNombre = (Start: xNombre, End: Next(xNombre));
                var rSup = (Start: xSup, End: Next(xSup));
                var rLoc = (Start: xLoc, End: Next(xLoc));
                var rDept = (Start: xDept, End: Next(xDept));
                var rOrden = (Start: xOrden, End: Next(xOrden));
                var rLat = (Start: xLat, End: Next(xLat));
                var rLon = (Start: xLon, End: double.MaxValue);

                // 5) Palabras debajo del header
                var dataWords = words
                    .Where(w => BandY(w.BoundingBox.Bottom) < headerY - 2) // margen
                    .ToList();

                if (dataWords.Count == 0)
                    continue;

                // 6) Agrupar por filas visuales (Y) con tolerancia mayor (tu PDF lo necesita)
                var rows = GroupByVisualRows(dataWords, tol: 4.0);

                var result = new List<LotRow>();

                foreach (var row in rows)
                {
                    // cortar cuando empezamos otra sección (p. ej. “Puntos sensibles”)
                    var rowTextAll = string.Join(" ", row.Select(w => w.Text)).ToUpperInvariant();
                    if (rowTextAll.Contains("PUNTOS") && rowTextAll.Contains("SENSIBLES"))
                        break;

                    var xs = new List<(string key, double x)>
                        {
                            ("NOMBRE", xNombre),
                            ("SUPERFICIE", xSup),
                            ("LOCALIDAD", xLoc),
                            ("DEPARTAMENTO", xDept),
                            ("ORDEN", xOrden),
                            ("LATITUD", xLat),
                            ("LONGITUD", xLon),
                        }
                        .OrderBy(t => t.x)
                        .ToList();

                    string colNombre = JoinInRange(row, Start("NOMBRE"), End("NOMBRE"));
                    string colSup = JoinInRange(row, Start("SUPERFICIE"), End("SUPERFICIE"));
                    string colLoc = JoinInRange(row, Start("LOCALIDAD"), End("LOCALIDAD"));
                    string colDept = JoinInRange(row, Start("DEPARTAMENTO"), End("DEPARTAMENTO"));
                    string colOrden = JoinInRange(row, Start("ORDEN"), End("ORDEN"));
                    string colLat = JoinInRange(row, Start("LATITUD"), End("LATITUD"));
                    string colLon = JoinInRange(row, Start("LONGITUD"), End("LONGITUD"));

                    var nNombre = Normalize(colNombre);
                    var nSup = Normalize(colSup);

                    bool looksLikeHeaderFragment =
                        nNombre is "HAS" or "HAS." or "HAS)" or "HAS(" ||
                        colNombre.Contains("(Has", StringComparison.OrdinalIgnoreCase) ||
                        nNombre is "NOMBRE" ||
                        Normalize(colLoc) is "LOCALIDAD" ||
                        Normalize(colDept) is "DEPARTAMENTO" ||
                        Normalize(colOrden) is "ORDEN" ||
                        Normalize(colLat) is "LATITUD" ||
                        Normalize(colLon) is "LONGITUD" ||
                        nSup is "HAS" || colSup.Contains("(Has", StringComparison.OrdinalIgnoreCase);

                    if (looksLikeHeaderFragment)
                        continue;

                    // saltar header repetido
                    if (Normalize(colNombre) == "NOMBRE" || Normalize(colLoc) == "LOCALIDAD" || Normalize(colDept) == "DEPARTAMENTO")
                        continue;

                    var sup = ParseDecimalNullable(colSup);
                    var orden = ParseIntNullable(colOrden);
                    var lat = ParseDecimalNullable(colLat);
                    var lon = ParseDecimalNullable(colLon);

                    // descartar filas vacías
                    if (string.IsNullOrWhiteSpace(colNombre)
                        && sup is null
                        && string.IsNullOrWhiteSpace(colLoc)
                        && string.IsNullOrWhiteSpace(colDept)
                        && orden is null
                        && lat is null
                        && lon is null)
                        continue;

                    result.Add(new LotRow(
                        Nombre: NullIfEmpty(colNombre),
                        SuperficieHa: sup,
                        Localidad: NullIfEmpty(colLoc),
                        Departamento: NullIfEmpty(colDept),
                        Orden: orden,
                        Latitud: lat,
                        Longitud: lon
                    ));
                }

                // 7) Merge de “segunda línea visual” (nombre/localidad/depto partidos)
                result = MergeContinuationLines(result);

                if (result.Count > 0)
                    return result;
            }

            return new List<LotRow>();
        }

        private static List<LotRow> MergeContinuationLines(List<LotRow> rows)
        {
            // Ejemplo:
            // Row0: Nombre="LOTE PRUEBA", Loc="CAMILO", Dept="MARCOS", Sup=2.30, Orden=1, Lat/Lon=...
            // Row1: Nombre="2", Loc="ALDAO", Dept="JUAREZ", Sup=null, Orden=null, Lat/Lon=null
            //
            // => Row0 Nombre="LOTE PRUEBA 2", Loc="CAMILO ALDAO", Dept="MARCOS JUAREZ" y Row1 se elimina.

            for (int i = 0; i < rows.Count - 1; i++)
            {
                var a = rows[i];
                var b = rows[i + 1];

                bool bHasNoVertex = b.Orden is null && b.Latitud is null && b.Longitud is null;
                bool bLooksLikeContinuation =
                    bHasNoVertex &&
                    (!string.IsNullOrWhiteSpace(b.Nombre)
                      || !string.IsNullOrWhiteSpace(b.Localidad)
                      || !string.IsNullOrWhiteSpace(b.Departamento)) &&
                    b.SuperficieHa is null;

                if (!bLooksLikeContinuation)
                    continue;

                // si b es solo un "2" o texto corto, lo pegamos al nombre
                var newName = AppendDedup(a.Nombre, b.Nombre);
                var newLoc = AppendDedup(a.Localidad, b.Localidad);
                var newDept = AppendDedup(a.Departamento, b.Departamento);

                rows[i] = a with
                {
                    Nombre = NullIfEmpty(newName),
                    Localidad = NullIfEmpty(newLoc),
                    Departamento = NullIfEmpty(newDept)
                };

                rows.RemoveAt(i + 1);
                i--; // re-evaluar por si hay otra continuación
            }

            return rows;
        }

        private static IEnumerable<List<Word>> GroupByVisualRows(List<Word> words, double tol)
        {
            var ordered = words.OrderByDescending(w => w.BoundingBox.Bottom).ToList();
            var groups = new List<List<Word>>();

            foreach (var w in ordered)
            {
                var y = w.BoundingBox.Bottom;
                var placed = false;

                foreach (var g in groups)
                {
                    var gy = g[0].BoundingBox.Bottom;
                    if (Math.Abs(gy - y) <= tol)
                    {
                        g.Add(w);
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                    groups.Add(new List<Word> { w });
            }

            return groups.OrderByDescending(g => g[0].BoundingBox.Bottom);
        }

        private static string JoinInRange(IEnumerable<Word> row, double xStart, double xEnd)
        {
            var parts = row
                .Where(w =>
                {
                    var x = (w.BoundingBox.Left + w.BoundingBox.Right) / 2.0; // centro X
                    return x >= xStart && x < xEnd;
                })
                .OrderBy(w => w.BoundingBox.Left)
                .Select(w => w.Text.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            return string.Join(" ", parts).Trim();
        }

        private static double FindX(List<Word> headerLine, string keyword)
        {
            var w = headerLine.FirstOrDefault(x => Normalize(x.Text) == keyword);
            if (w is null) return -1;
            return (w.BoundingBox.Left + w.BoundingBox.Right) / 2.0;
        }

        private static int BandY(double y) => (int)Math.Round(y / 3.0); // “bandas” más finas

        private static string Normalize(string s)
            => new string(s.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());

        private static string? NullIfEmpty(string s)
            => string.IsNullOrWhiteSpace(s) ? null : Regex.Replace(s.Trim(), @"\s+", " ");

        private static string AppendDedup(string? a, string? b)
        {
            var aa = (a ?? "").Trim();
            var bb = (b ?? "").Trim();

            if (string.IsNullOrWhiteSpace(bb)) return aa;
            if (string.IsNullOrWhiteSpace(aa)) return bb;

            if (string.Equals(aa, bb, StringComparison.OrdinalIgnoreCase))
                return aa;

            var aaU = aa.ToUpperInvariant();
            var bbU = bb.ToUpperInvariant();

            if (aaU.EndsWith(" " + bbU) || aaU.Contains(" " + bbU + " "))
                return aa;

            return $"{aa} {bb}".Trim();
        }

        private static int? ParseIntNullable(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var cleaned = new string(s.Where(c => char.IsDigit(c) || c == '-').ToArray());
            return int.TryParse(cleaned, out var v) ? v : null;
        }

        private static decimal? ParseDecimalNullable(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            // Aceptar coma o punto, y signos
            s = s.Trim().Replace(",", ".");
            var cleaned = new string(s.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());

            return decimal.TryParse(
                cleaned,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var v
            ) ? v : null;
        }
    }
}

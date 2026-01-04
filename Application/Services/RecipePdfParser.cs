using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace APIAgroConnect.Application.Services
{
    public sealed class RecipePdfParser : IRecipePdfParser
    {
        private readonly PdfLotsExtractor _lotsExtractor;

        private static readonly CultureInfo Ar = new("es-AR");
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public RecipePdfParser(PdfLotsExtractor lotsExtractor)
        {
            _lotsExtractor = lotsExtractor;
        }

        // Interfaz actual: Parse(Stream, string)
        public ParsedRecipe Parse(Stream pdfStream, string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                throw new ArgumentException("Texto PDF vacío.", nameof(rawText));

            // Normalizado para búsquedas/heurísticas (sin romper el contenido)
            var normalizedText = Normalize(rawText);

            var page1 = Between(normalizedText, "", "-----PAGE-----").Trim();
            var page2 = AfterNth(normalizedText, "-----PAGE-----", 1).Trim();

            var rfdNumber = ParseLong(FindGroup(page1, @"RFD\s*Nro\.\s*:\s*(\d+)", RegexOptions.IgnoreCase)) ?? 0;
            var status = (FindGroup(page1, @"RECETA\s+FITOSANITARIA\s+DIGITAL\s*(ABIERTA|CERRADA|ANULADA)", RegexOptions.IgnoreCase) ?? "").Trim();

            var issueDate = ParseDate(FindGroup(page1, @"Fecha\s+de\s+emisión:\s*([0-9]{1,2}/[0-9]{1,2}/[0-9]{4})", RegexOptions.IgnoreCase));
            var possibleStartDate = ParseDateNullable(FindGroup(page1, @"Fecha\s+posible\s+inicio\s+aplicación:\s*([0-9]{1,2}/[0-9]{1,2}/[0-9]{4})", RegexOptions.IgnoreCase));
            var recommendedDate = ParseDateNullable(FindGroup(page1, @"Fecha\s+recomendada\s+de\s+aplicación:\s*([0-9]{1,2}/[0-9]{1,2}/[0-9]{4})", RegexOptions.IgnoreCase));
            var expirationDate = ParseDateNullable(FindGroup(page1, @"Fecha\s+de\s+vencimiento:\s*([0-9]{1,2}/[0-9]{1,2}/[0-9]{4})", RegexOptions.IgnoreCase));

            // ===== Requester / Advisor robustos por bloque con fallback =====
            var requesterBlock = Between(page1, "Datos usuario responsable solicitante", "Datos asesor fitosanitario");
            if (string.IsNullOrWhiteSpace(requesterBlock))
                requesterBlock = page1;

            var advisorBlock = Between(page1, "Datos asesor fitosanitario", "Datos de aplicación");
            if (string.IsNullOrWhiteSpace(advisorBlock))
                advisorBlock = page1;

            var requesterTaxId =
     FindGroup(requesterBlock,
         // acepta guiones/espacios (20-12345678-3, etc)
         @"CUIL\s*/\s*NRO\.\s*DOCUMENTO\s*/\s*CUIT:\s*([0-9\-\s]{8,15})",
         RegexOptions.IgnoreCase)?.Trim() ?? "";

            requesterTaxId = Regex.Replace(requesterTaxId, @"\D", ""); // deja solo dígitos

            var requesterName =
                 ExtractNameBeforeAnchor(
                     requesterBlock,
                     @"CUIL\s*/\s*NRO\.\s*DOCUMENTO\s*/\s*CUIT:",
                     takeLastMatch: false, // 👈 requester: primero
                     preferKeywords: new[] { "MUNICIPALIDAD", "COMUNA", "MINISTERIO", "SECRETAR", "COOPERATIVA", "S.A", "S.R.L" });

            if (string.IsNullOrWhiteSpace(requesterName))
            {
                // Fallback: tomar el primer "Nombre / Razón social:" del bloque
                requesterName = FindGroup(
                    requesterBlock,
                    @"Nombre\s*/\s*Razón\s*social:+\s*(?<name>.+?)(?=\s*(CUIL\s*/|Domicilio:|Contacto:|Datos\s+asesor|Datos\s+de\s+aplicación|$))",
                    RegexOptions.IgnoreCase) ?? "";
                requesterName = CleanupName(requesterName);
            }

            var advisorLicense =
                FindGroup(advisorBlock,
                    @"M\.P\.\s*:\s*([0-9]{1,10})",
                    RegexOptions.IgnoreCase)?.Trim() ?? "";

            var advisorName =
                 ExtractNameBeforeAnchor(
                     advisorBlock,
                     @"M\.P\.\s*:\s*[0-9]{1,10}",
                     takeLastMatch: true);

            // ===== Datos de aplicación =====
            var applicationType = FindGroup(page1, @"Tipo\s+de\s+aplicación:\s*(.+?)\s*Cultivo:", RegexOptions.IgnoreCase)?.Trim();
            var crop = FindGroup(page1, @"Cultivo:\s*(.+?)\s*Diagnóstico:", RegexOptions.IgnoreCase)?.Trim();
            var diagnosis = FindGroup(page1, @"Diagnóstico:\s*(.+?)\s*Tratamiento:", RegexOptions.IgnoreCase)?.Trim();
            var treatment = FindGroup(page1, @"Tratamiento:\s*(.+?)\s*Máquina\s+a\s+utilizar:", RegexOptions.IgnoreCase)?.Trim();
            var machineToUse = FindGroup(page1, @"Máquina\s+a\s+utilizar:\s*(.+?)\s*Nro\.\s*matrícula\s+máquina:", RegexOptions.IgnoreCase)?.Trim();

            // ===== Productos =====
            var productsBlock = Between(page1, "Productos recomendados para ser aplicados", "Condiciones climatológicas recomendadas para el tratamiento");
            var products = ParseProducts(productsBlock);

            // ===== Clima =====
            var climateBlock = Between(page1, "Condiciones climatológicas recomendadas para el tratamiento", "Indicaciones generales para la aplicación:");

            var tempMin = ParseDecimalNullable(FindGroup(climateBlock, @"Temperatura.*?:\s*([0-9]+(?:[.,][0-9]+)?)\s*-\s*([0-9]+(?:[.,][0-9]+)?)", RegexOptions.IgnoreCase, 1));
            var tempMax = ParseDecimalNullable(FindGroup(climateBlock, @"Temperatura.*?:\s*([0-9]+(?:[.,][0-9]+)?)\s*-\s*([0-9]+(?:[.,][0-9]+)?)", RegexOptions.IgnoreCase, 2));

            var humidityMin = ParseDecimalNullable(FindGroup(climateBlock, @"Humedad\s+relativa.*?:\s*([0-9]+(?:[.,][0-9]+)?)\s*-\s*([0-9]+(?:[.,][0-9]+)?)", RegexOptions.IgnoreCase, 1));
            var humidityMax = ParseDecimalNullable(FindGroup(climateBlock, @"Humedad\s+relativa.*?:\s*([0-9]+(?:[.,][0-9]+)?)\s*-\s*([0-9]+(?:[.,][0-9]+)?)", RegexOptions.IgnoreCase, 2));

            var windMin = ParseDecimalNullable(FindGroup(climateBlock, @"Velocidad\s+del\s+viento.*?:\s*([0-9]+(?:[.,][0-9]+)?)\s*-\s*([0-9]+(?:[.,][0-9]+)?)", RegexOptions.IgnoreCase, 1));
            var windMax = ParseDecimalNullable(FindGroup(climateBlock, @"Velocidad\s+del\s+viento.*?:\s*([0-9]+(?:[.,][0-9]+)?)\s*-\s*([0-9]+(?:[.,][0-9]+)?)", RegexOptions.IgnoreCase, 2));

            var windDirection = FindGroup(climateBlock, @"Dirección\s+del\s+viento\s*:\s*([A-Z]{1,3}\s*-\s*[A-Z]{1,3})", RegexOptions.IgnoreCase)?.Trim();

            // Indicaciones
            var notes = FindGroup(page1, @"Indicaciones\s+generales\s+para\s+la\s+aplicación:\s*(.+?)(?:\s*Página\s*1\s*de\s*2|$)", RegexOptions.IgnoreCase)?.Trim();

            // Unidad (página 2)
            var unitSurfaceHa = ParseDecimalNullable(FindGroup(page2, @"Superficie\s+unidad\s+de\s+uso\s+Has\.\s*:\s*([0-9]+(?:[.,][0-9]+)?)", RegexOptions.IgnoreCase)?.Trim());

            // ===== LOTES por tabla (PdfLotsExtractor) =====
            if (pdfStream.CanSeek)
                pdfStream.Position = 0;

            var lotRows = _lotsExtractor.ExtractLots(pdfStream);
            var lots = BuildLotsFromLotRows(lotRows, normalizedText);

            return new ParsedRecipe
            {
                RfdNumber = rfdNumber,
                Status = status,

                IssueDate = issueDate,
                PossibleStartDate = possibleStartDate,
                RecommendedDate = recommendedDate,
                ExpirationDate = expirationDate,

                RequesterName = requesterName,
                RequesterTaxId = requesterTaxId,

                AdvisorName = advisorName,
                AdvisorLicense = advisorLicense,

                ApplicationType = applicationType,
                Crop = crop,
                Diagnosis = diagnosis,
                Treatment = treatment,
                MachineToUse = machineToUse,

                UnitSurfaceHa = unitSurfaceHa,

                TempMin = tempMin,
                TempMax = tempMax,
                HumidityMin = humidityMin,
                HumidityMax = humidityMax,
                WindMinKmh = windMin,
                WindMaxKmh = windMax,
                WindDirection = windDirection,

                Notes = notes,

                Products = products,
                Lots = lots
            };
        }

        // =========================
        // LOTES: agrupar cabecera + vertices y corregir localidad/depto corridos
        // =========================
        private static List<ParsedLot> BuildLotsFromLotRows(List<PdfLotsExtractor.LotRow> rows, string normalizedText)
        {
            var lots = new List<ParsedLot>();
            ParsedLot? current = null;

            foreach (var r in rows)
            {
                var nombre = (r.Nombre ?? "").Trim();

                // Filtrar “basura” típica de headers que se cuelan
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    var upper = nombre.ToUpperInvariant();
                    if (upper is "NOMBRE" or "LOTES" or "(HAS.)" or "SUPERFICIE" or "LOCALIDAD" or "DEPARTAMENTO")
                        continue;
                }

                bool hasVertex = r.Orden is not null && r.Latitud is not null && r.Longitud is not null;

                // Inicio de lote: tiene nombre y al menos superficie o localidad/depto
                bool isLotHeader =
                    !string.IsNullOrWhiteSpace(nombre) &&
                    (r.SuperficieHa is not null ||
                     !string.IsNullOrWhiteSpace(r.Localidad) ||
                     !string.IsNullOrWhiteSpace(r.Departamento));

                if (isLotHeader)
                {
                    var (locality, department) =
                        ParseLocalityAndDepartment(r.Localidad, r.Departamento, normalizedText);

                    current = new ParsedLot
                    {
                        LotName = TitleCaseSafe(nombre),
                        SurfaceHa = r.SuperficieHa,
                        Locality = locality,
                        Department = department,
                        Vertices = new List<ParsedVertex>()
                    };

                    lots.Add(current);

                    if (hasVertex)
                    {
                        current.Vertices.Add(new ParsedVertex
                        {
                            Order = r.Orden!.Value,
                            Latitude = r.Latitud!.Value,
                            Longitude = r.Longitud!.Value
                        });
                    }

                    continue;
                }

                // Vertex: se agrega al lote actual
                if (hasVertex && current is not null)
                {
                    current.Vertices.Add(new ParsedVertex
                    {
                        Order = r.Orden!.Value,
                        Latitude = r.Latitud!.Value,
                        Longitude = r.Longitud!.Value
                    });
                }
            }

            foreach (var l in lots)
                l.Vertices = l.Vertices.OrderBy(v => v.Order).ToList();

            return lots;
        }

        private static (string locality, string department) ParseLocalityAndDepartment(
    string? rawLocality,
    string? rawDepartment,
    string normalizedText)
        {
            static string CleanToken(string t) => Regex.Replace(t ?? "", @"[^A-ZÁÉÍÓÚÜÑ]", "", RegexOptions.IgnoreCase);

            var loc = (rawLocality ?? "").Trim();
            var dept = (rawDepartment ?? "").Trim();

            // Caso ideal
            if (!string.IsNullOrWhiteSpace(loc) && !string.IsNullOrWhiteSpace(dept))
                return (TitleCaseSafe(loc), TitleCaseSafe(dept));

            if (string.IsNullOrWhiteSpace(loc))
                return ("", TitleCaseSafe(dept));

            // Normalizar tokens y también tokens "limpios" (sin signos)
            var tokens = loc.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();
            var clean = tokens.Select(t => CleanToken(t).ToUpperInvariant()).ToArray();

            // Caso típico corrido:
            // "Marcos Aldao Juarez" (todo vino en Localidad) => Dept: "Marcos Juarez", Loc: "Camilo Aldao"
            if (string.IsNullOrWhiteSpace(dept) &&
                clean.Length == 3 &&
                clean[2] == "JUAREZ")
            {
                var deptGuess = $"{tokens[0]} {tokens[2]}"; // Marcos Juarez

                // Heurística 1: si el medio es ALDAO, casi siempre la localidad real es "CAMILO ALDAO"
                var midClean = clean[1];
                var locGuess = tokens[1];

                if (midClean == "ALDAO")
                    locGuess = "CAMILO ALDAO";

                // Heurística 2: si aparece en el texto completo, usarlo
                if (Regex.IsMatch(normalizedText, @"\bCAMILO\s+ALDAO\b", RegexOptions.IgnoreCase))
                    locGuess = "CAMILO ALDAO";

                return (TitleCaseSafe(locGuess), TitleCaseSafe(deptGuess));
            }

            // Si vinieran 4 tokens: 2+2
            if (string.IsNullOrWhiteSpace(dept) && tokens.Length == 4)
            {
                var locGuess = string.Join(" ", tokens.Take(2));
                var deptGuess = string.Join(" ", tokens.Skip(2));
                return (TitleCaseSafe(locGuess), TitleCaseSafe(deptGuess));
            }

            // Fallback
            return (TitleCaseSafe(loc), TitleCaseSafe(dept));
        }


        // =========================
        // Productos
        // =========================
        private static List<ParsedProduct> ParseProducts(string block)
        {
            var s = Regex.Replace(block ?? "", @"\s+", " ").Trim();

            s = s.Replace("Tipo deproducto", " ")
                 .Replace("Nombre", " ")
                 .Replace("Nro registroSENASA", " ")
                 .Replace("Clasetoxicológica", " ")
                 .Replace("Dosis", " ")
                 .Replace("UM/UM", " ")
                 .Replace("Total producto aaplicar", " ");

            s = Regex.Replace(s, @"(?<=[A-ZÁÉÍÓÚÜÑ])(?=\d)", " ");
            s = Regex.Replace(s, @"(?<=\d)(?=[A-ZÁÉÍÓÚÜÑ])", " ");

            s = Regex.Replace(s, @"\b(FORMULADO|CALDO|SÓLIDO|SOLIDO|LIQUIDO|LÍQUIDO|CEBO|GRANULADO)(?=[A-ZÁÉÍÓÚÜÑ])", "$1 ");
            s = Regex.Replace(s, @"((?:I|II|III|IV)\s*-\s*[A-ZÁÉÍÓÚÜÑ ]+)(?=\d)", "$1 ");
            s = Regex.Replace(s, @"\s+", " ").Trim();

            var rx = new Regex(
                @"(?<type>FORMULADO|CALDO|SÓLIDO|SOLIDO|LIQUIDO|LÍQUIDO|CEBO|GRANULADO)\s+" +
                @"(?<name>.+?)\s+" +
                @"(?<senasa>\d{4,6})\s+" +
                @"(?<tox>(?:I|II|III|IV)\s*-\s*[A-ZÁÉÍÓÚÜÑ ]+)\s+" +
                @"(?<dose>\d+(?:[.,]\d+)?)\s*(?<doseUnit>[A-Z/]+)\s+" +
                @"(?<total>\d+(?:[.,]\d+)?)\s*(?<totalUnit>[A-Z]+)",
                RegexOptions.IgnoreCase);

            var list = new List<ParsedProduct>();

            foreach (Match m in rx.Matches(s))
            {
                var name = m.Groups["name"].Value.Trim();
                name = Regex.Replace(name, @"\bVEGETAL\s*AKTIV\b", "VEGETAL AKTIV", RegexOptions.IgnoreCase);

                list.Add(new ParsedProduct
                {
                    ProductType = m.Groups["type"].Value.Trim(),
                    ProductName = name,
                    SenasaRegistry = m.Groups["senasa"].Value.Trim(),
                    ToxicologicalClass = m.Groups["tox"].Value.Trim(),
                    DoseValue = ParseDecimalNullable(m.Groups["dose"].Value),
                    DoseUnit = m.Groups["doseUnit"].Value.Trim(),
                    DosePerUnit = null,
                    TotalValue = ParseDecimalNullable(m.Groups["total"].Value),
                    TotalUnit = m.Groups["totalUnit"].Value.Trim()
                });
            }

            return list;
        }

        // =========================
        // ExtractNameBeforeAnchor (solicitante/asesor)
        // =========================
        private static string ExtractNameBeforeAnchor(
    string text,
    string anchorPattern,
    bool takeLastMatch = true,
    string[]? preferKeywords = null)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            var anchor = Regex.Match(text, anchorPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!anchor.Success) return "";

            var upto = text.Substring(0, anchor.Index);

            var nameRx = new Regex(
                @"Nombre\s*/\s*Razón\s*social:+\s*(?<name>.+?)(?=\s*(Nombre\s*/\s*Razón\s*social:|CUIL\s*/|M\.P\.|Domicilio:|Contacto:|Datos\s+de\s+aplicación|$))",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var matches = nameRx.Matches(upto).Cast<Match>().Where(m => m.Success).ToList();
            if (matches.Count == 0) return "";

            // 1) Si hay keywords preferidas (ej. MUNICIPALIDAD/MINISTERIO/COMUNA), elegir la primera que matchee
            if (preferKeywords is { Length: > 0 })
            {
                foreach (var kw in preferKeywords)
                {
                    var hit = matches.FirstOrDefault(m =>
                        m.Groups["name"].Value.Contains(kw, StringComparison.OrdinalIgnoreCase));
                    if (hit is not null)
                        return CleanupName(hit.Groups["name"].Value);
                }
            }

            // 2) Elegir primero o último según use-case
            var chosen = takeLastMatch ? matches.Last() : matches.First();
            return CleanupName(chosen.Groups["name"].Value);
        }


        private static string CleanupName(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.Replace("Nombre / Razón social::", "")
                 .Replace("Nombre / Razón social:", "");
            s = Regex.Replace(s, @"\s+", " ").Trim();
            return s;
        }

        // =========================
        // Helpers de texto / parse
        // =========================
        private static string Normalize(string text)
        {
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");
            text = Regex.Replace(text, @"[ \t]+", " ");

            var labels = new[]
            {
                "RECETA FITOSANITARIA DIGITAL",
                "ABIERTA", "CERRADA", "ANULADA",
                "Fecha de emisión:",
                "Fecha posible inicio aplicación:",
                "RFD Nro.:",
                "Fecha recomendada de aplicación:",
                "Fecha de vencimiento:",
                "Datos usuario responsable solicitante",
                "Datos asesor fitosanitario",
                "Datos de aplicación",
                "Tipo de aplicación:",
                "Cultivo:",
                "Diagnóstico:",
                "Tratamiento:",
                "Máquina a utilizar:",
                "Nro. matrícula máquina:",
                "Productos recomendados para ser aplicados",
                "Condiciones climatológicas recomendadas para el tratamiento",
                "Indicaciones generales para la aplicación:",
                "Área de tratamiento y puntos sensibles",
                "Superficie unidad de uso Has.:",
                "Lotes",
                "Puntos sensibles",
                "-----PAGE-----"
            };

            foreach (var l in labels)
                text = text.Replace(l, "\n" + l);

            text = Regex.Replace(text, @"Página\s*\d+\s*de\s*\d+", "", RegexOptions.IgnoreCase);

            return text.Trim();
        }

        private static string Between(string text, string start, string end)
        {
            var i = string.IsNullOrEmpty(start) ? 0 : text.IndexOf(start, StringComparison.OrdinalIgnoreCase);
            if (i < 0) return "";
            if (!string.IsNullOrEmpty(start)) i += start.Length;

            var j = text.IndexOf(end, i, StringComparison.OrdinalIgnoreCase);
            if (j < 0) return text.Substring(i);

            return text.Substring(i, j - i);
        }

        private static string AfterNth(string text, string marker, int nth)
        {
            var idx = -1;
            for (int k = 0; k < nth; k++)
            {
                idx = text.IndexOf(marker, idx + 1, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return "";
            }
            return text[(idx + marker.Length)..];
        }

        private static string? FindGroup(string text, string pattern, RegexOptions options, int groupIndex = 1)
        {
            var m = Regex.Match(text ?? "", pattern, options | RegexOptions.Singleline);
            return m.Success ? m.Groups[groupIndex].Value : null;
        }

        private static DateOnly ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return default;

            if (DateOnly.TryParseExact(value.Trim(), "d/M/yyyy", Ar, DateTimeStyles.None, out var d))
                return d;

            return DateOnly.Parse(value.Trim(), Ar);
        }

        private static DateOnly? ParseDateNullable(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return ParseDate(value);
        }

        private static long? ParseLong(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (long.TryParse(Regex.Replace(s, @"\D", ""), out var n)) return n;
            return null;
        }

        private static decimal? ParseDecimalNullable(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim().Replace(",", ".");
            if (decimal.TryParse(s, NumberStyles.Number, Inv, out var d)) return d;
            return null;
        }

        private static string TitleCaseSafe(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = Regex.Replace(s.Trim(), @"\s+", " ");
            return Ar.TextInfo.ToTitleCase(s.ToLower(Ar));
        }
    }
}

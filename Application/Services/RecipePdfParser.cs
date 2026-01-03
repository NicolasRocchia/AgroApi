using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace APIAgroConnect.Application.Services
{
    public sealed class RecipePdfParser : IRecipePdfParser
    {
        private static readonly CultureInfo Ci = CultureInfo.InvariantCulture;

        public ParsedRecipe Parse(string pdfText)
        {
            pdfText = pdfText.Replace("\r", "");

            var r = new ParsedRecipe();

            r.RfdNumber = MatchLong(pdfText, @"RFD\s*Nro\.?:\s*(\d+)");
            r.Status = MatchString(pdfText, @"\b(ABIERTA|CERRADA)\b") ?? "ABIERTA";

            r.IssueDate = MatchDate(pdfText, @"Fecha de emisión:\s*(\d{1,2}\/\d{1,2}\/\d{4})")
                          ?? throw new Exception("No se pudo leer Fecha de emisión.");

            r.PossibleStartDate = MatchDate(pdfText, @"Fecha posible inicio aplicación:\s*(\d{1,2}\/\d{1,2}\/\d{4})");
            r.RecommendedDate = MatchDate(pdfText, @"Fecha recomendada de aplicación:\s*(\d{1,2}\/\d{1,2}\/\d{4})");
            r.ExpirationDate = MatchDate(pdfText, @"Fecha de vencimiento:\s*(\d{1,2}\/\d{1,2}\/\d{4})");

            r.RequesterName = MatchLineAfter(pdfText, @"Usuario Responsable Solicitante") ?? "";
            r.RequesterTaxId = MatchString(pdfText, @"CUIL\/CUIT:\s*([0-9]{11})") ?? "";

            r.AdvisorName = MatchLineAfter(pdfText, @"Asesor fitosanitario") ?? "";
            r.AdvisorLicense = MatchString(pdfText, @"M\.P\.\s*:\s*([0-9]+)") ?? "";

            r.ApplicationType = MatchString(pdfText, @"Tipo de aplicación:\s*([^\n]+)");
            r.Crop = MatchString(pdfText, @"Cultivo:\s*([^\n]+)");
            r.Diagnosis = MatchString(pdfText, @"Diagnóstico:\s*([^\n]+)");
            r.Treatment = MatchString(pdfText, @"Tratamiento:\s*([^\n]+)");
            r.MachineToUse = MatchString(pdfText, @"Máquina a utilizar:\s*([^\n]+)");

            r.UnitSurfaceHa = MatchDecimal(pdfText, @"Superficie unidad de uso:\s*([0-9]+(?:\.[0-9]+)?)\s*Has");

            r.TempMin = MatchDecimal(pdfText, @"Temperatura recomendada:\s*([0-9]+(?:\.[0-9]+)?)\s*-\s*");
            r.TempMax = MatchDecimal(pdfText, @"Temperatura recomendada:\s*[0-9]+(?:\.[0-9]+)?\s*-\s*([0-9]+(?:\.[0-9]+)?)");
            r.HumidityMin = MatchDecimal(pdfText, @"Humedad recomendada:\s*([0-9]+(?:\.[0-9]+)?)\s*-\s*");
            r.HumidityMax = MatchDecimal(pdfText, @"Humedad recomendada:\s*[0-9]+(?:\.[0-9]+)?\s*-\s*([0-9]+(?:\.[0-9]+)?)");
            r.WindMinKmh = MatchDecimal(pdfText, @"Viento recomendado:\s*([0-9]+(?:\.[0-9]+)?)\s*-\s*");
            r.WindMaxKmh = MatchDecimal(pdfText, @"Viento recomendado:\s*[0-9]+(?:\.[0-9]+)?\s*-\s*([0-9]+(?:\.[0-9]+)?)\s*km\/h");
            r.WindDirection = MatchString(pdfText, @"Dirección recomendada:\s*([A-ZÑ\- ]+)");

            r.Notes = MatchBlock(pdfText, @"Indicaciones generales\s*\n", @"Área de tratamiento");

            // Vértices: "1 - -33.124... -62.112..."
            var lot = new ParsedLot
            {
                LotName = MatchString(pdfText, @"Lote:\s*([^\n]+)") ?? "LOTE",
                Locality = MatchString(pdfText, @"Localidad:\s*([^\n]+)"),
                Department = MatchString(pdfText, @"Dpto\.?:\s*([^\n]+)"),
                SurfaceHa = MatchDecimal(pdfText, @"Superficie\s*:\s*([0-9]+(?:\.[0-9]+)?)")
            };

            var vx = new Regex(@"(?m)^\s*(\d+)\s*-\s*(-?\d+\.\d+)\s+(-?\d+\.\d+)\s*$");
            foreach (Match m in vx.Matches(pdfText))
            {
                lot.Vertices.Add(new ParsedVertex
                {
                    Order = int.Parse(m.Groups[1].Value),
                    Latitude = decimal.Parse(m.Groups[2].Value, Ci),
                    Longitude = decimal.Parse(m.Groups[3].Value, Ci)
                });
            }

            if (lot.Vertices.Count > 0) r.Lots.Add(lot);

            return r;
        }

        private static long MatchLong(string t, string pattern)
            => long.Parse(Regex.Match(t, pattern, RegexOptions.IgnoreCase).Groups[1].Value);

        private static string? MatchString(string t, string pattern)
        {
            var m = Regex.Match(t, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }

        private static decimal? MatchDecimal(string t, string pattern)
        {
            var m = Regex.Match(t, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!m.Success) return null;
            return decimal.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        private static DateOnly? MatchDate(string t, string pattern)
        {
            var m = Regex.Match(t, pattern, RegexOptions.IgnoreCase);
            if (!m.Success) return null;

            if (DateTime.TryParseExact(m.Groups[1].Value.Trim(), "d/M/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return DateOnly.FromDateTime(dt);

            return null;
        }

        private static string? MatchBlock(string t, string startPattern, string endPattern)
        {
            var start = Regex.Match(t, startPattern, RegexOptions.IgnoreCase);
            if (!start.Success) return null;

            var afterStart = t.Substring(start.Index + start.Length);
            var end = Regex.Match(afterStart, endPattern, RegexOptions.IgnoreCase);
            return end.Success ? afterStart.Substring(0, end.Index).Trim() : afterStart.Trim();
        }

        private static string? MatchLineAfter(string t, string header)
        {
            var m = Regex.Match(t, header + @"\s*\n([^\n]+)", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }
    }
}

using System;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using DengueAlert.Api.Repository;
using DengueAlert.Api.Models;
using DengueAlert.Api.Helpers;
using Microsoft.Extensions.Logging;

namespace DengueAlert.Api.Services
{
    public class AlertDengueService
    {
        private readonly HttpClient _http;
        private readonly IDengueRepository _repo;
        private readonly ILogger<AlertDengueService> _logger;
        private const int GEO_BELO_HORIZONTE = 3106200;

        public AlertDengueService(HttpClient http, IDengueRepository repo, ILogger<AlertDengueService> logger)
        {
            _http = http;
            _repo = repo;
            _logger = logger;
        }

        public async Task ImportLastMonthsAsync(int months = 6)
        {
            var (ewStart, eyStart, ewEnd, eyEnd) = EpidemiologicalWeekHelper.GetWeekRangeForLastMonths(months);
            _logger.LogInformation("Computed EW range for last {months} months => ewStart={ewStart} eyStart={eyStart} ewEnd={ewEnd} eyEnd={eyEnd}",
                months, ewStart, eyStart, ewEnd, eyEnd);

            var periods = EpidemiologicalWeekHelper.SplitByYear(ewStart, eyStart, ewEnd, eyEnd);
            _logger.LogInformation("Periods to request: {@periods}", periods);

            foreach (var p in periods)
            {
                var relativeUrl = $"/api/alertcity?geocode={GEO_BELO_HORIZONTE}&disease=dengue&format=json&ew_start={p.ewStart}&ew_end={p.ewEnd}&ey_start={p.eyStart}&ey_end={p.eyEnd}";
                _logger.LogInformation("Requesting AlertaDengue (relative): {url}", relativeUrl);

                HttpResponseMessage? res = null;
                try
                {
                    res = await _http.GetAsync(relativeUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Relative request failed. Tentando fallback com URL absoluta para debugging.");

                    try
                    {
                        var absoluteUrl = $"https://info.dengue.mat.br/api/alertcity?geocode={GEO_BELO_HORIZONTE}&disease=dengue&format=json&ew_start={p.ewStart}&ew_end={p.ewEnd}&ey_start={p.eyStart}&ey_end={p.eyEnd}";
                        _logger.LogInformation("Attempting absolute URL: {absoluteUrl}", absoluteUrl);
                        res = await _http.GetAsync(absoluteUrl);
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogError(ex2, "Fallback absolute request failed for period {p}", p);
                        continue;
                    }
                }

                if (res == null)
                {
                    _logger.LogWarning("No response for period {p}", p);
                    continue;
                }

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AlertaDengue returned non-success {status} for period {p}", res.StatusCode, p);
                    var respText = await res.Content.ReadAsStringAsync();
                    _logger.LogDebug("Response content (non-success): {content}", respText);
                    continue;
                }

                var content = await res.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("Empty content returned for period {p}", p);
                    continue;
                }

                JsonDocument doc;
                try
                {
                    doc = JsonDocument.Parse(content);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "JSON parse failed for period {p}. Content starts: {start}", p, content?.Substring(0, Math.Min(200, content.Length)));
                    continue;
                }

                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                {
                    _logger.LogWarning("Returned JSON is not an array for period {p}. Kind={kind}", p, doc.RootElement.ValueKind);
                    continue;
                }

                var processed = 0;
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    try
                    {
                        var alert = MapJsonElementToDengueAlert(el, GEO_BELO_HORIZONTE);
                        if (alert != null)
                        {
                            await _repo.UpsertAsync(alert);
                            processed++;
                            _logger.LogDebug("Upserted alert ey={ey} ew={ew} id={id}", alert.EndYear, alert.EndWeek, alert.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed mapping/upsert for element: {element}", el.ToString());
                    }
                }

                _logger.LogInformation("Finished period {p}. Processed {count} elements.", p, processed);
            }
        }


        private DengueAlerta? MapJsonElementToDengueAlert(JsonElement el, int geocode)
        {
            static long? GetLong(JsonElement src, string name)
            {
                if (!src.TryGetProperty(name, out var prop)) return null;
                try
                {
                    if (prop.ValueKind == JsonValueKind.Number)
                    {
                        if (prop.TryGetInt64(out var v64)) return v64;
                        if (prop.TryGetDouble(out var dv)) return Convert.ToInt64(dv);
                    }
                    if (prop.ValueKind == JsonValueKind.String)
                    {
                        var s = prop.GetString();
                        if (long.TryParse(s, out var p)) return p;
                    }
                }
                catch {  }
                return null;
            }

            static int? GetInt(JsonElement src, string name)
            {
                if (!src.TryGetProperty(name, out var prop)) return null;
                try
                {
                    if (prop.ValueKind == JsonValueKind.Number)
                    {
                        if (prop.TryGetInt32(out var v32)) return v32;
                        if (prop.TryGetInt64(out var v64) && v64 >= int.MinValue && v64 <= int.MaxValue) return (int)v64;
                        if (prop.TryGetDouble(out var dv))
                        {
                            if (Math.Abs(dv - Math.Round(dv)) < 1e-6) return (int)Math.Round(dv);
                        }
                    }
                    if (prop.ValueKind == JsonValueKind.String)
                    {
                        var s = prop.GetString();
                        if (int.TryParse(s, out var p)) return p;
                        if (double.TryParse(s, out var pd) && Math.Abs(pd - Math.Round(pd)) < 1e-6) return (int)Math.Round(pd);
                    }
                }
                catch { }
                return null;
            }

            static double? GetDouble(JsonElement src, string name)
            {
                if (!src.TryGetProperty(name, out var prop)) return null;
                try
                {
                    if (prop.ValueKind == JsonValueKind.Number)
                    {
                        if (prop.TryGetDouble(out var dv)) return dv;
                    }
                    if (prop.ValueKind == JsonValueKind.String)
                    {
                        var s = prop.GetString();
                        if (double.TryParse(s, out var pd)) return pd;
                    }
                }
                catch { }
                return null;
            }

            static string? GetString(JsonElement src, string name)
            {
                if (!src.TryGetProperty(name, out var prop)) return null;
                try
                {
                    if (prop.ValueKind == JsonValueKind.String) return prop.GetString();
                    return prop.GetRawText();
                }
                catch { return null; }
            }

            var idVal = GetLong(el, "id");
            if (!idVal.HasValue)
            {
                _logger.LogWarning("Element missing 'id' or id not parseable: {el}", el.ToString());
                return null;
            }
            var id = idVal.Value;

            int seNumeric = 0;
            if (el.TryGetProperty("SE", out var seProp))
            {
                try
                {
                    if (seProp.ValueKind == JsonValueKind.Number)
                    {
                        if (seProp.TryGetInt32(out var s32)) seNumeric = s32;
                        else if (seProp.TryGetInt64(out var s64)) seNumeric = (int)s64;
                        else if (seProp.TryGetDouble(out var sd)) seNumeric = (int)sd;
                    }
                    else if (seProp.ValueKind == JsonValueKind.String)
                    {
                        var s = seProp.GetString();
                        if (!int.TryParse(s, out seNumeric)) seNumeric = 0;
                    }
                }
                catch
                {
                    seNumeric = 0;
                }
            }

            if (seNumeric <= 0)
            {
                _logger.LogWarning("SE value parsed as 0 or missing for id={id}. Element: {el}", id, el.ToString());
            }

            var ey = seNumeric / 100;
            var ew = seNumeric % 100;

            var dataIniMs = GetLong(el, "data_iniSE");
            DateTime dataIni = dataIniMs.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(dataIniMs.Value).UtcDateTime : DateTime.UtcNow;

            var alert = new DengueAlerta
            {
                Id = id,
                GeoCode = geocode,
                SemanaEpidemiologica = seNumeric,
                EndWeek = ew,
                EndYear = ey,
                DataIniSE = dataIni,
                CasosEst = GetDouble(el, "casos_est"),
                CasosEstMin = GetDouble(el, "casos_est_min"),
                CasosEstMax = GetDouble(el, "casos_est_max"),
                Casos = GetInt(el, "casos"),
                PRt1 = GetDouble(el, "p_rt1"),
                PInc100k = GetDouble(el, "p_inc100k"),
                VersaoModelo = GetString(el, "versao_modelo"),
                Rt = GetDouble(el, "Rt"),
                Pop = GetInt(el, "pop"),
                TempMin = GetDouble(el, "tempmin"),
                TempMed = GetDouble(el, "tempmed"),
                TempMax = GetDouble(el, "tempmax"),
                UmidMin = GetDouble(el, "umidmin"),
                UmidMed = GetDouble(el, "umidmed"),
                UmidMax = GetDouble(el, "umidmax"),
                Receptivo = GetInt(el, "receptivo"),
                Transmissao = GetInt(el, "transmissao"),
                Nivel = GetInt(el, "nivel"),
                NivelInc = GetInt(el, "nivel_inc"),
                CasProv = GetInt(el, "casprov"),
                NotifAccumYear = GetInt(el, "notif_accum_year"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return alert;
        }

        public IEnumerable<SemanaParams> GetUltimasSemanasCompletas(int count)
        {
            var semanas = new List<SemanaParams>();
           
            DateTime referenceDate = DateTime.Now;

            DateTime ultimoDiaCompleto = EpidemiologicalWeekHelper.GetUltimoDiaSECompleta(referenceDate);

            for (int i = 0; i < count; i++)
            {
                DateTime dataDaSemana = ultimoDiaCompleto.AddDays(-7 * i);

                var (week, year) = EpidemiologicalWeekHelper.GetEpiWeekForDate(dataDaSemana);

                semanas.Add(new SemanaParams
                {
                    ew = week,
                    ey = year
                });
            }

            return semanas;
        }
    }
}
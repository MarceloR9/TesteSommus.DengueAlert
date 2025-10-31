using Microsoft.AspNetCore.Mvc;
using DengueAlert.Api.Repository;
using System.Threading.Tasks;

namespace DengueAlert.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DengueController : ControllerBase
    {
        private readonly IDengueRepository _repo;
        private readonly Services.AlertDengueService _alteraService;
        private readonly ILogger<DengueController> _logger;

        public DengueController(IDengueRepository repo, Services.AlertDengueService alteraService, ILogger<DengueController> logger)
        {
            _repo = repo;
            _alteraService = alteraService;
            _logger = logger;
        }

        [HttpGet("semana")]
        public async Task<IActionResult> GetBySemana([FromQuery] int ew, [FromQuery] int ey)
        {
            _logger.LogInformation("GetBySemana called with ew={ew}, ey={ey}", ew, ey);

            if (ew <= 0 || ey <= 0) return BadRequest("Parâmetros ew e ey são obrigatórios e devem ser positivos.");

            var item = await _repo.GetByWeekAsync(ew, ey);
            if (item == null)
            {
                _logger.LogInformation("No data found for ey={ey}, ew={ew}", ey, ew);
                return NotFound(new { message = $"Nenhum registro encontrado para ey={ey}, ew={ew}" });
            };

            var dto = new
            {
                semana_epidemiologica = $"{item.EndYear}-{item.EndWeek:D2}",
                casos_est = item.CasosEst,
                casos_notificados = item.Casos,
                nivel_alerta = item.Nivel
            };

            return Ok(dto);
        }

        [HttpPost("importar")]
        public async Task<IActionResult> Importar()
        {
            await _alteraService.ImportLastMonthsAsync(6);
            return Ok(new { message = "Importação iniciada (últimos 6 meses)." });
        }

        [HttpGet("ultimas-semanas/{count}")]
        public async Task<IActionResult> GetLastNWeeks(int count = 3)
        {
            if (count <= 0 || count > 10)
                return BadRequest("O parâmetro 'count' deve estar entre 1 e 10.");

            var semanasACompletar = _alteraService.GetUltimasSemanasCompletas(count);

            var results = new List<object>();

            foreach (var semana in semanasACompletar)
            {
                var item = await _repo.GetByWeekAsync(semana.ew, semana.ey);

                results.Add(new
                {
                    semana_epidemiologica = $"{semana.ey}-{semana.ew:D2}",
                    casos_est = item?.CasosEst, 
                    casos_notificados = item?.Casos,
                    nivel_alerta = item?.Nivel,
                    error = item == null ? $"Dados indisponíveis (SE {semana.ew}/{semana.ey})" : (string?)null
                });
            }
            return Ok(results);
        }
    }
}
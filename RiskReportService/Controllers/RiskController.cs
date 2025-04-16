using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiskReportService.Models;
using RiskReportService.Services.Interfaces;

namespace RiskReportService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class RiskController : ControllerBase
    {
        private readonly IRiskAssessmentService _riskService;
        private readonly IServiceProvider _services;

        public RiskController(IRiskAssessmentService riskService, IServiceProvider services)
        {
            _riskService = riskService;
            _services = services;
        }

        [HttpGet("{patientId}")]
        public async Task<ActionResult<RiskLevel>> GetRiskLevel(Guid patientId)
        {
            try
            {
                var risk = await _riskService.AssessRiskAsync(patientId);
                return Ok(risk);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur dans l'évaluation du risque : {ex.Message}");
            }
        }
    }
}

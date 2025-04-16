using Microsoft.AspNetCore.Mvc;
using RiskAssessmentService.Services;
using RiskAssessmentService.Enums;

namespace RiskAssessmentControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiskController : ControllerBase 
    {
        private readonly IRiskAssessmentService _riskService;

        public RiskController(IRiskAssessmentService riskService)
        {
            _riskService = riskService;
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
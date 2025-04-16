using Moq;
using RiskReportService.Models;
using RiskReportService.Services;
using RiskReportService.Services.Interfaces;
using Xunit;

namespace RiskReportService.Tests
{
    public class RiskAssessmentServiceTests
    {
        private readonly Mock<IPatientClient> _mockPatientClient;
        private readonly Mock<INoteClient> _mockNoteClient;
        private readonly RiskAssessmentService _service;

        public RiskAssessmentServiceTests()
        {
            _mockPatientClient = new Mock<IPatientClient>();
            _mockNoteClient = new Mock<INoteClient>();
            _service = new RiskAssessmentService(_mockPatientClient.Object, _mockNoteClient.Object);
        }

        [Theory]
        [InlineData("TestNone", 40, 1, 0, RiskLevel.None)]
        [InlineData("TestBorderline", 45, 1, 3, RiskLevel.Borderline)]
        [InlineData("TestInDanger", 25, 0, 3, RiskLevel.InDanger)] // homme <30 ans, 3 triggers
        [InlineData("TestEarlyOnset", 25, 0, 5, RiskLevel.EarlyOnset)] // homme <30 ans, 5 triggers
        [InlineData("TestInDangerF", 25, 1, 4, RiskLevel.InDanger)] // femme <30 ans, 4 triggers
        [InlineData("TestEarlyOnsetF", 25, 1, 7, RiskLevel.EarlyOnset)] // femme <30 ans, 7 triggers
        [InlineData("TestEarlyOnsetOld", 65, 0, 8, RiskLevel.EarlyOnset)] // >30 ans, 8 triggers
        public async Task AssessRiskAsync_ReturnsExpectedRiskLevel(string name, int age, int genre, int triggerCount, RiskLevel expectedRisk)
        {
            // Arrange
            var dateOfBirth = DateTime.Today.AddYears(-age);

            _mockPatientClient.Setup(p => p.GetPatientByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new PatientDto
                {
                    Id = Guid.NewGuid(),
                    Nom = name,
                    Prenom = name,
                    Genre = genre,
                    DateNaissance = dateOfBirth,
                    Adresse = "Test Address",
                    Telephone = "1234567890"
                });

            var triggers = new List<string>
            {
                "Hémoglobine A1C", "Microalbumine", "Taille", "Poids", "Fumeur",
                "Fumeuse", "Anormal", "Cholestérol", "Vertiges", "Rechute",
                "Réaction", "Anticorps"
            };

            string noteText = string.Join(" ", triggers.Take(triggerCount));

            _mockNoteClient.Setup(n => n.GetNotesByPatientIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<NoteDto>
                {
                    new NoteDto { Content = noteText }
                });

            // Act
            var risk = await _service.AssessRiskAsync(Guid.NewGuid());

            // Assert
            Assert.Equal(expectedRisk, risk);
        }
    }
}

using System;
using System.Collections.Generic;

namespace FitLifeFitness.Models
{
    public class AnalyticsDashboardDTO
    {
        public string UserId { get; set; } = string.Empty;

        public int CrowdCount { get; set; }

        public List<ClassResultDTO> ClassResults { get; set; } = new();

        public List<SoloTrainingResultsDTO> SoloTrainingResults { get; set; } = new();

        public DateTime ServerTimeUtc { get; set; }
    }
}
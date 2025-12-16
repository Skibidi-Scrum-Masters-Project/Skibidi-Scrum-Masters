using System;
using System.Collections.Generic;

namespace FitLifeFitness.Models
{
    public sealed class AnalyticsCompareDTO
    {
        public string UserId { get; set; } = string.Empty;

        public int Rank { get; set; }
        public int ActiveMembers { get; set; }

        public List<CompareMetricDTO> Metrics { get; set; } = new();
        public List<LeaderboardRowDTO> Leaderboard { get; set; } = new();

        public DateTime PeriodStartUtc { get; set; }
        public DateTime PeriodEndUtc { get; set; }
    }

    public sealed class CompareMetricDTO
    {
        public string Name { get; set; } = string.Empty;
        public double You { get; set; }
        public double Avg { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public sealed class LeaderboardRowDTO
    {
        public int Rank { get; set; }
        public string UserId { get; set; } = string.Empty;
        public double Score { get; set; }
        public string StatsText { get; set; } = string.Empty;
    }
}
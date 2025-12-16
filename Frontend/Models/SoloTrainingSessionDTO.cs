
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace FitLifeFitness.Models
{
    public class SoloTrainingSessionDTO
    {

        public string? Id { get; set; }

        public string? WorkoutProgramId { get; set; }

        public string? WorkoutProgramName { get; set; }


        public string UserId { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public int DurationMinutes { get; set; }

        public List<ExerciseDTO> Exercises { get; set; } = new List<ExerciseDTO>();
    }
    public enum TrainingType
    {
        Cardio,
        UpperBody,
        LowerBody,
        SixPack
    }
}
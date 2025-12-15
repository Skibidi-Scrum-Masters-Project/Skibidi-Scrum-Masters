using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitLifeFitness.Models
{
    public class WorkoutProgramDTO
    {

        public string? Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<ExerciseType> ExerciseTypes { get; set; } = new List<ExerciseType>();
    }
        public enum ExerciseType
    {
        BenchPress,
        Squat,
        Deadlift,
        PullUp,
        PushUp
    }
}
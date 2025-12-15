    namespace FitLifeFitness.Models;
    public class ExerciseDTO
    {
    
        public ExerciseType ExerciseType { get; set; }
  
        public double Volume { get; set; } 
        public List<Set> Sets { get; set; } = new List<Set>();
    }
        public class Set
    {
        public int Repetitions { get; set; }
        public double Weight { get; set; }
        // UI-only flag to indicate this set was finished by the user
        public bool IsFinished { get; set; }
        // UI-only flag to indicate this set was prefilled from previous session
        public bool IsPlaceholder { get; set; }
            // UI-only flag to indicate the user has typed a value for this set
            public bool HasUserValue { get; set; }
    }
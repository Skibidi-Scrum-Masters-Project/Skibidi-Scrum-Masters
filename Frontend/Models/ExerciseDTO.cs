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
    }
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
        // Chest
        BenchPress,
        InclineBench,
        DeclineBench,
        ChestFly,
        PushUp,
        DumbbellBench,

        // Back
        Deadlift,
        RomanianDeadlift,
        BarbellRow,
        DumbbellRow,
        PullUp,
        LatPulldown,
        SeatedCableRow,
        FacePull,

        // Shoulders
        OverheadPress,
        ArnoldPress,
        LateralRaise,
        FrontRaise,
        RearDeltFly,

        // Legs / Glutes
        Squat,
        FrontSquat,
        BulgarianSplitSquat,
        Lunges,
        LegPress,
        HipThrust,
        GluteBridge,
        CalfRaise,

        // Arms
        BicepCurl,
        HammerCurl,
        TricepExtension,
        TricepDip,
        CloseGripBench,

        // Core / Conditioning
        Plank,
        RussianTwist,
        HangingLegRaise,
        MountainClimber,
        Burpee,
        KettlebellSwing,
        BoxJump,
        JumpSquat,

        // Misc / Carries
        FarmerCarry,
        SledPush,
        BattleRope,
        TRXRow,
        SingleLegDeadlift,
        CableWoodchop
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FitnessApp.Shared.Models
{
    public class Exercise
    {
        [Required]
        [BsonElement("exerciseType")]
        public ExerciseType ExerciseType { get; set; }
        [Required]
        public double Volume { get; set; }
        public List<Set> Sets { get; set; } = new List<Set>();
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
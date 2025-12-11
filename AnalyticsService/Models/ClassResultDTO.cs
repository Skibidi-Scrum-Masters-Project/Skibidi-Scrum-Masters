using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AnalyticsService.Models   
{
    public class ClassResultDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }
        public string ClassId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public double TotalCaloriesBurned { get; set; }
        public string Category { get; set; } = string.Empty;
        public int DurationMin { get; set; }
        public DateTime Date { get; set; }
    }
}
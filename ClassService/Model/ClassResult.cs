using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClassService.Model
{
    public class ClassResult
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }
        public string ClassId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public Double CaloriesBurned { get; set; }
        public Double Watt { get; set; }
        public int DurationMin { get; set; }
        public DateTime Date { get; set; }
        
        [BsonIgnore]
        public string EventId { get; set; } = string.Empty;
    }
}
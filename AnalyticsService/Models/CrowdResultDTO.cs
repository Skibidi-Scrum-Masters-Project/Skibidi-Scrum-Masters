using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AnalyticsService.Models
{
    public class CrowdResultDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }

        public DateTime ExitTime { get; set; }

        public timestatus Status { get; set; }

        public enum timestatus
        {
            Entered,
            Exited
        }


    }

    
    
}

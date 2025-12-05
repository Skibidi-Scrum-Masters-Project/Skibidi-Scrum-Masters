using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FitnessApp.Shared.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }
        
        [Required]
        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [BsonElement("firstName")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [BsonElement("lastName")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;
        
        [BsonElement("licensePlate")]
        public string? LicensePlate { get; set; }
        
        [BsonElement("role")]
        public Role Role { get; set; } = Role.Member;
        
        [BsonElement("salt")]
        public string? Salt { get; set; }
        
        [BsonElement("hashedPassword")]
        public string? HashedPassword { get; set; }
    }
}
using Amazon.DynamoDBv2.DataModel;
using System;

namespace social_media9.Api.Models
{
    [DynamoDBTable("Users")]
    public class User
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        // Add more fields as needed
    }
}

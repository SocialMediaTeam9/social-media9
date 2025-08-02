using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using social_media9.Api.Data;
using social_media9.Api.Models;


namespace social_media9.Api.Data
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private const string TableName = "Comments";

        public CommentRepository(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb = dynamoDb;
        }

        public async Task SaveCommentAsync(Comment comment)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                ["PK"] = new AttributeValue($"CONTENT#{comment.ContentId}"),
                ["SK"] = new AttributeValue($"COMMENT#{comment.CommentId}"),
                ["commentId"] = new AttributeValue(comment.CommentId),
                ["userId"] = new AttributeValue(comment.UserId),
                ["username"] = new AttributeValue(comment.Username),
                ["text"] = new AttributeValue(comment.Text),
                ["createdAt"] = new AttributeValue(comment.CreatedAt.ToString("o"))
            };

            await _dynamoDb.PutItemAsync(new PutItemRequest
            {
                TableName = TableName,
                Item = item
            });
        }

        public async Task<List<Comment>> GetCommentsByContentAsync(string contentId)
        {
            var request = new QueryRequest
            {
                TableName = TableName,
                KeyConditionExpression = "PK = :pk and begins_with(SK, :prefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":pk"] = new AttributeValue($"CONTENT#{contentId}"),
                    [":prefix"] = new AttributeValue("COMMENT#")
                }
            };

            var result = await _dynamoDb.QueryAsync(request);
            return result.Items.Select(MapToComment).ToList();
        }

        private Comment MapToComment(Dictionary<string, AttributeValue> item)
        {
            return new Comment
            {
                CommentId = item["commentId"].S,
                ContentId = item["PK"].S.Split('#')[1],
                UserId = item["userId"].S,
                Username = item["username"].S,
                Text = item["text"].S,
                CreatedAt = DateTime.Parse(item["createdAt"].S)
            };
        }

        public async Task DeleteCommentAsync(string commentId, string contentId)
        {
            var request = new DeleteItemRequest
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue($"CONTENT#{contentId}") },
                { "SK", new AttributeValue($"COMMENT#{commentId}") }
            }
            };

            await _dynamoDb.DeleteItemAsync(request);
        }

        public async Task<bool> UpdateCommentAsync(string contentId, string commentId, string newText)
        {
            var key = new Dictionary<string, AttributeValue>
        {
            { "PK", new AttributeValue { S = $"CONTENT#{contentId}" } },
            { "SK", new AttributeValue { S = $"COMMENT#{commentId}" } }
        };

            var updateRequest = new UpdateItemRequest
            {
                TableName = TableName,
                Key = key,
                UpdateExpression = "SET #text = :t",
                ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#text", "text" }  // Use expression attribute names in case "text" is a reserved word
            },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":t", new AttributeValue { S = newText } }
            }
            };

            var response = await _dynamoDb.UpdateItemAsync(updateRequest);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

    }
}
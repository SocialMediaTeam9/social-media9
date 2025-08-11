using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class InboxController : ControllerBase
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _config;
    private readonly ILogger<InboxController> _logger;

    public InboxController(IAmazonSQS sqsClient, IConfiguration config, ILogger<InboxController> logger)
    {
        _sqsClient = sqsClient;
        _config = config;
        _logger = logger;
    }

    [HttpPost("/users/{username}/inbox")]
    [AllowAnonymous]
    public async Task<IActionResult> PostToInbox(string username, [FromBody] object activity)
    {
        var queueUrl = _config["AWS:InboundSqsQueueUrl"];

        try
        {
            await _sqsClient.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = JsonSerializer.Serialize(activity)
            });

            _logger.LogInformation("Successfully queued activity for user inbox: {Username}", activity);

            _logger.LogInformation("Successfully queued activity for user inbox: {Username}", username);

            // Return 202 Accepted to the remote server immediately.
            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to SQS queue {QueueUrl}", queueUrl);
            return StatusCode(500, "Internal error while queueing activity.");
        }
    }
}
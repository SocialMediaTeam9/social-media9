using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class InboxController : ControllerBase
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _config;
    
    public InboxController(IAmazonSQS sqsClient, IConfiguration config)
    {
        _sqsClient = sqsClient;
        _config = config;
    }

    [HttpPost("/users/{username}/inbox")]
    public async Task<IActionResult> PostToInbox(string username, [FromBody] object activity)
    {
        // Because this request went through the HttpSignatureValidationMiddleware,
        // we can trust that it is authentic.

        await _sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = _config["Aws:InboundSqsQueueUrl"],
            MessageBody = JsonSerializer.Serialize(activity)
        });

        return Accepted();
    }
}
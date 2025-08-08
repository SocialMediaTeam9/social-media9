using MediatR;
using Newtonsoft.Json.Linq;
using social_media9.Api.Infrastructure.ActivityPub.Services;

namespace social_media9.Api.Application.ActivityPub.Commands;

public record SendActivityCommand(string InboxUrl, JObject Activity, string PrivateKeyPem) : IRequest;

public class SendActivityCommandHandler : IRequestHandler<SendActivityCommand>
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly HttpSignatureService _signatureService;

    public SendActivityCommandHandler(IHttpClientFactory httpFactory, HttpSignatureService signatureService)
    {
        _httpFactory = httpFactory;
        _signatureService = signatureService;
    }

    public async Task Handle(SendActivityCommand request, CancellationToken cancellationToken)
    {
        var client = _httpFactory.CreateClient();

        var json = request.Activity.ToString(Newtonsoft.Json.Formatting.None);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/activity+json");

        var signedHeaders = _signatureService.SignHeaders(
            request.InboxUrl,
            json,
            request.PrivateKeyPem
        );

        foreach (var header in signedHeaders)
        {
            content.Headers.Add(header.Key, header.Value);
        }

        var response = await client.PostAsync(request.InboxUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

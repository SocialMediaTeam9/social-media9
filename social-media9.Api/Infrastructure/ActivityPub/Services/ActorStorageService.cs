using social_media9.Api.Domain.ActivityPub.Entities;

namespace social_media9.Api.Infrastructure.ActivityPub.Services;

public interface IActorStorageService
{
    Task<Actor?> GetActorAsync(string username);
    Task SaveActorAsync(Actor actor);
    Task<IEnumerable<object>> GetOutboxAsync(string username);
}

public class InMemoryActorStorageService : IActorStorageService
{
    private readonly Dictionary<string, Actor> _actors = new();
    private readonly Dictionary<string, List<object>> _outbox = new();

    public Task<Actor?> GetActorAsync(string username)
    {
        _actors.TryGetValue(username, out var actor);
        return Task.FromResult(actor);
    }

    public Task SaveActorAsync(Actor actor)
    {
        _actors[actor.PreferredUsername] = actor;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<object>> GetOutboxAsync(string username)
    {
        _outbox.TryGetValue(username, out var activities);
        return Task.FromResult((IEnumerable<object>)(activities ?? new()));
    }
}

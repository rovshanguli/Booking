using System.Collections.Concurrent;
using Api.Models;

namespace Api.Repositories
{

    public interface IHomeRepository
    {
        Task UpsertAsync(Home home);
        Task<IReadOnlyCollection<Home>> GetAllAsync();
    }

    public class InMemoryHomeRepository : IHomeRepository
    {
        private readonly ConcurrentDictionary<string, Home> _store = new();

        public Task UpsertAsync(Home home)
        {
            _store[home.HomeId] = home;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Home>> GetAllAsync()
            => Task.FromResult((IReadOnlyCollection<Home>)_store.Values.ToList());
    }


}

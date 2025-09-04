using Api.DTOs;
using Api.Models;

namespace Api.Services
{
    public interface IHomeService
    {
        Task<List<Home>> GetAvailableHomes(DateOnly startDate, DateOnly endDate, CancellationToken ct = default);
    }
}

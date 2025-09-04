using System.Globalization;
using Api.DTOs;
using Api.Models;
using Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Services
{
    public class HomeService : IHomeService
    {
        private readonly IHomeRepository _repo;

        public HomeService(IHomeRepository repo)
        {
            _repo = repo;
        }


        public async Task<List<Home>> GetAvailableHomes(DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
        {
            List<DayHomesDto> perDayHomes = await GetHomeIdsPerDayAsync(startDate,endDate);
            var result = IntersectByPivot(perDayHomes);

            var allHomes = await _repo.GetAllAsync();
            var set = new HashSet<string>(result.homeIds, StringComparer.Ordinal);
            var homes = allHomes.Where(h => set.Contains(h.HomeId)).ToList();

            return homes;
        }


        private async Task<List<DayHomesDto>> GetHomeIdsPerDayAsync(DateOnly start, DateOnly end, CancellationToken ct = default)
        {
           

            var homes = await _repo.GetAllAsync();

            var slotSetsByHomeId = homes.ToDictionary(
                h => h.HomeId,
                h => (h.AvailableSlots ?? new())
                    .ToHashSet(StringComparer.Ordinal),
                StringComparer.Ordinal);

            var result = new List<DayHomesDto>();

            for (var d = start; d <= end; d = d.AddDays(1))
            {
                var key = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                var dayHomeIds = slotSetsByHomeId
                    .Where(kvp => kvp.Value.Contains(key))
                    .Select(kvp => kvp.Key)
                    .ToArray();

                if (dayHomeIds.Length > 0)
                {
                    result.Add(new DayHomesDto(Date: key, Homes: dayHomeIds));
                }
            }

            return result;
        }

        private static (string pivotDate, List<string> homeIds) IntersectByPivot(List<DayHomesDto> perDay)
        {
            if (perDay is null || perDay.Count == 0)
                return (default, new List<string>());

            if (perDay.Any(p => p.Homes is null || p.Homes.Count == 0))
                return (default, new List<string>());

            var ordered = perDay.OrderBy(p => p.Homes.Count).ToArray();
            var pivotDate = ordered[0].Date;

            var otherSets = new List<HashSet<string>>(ordered.Length - 1);
            for (int i = 1; i < ordered.Length; i++)
            {
                var hs = ordered[i].Homes as HashSet<string>;
                otherSets.Add(hs ?? new HashSet<string>(ordered[i].Homes, StringComparer.Ordinal));
            }

            var result = new List<string>();
            var pivotEnum = ordered[0].Homes.Distinct(StringComparer.Ordinal);
            foreach (var id in pivotEnum)
            {
                bool ok = true;
                foreach (var set in otherSets)
                {
                    if (!set.Contains(id)) { ok = false; break; }
                }
                if (ok) result.Add(id);
            }

            return (pivotDate, result);
        }
    }
}

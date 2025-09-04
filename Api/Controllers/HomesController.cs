using System.Globalization;
using Api.DTOs;
using Api.Models;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class HomesController : ControllerBase
    {
        private readonly IHomeService _service;
        private readonly IHomeRepository _repo;

        public HomesController(IHomeService service, IHomeRepository repo)
        {
            _service = service;
            _repo = repo;
        }

        


        [HttpGet("available-homes")]
        public async Task<IActionResult> GetAvailableHomes([FromQuery] string startDate, [FromQuery] string endDate)
        {
            var startDt = ParseToDate(startDate);
            var endDt = ParseToDate(endDate);

            if (startDt is null || endDt is null)
                return BadRequest("Invalid date format. Use yyyy-MM-dd (e.g., 2025-07-15).");

            if (endDt < startDt)
                return BadRequest("`endDate` must be greater than or equal to `startDate`.");

            List<Home> homes = await _service.GetAvailableHomes(startDt.Value,endDt.Value);

            return Ok(homes.OrderBy(h => int.Parse(h.HomeId)));
        }


       

        
        private static DateOnly? ParseToDate(string input)
        {
            var norm = NormalizeDateString(input);
            if (norm is null) return null;

            return DateOnly.TryParseExact(norm, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                          DateTimeStyles.None, out var dt)
                ? dt
                : null;
        }

        private static string? NormalizeDateString(string? input)
        {
            var s = input?.Trim();
            if (string.IsNullOrEmpty(s)) return null;

            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var dtExact))
                return dtExact.ToString("yyyy-MM-dd");

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt.ToString("yyyy-MM-dd");

            return null;
        }

        
    }
}

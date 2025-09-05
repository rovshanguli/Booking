using Api.Models;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.RegularExpressions;
using Api.Extensions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class HomesController : ControllerBase
    {
        private readonly IHomeService _service;

        public HomesController(IHomeService service)
        {
            _service = service;
        }

        [HttpGet("available-homes")]
        public async Task<IActionResult> GetAvailableHomes([FromQuery] string? startDate, [FromQuery] string? endDate)
        {
            var startDt = startDate.ParseToDate();
            var endDt = endDate.ParseToDate();

            if (startDt is null || endDt is null)
                return BadRequest("Invalid date format. Use yyyy-MM-dd (e.g., 2025-07-15).");

            if (endDt < startDt)
                return BadRequest("`endDate` must be greater than or equal to `startDate`.");

            List<Home> homes = await _service.GetAvailableHomes(startDt.Value, endDt.Value);

            return Ok(homes.OrderBy(h => int.Parse(h.HomeId)));
        }
    }
}
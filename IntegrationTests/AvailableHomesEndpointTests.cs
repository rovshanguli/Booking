using Api.Models;
using Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace IntegrationTests
{
    public class AvailableHomesEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AvailableHomesEndpointTests(CustomWebApplicationFactory factory) => _factory = factory;

        [Fact]
        public async Task ValidDates_ReturnsIntersection_NumericSorted()
        {
            var repo = new InMemoryHomeRepository();
            await repo.UpsertAsync(new Home { HomeId = "10", AvailableSlots = new List<string> { "2025-07-01", "2025-07-02", "2025-07-03" } });
            await repo.UpsertAsync(new Home { HomeId = "2", AvailableSlots = new List<string> { "2025-07-01", "2025-07-02" } });
            await repo.UpsertAsync(new Home { HomeId = "001", AvailableSlots = new List<string> { "2025-07-01" } });
            await repo.UpsertAsync(new Home { HomeId = "7", AvailableSlots = new List<string> { "2025-07-02" } });

            var client = _factory.WithWebHostBuilder(b =>
            {
                b.ConfigureTestServices(s =>
                {
                    s.RemoveAll<IHomeRepository>();
                    s.AddSingleton<IHomeRepository>(repo);
                });
            }).CreateClient();

            var resp = await client.GetAsync("/api/available-homes?startDate=2025-07-01&endDate=2025-07-02");

            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            resp.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

            var payload = await resp.Content.ReadFromJsonAsync<List<HomeDto>>();
            payload.Should().NotBeNull();
            payload!.ConvertAll(h => h.HomeId).Should().Equal("2", "10");
        }

        [Fact]
        public async Task Sorting_WithLeadingZeros_IsNumeric()
        {
            var repo = new InMemoryHomeRepository();
            var slots = new List<string> { "2025-07-01", "2025-07-02" };
            await repo.UpsertAsync(new Home { HomeId = "10", AvailableSlots = slots });
            await repo.UpsertAsync(new Home { HomeId = "2", AvailableSlots = slots });
            await repo.UpsertAsync(new Home { HomeId = "001", AvailableSlots = slots });

            var client = _factory.WithWebHostBuilder(b =>
            {
                b.ConfigureTestServices(s =>
                {
                    s.RemoveAll<IHomeRepository>();
                    s.AddSingleton<IHomeRepository>(repo);
                });
            }).CreateClient();

            var resp = await client.GetAsync("/api/available-homes?startDate=2025-07-01&endDate=2025-07-02");

            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var payload = await resp.Content.ReadFromJsonAsync<List<HomeDto>>();
            payload!.ConvertAll(h => h.HomeId).Should().Equal("001", "2", "10");
        }

        [Fact]
        public async Task NoMatches_Returns200_EmptyArray()
        {
            var repo = new InMemoryHomeRepository();
            await repo.UpsertAsync(new Home { HomeId = "1", AvailableSlots = new List<string> { "2025-07-01" } });
            await repo.UpsertAsync(new Home { HomeId = "2", AvailableSlots = new List<string> { "2025-07-03" } });

            var client = _factory.WithWebHostBuilder(b =>
            {
                b.ConfigureTestServices(s =>
                {
                    s.RemoveAll<IHomeRepository>();
                    s.AddSingleton<IHomeRepository>(repo);
                });
            }).CreateClient();

            var resp = await client.GetAsync("/api/available-homes?startDate=2025-08-10&endDate=2025-08-12");

            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var payload = await resp.Content.ReadFromJsonAsync<List<HomeDto>>();
            payload.Should()!.NotBeNull();
            payload!.Should().BeEmpty();
        }

        [Fact]
        public async Task InvalidDateFormat_Returns400_WithMessage()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/available-homes?startDate=2025/07/01&endDate=2025-07-10");

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var text = await resp.Content.ReadAsStringAsync();
            text.Should().Be("Invalid date format. Use yyyy-MM-dd (e.g., 2025-07-15).");
        }

        [Fact]
        public async Task EndBeforeStart_Returns400_WithMessage()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/available-homes?startDate=2025-07-10&endDate=2025-07-01");

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var text = await resp.Content.ReadAsStringAsync();
            text.Should().Be("`endDate` must be greater than or equal to `startDate`.");
        }

        [Fact]
        public async Task MissingParam_Returns400_FormatMessage()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/available-homes?startDate=2025-07-01");

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var text = await resp.Content.ReadAsStringAsync();
            text.Should().Be("Invalid date format. Use yyyy-MM-dd (e.g., 2025-07-15).");
        }

        private sealed class HomeDto
        {
            public string HomeId { get; set; } = "";
            public List<string>? AvailableSlots { get; set; }
        }
    }
}
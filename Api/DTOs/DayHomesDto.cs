namespace Api.DTOs
{
    public record DayHomesDto(string Date, IReadOnlyCollection<string> Homes);
}

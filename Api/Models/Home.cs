namespace Api.Models
{
    public class Home
    {
        public string HomeId { get; set; } = default!;
        public string HomeName { get; set; } = default!;
        public List<string> AvailableSlots { get; set; } = new();
    }

}

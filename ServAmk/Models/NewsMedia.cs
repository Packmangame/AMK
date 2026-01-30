namespace ServAmk.Models
{
    public class NewsMedia
    {
        public Guid Id { get; set; }
        public Guid NewsId { get; set; }
        public string Type { get; set; } = "";
        public string MediaUrl { get; set; } = "";
        public string Caption { get; set; } = "";
    }
}

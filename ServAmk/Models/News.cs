namespace ServAmk.Models
{
    public class News
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string ShortDescription { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime PublishDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid AuthorId { get; set; }
    }
}

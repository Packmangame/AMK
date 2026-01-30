using SQLite;
using System.IO;

namespace AMK.Models.Headlines
{
    [Table("NewsMedia")]
    public class NewsMedia
    {
        [PrimaryKey, AutoIncrement]
        public int ID_newsMedia { get; set; }

        [NotNull]
        public int ID_news { get; set; }

        [NotNull]
        public string MediaUrl { get; set; }

        [NotNull]
        public string IconUrl { get; set; }

        public int ID_medType { get; set; }
        
        public string Caption { get; set; }

        // Convenience flags for UI rendering (not stored in DB)
        [Ignore]
        public bool IsImage
        {
            get
            {
                if (ID_medType == 1) return true;
                if (string.IsNullOrWhiteSpace(MediaUrl)) return false;
                var ext = Path.GetExtension(MediaUrl).ToLowerInvariant();
                return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp";
            }
        }

        [Ignore]
        public bool IsVideo
        {
            get
            {
                if (ID_medType == 2) return true;
                if (string.IsNullOrWhiteSpace(MediaUrl)) return false;
                var ext = Path.GetExtension(MediaUrl).ToLowerInvariant();
                return ext is ".mp4" or ".mov" or ".m4v";
            }
        }

        [Ignore]
        public bool IsAudio
        {
            get
            {
                if (ID_medType == 3) return true;
                if (string.IsNullOrWhiteSpace(MediaUrl)) return false;
                var ext = Path.GetExtension(MediaUrl).ToLowerInvariant();
                return ext is ".mp3" or ".wav" or ".m4a" or ".aac";
            }
        }
    }
}

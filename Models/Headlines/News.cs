using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Models.Headlines
{
    [Table("News")]
    public class News
    {
        [PrimaryKey, AutoIncrement]
        public int ID_news { get; set; }

        [NotNull]
        public string Title { get; set; }

        [NotNull]
        public string Content { get; set; }

        [NotNull, MaxLength(500)]
        public string ShortDescription { get; set; }

        [NotNull]
        public int AuthorID { get; set; } /*UserID*/

        [NotNull]
        public DateTime PublishDate { get; set; }

        [NotNull]
        public DateTime CreatedAt { get; set; }
    }
}

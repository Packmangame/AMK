using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

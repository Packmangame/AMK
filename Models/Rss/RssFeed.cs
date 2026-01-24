using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Models.Rss
{
    public class RssFeed
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Color { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}

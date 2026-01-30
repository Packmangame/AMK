using AMK.Models.Headlines;
using AMK.Models.Rss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Services
{
    public interface INewsService
    {
        Task<List<RssNewsItems>> LoadNewsFromFeedAsync(string feedUrl, string sourceName);
        Task<List<RssNewsItems>> LoadAllNewsAsync(int skip, int take);
        
    }
}

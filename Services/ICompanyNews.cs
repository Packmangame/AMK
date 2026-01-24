using AMK.Models.Headlines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Services
{
    public interface ICompanyNews
    {
        Task<List<News>> GetAllNewsAsync();
        Task<List<News>> GetNewsByAuthorAsync(int authorId);
        Task<News> GetNewsByIdAsync(int id);
        Task<int> AddNewsAsync(News news);
        Task<int> UpdateNewsAsync(News news);
        Task<int> DeleteNewsAsync(int id);
        Task<bool> NewsExistsAsync(int id);

        // Media
        Task<List<NewsMedia>> GetMediaForNewsAsync(int newsId);
        Task<int> AddMediaAsync(NewsMedia media);
        Task<int> DeleteMediaAsync(int mediaId);
        Task<int> DeleteMediaByNewsAsync(int newsId);
    }
}

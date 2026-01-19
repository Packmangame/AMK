using AMK.Models.Headlines;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Services
{
    public class CompanyNews: ICompanyNews
    {
        public CompanyNews(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            InitializeDatabaseAsync();
        }

        private SQLiteAsyncConnection _database;

        private async Task InitializeDatabaseAsync()
        {
            await _database.CreateTableAsync<News>();
            await _database.CreateTableAsync<NewsMedia>();
            await _database.CreateTableAsync<MediaTypes>();

          
            var mediaTypesCount = await _database.Table<MediaTypes>().CountAsync();
            if (mediaTypesCount == 0)
            {
                var defaultTypes = new List<MediaTypes>
                {
                    new MediaTypes { TypeName = "Изображение" },
                    new MediaTypes { TypeName = "Видео" },
                    new MediaTypes { TypeName = "Аудио" },
                    new MediaTypes { TypeName = "Документ" }
                };

                await _database.InsertAllAsync(defaultTypes);
            }

        }

        public async Task<List<News>> GetAllNewsAsync()
        {
            return await _database.Table<News>()
                .OrderByDescending(n => n.PublishDate)
                .ToListAsync();
        }

        public async Task<List<News>> GetNewsByAuthorAsync(int authorId)
        {
            return await _database.Table<News>()
                .Where(n => n.AuthorID == authorId)
                .OrderByDescending(n => n.PublishDate)
                .ToListAsync();
        }

        public async Task<News> GetNewsByIdAsync(int id)
        {
            return await _database.Table<News>()
                .Where(n => n.ID_news == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> AddNewsAsync(News news)
        {
            news.CreatedAt = DateTime.Now;
            if (news.PublishDate == default)
                news.PublishDate = DateTime.Now;

            return await _database.InsertAsync(news);
        }

        public async Task<int> UpdateNewsAsync(News news)
        {
            return await _database.UpdateAsync(news);
        }

        public async Task<int> DeleteNewsAsync(int id)
        {
            var news = await GetNewsByIdAsync(id);
            if (news != null)
            {
                // Удаляем связанные медиа
                await _database.Table<NewsMedia>()
                    .Where(m => m.ID_news == id)
                    .DeleteAsync();

                return await _database.DeleteAsync(news);
            }
            return 0;
        }

        public async Task<bool> NewsExistsAsync(int id)
        {
            var news = await GetNewsByIdAsync(id);
            return news != null;
        }
    }
}

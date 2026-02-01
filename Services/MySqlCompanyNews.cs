using System.Diagnostics;
using AMK.Config;
using AMK.Models.Headlines;
using MySqlConnector;

namespace AMK.Services;

/// <summary>
/// Новости компании + медиа поверх MySQL (Timeweb Cloud).
/// Работает с таблицами/полями как в вашей БД:
/// - News(ID_news, Title, ShortDescription, Content, PublishDate, CreatedAt, AuthorId)
/// - NewsMedia(ID_newsMedia, ID_news, IconUrl, MediaUrl, Caption, ID_medType)
/// </summary>
public sealed class MySqlCompanyNews : ICompanyNews
{
    private readonly string _cs;

    public MySqlCompanyNews()
    {
        _cs = DbConfig.MySqlConnectionString;
    }

    private async Task<MySqlConnection> OpenAsync()
    {
        var conn = new MySqlConnection(_cs);
        await conn.OpenAsync();
        return conn;
    }

    public async Task<List<News>> GetAllNewsAsync()
    {
        var list = new List<News>();
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
SELECT
  ID_news,
  Title,
  Content,
  ShortDescription,
  AuthorId AS AuthorID,
  PublishDate,
  CreatedAt
FROM `News`
ORDER BY PublishDate DESC, ID_news DESC;";

            await using var cmd = new MySqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(MapNews(r));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllNews error: {ex.Message}");
        }
        return list;
    }

    public async Task<List<News>> GetNewsByAuthorAsync(int authorId)
    {
        var list = new List<News>();
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
SELECT
  ID_news,
  Title,
  Content,
  ShortDescription,
  AuthorId AS AuthorID,
  PublishDate,
  CreatedAt
FROM `News`
WHERE AuthorId=@a
ORDER BY PublishDate DESC, ID_news DESC;";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@a", authorId);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(MapNews(r));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetNewsByAuthor error: {ex.Message}");
        }
        return list;
    }

    public async Task<News> GetNewsByIdAsync(int id)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
SELECT
  ID_news,
  Title,
  Content,
  ShortDescription,
  AuthorId AS AuthorID,
  PublishDate,
  CreatedAt
FROM `News`
WHERE ID_news=@id
LIMIT 1;";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;
            return MapNews(r);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetNewsById error: {ex.Message}");
            return null;
        }
    }

    public async Task<int> AddNewsAsync(News news)
    {
        try
        {
            await using var conn = await OpenAsync();

            news.CreatedAt = DateTime.Now;
            if (news.PublishDate == default) news.PublishDate = DateTime.Now;

            const string sql = @"
INSERT INTO `News` (Title, Content, ShortDescription, AuthorId, PublishDate, CreatedAt)
VALUES (@t,@c,@s,@a,@p,@cr);
SELECT LAST_INSERT_ID();";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@t", news.Title);
            cmd.Parameters.AddWithValue("@c", news.Content);
            cmd.Parameters.AddWithValue("@s", news.ShortDescription);
            cmd.Parameters.AddWithValue("@a", news.AuthorID);
            cmd.Parameters.AddWithValue("@p", news.PublishDate);
            cmd.Parameters.AddWithValue("@cr", news.CreatedAt);

            news.ID_news = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return news.ID_news;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AddNews error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> UpdateNewsAsync(News news)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
UPDATE `News` SET
  Title=@t,
  Content=@c,
  ShortDescription=@s,
  AuthorId=@a,
  PublishDate=@p
WHERE ID_news=@id;";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@t", news.Title);
            cmd.Parameters.AddWithValue("@c", news.Content);
            cmd.Parameters.AddWithValue("@s", news.ShortDescription);
            cmd.Parameters.AddWithValue("@a", news.AuthorID);
            cmd.Parameters.AddWithValue("@p", news.PublishDate == default ? DateTime.Now : news.PublishDate);
            cmd.Parameters.AddWithValue("@id", news.ID_news);
            return await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UpdateNews error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteNewsAsync(int id)
    {
        try
        {
            await using var conn = await OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            // На всякий случай удаляем медиа явно
            await using (var cmd1 = new MySqlCommand("DELETE FROM `NewsMedia` WHERE ID_news=@id;", conn, tx))
            {
                cmd1.Parameters.AddWithValue("@id", id);
                await cmd1.ExecuteNonQueryAsync();
            }

            await using (var cmd2 = new MySqlCommand("DELETE FROM `News` WHERE ID_news=@id;", conn, tx))
            {
                cmd2.Parameters.AddWithValue("@id", id);
                var affected = await cmd2.ExecuteNonQueryAsync();
                await tx.CommitAsync();
                return affected;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DeleteNews error: {ex.Message}");
            return 0;
        }
    }

    public async Task<bool> NewsExistsAsync(int id)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "SELECT COUNT(*) FROM `News` WHERE ID_news=@id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }
        catch
        {
            return false;
        }
    }

    // Media

    public async Task<List<NewsMedia>> GetMediaForNewsAsync(int newsId)
    {
        var list = new List<NewsMedia>();
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
SELECT
  ID_newsMedia,
  ID_news,
  MediaUrl,
  IconUrl,
  ID_medType,
  Caption
FROM `NewsMedia`
WHERE ID_news=@id
ORDER BY ID_newsMedia;";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", newsId);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(MapMedia(r));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetMediaForNews error: {ex.Message}");
        }
        return list;
    }

    public async Task<int> AddMediaAsync(NewsMedia media)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
INSERT INTO `NewsMedia` (ID_news, IconUrl, MediaUrl, Caption, ID_medType)
VALUES (@n,@i,@u,@c,@t);
SELECT LAST_INSERT_ID();";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@n", media.ID_news);
            cmd.Parameters.AddWithValue("@i", media.IconUrl);
            cmd.Parameters.AddWithValue("@u", media.MediaUrl);
            cmd.Parameters.AddWithValue("@c", (object?)media.Caption ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@t", media.ID_medType);

            media.ID_newsMedia = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return media.ID_newsMedia;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AddMedia error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteMediaAsync(int mediaId)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "DELETE FROM `NewsMedia` WHERE ID_newsMedia=@id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", mediaId);
            return await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> DeleteMediaByNewsAsync(int newsId)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "DELETE FROM `NewsMedia` WHERE ID_news=@id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", newsId);
            return await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            return 0;
        }
    }

    private static News MapNews(MySqlDataReader r)
    {
        return new News
        {
            ID_news = r.GetInt32("ID_news"),
            Title = r.GetString("Title"),
            Content = r.GetString("Content"),
            ShortDescription = r.GetString("ShortDescription"),
            AuthorID = r.GetInt32("AuthorID"),
            PublishDate = r.GetDateTime("PublishDate"),
            CreatedAt = r.GetDateTime("CreatedAt"),
        };
    }

    private static NewsMedia MapMedia(MySqlDataReader r)
    {
        return new NewsMedia
        {
            ID_newsMedia = r.GetInt32("ID_newsMedia"),
            ID_news = r.GetInt32("ID_news"),
            MediaUrl = r.GetString("MediaUrl"),
            IconUrl = r.GetString("IconUrl"),
            ID_medType = r.GetInt32("ID_medType"),
            Caption = r.IsDBNull(r.GetOrdinal("Caption")) ? null : r.GetString("Caption"),
        };
    }
}

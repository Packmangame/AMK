using AMK.Models.Headlines;
using AMK.Models.Rss;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace AMK.Services
{
    public class RssNewsServices : INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly List<RssFeed> _availableFeeds;

        public RssNewsServices()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };

            // Some RSS/CDN endpoints refuse requests without a User-Agent
            try
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AMKApp/1.0 (+https://amk.local)");
                _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml, application/xml, text/xml, */*");
            }
            catch
            {
                // ignore header parsing errors
            }

            //Рабочие фиды
            _availableFeeds = new List<RssFeed>
            {
                new RssFeed
                {
                    Name = "Колёса",
                    Url = "https://www.kolesa.ru/export/rss.xml",
                },
                new RssFeed
                {
                    Name = "Mail Авто",
                    Url = "https://auto.mail.ru/rss/",
                },
                new RssFeed
                {
                    Name = "Motor",
                    Url = "https://motor.ru/exports/rss.xml",
                }
            };
        }
        public async Task<List<RssNewsItems>> LoadNewsFromFeedAsync(string feedUrl, string sourceName)
        {
            var newsItems = new List<RssNewsItems>();

            try
            {
                using var response = await _httpClient.GetAsync(feedUrl);
                if (!response.IsSuccessStatusCode)
                    return newsItems;

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var xmlReader = XmlReader.Create(stream);

                var feed = SyndicationFeed.Load(xmlReader);

                // Берём больше элементов, чтобы работала пагинация на уровне приложения
                foreach (var item in feed.Items.Take(60))
                {
                    var rawImageUrl = ExtractImageUrl(item);
                    var articleUrl = item.Links.FirstOrDefault()?.Uri?.ToString();

                    var newsItem = new RssNewsItems
                    {
                        Title = item.Title?.Text ?? "Без заголовка",
                        Description = ExtractDescription(item),
                        ImageUrl = NormalizeImageUrl(rawImageUrl, feedUrl, articleUrl),
                        PublishDate = item.PublishDate.DateTime,
                        Link = articleUrl ?? string.Empty,
                        Source = sourceName,
                        IsRead = false
                    };

                    // Очистка HTML из описания
                    newsItem.Description = CleanHtml(newsItem.Description);

                    //Укорачиваем описание
                    if (newsItem.Description.Length > 150)
                    {
                        newsItem.Description = newsItem.Description.Substring(0, 150) + "...";
                    }

                    newsItems.Add(newsItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки фида {sourceName}: {ex.Message}");
            }

            return newsItems;
        }

        public async Task<List<RssNewsItems>> LoadAllNewsAsync(int skip, int take)
        {
            var allNews = new List<RssNewsItems>();
            var tasks = new List<Task<List<RssNewsItems>>>();

            foreach (var feed in _availableFeeds.Where(f => f.IsEnabled))
            {
                tasks.Add(LoadNewsFromFeedAsync(feed.Url, feed.Name));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var newsList in results)
            {
                allNews.AddRange(newsList);
            }

            return allNews
                .OrderByDescending(n => n.PublishDate)
                .Skip(Math.Max(0, skip))
                .Take(Math.Max(0, take))
                .ToList();
        }

        private string ExtractDescription(SyndicationItem item)
        {
            // Пробуем разные варианты получения описания
            if (!string.IsNullOrEmpty(item.Summary?.Text))
                return item.Summary.Text;

            if (item.Content is TextSyndicationContent textContent)
                return textContent.Text;

            // Пробуем извлечь из расширенных элементов (content:encoded, description, etc.)
            foreach (var extension in item.ElementExtensions)
            {
                if (extension.OuterName is not ("description" or "encoded" or "content"))
                    continue;

                try
                {
                    var el = extension.GetObject<XElement>();
                    var v = el?.Value;
                    if (!string.IsNullOrWhiteSpace(v))
                        return v;
                }
                catch
                {
                    // fall back
                }

                try
                {
                    var s = extension.GetObject<string>();
                    if (!string.IsNullOrWhiteSpace(s))
                        return s;
                }
                catch
                {
                    // ignore
                }
            }

            return string.Empty;
        }

        private string ExtractImageUrl(SyndicationItem item)
        {
            try
            {
                // 1) enclosure links (often contain image URLs)
                foreach (var link in item.Links)
                {
                    if (!string.Equals(link.RelationshipType, "enclosure", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var uri = link.Uri?.ToString();
                    if (string.IsNullOrWhiteSpace(uri))
                        continue;

                    // If MediaType not set, still accept common image extensions
                    if (!string.IsNullOrWhiteSpace(link.MediaType) && !link.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        continue;

                    return uri;
                }

                // 2) Media RSS extensions: media:thumbnail, media:content, enclosure, etc.
                foreach (var extension in item.ElementExtensions)
                {
                    if (extension.OuterName is not ("thumbnail" or "content" or "enclosure" or "image"))
                        continue;

                    var url = TryReadUrlFromExtension(extension);
                    if (!string.IsNullOrWhiteSpace(url))
                        return url;
                }

                //HTML описания
                var description = ExtractDescription(item);
                var match = Regex.Match(description, @"<img[^>]+src\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);

                if (match.Success)
                    return match.Groups[1].Value;

                return "https://avatars.mds.yandex.net/i?id=4557b5d4094c2fc4f1ddd2ed249a764f60b1c8f3-16326063-images-thumbs&n=13";
            }
            catch
            {
                return "https://avatars.mds.yandex.net/i?id=4557b5d4094c2fc4f1ddd2ed249a764f60b1c8f3-16326063-images-thumbs&n=13";
            }
        }

        private static string TryReadUrlFromExtension(SyndicationElementExtension extension)
        {
            try
            {
                var el = extension.GetObject<XElement>();
                if (el == null)
                    return null;

                // Most common attributes for media urls
                var url = el.Attribute("url")?.Value
                          ?? el.Attribute("href")?.Value
                          ?? el.Attribute(XName.Get("url"))?.Value
                          ?? el.Attribute(XName.Get("href"))?.Value;

                if (!string.IsNullOrWhiteSpace(url))
                    return url;
            }
            catch
            {
                // ignore
            }

            return null;
        }

        private string NormalizeImageUrl(string url, string? baseUrl, string? articleUrl)
        {
            const string placeholder = "https://via.placeholder.com/150/CCCCCC/808080?text=Нет+фото";

            if (string.IsNullOrWhiteSpace(url))
                return placeholder;

            url = WebUtility.HtmlDecode(url).Trim().Trim('"', '\'');

            // //example.com/img.jpg
            if (url.StartsWith("//"))
                url = "https:" + url;

            // absolute
            if (Uri.TryCreate(url, UriKind.Absolute, out var abs))
                return abs.ToString();

            // relative to feed
            if (!string.IsNullOrWhiteSpace(baseUrl) && Uri.TryCreate(baseUrl, UriKind.Absolute, out var feedUri) && Uri.TryCreate(feedUri, url, out var rel1))
                return rel1.ToString();

            // relative to article
            if (!string.IsNullOrWhiteSpace(articleUrl) && Uri.TryCreate(articleUrl, UriKind.Absolute, out var artUri) && Uri.TryCreate(artUri, url, out var rel2))
                return rel2.ToString();

            return placeholder;
        }

        private string CleanHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            //Удаление HTML тегов
            var withoutTags = Regex.Replace(html, @"<[^>]*>", string.Empty);

            //Замена HTML-сущности
            var decoded = WebUtility.HtmlDecode(withoutTags);

            //Удаление лишних пробелов
            return Regex.Replace(decoded, @"\s+", " ").Trim();
        }
        public List<RssFeed> GetAvailableFeeds() => _availableFeeds;
    }
}

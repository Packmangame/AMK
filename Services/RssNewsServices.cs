using AMK.Models.Headlines;
using AMK.Models.Rss;
using System.ServiceModel.Syndication;
using System.Xml;

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

                foreach (var item in feed.Items.Take(15))
                {
                    var newsItem = new RssNewsItems
                    {
                        Title = item.Title?.Text ?? "Без заголовка",
                        Description = ExtractDescription(item),
                        ImageUrl = ExtractImageUrl(item),
                        PublishDate = item.PublishDate.DateTime,
                        Link = item.Links.FirstOrDefault()?.Uri?.ToString() ?? string.Empty,
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

        public async Task<List<RssNewsItems>> LoadAllNewsAsync()
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

            return allNews.OrderByDescending(n => n.PublishDate)
                .Take(15)
                .ToList();
        }

         string ExtractDescription(SyndicationItem item)
        {
            // Пробуем разные варианты получения описания
            if (!string.IsNullOrEmpty(item.Summary?.Text))
                return item.Summary.Text;

            if (item.Content is TextSyndicationContent textContent)
                return textContent.Text;

            // Пробуем извлечь из расширенных элементов
            var elementExtensions = item.ElementExtensions;
            foreach (var extension in elementExtensions)
            {
                if (extension.OuterName == "description" || extension.OuterName == "encoded")
                {
                    var content = extension.GetObject<string>();
                    if (!string.IsNullOrEmpty(content))
                        return content;
                }
            }

            return string.Empty;
        }

        string ExtractImageUrl(SyndicationItem item)
        {
            try
            {
                //(Media RSS)
                var mediaExtensions = item.ElementExtensions
                    .Where(x => x.OuterName == "thumbnail" || x.OuterName == "content")
                    .ToList();

                foreach (var extension in mediaExtensions)
                {
                    var element = extension.GetObject<System.Xml.Linq.XElement>();
                    var urlAttr = element?.Attribute("url");
                    if (urlAttr != null && !string.IsNullOrEmpty(urlAttr.Value))
                    {
                        if (urlAttr.Value.StartsWith("http"))
                            return urlAttr.Value;
                    }
                }

                //HTML описания
                var description = ExtractDescription(item);
                var match = System.Text.RegularExpressions.Regex.Match(description,@"<img[^>]+src=[""']([^""']+)[""']",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (match.Success)
                    return match.Groups[1].Value;

                return "https://via.placeholder.com/150/CCCCCC/808080?text=Нет+фото";
            }
            catch
            {
                return "https://via.placeholder.com/150/CCCCCC/808080?text=Нет+фото";
            }
        }

        string CleanHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            //Удаление HTML тегов
            var withoutTags = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]*>", string.Empty);

            //Замена HTML-сущности
            var decoded = System.Net.WebUtility.HtmlDecode(withoutTags);

            //Удаление лишних пробелов
            return System.Text.RegularExpressions.Regex.Replace(decoded, @"\s+", " ").Trim();
        }
        public List<RssFeed> GetAvailableFeeds() => _availableFeeds;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Models.Headlines
{
    public class NewsListItem : ObservableObject
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime PublishDate { get; set; }
        public int AuthorId { get; set; }
        public Color ReadColor { get; set; }
    }
}

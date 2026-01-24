using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Models.Headlines
{
    public class RssNewsItems : INotifyPropertyChanged
    {
        private string _title;
        private string _description;
        private string _imageUrl;
        private DateTime _publishDate;
        private string _link;
        private string _source;
        private bool _isRead;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set
            {
                _imageUrl = value;
                OnPropertyChanged(nameof(ImageUrl));
            }
        }

        public DateTime PublishDate
        {
            get => _publishDate;
            set
            {
                _publishDate = value;
                OnPropertyChanged(nameof(PublishDate));
                OnPropertyChanged(nameof(FormattedDate));
            }
        }

        public string FormattedDate => PublishDate.ToString("dd.MM.yyyy HH:mm");

        public string Link
        {
            get => _link;
            set
            {
                _link = value;
                OnPropertyChanged(nameof(Link));
            }
        }

        public string Source
        {
            get => _source;
            set
            {
                _source = value;
                OnPropertyChanged(nameof(Source));
            }
        }

        public bool IsRead
        {
            get => _isRead;
            set
            {
                _isRead = value;
                OnPropertyChanged(nameof(IsRead));
                OnPropertyChanged(nameof(ReadColor));
            }
        }
        public Color ReadColor => IsRead ? Colors.Gray : Colors.Black;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using System.Globalization;
using AMK.Resources.Strings;


namespace AMK.Services
{
    public class LocalizationService
    {
        private static LocalizationService _instance;
        public static LocalizationService Instance => _instance ??= new LocalizationService();

        // Текущая культура
        private CultureInfo _currentCulture;

        // Событие для уведомления об изменении языка
        public event EventHandler LanguageChanged;

        private LocalizationService()
        {
            // Инициализация с сохраненным языком или языком системы
            InitializeCulture();
        }

        private void InitializeCulture()
        {
            // Пробуем загрузить сохраненный язык
            var savedLang = Preferences.Get("AppLanguage", null);

            if (!string.IsNullOrEmpty(savedLang))
            {
                _currentCulture = new CultureInfo(savedLang);
            }
            else
            {
                // Используем язык системы
                _currentCulture = CultureInfo.CurrentUICulture;

                // Если язык системы не русский и не английский, используем английский по умолчанию
                if (_currentCulture.Name != "ru-RU" && _currentCulture.Name != "en-US")
                {
                    _currentCulture = new CultureInfo("en-US");
                }
            }

            ApplyCulture();
        }

        // Получить текущий язык
        public string GetCurrentLanguage()
        {
            return _currentCulture.Name;
        }

        // Получить отображаемое имя языка
        public string GetLanguageDisplayName()
        {
            return _currentCulture.Name == "ru-RU" ? "Русский" : "English";
        }

        // Переключить язык
        public void ToggleLanguage()
        {
            _currentCulture = _currentCulture.Name == "ru-RU"
                ? new CultureInfo("en-US")
                : new CultureInfo("ru-RU");

            ApplyCulture();
            SaveLanguage();
            OnLanguageChanged();
        }

        // Установить конкретный язык
        public void SetLanguage(string languageCode)
        {
            _currentCulture = new CultureInfo(languageCode);
            ApplyCulture();
            SaveLanguage();
            OnLanguageChanged();
        }

        // Применить культуру
        private void ApplyCulture()
        {
            // Устанавливаем культуру для текущего потока
            Thread.CurrentThread.CurrentCulture = _currentCulture;
            Thread.CurrentThread.CurrentUICulture = _currentCulture;

            // Устанавливаем культуру для ресурсов
            Strings.Culture = _currentCulture;

            // Обновляем ресурсы в словаре приложения
            UpdateApplicationResources();
        }

        // Обновить ресурсы в словаре приложения
        private void UpdateApplicationResources()
        {
            if (Application.Current == null) return;

            var resources = Application.Current.Resources;

            // Обновляем все строки
            UpdateResource(resources, "_login", GetString("_login"));
            UpdateResource(resources, "_password", GetString("_password"));
            UpdateResource(resources, "_enter", GetString("_enter"));
            UpdateResource(resources, "_language", GetString("_language"));
            UpdateResource(resources, "_rus", GetString("_rus"));
            UpdateResource(resources, "_eng", GetString("_eng"));
            UpdateResource(resources, "_welcome", GetString("_welcome"));
            UpdateResource(resources, "_profile", GetString("_profile"));
            UpdateResource(resources, "_calendar", GetString("_calendar"));
            UpdateResource(resources, "_news", GetString("_news"));
            UpdateResource(resources, "_lerning", GetString("_lerning"));
            UpdateResource(resources, "_amkNews", GetString("_amkNews"));
            UpdateResource(resources, "_settings", GetString("_settings"));
            UpdateResource(resources, "_amk", GetString("_amk"));
        }

        private void UpdateResource(ResourceDictionary resources, string key, string value)
        {
            if (resources.ContainsKey(key))
            {
                resources[key] = value;
            }
            else
            {
                resources.Add(key, value);
            }
        }

        // Получить локализованную строку
        public string GetString(string key)
        {
            return key switch
            {
                "_login" => Strings._login,
                "_password" => Strings._password,
                "_enter" => Strings._enter,
                "_language" => Strings._language,
                "_rus" => Strings._rus,
                "_eng" => Strings._eng,
                "_welcome" => Strings._welcome,
                "_profile" => Strings._profile,
                "_calendar" => Strings._calendar,
                "_news" => Strings._news,
                "_lerning" => Strings._lerning,
                "_amkNews" => Strings._amkNews,
                "_settings" => Strings._settings,
                "_amk" => Strings._amk,
                _ => key
            };
        }

        // Сохранить выбор языка
        private void SaveLanguage()
        {
            Preferences.Set("AppLanguage", _currentCulture.Name);
        }

        protected virtual void OnLanguageChanged()
        {
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

    }
}

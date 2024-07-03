using System.Windows;

namespace TAMHubApp.Infrastructures.Helpers
{
    public static class LocalizationHelper
    {
        public static string TranslateString(this string key)
        {
            var res = (string)Application.Current.TryFindResource(key);
            return !string.IsNullOrEmpty(res) ? res : key;
        }
        public static void SetLanguage(string languageCode)
        {
            try
            {
                Application.Current.Resources.MergedDictionaries.Where(x => x.Source.OriginalString.Contains("LocalizationResources")).ToList().ForEach(x =>
                {
                    Application.Current.Resources.MergedDictionaries.Remove(x);
                });

                ResourceDictionary newDic = new ResourceDictionary();
                newDic.Source = new Uri($"/Resources/Localizations/LocalizationResources.{languageCode}.xaml", UriKind.RelativeOrAbsolute);
                Application.Current.Resources.MergedDictionaries.Add(newDic);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error loading language resources: {ex.Message}");
            }
            var fggh = "dw".TranslateString();
        }
    }
}

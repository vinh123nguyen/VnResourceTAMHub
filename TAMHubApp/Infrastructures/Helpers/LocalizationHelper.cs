using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TAMHubApp.Properties;
using TAMHubApp.ViewModels;

namespace TAMHubApp.Infrastructures.Helpers
{
    public static class LocalizationHelper
    {
        public static string TranslateString(this string key)
        {
            var res = (string)Application.Current.TryFindResource(key);
            return !string.IsNullOrEmpty(res) ? res : key;
        }
        public static void SetLanguage()
        {
            try
            {
                var languageCode = Properties.Settings.Default.Language;
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
        }
        public static void SetLanguage(string languageCode)
        {
            try
            {
                if (Properties.Settings.Default.Language == languageCode)
                    return;
                Application.Current.Resources.MergedDictionaries.Where(x => x.Source.OriginalString.Contains("LocalizationResources")).ToList().ForEach(x =>
                {
                    Application.Current.Resources.MergedDictionaries.Remove(x);
                });

                ResourceDictionary newDic = new ResourceDictionary();
                newDic.Source = new Uri($"/Resources/Localizations/LocalizationResources.{languageCode}.xaml", UriKind.RelativeOrAbsolute);
                Application.Current.Resources.MergedDictionaries.Add(newDic);
                Properties.Settings.Default.Language = languageCode;
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
                var homeviewModel = ServiceLocator.Instance.GetRequiredService<HomeViewModel>();
                homeviewModel.ChangeImageWelcome();
                homeviewModel.onChangeLang();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language resources: {ex.Message}");
            }
        }
    }
}

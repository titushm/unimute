using System;
using System.Linq;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml;
using Microsoft.Gaming.XboxGameBar;
using UnhandledExceptionEventArgs = System.UnhandledExceptionEventArgs;
using Windows.Storage;
using Windows.Foundation.Collections;
using Microsoft.Gaming.XboxGameBar.Input;
using System.Collections.Generic;
using Windows.System;

namespace Unimute
{
	public static class AppHelper
	{
		public static readonly Version Version = new Version(1, 1, 0);
		public static XboxGameBarWidget MainWidget;
		public static XboxGameBarWidget SettingsWidget;

		public static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			string errorMessage = $"Unhandled Exception: {e.ExceptionObject}";
			DisplayToast("Error", errorMessage);
		}

		private static void DisplayToast(string message, string title)
		{
			string xmlToastTemplate = $@"
				<toast>
					<visual>
						<binding template='ToastGeneric'>
							<text>{title}</text>
							<text>{message}</text>
						</binding>
					</visual>
				</toast>";

			XmlDocument document = new XmlDocument();
			document.LoadXml(xmlToastTemplate);
			ToastNotification toast = new ToastNotification(document);
			ToastNotificationManager.CreateToastNotifier().Show(toast);
		}

		public static void UpdateTheme()
		{
			bool isDarkMode = MainWidget.RequestedTheme == ElementTheme.Dark;
			string themeFile = isDarkMode ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
			Uri themeUri = new Uri($"ms-appx:///{themeFile}");

			var currentThemeUri = Application.Current.Resources.MergedDictionaries
				.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("DarkTheme.xaml") || d.Source.OriginalString.Contains("LightTheme.xaml")))?.Source;

			if (currentThemeUri != null && currentThemeUri.OriginalString.Equals(themeUri.OriginalString, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			var existingTheme = Application.Current.Resources.MergedDictionaries
				.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("DarkTheme.xaml") || d.Source.OriginalString.Contains("LightTheme.xaml")));

			if (existingTheme != null)
			{
				Application.Current.Resources.MergedDictionaries.Remove(existingTheme);
			}

			var newTheme = new ResourceDictionary { Source = themeUri };
			Application.Current.Resources.MergedDictionaries.Add(newTheme);

			if (Window.Current?.Content is FrameworkElement rootElement)
			{
				rootElement.RequestedTheme = isDarkMode ? ElementTheme.Dark : ElementTheme.Light;
			}
		}

		public static void ResizeWidget(double width, double height, XboxGameBarWidget widget) {
			widget.TryResizeWindowAsync(new Windows.Foundation.Size(width, height));
		}
		public static T GetSetting<T>(string key, Func<T> defaultValueFactory = null) {
			if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out object value)) {
				try {
					if (typeof(T) == typeof(int[])) {
						string str = value as string;
						object result = string.IsNullOrEmpty(str) ? Array.Empty<int>() : str.Split(',').Select(int.Parse).ToArray();
						return (T)result;
					}

					if (value is T typedValue) {
						return typedValue;
					}

					return (T)Convert.ChangeType(value, typeof(T));
				} catch {
					return defaultValueFactory != null ? defaultValueFactory() : default;
				}
			}
			return defaultValueFactory != null ? defaultValueFactory() : default;
		}

		public static void SetSetting<T>(string key, T value) {
			ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

			if (value is int[] array) {
				localSettings.Values[key] = string.Join(",", array);
				return;
			}
			localSettings.Values[key] = value;
		}
	}
}
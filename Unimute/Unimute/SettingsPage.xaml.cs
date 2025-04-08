using Microsoft.Gaming.XboxGameBar;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Unimute {

	public sealed partial class SettingsPage {
		public SettingsPage() {
			InitializeComponent();
		}

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			AppHelper.ResizeWidget(Width, Height, AppHelper.SettingsWidget);
			AppHelper.MainWidget.RequestedThemeChanged += OnThemeChanged;
			Dispatcher.RunAsync(CoreDispatcherPriority.Normal, AppHelper.UpdateTheme);
			PopulateConfig();
		}

		private void PopulateConfig() {
			bool inputMode = AppHelper.GetSetting("InputMode", () => false);
			InputToggleSwitch.IsOn = inputMode;
			VersionTextBox.Text = AppHelper.Version.ToString();
		}

		private void InputToggleSwitch_Toggled(object sender, RoutedEventArgs e) {
			AppHelper.SetSetting("InputMode", InputToggleSwitch.IsOn);
		}

		private void LinkButton_Click(object sender, RoutedEventArgs e) {
			if (sender is Button button) AppHelper.MainWidget.LaunchUriAsync(new Uri(button.Tag.ToString()));
		}

		private async void OnThemeChanged(XboxGameBarWidget sender, object args) {
			try {
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, AppHelper.UpdateTheme);
			} catch {
				AppHelper.MainWidget.RequestedThemeChanged -= OnThemeChanged;
			}
		}
	}
}
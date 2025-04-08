using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Gaming.XboxGameBar;
using Windows.UI.Xaml.Input;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using System;

namespace Unimute {

	public sealed partial class MainPage {
		public MainPage() {
			InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			AppHelper.MainWidget.RequestedOpacityChanged += Widget_RequestedOpacityChanged;
		}
		private async void Widget_RequestedOpacityChanged(XboxGameBarWidget sender, object args)
		{
			await MuteButton.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				Opacity = AppHelper.MainWidget.RequestedOpacity;
			});
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e) {
			AppHelper.ResizeWidget(Width, Height, AppHelper.MainWidget);
			await MuteButton.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				Opacity = AppHelper.MainWidget.RequestedOpacity;
			});
			bool volumeBarHidden = AppHelper.GetSetting("HideVolumeBar", () => false);
			if (volumeBarHidden) AppHelper.ResizeWidget(Height, Height, AppHelper.MainWidget);
			AppHelper.MainWidget.RequestedThemeChanged += OnThemeChanged;
			bool isMuted = AppHelper.GetSetting("IsMuted", () => false);
			bool isDefault = !AppHelper.GetSetting("InputMode", () => false);
			SetMuteButtonState(isMuted);
			if (!await CheckPermissions()) return;
			await MicControl.SetMute(isDefault, isMuted);

			double volume = AppHelper.GetSetting("Volume", () => 100);
			VolumeSlider.Value = volume;
		}

		private async Task<bool> CheckPermissions() {
			bool hasPermissions = await MicControl.Initialize();
			if (!hasPermissions) {
				SetupGrantPage();
			}
			return hasPermissions;
		}
		
		private void SetMuteButtonState(bool isMuted) {
			AppHelper.SetSetting("IsMuted", isMuted);
			MuteButtonIcon.Template = (ControlTemplate)Application.Current.Resources[isMuted ? "MuteIcon" : "UnmuteIcon"];
			ToolTipService.SetToolTip(MuteButton, isMuted ? "Click to Unmute" : "Click to Mute");
		}
		
		private async void MuteButton_Click(object sender, RoutedEventArgs e) {
			bool isMuted = AppHelper.GetSetting("IsMuted", () => false);
			isMuted = !isMuted;
			SetMuteButtonState(isMuted);
			if (!await CheckPermissions()) return;
			bool isDefault = !AppHelper.GetSetting("InputMode", () => false);
			MicControl.SetMute(isDefault, isMuted);
		}

		private void SetupGrantPage() {
			PermissionsPageSetup setup = new PermissionsPageSetup {
				State = PermissionState.Grant,
				MicButtonVisibility = Visibility.Visible,
				MicIconTemplate = (ControlTemplate)Application.Current.Resources["MuteIcon"],
				ActionButtonStyle = (Style)Application.Current.Resources["Widget:Button:Invert"],
				ActionButtonText = "Grant",
				Title = "Microphone Access",
				Description = "Permission required"
			};
			Frame?.Navigate(typeof(PermissionsPage), setup);
		}
		
		private void OnThemeChanged(XboxGameBarWidget sender, object args) {
			Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, AppHelper.UpdateTheme);
		}

		private async void VolumeSlider_PointerCaptureLost(object sender, PointerRoutedEventArgs e) {
			double previousValue = AppHelper.GetSetting<double>("Volume");
			if (previousValue == VolumeSlider.Value || !await CheckPermissions()) return;
			AppHelper.SetSetting("Volume", VolumeSlider.Value);
			bool isDefault = !AppHelper.GetSetting("InputMode", () => false);
			MicControl.SetVolume(isDefault, VolumeSlider.Value);
		}
	}
}
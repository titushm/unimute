using Microsoft.Gaming.XboxGameBar;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Unimute {
	public enum PermissionState {
		Grant,
		Waiting,
		Success,
		Reload
	}
	
	public class PermissionsPageSetup {
		public PermissionState State { get; set; }
		public Visibility MicButtonVisibility { get; set; }
		public ControlTemplate MicIconTemplate { get; set; }
		public Style ActionButtonStyle { get; set; }
		public string ActionButtonText { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
	}

	public sealed partial class PermissionsPage {
		public PermissionsPage() {
			InitializeComponent();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			if (e.Parameter is PermissionsPageSetup setup) {
				MicButton.Visibility = setup.MicButtonVisibility;
				MuteIcon.Template = setup.MicIconTemplate;
				ActionButton.Style = setup.ActionButtonStyle;
				ActionButton.Content = setup.ActionButtonText;
				TitleTextBox.Text = setup.Title;
				ActionButton.Tag = setup.State;
				DescriptionTextBox.Text = setup.Description;

				switch (setup.State) {
					case PermissionState.Waiting:
						WaitingPageLoaded();
						break;
				}
			}
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e) {
			base.OnNavigatedFrom(e);

			PermissionState currentState = (PermissionState)ActionButton.Tag;
			switch (currentState) {
				case PermissionState.Waiting:
					WaitingPageUnloaded();
					break;
			}
		}

		private async void WaitingPageLoaded() {
			await AppHelper.MainWidget.LaunchUriAsync(new Uri("ms-settings:privacy-microphone"));
			AppHelper.MainWidget.GameBarDisplayModeChanged += OnGameBarDisplayModeChanged;
		}

		private void WaitingPageUnloaded() {
			AppHelper.MainWidget.GameBarDisplayModeChanged -= OnGameBarDisplayModeChanged;
		}

		private async void ActionButton_Click(object sender, RoutedEventArgs e) {
			PermissionsPageSetup setup = new PermissionsPageSetup();
			switch ((PermissionState)ActionButton.Tag) {
				case PermissionState.Grant:
					setup = SetupWaitingPage();
					break;
				case PermissionState.Success:
					setup = SetupSuccessPage();
					break;

				case PermissionState.Waiting:
					CheckMicPermissions();
					return;
				
				case PermissionState.Reload:
					bool success = await MicControl.Initialize();
					if (success) {
						Frame.Navigate(typeof(MainPage));
						return;
					}
					setup = SetupGrantPage();
					break;
			}

			Frame.Navigate(typeof(PermissionsPage), setup);
		}

		private async void CheckMicPermissions() {
			bool success = await MicControl.Initialize();
			PermissionsPageSetup setup;
			ActionButton.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
				setup = SetupSuccessPage();
				if (!success) setup = SetupWaitingPage();
				Frame.Navigate(typeof(PermissionsPage), setup);
			});
		}

		private void OnGameBarDisplayModeChanged(XboxGameBarWidget sender, object args) {
			if (sender.GameBarDisplayMode == XboxGameBarDisplayMode.Foreground) {
				CheckMicPermissions();
			}
		}

		private PermissionsPageSetup SetupWaitingPage() {
			PermissionsPageSetup setup = new PermissionsPageSetup {
				State = PermissionState.Waiting,
				MicButtonVisibility = Visibility.Collapsed,
				MicIconTemplate = (ControlTemplate)Application.Current.Resources["MuteIcon"],
				ActionButtonStyle = (Style)Application.Current.Resources["Widget:Button:Warning"],
				ActionButtonText = "Waiting",
				Title = "Microphone Access",
				Description = "Make sure Microphone access is enabled for Unimute"
			};
			return setup;
		}

		private PermissionsPageSetup SetupSuccessPage() {
			PermissionsPageSetup setup = new PermissionsPageSetup {
				State = PermissionState.Reload,
				MicButtonVisibility = Visibility.Visible,
				MicIconTemplate = (ControlTemplate)Application.Current.Resources["UnmuteIcon"],
				ActionButtonStyle = (Style)Application.Current.Resources["Widget:Button:Success"],
				ActionButtonText = "Reload",
				Title = "Microphone Access",
				Description = "Permission Granted"
			};
			return setup;
		}

		private PermissionsPageSetup SetupGrantPage() {
			PermissionsPageSetup setup = new PermissionsPageSetup {
				State = PermissionState.Grant,
				MicButtonVisibility = Visibility.Visible,
				MicIconTemplate = (ControlTemplate)Application.Current.Resources["MuteIcon"],
				ActionButtonStyle = (Style)Application.Current.Resources["Widget:Button:Invert"],
				ActionButtonText = "Grant",
				Title = "Microphone Access",
				Description = "Permission required"
			};
			return setup;
		}
	}
}

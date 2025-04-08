using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Gaming.XboxGameBar;

namespace Unimute {
	sealed partial class App {

		public App() {
			InitializeComponent();
			Suspending += OnSuspending;
			AppDomain.CurrentDomain.UnhandledException += AppHelper.OnUnhandledException;
		}
		protected override void OnActivated(IActivatedEventArgs args) {
			XboxGameBarWidgetActivatedEventArgs widgetArgs = null;
			if (args.Kind == ActivationKind.Protocol) {
				if (args is IProtocolActivatedEventArgs protocolArgs) {
					string scheme = protocolArgs.Uri.Scheme;
					if (scheme.Equals("ms-gamebarwidget")) {
						widgetArgs = args as XboxGameBarWidgetActivatedEventArgs;
					}
				}
			}
			if (widgetArgs != null) {
				if (widgetArgs.IsLaunchActivation) {
					var rootFrame = new Frame();
					Window.Current.Content = rootFrame;
					if (widgetArgs.AppExtensionId == "titushm-unimute") {
						AppHelper.MainWidget = new XboxGameBarWidget(
							widgetArgs,
							Window.Current.CoreWindow,
							rootFrame);
						rootFrame.Navigate(typeof(MainPage), AppHelper.MainWidget);
						AppHelper.MainWidget.SettingsClicked += OnSettingsClicked;

                    }
					else if (widgetArgs.AppExtensionId == "titushm-unimute-settings") {
						AppHelper.SettingsWidget = new XboxGameBarWidget(
							widgetArgs,
							Window.Current.CoreWindow,
							rootFrame);
						rootFrame.Navigate(typeof(SettingsPage));
					}
					else {
						return;
					}
					Window.Current.Activate();
				}
			}
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e) {
			Frame rootFrame = Window.Current.Content as Frame;
			if (rootFrame == null) {
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
					//TODO: Load state from previously suspended application
				}

				Window.Current.Content = rootFrame;
			}

			if (e.PrelaunchActivated == false) {
				if (rootFrame.Content == null) {
					rootFrame.Navigate(typeof(MainPage), e.Arguments);
				}
				Window.Current.Activate();
			}
		}
		
		void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		private void OnSuspending(object sender, SuspendingEventArgs e) {
			SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
			AppHelper.MainWidget = null;
			deferral.Complete();
		}

		private async void OnSettingsClicked(XboxGameBarWidget sender, object args) {
			await sender.ActivateSettingsAsync();
		}
	}
}


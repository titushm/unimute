using System;
using System.Collections.Generic;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using System.Threading.Tasks;

namespace Unimute {
	public static class MicControl {
		private static MediaCapture _capture;

		public static async Task<bool> Initialize(string id = null) {
			try {
				_capture = new MediaCapture();
				MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings
				{
					StreamingCaptureMode = StreamingCaptureMode.Audio
				};
				if (id != null) settings.AudioDeviceId = id;
				await _capture.InitializeAsync(settings);
			}
			catch {
				return false;
			}
			return true;
		}

		private static async Task<List<string>> GetDeviceIds(bool isDefault) {
			List<string> deviceIds = new List<string>();
			if (isDefault) {
				deviceIds.Add(null);
			}
			else {
				DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);
				if (devices == null) {
					return deviceIds;
				}
				foreach (DeviceInformation device in devices) {
					if (device.IsEnabled) {
						deviceIds.Add(device.Id);
					}
				}
			}

			return deviceIds;
		}

		public static async Task SetMute(bool isDefault, bool mute) {
			List<string> deviceIds = await GetDeviceIds(isDefault);
			foreach (string deviceId in deviceIds) {
				bool initialized = await Initialize(deviceId);
				if (initialized && _capture != null && _capture.AudioDeviceController != null) {
					_capture.AudioDeviceController.Muted = mute;
				}
			}
		}

		public static async Task SetVolume(bool isDefault, double volume) {
			List<string> deviceIds = await GetDeviceIds(isDefault);
			foreach (string deviceId in deviceIds) {
				bool initialized = await Initialize(deviceId);
				if (initialized && _capture != null && _capture.AudioDeviceController != null) {
					_capture.AudioDeviceController.VolumePercent = (float)volume;
				}
			}
		}
	}
}

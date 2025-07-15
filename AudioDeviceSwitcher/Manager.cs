using NAudio.CoreAudioApi;

namespace AudioDeviceSwitcher {
    public class Manager : IDisposable {
        private readonly MMDeviceEnumerator _DeviceEnumerator;
        private readonly IPolicyConfig _PolicyConfig;

        public Manager() {
            _DeviceEnumerator = new MMDeviceEnumerator();
            _PolicyConfig = GetPolicyConfig();
        }

        public DeviceCycleGroup ListDevices(DataFlow type = DataFlow.All) {
            List<Device> devices = [];
            foreach (var device in _DeviceEnumerator.EnumerateAudioEndPoints(type, DeviceState.Active | DeviceState.Disabled)) {
                try {
                    string name = device.FriendlyName;
                } catch {
                    continue;
                }

                devices.Add(new Device(device, this));
            }
            return new DeviceCycleGroup(devices);
        }

        public Device GetDeviceById(string id) {
            return new Device(_DeviceEnumerator.GetDevice(id), this);
        }

        public void DisableAll() {
            foreach (var device in ListDevices()) {
                device.Enabled = false;
            }
        }

        public void SetDeviceEnabled(string instanceId, bool enabled) {
            _PolicyConfig.SetEndpointVisibility(instanceId, enabled);
        }

        private static IPolicyConfig GetPolicyConfig() {
            return (IPolicyConfig)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")));
        }

        public void Dispose() {
            _DeviceEnumerator.Dispose();
        }
    }
}

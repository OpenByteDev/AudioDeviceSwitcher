using NAudio.CoreAudioApi;

namespace AudioDeviceSwitcher {
    public class Device : IDisposable {
        private readonly MMDevice _Underlying;
        private readonly Manager _Manager;

        internal Device(MMDevice underlying, Manager manager) {
            _Underlying = underlying;
            _Manager = manager;
        }

        public string Name => _Underlying.FriendlyName;
        public string Id => _Underlying.ID;

        public bool Enabled {
            get => _Underlying.State == DeviceState.Active;
            set {
                var targetState = value ? DeviceState.Active : DeviceState.Disabled;
                if (_Underlying.State != targetState) {
                    _Manager.SetDeviceEnabled(Id, value);
                }
            }
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
            _Underlying.Dispose();
        }
    }
}

using System.Collections;
using System.Diagnostics;

namespace AudioDeviceSwitcher {
    public class DeviceCycleGroup : IEnumerable<Device> {
        private readonly List<Device> _Devices;

        public DeviceCycleGroup(List<Device> devices) {
            Debug.Assert(devices.Count > 0);
            _Devices = devices;
        }

        private uint? _ActiveIndex;
        public uint ActiveIndex {
            get => _ActiveIndex ??= FindActiveIndex();
            set => SetActiveIndex(value);
        }

        private uint FindActiveIndex() {
            var index = _Devices.FindIndex(device => device.Enabled);
            return index == -1 ? 0u : (uint)index;
        }
        private void SetActiveIndex(uint index) {
            _ActiveIndex = index;
            for (var i = 0; i < _Devices.Count; i++) {
                var device = _Devices[i];
                device.Enabled = i == index;
            }
        }

        public void Advance() {
            ActiveIndex = (ActiveIndex + 1) % (uint)_Devices.Count;
        }

        public IEnumerator<Device> GetEnumerator() => _Devices.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _Devices.GetEnumerator();
    }
}

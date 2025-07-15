using AudioDeviceSwitcher;
using NAudio.CoreAudioApi;
using Newtonsoft.Json.Linq;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Plugin {
    public class Plugin {
        private const string UUID_BASE = "com.openbyte.audiodeviceswitcher";
        private const string CYLCE2_ACTION = UUID_BASE + ".cycle2";
        private const string CYLCE3_ACTION = UUID_BASE + ".cycle3";
        private const string TOGGLE_ACTION = UUID_BASE + ".toggle";
        private const string DISABLE_ALL_ACTION = UUID_BASE + ".disable-all";

        public readonly Dictionary<string, JObject> Settings = [];
        public readonly Dictionary<string, DeviceCycleGroup> CycleGroups = [];
        public readonly Dictionary<string, Device> Toggles = [];
        public readonly Dictionary<string, bool> DisableOthers = [];

        private readonly Manager _Manager = new();
        private readonly StreamDeckConnection _Connection;

        public Plugin(StreamDeckConnection connection) {
            _Connection = connection;

            connection.OnKeyUp += Connection_OnKeyUp;
            connection.OnWillAppear += Connection_OnWillAppear;
            connection.OnSendToPlugin += Connection_OnSendToPlugin;
            connection.OnDidReceiveSettings += Connection_OnDidReceiveSettings;
        }

        private void Connection_OnDidReceiveSettings(object? sender, StreamDeckEventReceivedEventArgs<DidReceiveSettingsEvent> args) {
            var settings = Settings[args.Event.Context] = args.Event.Payload.Settings;

            switch (args.Event.Action) {
                case TOGGLE_ACTION:
                    var deviceId = settings["device"]?.ToString();
                    if (deviceId is null) {
                        return;
                    }
                    lock (Toggles) {
                        Toggles[args.Event.Context] = _Manager.GetDeviceById(deviceId);
                    }
                    break;
                case CYLCE2_ACTION: {
                        var deviceIds = new[] {
                            settings["firstDevice"]?.ToString(),
                            settings["secondDevice"]?.ToString()
                        };
                        if (deviceIds.Any(id => id == null)) {
                            return;
                        }
                        var devices = deviceIds.Select(_Manager.GetDeviceById!).ToList();
                        lock (CycleGroups) {
                            CycleGroups[args.Event.Context] = new DeviceCycleGroup(devices);
                        }
                    }
                    break;
                case CYLCE3_ACTION: {
                        var deviceIds = new[] {
                            settings["firstDevice"]?.ToString(),
                            settings["secondDevice"]?.ToString(),
                            settings["thirdDevice"]?.ToString()
                        };
                        if (deviceIds.Any(id => id == null)) {
                            return;
                        }
                        var devices = deviceIds.Select(_Manager.GetDeviceById!).ToList();
                        lock (CycleGroups) {
                            CycleGroups[args.Event.Context] = new DeviceCycleGroup(devices);
                        }
                    }
                    break;
            }

            UpdateDeviceList(args.Event.Context);
        }

        private void Connection_OnSendToPlugin(object? sender, StreamDeckEventReceivedEventArgs<SendToPluginEvent> args) {
            var @event = args.Event.Payload["event"]!.ToString();
            switch (@event) {
                case "getDevices":
                    UpdateDeviceList(args.Event.Context);
                    break;
            }
        }

        private DataFlow ParseDeviceType(string? input, DataFlow defaultValue = DataFlow.All) {
            if (input is null) {
                return defaultValue;
            }
            if (Enum.TryParse<DataFlow>(input.ToString(), out var deviceType)) {
                return deviceType;
            }
            return defaultValue;
        }

        private void UpdateDeviceList(string context) {
            // TODO: cache or debounce this

            var items = new JArray();
            var payload = new JObject {
                ["event"] = "getDevices",
                ["items"] = items
            };

            var deviceType = ParseDeviceType(Settings[context]?["deviceType"]?.ToString());

            foreach (var device in _Manager.ListDevices(deviceType)) {
                items.Add(new JObject {
                    ["value"] = device.Id,
                    ["label"] = device.Name
                });
            }

            Task.Run(() => _Connection.SendToPropertyInspectorAsync("getDevices", payload, context));
        }

        private void Connection_OnWillAppear(object? sender, StreamDeckEventReceivedEventArgs<WillAppearEvent> args) {
            Task.Run(() => _Connection.GetSettingsAsync(args.Event.Context));

            switch (args.Event.Action) {
                case TOGGLE_ACTION:
                    lock (Toggles) {
                        if (Toggles.TryGetValue(args.Event.Context, out var device)) {
                            UpdateActionState(args.Event.Context, device);
                        }
                    }
                    break;
                case CYLCE2_ACTION:
                case CYLCE3_ACTION:
                    lock (CycleGroups) {
                        if (CycleGroups.TryGetValue(args.Event.Context, out var group)) {
                            UpdateActionState(args.Event.Context, group);
                        }
                    }
                    break;
            }
        }

        private void Connection_OnKeyUp(object? sender, StreamDeckEventReceivedEventArgs<KeyUpEvent> args) {
            switch (args.Event.Action) {
                case CYLCE2_ACTION:
                case CYLCE3_ACTION:
                    lock (CycleGroups) {
                        if (CycleGroups.TryGetValue(args.Event.Context, out var group)) {
                            group.Advance();
                            UpdateActionState(args.Event.Context, group);
                        }
                    }
                    break;
                case TOGGLE_ACTION:
                    lock (Toggles) {
                        if (Toggles.TryGetValue(args.Event.Context, out var device)) {
                            device.Enabled = !device.Enabled;
                            UpdateActionState(args.Event.Context, device);
                        }
                    }
                    break;
                case DISABLE_ALL_ACTION:
                    var deviceType = ParseDeviceType(Settings[args.Event.Context]?["deviceType"]?.ToString());
                    foreach (var device in _Manager.ListDevices(deviceType)) {
                        device.Enabled = false;
                    }
                    break;
            }
        }

        private void UpdateActionState(string context, Device device) => SetActionState(context, device.Enabled ? 1u : 0u);
        private void UpdateActionState(string context, DeviceCycleGroup group) => SetActionState(context, group.ActiveIndex);
        private void SetActionState(string context, uint state) {
            Task.Run(() => _Connection.SetStateAsync(state, context));
        }
    }
}

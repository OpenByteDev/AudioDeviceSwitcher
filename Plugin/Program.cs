using CommandLine;
using streamdeck_client_csharp;
using System.Diagnostics;

namespace Plugin {
    public class Options {
        [Option("port", Required = true, HelpText = "The websocket port to connect to", SetName = "port")]
        public int Port { get; set; }

        [Option("pluginUUID", Required = true, HelpText = "The UUID of the plugin")]
        public string PluginUUID { get; set; }

        [Option("registerEvent", Required = true, HelpText = "The event triggered when the plugin is registered?")]
        public string RegisterEvent { get; set; }

        [Option("info", Required = true, HelpText = "Extra JSON launch data")]
        public string Info { get; set; }
    }

    class Program {
        // StreamDeck launches the plugin with these details
        // -port [number] -pluginUUID [GUID] -registerEvent [string?] -info [json]
        static void Main(string[] args) {
            // The command line args parser expects all args to use `--`, so, let's append
            for (int count = 0; count < args.Length; count++) {
                if (args[count].StartsWith("-") && !args[count].StartsWith("--")) {
                    args[count] = $"-{args[count]}";
                }
            }

            var parser = new Parser(with => {
                with.EnableDashDash = true;
                with.CaseInsensitiveEnumValues = true;
                with.CaseSensitive = false;
                with.IgnoreUnknownArguments = true;
                with.HelpWriter = Console.Error;
            });

            var options = parser.ParseArguments<Options>(args);
            options.WithParsed(RunPlugin);
        }

        static void RunPlugin(Options options) {
            var connectEvent = new ManualResetEvent(false);
            var disconnectEvent = new ManualResetEvent(false);

            var connection = new StreamDeckConnection(options.Port, options.PluginUUID, options.RegisterEvent);

            connection.OnConnected += (sender, args) => {
                connectEvent.Set();
            };

            connection.OnDisconnected += (sender, args) => {
                disconnectEvent.Set();
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                Debugger.Launch();
            };

            var plugin = new Plugin(connection);

            connection.Run();

            disconnectEvent.WaitOne();
        }
    }
}
using System;

namespace VirtrollOptimization
{
	public class SupervisorConfigFactory<ConfigClass>
	{
		public SupervisorConfigFactory()
		{
		}

		public static ConfigClass ParseArgs(string args) {
			string configText = null;
			if (args.Length >= 1 && args[0] == "-stdin") {
				configText = ReadFromStdin();
			} else if (args.Length >= 2 && args[0] == "-config") {
				configText = ReadFromPath(args[1]);
			} else {
				configText = ReadFromPath("config.json");
			}

			return new ConfigClass(configText);
		}

		public static string ReadFromStdin() {
			string configText = "";
			string line;
			while ((line = Console.ReadLine()) != null && line != "") {
				configText += line;
			}

			Logger.Info("Config read from stdin:");
			Logger.Info(configText);

			return new SupervisorConfig(configText);
		}

		public static string ReadFromPath(string filePath = "config.json") {
			string configText = System.IO.File.ReadAllText(filePath);
			Logger.Info("Config read from {0}", filePath);

			return new SupervisorConfig(configText);
		}
	}
}


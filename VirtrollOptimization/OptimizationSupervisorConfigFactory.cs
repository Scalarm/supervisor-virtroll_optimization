using System;

namespace VirtrollOptimization
{
	public class OptimizationSupervisorConfigFactory
	{
		public OptimizationSupervisorConfigFactory()
		{
		}

		public static OptimizationSupervisorConfig ParseArgs(string[] args) {
			OptimizationSupervisorConfig config = null;
			if (args.Length >= 1 && args[0] == "-stdin") {
				config = ReadFromStdin();
			} else if (args.Length >= 2 && args[0] == "-config") {
				config = ReadFromPath(args[1]);
			} else {
				config = ReadFromPath("config.json");
			}

			return config;
		}

		public static OptimizationSupervisorConfig ReadFromStdin() {
			string configText = "";
			string line;
			while ((line = Console.ReadLine()) != null && line != "") {
				configText += line;
			}

			Logger.Info("Config read from stdin:");
			Logger.Info(configText);

			return new OptimizationSupervisorConfig(configText);
		}

		public static OptimizationSupervisorConfig ReadFromPath(string filePath = "config.json") {
			string configText = System.IO.File.ReadAllText(filePath);
			Logger.Info(string.Format("Config read from {0}", filePath));

			return new OptimizationSupervisorConfig(configText);
		}
	}
}


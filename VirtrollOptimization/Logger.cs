using System;

namespace VirtrollOptimization
{
	public class Logger
	{
		public static string GetDate() {
			return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff (zz)");
		}

		public static void Info(string message)
		{
			Console.WriteLine("[{0}] [INFO] {1}", GetDate(), message);
		}

		public static void Error(string message)
		{
			Console.WriteLine("[{0}] [ERROR] {1}", GetDate(), message);
		}
	}
}


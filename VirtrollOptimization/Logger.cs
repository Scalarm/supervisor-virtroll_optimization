using System;

namespace VirtrollOptimization
{
	public class Logger
	{
		public static void Info(string message)
		{
			string date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff (zz)");
			Console.WriteLine("[{0}] {1}", date, message);
		}
	}
}


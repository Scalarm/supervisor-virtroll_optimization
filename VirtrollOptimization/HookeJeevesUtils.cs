using System;
using Optimization.Core;

namespace VirtrollOptimization
{
	public class HookeJeevesUtils
	{
		//196200 // TODO: WAT?
		public static void NewOptimumFound(object sender, OptimumEventArgs e)
		{
			Console.WriteLine("Nowe minimum: " + e.Point.Result
			                  + ", iteracja: " + e.Step
			                  + ", wywołania funkcji celu: " + e.EvalExecutionCount);
			CommonOptimizationUtils.SaveOptimum(e, "HJ_results.txt");
		}
	}
}


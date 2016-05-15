using System;
using Optimization.Core;

namespace VirtrollOptimization
{
	public class GeneticUtils
	{
		public static void NewOptimumFound(object sender, OptimumEventArgs e)
		{
			Console.WriteLine("Nowe minimum: " + e.Point.Result 
			                  + ", iteracja: " + e.Step 
			                  + ", wywołania funkcji celu: " + e.EvalExecutionCount);
			CommonOptimizationUtils.SaveOptimum(e, "Genetic_results.txt");
		}
	}
}


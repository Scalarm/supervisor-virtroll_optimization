using System;
using Optimization.Core;
using Scalarm;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;

namespace VirtrollOptimization
{
	public class CommonOptimizationUtils
	{
		public SupervisedExperiment Experiment { get; set; }

		public void EndOfCalculation(object sender, OptimumEventArgs e) {
			// TODO resolve results to JSON
			var finalResults = new Dictionary<string, object>();
			finalResults.Add("point", e.Point.Inputs);
			this.Experiment.MarkAsComplete(JsonConvert.SerializeObject(finalResults));
		}

		public static void NewOptimumFound(object sender, OptimumEventArgs e)
		{
			Logger.Info("New minimum: " + e.Point.Result 
			            + ", iteration: " + e.Step 
			            + ", evaluations count: " + e.EvalExecutionCount);
			// TODO: save to object state
			CommonOptimizationUtils.SaveOptimum(e, "Genetic_results.txt");
		}

		public static void SaveOptimum(OptimumEventArgs e, string filename)
		{
			filename = filename.Replace(":", "_");
			filename = filename.Replace("-", "_");

			System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true);
			file.Write(e.ToString());
			file.Close();
		}

		public static void NextStep(object sender, NextStepEventArgs e)
		{
			Logger.Info("Iteration: " + e.Step + ", eval execution count: " + e.EvalExecutionCount);
		}

		public CommonOptimizationUtils(SupervisedExperiment experiment) {
			this.Experiment = experiment;
		}


	}
}


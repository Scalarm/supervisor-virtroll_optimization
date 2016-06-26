using System;
using Optimization.Core;
using Scalarm;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;

namespace VirtrollOptimization
{
	public abstract class CommonOptimizationUtils
	{
		public SupervisedExperiment Experiment { get; set; }

		protected string _methodType;

		public void EndOfCalculation(object sender, OptimumEventArgs e) {
			// TODO resolve results to JSON
			var finalResults = new Dictionary<string, object>();
			finalResults.Add("method_type", this._methodType);
			finalResults.Add("step", e.Step);
			finalResults.Add("eval_execution_count", e.EvalExecutionCount);
			finalResults.Add("parameters", e.Point.Inputs);
			finalResults.Add("values", e.Point.PartialResults);
			finalResults.Add("global_value", e.Point.Result);
			// TODO: pareto
			// TODO: population

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


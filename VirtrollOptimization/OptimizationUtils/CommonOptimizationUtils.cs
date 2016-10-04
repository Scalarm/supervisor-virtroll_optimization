using System;
using Optimization.Core;
using Scalarm;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;

namespace VirtrollOptimization
{
	public class OptimizationNewOptimumFoundState {
		public string event_type = "new_optimum_found";
		public double moe;
		public double[] values;
		public int iteration;
		public int evaluations_count;
	}

	public class OptimizationNextStepState {
		public string event_type = "next_step"; 
		public int iteration;
		public int evaluations_count;
	}

	public abstract class CommonOptimizationUtils
	{
		public SupervisedExperiment Experiment { get; set; }

		protected string _methodType;

		public abstract void BindEvents();

		public void EndOfCalculation(object sender, OptimumEventArgs e) {
			// TODO resolve results to JSON
			var finalResults = new Dictionary<string, object>();
			finalResults.Add("method_type", this._methodType);
			finalResults.Add("step", e.Step);
			finalResults.Add("eval_execution_count", e.EvalExecutionCount);
			finalResults.Add("parameters", e.Point.Values);
			finalResults.Add("global_error", e.Point.Error);
			// TODO: pareto
			// TODO: population

			Scalarm.Client client = Experiment.Client;

			OptimizationNewOptimumFoundState intermediateResult = new OptimizationNewOptimumFoundState () {
				event_type = "end_of_calculations",
				moe = e.Point.Error,
				iteration = e.Step,
				evaluations_count = e.EvalExecutionCount
			};

			var request = new RestRequest("/experiments/{id}/supervisor_run/state_history", Method.POST);
			request.AddUrlSegment("id", Experiment.Id);
			request.AddParameter("state", JsonConvert.SerializeObject(intermediateResult));
			IRestResponse restResponse = client.Execute(request);
			if (restResponse.ErrorException != null) {
				const string message = "HTTP request error on sending progress_info";
				Logger.Info(String.Format("{0}: {1}", message, restResponse.ErrorMessage));
			} else {
			}

			this.Experiment.MarkAsComplete(JsonConvert.SerializeObject(finalResults));
		}

		public void NewOptimumFound(object sender, OptimumEventArgs e)
		{
			Logger.Info("New minimum: " + e.Point.Error 
			            + ", iteration: " + e.Step 
			            + ", evaluations count: " + e.EvalExecutionCount);
			// TODO: save to object state
			// CommonOptimizationUtils.SaveOptimum(e, "Genetic_results.txt");

			Scalarm.Client client = Experiment.Client;

			OptimizationNewOptimumFoundState intermediateResult = new OptimizationNewOptimumFoundState () {
				moe = e.Point.Error,
				iteration = e.Step,
				evaluations_count = e.EvalExecutionCount,
				values = e.Point.Values
			};

			// FIXME: make a method in Scalarm Client lib
			var request = new RestRequest("/experiments/{id}/supervisor_run/state_history", Method.POST);
			request.AddUrlSegment("id", Experiment.Id);
			request.AddParameter("state", JsonConvert.SerializeObject(intermediateResult));
			IRestResponse restResponse = client.Execute(request);
			if (restResponse.ErrorException != null) {
				const string message = "HTTP request error on sending progress_info";
				Logger.Info(String.Format("{0}: {1}", message, restResponse.ErrorMessage));
			} else {
				// FIXME: handle errors (error in response json)
			}
		}

		public static void SaveOptimum(OptimumEventArgs e, string filename)
		{
			filename = filename.Replace(":", "_");
			filename = filename.Replace("-", "_");

			System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true);
			file.Write(e.ToString());
			file.Close();
		}

		public void NextStep(object sender, NextStepEventArgs e)
		{
			Logger.Info("Iteration: " + e.Step + ", eval execution count: " + e.EvalExecutionCount);

			OptimizationNextStepState intermediateResult = new OptimizationNextStepState () {
				iteration = e.Step,
				evaluations_count = e.EvalExecutionCount
			};

			Scalarm.Client client = Experiment.Client;

			// FIXME: make a method in Scalarm Client lib
			var request = new RestRequest("/experiments/{id}/supervisor_run/state_history", Method.POST);
			request.AddUrlSegment("id", Experiment.Id);
			request.AddParameter("state", JsonConvert.SerializeObject(intermediateResult));
			IRestResponse restResponse = client.Execute(request);
			if (restResponse.ErrorException != null) {
				const string message = "HTTP request error on sending progress_info";
				Logger.Info(String.Format("{0}: {1}", message, restResponse.ErrorMessage));
			} else {
				// FIXME: handle errors (error in response json)
			}
		}

		public CommonOptimizationUtils(SupervisedExperiment experiment) {
			this.Experiment = experiment;
		}

	}
}


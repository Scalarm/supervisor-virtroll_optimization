using System;
using Optimization.Core;
using Scalarm;
using System.Collections.Generic;
using System.Threading;

namespace VirtrollOptimization
{
	public abstract class CommonOptimizationUtils
	{
		SupervisedExperiment Experiment { public get; public set; }

		public void EndOfCalculation(object sender) {
			this.Experiment.MarkAsComplete("{\"result\": \"finished\"}");
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
			
		// TODO: reimplement this and Scalarm backend
		/// <summary>
		/// Return a specific SimulationParams (Scalarm Point) from collection that matches the provided OptimizationPoint.
		/// </summary>
		public static SimulationParams FindP(IList<SimulationParams> all, OptimizationPoint point)
		{
			// TODO: let's assume, we have a SimulationParams list with indexes and we know which OP point have which index...
			// TODO: convert OptimizationPoint to SimulationParams OR provide SimulationParams directly to this fun
			// TODO: find in SimulationParams

			for (int i = 0; i < all.Count; ++i)
			{
				var o = all[i].Input;

				if ((double)o["0"] == point.Inputs[0]
				    && (double)o["1"] == point.Inputs[1]
				    && (double)o["2"] == point.Inputs[2])
				{
					return all[i];
				}
			}

			for (int i = 0; i < all.Count; ++i)
			{
				var o = all[i].Input;

				if ((float)o["0"] == (float)point.Inputs[0]
				    && (float)o["1"] == (float)point.Inputs[1]
				    && (float)o["2"] == (float)point.Inputs[2])
				{
					return all[i];
				}
			}

			for (int i = 0; i < all.Count; ++i)
			{
				var o = all[i].Input;

				if ((double)o["0"] == point.Inputs[0]
				    && (double)o["1"] == point.Inputs[1])
				{
					Logger.Info("Point not found");
					return all[i];
				}
			}

			for (int i = 0; i < all.Count; ++i)
			{
				var o = all[i].Input;

				if ((double)o["0"] == point.Inputs[0]
				    && (double)o["2"] == point.Inputs[2])
				{
					Logger.Info("Point not found");
					return all[i];
				}
			}

			for (int i = 0; i < all.Count; ++i)
			{
				var o = all[i].Input;

				if ((double)o["1"] == point.Inputs[1]
				    && (double)o["2"] == point.Inputs[2])
				{
					Logger.Info("Point not found");
					return all[i];
				}
			}

			return null;
		}

		public CommonOptimizationUtils(Experiment experiment) {
			this.Experiment = experiment;
		}

		// TODO: ignore N exceptions (or for N amount of time...)
		private bool WaitAndIgnoreExceptions()
		{
			try
			{
				this.Experiment.WaitForDone();
				return true;
			}
			catch (Exception ex)
			{
				Logger.Info(String.Format("An exception occured on waiting for results: {0}", ex.ToString()));
				return false;
			}
		}

		public void ScalarmFunctionEvaluator(List<OptimizationPoint> points)
		{
			// prepare points
			List<Scalarm.ValuesMap> scalarmPoints = new List<ValuesMap>();
			for (int i = 0; i < points.Count; ++i)
			{
				OptimizationPoint p = points[i];

				Scalarm.ValuesMap paramValues = new Scalarm.ValuesMap();

				// TODO: use input parameters specified in config
				paramValues.Add("sampleId", 0);
				paramValues.Add("inputId", 0);
				paramValues.Add("0", p.Inputs[0]); // t_start
				paramValues.Add("1", p.Inputs[1]); // E20
				paramValues.Add("2", p.Inputs[2]); // Sp20

				scalarmPoints.Add(paramValues);
			}


			// calcualte
			((SupervisedExperiment)Experiment).SchedulePoints(scalarmPoints);

			int wc = 0;
			while (!this.WaitAndIgnoreExceptions())
			{
				Thread.Sleep(1000 * 10);

				++wc;
				if (wc == 10) throw new Exception("Scalarm Wait _daniel");
			}


			// get
			var results = Experiment.GetResults();
			for (int i = 0; i < points.Count; ++i)
			{
				var r = FindP(results, points[i]);
				double beta_end = Convert.ToDouble(r.Output["Beta_end"]);

				points[i].PartialResults = new List<double>()
				{
					beta_end
				};

			}

			//Logger.Info("min finded...");
		}
	}
}


using System;
using Optimization.Core;
using Scalarm;
using System.Collections.Generic;
using System.Threading;

namespace VirtrollOptimization
{
	public class ScalarmEvaluator
	{
		public ScalarmParameter[] ScalarmParameters {
			get;
			set;
		}

		public string MoeName;

		public SupervisedExperiment Experiment { get; set; }

		public ScalarmEvaluator(ScalarmParameter[] scalarmParameters, string moeName="moe")
		{
			this.ScalarmParameters = scalarmParameters;
			this.MoeName = moeName;
		}

		// TODO: this should be a class, which have a simulation input definition in its state
		// not neccessarily - it can be a method in class with state (quicker solution)
		public void ScalarmFunctionEvaluator(List<OptimizationPoint> optimizationPoints)
		{
			// prepare points
			List<Scalarm.ValuesMap> scalarmPoints = new List<ValuesMap>();
			for (int i = 0; i < optimizationPoints.Count; ++i)
			{
				OptimizationPoint optPoint = optimizationPoints[i];

				Scalarm.ValuesMap paramValues = new Scalarm.ValuesMap();

				for (int sp = 0; i < this.ScalarmParameters.Length; ++sp) {
					paramValues.Add(this.ScalarmParameters[sp].id, optPoint.Inputs[sp]);
				}

				scalarmPoints.Add(paramValues);
			}

			/// delegate computations to Scalarm

			((SupervisedExperiment)Experiment).SchedulePoints(scalarmPoints);

			int wc = 0;
			while (!this.WaitAndIgnoreExceptions())
			{
				Thread.Sleep(1000 * 10);

				++wc;
				if (wc == 10) throw new Exception("Scalarm Wait _daniel");
			}

			/// get results

			var results = Experiment.GetResults();
			for (int i = 0; i < optimizationPoints.Count; ++i)
			{
				// TODO: fun(OptimizationPoint) -> result
				SimulationParams scalarmResult = FindScalarmPoint(results, optimizationPoints[i]);

				// FIXME - use output by name given
				double moe = Convert.ToDouble(scalarmResult.Output[this.MoeName]);

				optimizationPoints[i].PartialResults = new List<double>()
				{
					moe
				};

			}

			//Logger.Info("min finded...");
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

		
		// TODO: reimplement this and Scalarm backend
		/// <summary>
		/// Return a specific SimulationParams (Scalarm Point) from collection that matches the provided
		/// OptimizationPoint (that is generated in optimization lib).
		/// </summary>
		public static SimulationParams FindScalarmPoint(IList<SimulationParams> scalarmResults, OptimizationPoint optimizationPoint)
		{
			// TODO: let's assume, we have a SimulationParams list with indexes and we know which OP point have which index...
			// TODO: convert OptimizationPoint to SimulationParams OR provide SimulationParams directly to this fun
			// TODO: find in SimulationParams

			//			string[] outputIds = scalarmResults.First().Output.Keys.ToArray();

			// FIXME: maybe just use the code from SA

			for (int i = 0; i < scalarmResults.Count; ++i)
			{
				var o = scalarmResults[i].Input;

				if ((double)o["0"] == optimizationPoint.Inputs[0]
				    && (double)o["1"] == optimizationPoint.Inputs[1]
				    && (double)o["2"] == optimizationPoint.Inputs[2])
				{
					return scalarmResults[i];
				}
			}

			for (int i = 0; i < scalarmResults.Count; ++i)
			{
				var o = scalarmResults[i].Input;

				if ((float)o["0"] == (float)optimizationPoint.Inputs[0]
				    && (float)o["1"] == (float)optimizationPoint.Inputs[1]
				    && (float)o["2"] == (float)optimizationPoint.Inputs[2])
				{
					return scalarmResults[i];
				}
			}

			for (int i = 0; i < scalarmResults.Count; ++i)
			{
				var o = scalarmResults[i].Input;

				if ((double)o["0"] == optimizationPoint.Inputs[0]
				    && (double)o["1"] == optimizationPoint.Inputs[1])
				{
					Logger.Info("Point not found");
					return scalarmResults[i];
				}
			}

			for (int i = 0; i < scalarmResults.Count; ++i)
			{
				var o = scalarmResults[i].Input;

				if ((double)o["0"] == optimizationPoint.Inputs[0]
				    && (double)o["2"] == optimizationPoint.Inputs[2])
				{
					Logger.Info("Point not found");
					return scalarmResults[i];
				}
			}

			for (int i = 0; i < scalarmResults.Count; ++i)
			{
				var o = scalarmResults[i].Input;

				if ((double)o["1"] == optimizationPoint.Inputs[1]
				    && (double)o["2"] == optimizationPoint.Inputs[2])
				{
					Logger.Info("Point not found");
					return scalarmResults[i];
				}
			}

			return null;
		}
	}
}


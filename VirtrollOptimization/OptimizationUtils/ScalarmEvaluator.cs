using System;
using Optimization.Core;
using Scalarm;
using System.Collections.Generic;
using System.Threading;
using System.Dynamic.Utils;
using System.Linq;
using Newtonsoft.Json;

namespace VirtrollOptimization
{
	public class ScalarmEvaluator
	{
		const int MAX_FAILS = 5*60*24*1;
		const int MAX_FAILS_NO_SIM = 5*60*24*4;
		const int FAIL_WAIT_SEC = 20;

		int failCount = 0;
		int failCountNoSim = 0;

		public ScalarmParameter[] ScalarmParameters {
			get;
			set;
		}

		public string MoeName;

		public SupervisedExperiment Experiment { get; set; }

		public ScalarmEvaluator(SupervisedExperiment experiment, ScalarmParameter[] scalarmParameters, string moeName="moe")
		{
			this.ScalarmParameters = scalarmParameters;
			this.MoeName = moeName;
			this.Experiment = experiment;
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

				for (int sp = 0; sp < this.ScalarmParameters.Length; ++sp) {
					paramValues.Add(this.ScalarmParameters[sp].id, optPoint.Inputs[sp]);
				}

				scalarmPoints.Add(paramValues);
			}

			/// delegate computations to Scalarm

			Experiment.SchedulePoints(scalarmPoints);

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
				SimulationParams scalarmResult = this.FindScalarmPoint(results, optimizationPoints[i]);

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
			while (true) {
				try {
					this.Experiment.WaitForDone ();
					return true;
				} catch (NoActiveSimulationManagersException simExc) {
					failCountNoSim += 1;
					Logger.Info (String.Format("There are no active Simulations Manager (wait {0}s, try {1}/{2})", FAIL_WAIT_SEC, failCountNoSim, MAX_FAILS_NO_SIM));
					if (failCount > MAX_FAILS_NO_SIM) {
						Logger.Info (
							String.Format ("Maximum time waiting for Simulation Managers exceeded when waiting for results! ({0})", MAX_FAILS_NO_SIM)
						);
						return false;
					}
					Thread.Sleep (1000 * FAIL_WAIT_SEC);
				} catch (Exception ex) {
					failCount += 1;
					Logger.Info ("An exception occured when waiting for results:");
					Logger.Info (ex.ToString ());
					if (failCount > MAX_FAILS) {
						Logger.Info (
							String.Format ("Maximum count of fails exceeded when waiting for results! ({0})", MAX_FAILS)
						);
						return false;
					}
					Logger.Info (String.Format("Will retry in {0} seconds (try {1}/{2})", FAIL_WAIT_SEC, failCount, MAX_FAILS));
					Thread.Sleep (1000 * FAIL_WAIT_SEC);
				}
			}
		}

		
		// TODO: reimplement this and Scalarm backend
		/// <summary>
		/// Return a specific SimulationParams (Scalarm Point) from collection that matches the provided
		/// OptimizationPoint (that is generated in optimization lib).
		/// </summary>
		public SimulationParams FindScalarmPoint(IList<SimulationParams> scalarmResults, OptimizationPoint optimizationPoint)
		{
			// TODO: let's assume, we have a SimulationParams list with indexes and we know which OP point have which index...
			// TODO: convert OptimizationPoint to SimulationParams OR provide SimulationParams directly to this fun
			// TODO: find in SimulationParams

			// FIXME: optimize - compute it earlier
			string[] iids = this.ScalarmParameters.Select(i => i.id).ToArray();

			// FIXME: maybe just use conversions as in optimization example code

			var optPointJson = JsonConvert.SerializeObject(optimizationPoint.Inputs);

			// TODO: json repr cache?
			var res = scalarmResults.Where(
				r => JsonConvert.SerializeObject(r.Input.Flatten(iids)) == optPointJson
			);

			if (res.Any()) {
				return res.Last();
			} else {
				Logger.Error(String.Format(
					"Result not found for optimization point{0}", JsonConvert.SerializeObject(optimizationPoint.Inputs)
					));
				return null;
			}
		}
	}
}


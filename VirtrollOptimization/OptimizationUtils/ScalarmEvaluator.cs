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
		const int FAIL_WAIT_SEC = 20;
		// four days
		const int MAX_FAILS_NO_SIM = 5*60*24*4;

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
			int pointsCount = optimizationPoints.Count();

			// prepare points
			List<Scalarm.ValuesMap> scalarmPoints = new List<ValuesMap>();
			for (int i = 0; i < pointsCount; ++i)
			{
				OptimizationPoint optPoint = optimizationPoints[i];

				Scalarm.ValuesMap paramValues = new Scalarm.ValuesMap();

				for (int sp = 0; sp < this.ScalarmParameters.Length; ++sp) {
					paramValues.Add(this.ScalarmParameters[sp].id, optPoint.Values[sp]);
				}

				scalarmPoints.Add(paramValues);
			}

			/// delegate computations to Scalarm

			List<int> indexes = Experiment.SchedulePoints(scalarmPoints);
			Logger.Info (String.Format("Scheduled points indexes: {0}", String.Join(", ", indexes)));

			int wc = 0;
			while (!this.WaitAndIgnoreExceptions())
			{
				Thread.Sleep(1000 * 10);

				++wc;
				if (wc == 10) throw new Exception("Max count of exceptions when waiting for results reached.");
			}

			/// get results

			// get size, to get last results
			Experiment currentExp = Experiment.Client.GetExperimentById<SupervisedExperiment>(Experiment.Id);
			var currentExpSize = currentExp.Size;
			var oldExpSize = currentExpSize - pointsCount;

			var getResultsOptions = new GetResultsOptions () {
				WithIndex = true,
				MinIndex = oldExpSize+1,
				MaxIndex = currentExpSize,
			};

			var results = Experiment.GetResults(getResultsOptions);

			// match Scalarm results to optiomization points partial results
			// using simulation indexes
			for (int i = 0; i < optimizationPoints.Count; ++i)
			{
				// simulation_indexes are numbered from 1
				SimulationParams scalarmResult = null;
				int searchedIndex = i + oldExpSize + 1;
				foreach (SimulationParams r in results) {
					int resultIndex = Convert.ToInt32(r.Output["simulation_index"]);
					if (searchedIndex == resultIndex) {
						scalarmResult = r;
						break;
					}
				}

				if (scalarmResult == null) {
					Logger.Error(String.Format("Could not find Scalarm result for {0}",
						String.Join(", ", optimizationPoints[i].Values)));
				} else {
					double moe = Convert.ToDouble(scalarmResult.Output[this.MoeName]);

					optimizationPoints[i].PartialErrors = new List<double>()
					{
						moe
					};
				}
			}
		}


		/// <summary>
		/// Uses Experiment.WaitForDone and handles NoSimulationManagersExceptions.
		/// </summary>
		/// <returns><c>true</c>, if ended wating for done, <c>false</c> when maximum exceptions count reached.</returns>
		private bool WaitAndIgnoreExceptions()
		{
			while (true) {
				try {
					this.Experiment.WaitForDone ();
					return true;
				} catch (NoActiveSimulationManagersException) {
					failCountNoSim += 1;
					Logger.Info (String.Format("There are no active Simulations Manager (wait {0}s, try {1}/{2})", FAIL_WAIT_SEC, failCountNoSim, MAX_FAILS_NO_SIM));
					if (failCount > MAX_FAILS_NO_SIM) {
						Logger.Info (
							String.Format ("Maximum time waiting for Simulation Managers exceeded when waiting for results! ({0})", MAX_FAILS_NO_SIM)
						);
						return false;
					}
					Thread.Sleep (1000 * FAIL_WAIT_SEC);
				}
			}
		}
	}
}


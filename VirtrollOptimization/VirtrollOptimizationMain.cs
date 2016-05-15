using System;
using System.Threading;
using System.Collections.Generic;
using Optimization.Core;
using Scalarm;

namespace VirtrollOptimization
{
	class VirtrollOptimizationMain
	{
		public const string VERSION = "2016_05_08_1305";

		static Scalarm.Client Client = null;
		static Scalarm.Experiment Experiment = null;
		static string _scalarmScenarioId = "";
		static string _baseUrl = "";
		static string _plgLogin = "";
		static string _plgPass = "";
		static int Jobs = 10; // ilosc wątków do obliczeń

		// Usage: mono Program.exe -> will read config from config.json
		// Usage: mono Program.exe -config <path> -> read config from path
		// Usage: mono Program.exe -stdin -> will read config from stdin
		static void Main(string[] args)
		{
			Console.WriteLine("Virtroll Optimization C# Supervisor, version " + VERSION);

			SupervisorConfig config = SupervisorConfigFactory<SupervisorConfig>.ParseArgs(args);

			Scalarm.SupervisedExperiment experiment = config.GetExperiment();

			// ------------- optim

			List<InputProperties> parameters = config.GetInputProperties();

			Thread optimizationRunner = null;

			switch (config.MethodType) {
			case "genetic":
				Optimization.Genetic genetic =
					new Optimization.Genetic(CommonOptimizationUtils.ScalarmFunctionEvaluator,parameters, null);
				genetic.NextStepEvent += CommonOptimizationUtils.NextStep;
				genetic.NewOptimumFindedEvent += GeneticUtils.NewOptimumFound;
				// TODO
				// hj.EndOfCalculationEvent += HookeJeevesUtils.CalculationEnded;

				optimizationRunner = new Thread(() => {
					genetic.Optimize(10, 20, // populacja: 10 na początku do maksymalnie 20 na końcu
					                 0.00000001, // max error
					                 15); // max iterations
					experiment.MarkAsComplete("{\"result\": \"finished\"}");
				});
				optimizationRunner.Start();
				break;
			case "hooke_jeeves":
				//  HOOKE-JEEVES
				Optimization.HookeJeeves hj =
					new Optimization.HookeJeeves(CommonOptimizationUtils.ScalarmFunctionEvaluator, parameters, null);
				hj.NextStepEvent += CommonOptimizationUtils.NextStep;
				hj.NewOptimumFindedEvent += HookeJeevesUtils.NewOptimumFound;
				// TODO
				// hj.EndOfCalculationEvent += HookeJeevesUtils.CalculationEnded;

				double[] fStepSize = new double[] { 8.7 , 2180 , 5};
				double[] fMinStepSize = new double[] { 0.000000001, 0.000000001, 0.000000001 };

				optimizationRunner = new Thread(() =>
				                           {
					hj.Optimize(
						fStepSize, 
						fMinStepSize, 
						1, // mnożnik kroku roboczego 
						0.00000001, // max error
						15, // max iterations
						true); 
					experiment.MarkAsComplete("{\"result\": \"finished\"}");
				});
				optimizationRunner.Start();
				break;
			default:
				throw new Exception(String.Format("Provided method_type: \"{0}\" is not supported"));
				break;
			}

			if (optimizationRunner != null) {
				optimizationRunner.Join();
			}
		}





		private static bool Wait()
		{
			try
			{
				Experiment.WaitForDone();
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		/// <summary>
		/// Return a specific SimulationParams (Scalarm Point) from collection that matches the provided OptimizationPoint.
		/// </summary>
		static SimulationParams FindP(IList<SimulationParams> all, OptimizationPoint point)
		{
			// TODO: let's assume, we have a SimulationParams list with indexes and we know which OP point have which index...

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
					Logger.Log("not finded point", false);
					return all[i];
				}
			}

			for (int i = 0; i < all.Count; ++i)
			{
				var o = all[i].Input;

				if ((double)o["0"] == point.Inputs[0]
				    && (double)o["2"] == point.Inputs[2])
				{
					Logger.Log("not finded point", false);
					return all[i];
				}
			}

			for (int i = 0; i < all.Count; ++i)
			{
				var o = all[i].Input;

				if ((double)o["1"] == point.Inputs[1]
				    && (double)o["2"] == point.Inputs[2])
				{
					Logger.Log("not finded point", false);
					return all[i];
				}
			}

			return null;
		}
	}
}

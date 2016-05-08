using System;

namespace VirtrollOptimization
{
	class VirtorllOptimizationMain
	{
		static Scalarm.Client Client = null;
		static Scalarm.Experiment Experiment = null;
		static string _scalarmScenarioId = "";
		static string _baseUrl = "";
		static string _plgLogin = "";
		static string _plgPass = "";
		static int Jobs = 10; // ilosc wątków do obliczeń

		static void Main(string[] args)
		{
			// login to scalarm

			Console.WriteLine("podaj plg login: ");
			_plgLogin = Console.ReadLine();
			Console.WriteLine("podaj plg haslo: ");
			_plgPass = Console.ReadLine();

			// scalarm client
			Client = new BasicAuthClient(_baseUrl, _plgLogin, _plgPass);
			if(Client != null)
				Console.WriteLine("zalogowano");
			else
				Console.WriteLine("Nie zalogowano");

			// 1. get scenario
			var scenario = Client.GetScenarioById(_scalarmScenarioId);
			Console.WriteLine("pobrano scenariusz: " + scenario.Name);

			// 2. create supervised experiment

			// description
			string scalarmName = "test";
			Dictionary<string, object> experimentParams = new Dictionary<string, object>() {
				{"experiment_name", scalarmName + (new Random()).Next() },
				{"experiment_description", "test experiment"},
				{"execution_time_constraint", 20}
			};


			// stworzenie eksperymentu
			Experiment = scenario.CreateSupervisedExperiment(null, experimentParams);

			// 2a. add Jobs
			//Experiment.SchedulePrivateMachineJobs(Jobs, "5488a3874269a844730003e2");
			Experiment.ScheduleZeusJobs(Jobs, _plgLogin, _plgPass);


			// ------------- optim

			List<InputProperties> parameters = new List<InputProperties>
			{
				new InputProperties(InputValuesType.Range, new double[] {870 - 87, 870 + 87}),              // t_start
				new InputProperties(InputValuesType.Range, new double[] {218000 - 21800, 218000 + 21800}),  // E20
				new InputProperties(InputValuesType.Range, new double[] {50 - 5, 50 + 5})                   // Sp20
			};

			// GENETYK
			/*Optimization.Genetic genetic = new Optimization.Genetic(CustomFunctionsEvaluator, parameters, null);
            genetic.NextStepEvent += genetic_NextStepEvent;
            genetic.NewOptimumFindedEvent += genetic_NewOptimumFindedEvent;

            Thread thread = new Thread(() => {
                genetic.Optimize(10, 20, // populacja: 10 na początku do maksymalnie 20 na końcu
                    0.00000001, // max error
                    15); // max iterations
                ((SupervisedExperiment)Experiment).MarkAsComplete("{\"result\": \"finished\"}");
            });
            thread.Start();*/


			//  HOOKE-JEEVES
			Optimization.HookeJeeves hj = new Optimization.HookeJeeves(ScalarmFunctionEvaluator, parameters, null);
			hj.NextStepEvent += genetic_NextStepEvent;
			hj.NewOptimumFindedEvent += hj_NewOptimumFindedEvent;

			double[] fStepSize = new double[] { 8.7 , 2180 , 5};
			double[] fMinStepSize = new double[] { 0.000000001, 0.000000001, 0.000000001 };

			Thread thread = new Thread(() =>
			                           {
				hj.Optimize(
					fStepSize, 
					fMinStepSize, 
					1, // mnożnik kroku roboczego 
					0.00000001, // max error
					15, // max iterations
					true); 
				((SupervisedExperiment)Experiment).MarkAsComplete("{\"result\": \"finished\"}");
			});
			thread.Start();
		}

		//196200
		static void hj_NewOptimumFindedEvent(object sender, OptimumEventArgs e)
		{
			Console.WriteLine("Nowe minimum: " + e.Point.Result
			                  + ", iteracja: " + e.Step
			                  + ", wywołania funkcji celu: " + e.EvalExecutionCount);
			SaveOptimum(e, "HJ_results.txt");
		}

		static void genetic_NewOptimumFindedEvent(object sender, OptimumEventArgs e)
		{
			Console.WriteLine("Nowe minimum: " + e.Point.Result 
			                  + ", iteracja: " + e.Step 
			                  + ", wywołania funkcji celu: " + e.EvalExecutionCount);
			SaveOptimum(e, "Genetic_results.txt");
		}

		static void genetic_NextStepEvent(object sender, NextStepEventArgs e)
		{
			Console.WriteLine("Iteracja: " + e.Step + ", wywołania funkcji celu: " + e.EvalExecutionCount);
		}

		static void ScalarmFunctionEvaluator(List<OptimizationPoint> points)
		{
			// prepare points
			List<Scalarm.ValuesMap> scalarmPoints = new List<ValuesMap>();
			for (int i = 0; i < points.Count; ++i)
			{
				OptimizationPoint p = points[i];

				Scalarm.ValuesMap paramValues = new Scalarm.ValuesMap();
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
			while (!Wait())
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

			//Console.WriteLine("min finded...");
		}

		static void SaveOptimum(OptimumEventArgs e, string filename)
		{
			filename = filename.Replace(":", "_");
			filename = filename.Replace("-", "_");

			System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true);
			file.Write(e.ToString());
			file.Close();
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

		static SimulationParams FindP(IList<SimulationParams> all, OptimizationPoint point)
		{
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

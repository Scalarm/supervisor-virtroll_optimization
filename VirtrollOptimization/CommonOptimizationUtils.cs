using System;
using Optimization.Core;

namespace VirtrollOptimization
{
	public class CommonOptimizationUtils
	{
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
			Console.WriteLine("Iteracja: " + e.Step + ", wywołania funkcji celu: " + e.EvalExecutionCount);
		}

		public static void ScalarmFunctionEvaluator(List<OptimizationPoint> points)
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
	}
}


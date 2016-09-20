using System;
using System.Threading;
using System.Collections.Generic;
using Optimization.Core;
using Scalarm;
using RestSharp;
using System.Net;
using Newtonsoft.Json;

namespace VirtrollOptimization
{
	class VirtrollOptimizationMain
	{
		public const string VERSION = "2016_09_20";

		static Scalarm.Client Client = null;
		static Scalarm.Experiment Experiment = null;

		// Usage: mono VirtrollOptimization.exe -> will read config from config.json
		// Usage: mono VirtrollOptimization.exe -config <path> -> read config from path
		// Usage: mono VirtrollOptimization.exe -stdin -> will read config from stdin
		static void Main(string[] args)
		{
			ServicePointManager.ServerCertificateValidationCallback +=
				(sender, certificate, chain, sslPolicyErrors) => true;

			Logger.Info("Virtroll Optimization C# Supervisor, version " + VERSION);

			OptimizationSupervisorConfig config = OptimizationSupervisorConfigFactory.ParseArgs(args);

			Scalarm.SupervisedExperiment experiment = config.GetExperiment();

			List<InputProperties> parameters = config.GetInputProperties();


			Thread optimizationRunner = null;

			ScalarmEvaluator evaluator = new ScalarmEvaluator(experiment, config.Parameters, config.MoeName);

			switch (config.MethodType) {
			case "genetic":
				{
					Optimization.Genetic optimizer = new Optimization.Genetic(
						evaluator.ScalarmFunctionEvaluator,
						parameters,
						null
					);

					new GeneticUtils(experiment, optimizer).BindEvents();

					new Thread(() => {
						optimizer.Optimize(
							config.GeneticPopulationStart,
							config.GeneticPopulationMax,
						    config.OptimizationMaxError,
		                    config.OptimizationMaxIterations
						);
					}).Start();
				}
				break;
			case "hooke_jeeves":
				{
					Optimization.HookeJeeves optimizer = new Optimization.HookeJeeves(
					evaluator.ScalarmFunctionEvaluator,
						parameters,
						null
					);

					new HookeJeevesUtils(experiment, optimizer).BindEvents();

					new Thread(() =>
					{
						optimizer.Optimize(
							config.HookeJeevesStepSizes, 
							config.HookeJeevesMinStepSizes, 
							config.HookeJeevesWorkingStepMultiplier,
							config.OptimizationMaxError,
							config.OptimizationMaxIterations,
							config.HookeJeevesParallel);
					}).Start();
				}
				break;
			case "pso":
			{
				Optimization.Pso optimizer = new Optimization.Pso(
					evaluator.ScalarmFunctionEvaluator,
					parameters,
					null
					);

				new PsoUtils(experiment, optimizer).BindEvents();

				new Thread(() =>
				           {
					optimizer.Optimize(
						config.PsoParticlesCount,
						config.OptimizationMaxError,
						config.OptimizationMaxIterations);
				}).Start();
			}
				break;

			default:
				throw new Exception(String.Format("Provided method_type: \"{0}\" is not supported"));
			}

			if (optimizationRunner != null) {
				optimizationRunner.Join();
			}
		}
	}
}

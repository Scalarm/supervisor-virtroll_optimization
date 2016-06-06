using System;
using Newtonsoft.Json.Linq;

namespace VirtrollOptimization
{
	public class OptimizationSupervisorConfig : SupervisorConfig
	{
		public Double OptimizationMaxError { public get; set; }
		public Double OptimizationMaxIterations { public get; set; }

		public Double GeneticPopulationStart { public get; set; }
		public Double GeneticPopulationMax { public get; set; }

		public Double HookeJeevesWorkingStepMultiplier { public get; set; }
		public Boolean HookeJeevesParallel { public get; set; }

		public OptimizationSupervisorConfig(string configText) : base(configText)
		{
			JObject appConfig = this.RawAppConfig;

			this.OptimizationMaxError = OptionalJsonGet<Double>(appConfig, "optimization_max_error", 0.00000001);
			this.OptimizationMaxIterations = OptionalJsonGet<Double>(appConfig, "optimization_max_iterations", 15);

			this.GeneticPopulationStart = OptionalJsonGet<Double>(appConfig, "genetic_population_start", 10);
			this.GeneticPopulationMax = OptionalJsonGet<Double>(appConfig, "genetic_population_max", 20);

			this.HookeJeevesWorkingStepMultiplier = OptionalJsonGet<Double>(appConfig, "hj_working_step_multiplier", 1);
			this.HookeJeevesParallel = OptionalJsonGet<Double>(appConfig, "hj_parallel", true);
		}
	}
}


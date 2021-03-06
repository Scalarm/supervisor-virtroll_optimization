using System;
using Newtonsoft.Json.Linq;

namespace VirtrollOptimization
{
	public class OptimizationSupervisorConfig : SupervisorConfig
	{
		public string MoeName { get; set; }

		public Double OptimizationMaxError { get; set; }
		public int OptimizationMaxIterations { get; set; }

		public int GeneticPopulationStart { get; set; }
		public int GeneticPopulationMax { get; set; }
		// how many specimens should be created in each iteration by mutation
		// typically ~1/3, 1/5 of population start
		public int GeneticMutationsCount { get; set; }
		// how many specimens should be created in each iteration by crossing
		// typically ~1/3, 1/5 of population start
		public int GeneticCrossesCount { get; set; }

		public double HookeJeevesWorkingStepMultiplier { get; set; }
		public bool HookeJeevesParallel { get; set; }
		// e.g. { 8.7, 2180, 5 }
		public double[] HookeJeevesStepSizes { get; set; }
		// e.g. { 0.000000001, 0.000000001, 0.000000001 }
		public double[] HookeJeevesMinStepSizes { get; set; }

		public int PsoParticlesCount { get; set; }


		/// <summary>
		/// Please do not use this constructor - use new(string) instead
		/// </summary>
		public OptimizationSupervisorConfig() : base()
		{}

		public OptimizationSupervisorConfig(string configText) : base(configText)
		{
			JObject appConfig = this.RawAppConfig;

			this.MoeName = OptionalJsonGet<string>(appConfig, "moe_name", null);

			this.OptimizationMaxError = OptionalJsonGet<double>(appConfig, "optimization_max_error", 0.00000001);
			this.OptimizationMaxIterations = OptionalJsonGet<int>(appConfig, "optimization_max_iterations", 15);

			switch (this.MethodType) {
			case "genetic":
				this.GeneticPopulationStart = OptionalJsonGet<int>(appConfig, "genetic_population_start", 10);
				this.GeneticPopulationMax = OptionalJsonGet<int>(appConfig, "genetic_population_max", 20);
				this.GeneticMutationsCount = appConfig["genetic_mutations_count"].ToObject<int>();
				this.GeneticCrossesCount = appConfig["genetic_crosses_count"].ToObject<int>();
				break;
			case "hoooke_jeeves":
				this.HookeJeevesWorkingStepMultiplier = OptionalJsonGet<double>(appConfig, "hj_working_step_multiplier", 1);
				this.HookeJeevesParallel = OptionalJsonGet<bool>(appConfig, "hj_parallel", true);
				// TODO: check if step sizes array is the same size as parameters count?
				this.HookeJeevesStepSizes = appConfig["hj_step_sizes"].ToObject<double[]>();
				// TODO: check -||-
				this.HookeJeevesMinStepSizes = appConfig["hj_min_step_sizes"].ToObject<double[]>();
				break;
			case "pso":
				this.PsoParticlesCount = OptionalJsonGet<int>(appConfig, "pso_particles_count", 1);
				break;
			}
		}
	}
}


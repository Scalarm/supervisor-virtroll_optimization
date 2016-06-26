using System;
using Optimization.Core;
using Scalarm;

namespace VirtrollOptimization
{
	public class GeneticUtils : CommonOptimizationUtils
	{
		Optimization.Genetic Optimizer { get; set; }

		public GeneticUtils(SupervisedExperiment experiment, Optimization.Genetic genetic) :
			base(experiment)
		{
			this.Optimizer = genetic;
		}

		public void BindEvents()
		{
			this.Optimizer.NextStepEvent += CommonOptimizationUtils.NextStep;
			this.Optimizer.NewOptimumFindedEvent += CommonOptimizationUtils.NewOptimumFound;
			this.Optimizer.EndOfCalculationEvent += this.EndOfCalculation;
		}
	}
}


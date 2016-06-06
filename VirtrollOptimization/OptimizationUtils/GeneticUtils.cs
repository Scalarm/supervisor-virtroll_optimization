using System;
using Optimization.Core;
using Scalarm;

namespace VirtrollOptimization
{
	public class GeneticUtils : CommonOptimizationUtils
	{
		Optimization.Genetic Optimizer { public get; set; }

		public GeneticUtils(Experiment experiment, Optimization.Genetic genetic) :
			base(experiment)
		{
			this.Optimizer = genetic;
		}

		public void BindEvents()
		{
			this.Optimizer.NextStepEvent += CommonOptimizationUtils.NextStep;
			this.Optimizer.NewOptimumFindedEvent += CommonOptimizationUtils.NewOptimumFound;
			this.Optimizer.EndOfCalculationEvent += CommonOptimizationUtils.EndOfCalculation;
		}
	}
}


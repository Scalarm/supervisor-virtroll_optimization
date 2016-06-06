using System;
using Optimization.Core;
using Scalarm;

namespace VirtrollOptimization
{
	public class HookeJeevesUtils
	{
		Optimization.HookeJeeves Optimizer { public get; set; }

		public HookeJeevesUtils(Experiment experiment, Optimization.HookeJeeves optimizer) {
			this.Optimizer = optimizer;
		}

		public void BindEvents()
		{
			this.Optimizer.NextStepEvent += CommonOptimizationUtils.NextStep;
			this.Optimizer.NewOptimumFindedEvent += CommonOptimizationUtils.NewOptimumFound;
			this.Optimizer.EndOfCalculationEvent += CommonOptimizationUtils.EndOfCalculation;
		}
	}
}


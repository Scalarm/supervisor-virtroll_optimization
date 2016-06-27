using System;
using Optimization.Core;
using Scalarm;

namespace VirtrollOptimization
{
	public class PsoUtils : CommonOptimizationUtils
	{
		Optimization.Pso Optimizer { get; set; }

		public PsoUtils(SupervisedExperiment experiment, Optimization.Pso optimizer) :
			base(experiment)
		{
			this._methodType = "pso";
			this.Optimizer = optimizer;
		}

		public override void BindEvents()
		{
			this.Optimizer.NextStepEvent += CommonOptimizationUtils.NextStep;
			this.Optimizer.NewOptimumFindedEvent += CommonOptimizationUtils.NewOptimumFound;
			this.Optimizer.EndOfCalculationEvent += this.EndOfCalculation;
		}
	}
}


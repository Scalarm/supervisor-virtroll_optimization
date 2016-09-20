using System;
using Optimization.Core;
using Scalarm;

namespace VirtrollOptimization
{
	public class HookeJeevesUtils : CommonOptimizationUtils
	{
		Optimization.HookeJeeves Optimizer { get; set; }

		public HookeJeevesUtils(SupervisedExperiment experiment, Optimization.HookeJeeves optimizer) :
			base(experiment)
		{
			this._methodType = "hooke_jeeves";
			this.Optimizer = optimizer;
		}

		public override void BindEvents()
		{
			this.Optimizer.NextStepEvent += CommonOptimizationUtils.NextStep;
			this.Optimizer.NewOptimumFindedEvent += this.NewOptimumFound;
			this.Optimizer.EndOfCalculationEvent += this.EndOfCalculation;
		}
	}
}


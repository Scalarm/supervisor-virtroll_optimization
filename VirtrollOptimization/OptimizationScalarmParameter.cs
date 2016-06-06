using System;

namespace VirtrollOptimization
{
	/// <summary>
	/// Extend the ScalarmParameter with parameter properties used in optimization.
	/// </summary>
	public class OptimizationScalarmParameter : ScalarmParameter
	{
		public double StepSize;
		public double MinStepSize;

		public OptimizationScalarmParameter()
		{
			// default values
			this.MinStepSize = 0.000000001;
		}

		public override string ToString() {
			return string.Format("{0}, step size: {1}, min step size: {2}",
			                     base.ToString(), this.StepSize, this.MinStepSize);
		}
	}
}


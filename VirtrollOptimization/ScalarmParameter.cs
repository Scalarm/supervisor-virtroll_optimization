using System;

namespace VirtrollOptimization
{
	public class ScalarmParameter
	{
		public string id;
		public double min;
		public double max;
		// TODO: if type != float - ignore parameter
		public string type;

		public double[] Range {
			get {
				return new double[] { min, max };
			}
		}

		public override string ToString() {
			return string.Format("Parameter id: {0}, type: {1}, min: {2}, max: {3}", id, type, min, max);
		}
	}
}


using System;

namespace DoubleExtensions {

	public static class DoubleExtensions {

		public static double Scaled (this double d, double scaleFactor) {
			return d * scaleFactor;
		}

		public static double Rounded (this double d, double rounding) {
			return Math.Round(d / rounding) * rounding;
		}

	}

}
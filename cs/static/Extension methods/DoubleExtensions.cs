using System;

namespace DoubleExtensions {

	public static class DoubleExtensions {

		public static void Scale (this double d, double scaleFactor) {
			d *= scaleFactor;
		}
		public static double Scaled (this double d, double scaleFactor) {
			return d * scaleFactor;
		}

		public static void Round (this double d, double rounding) {
			d = Math.Round(d / rounding) * rounding;
		}
		public static double Rounded (this double d, double rounding) {
			return Math.Round(d / rounding) * rounding;
		}

	}

}
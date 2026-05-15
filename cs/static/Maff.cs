namespace Maff {

	public static class MaffConst {

		public const double
			deg45_inRad = 0.78539816339744828d,
			deg90_inRad = 1.5707963267948966d,
			radToDeg = 57.29577951308232d,
			degToRad = 0.017453292519943295d,
			pi2 = 6.283185307179586d
		;
		
	}

	public static class MaffFunc {
		int NextPow2(int x) {
			if (x <= 0) throw new ArgumentOutOfRangeException(nameof(x));
			uint ux = (uint)x - 1;
			ux |= ux >> 1;
			ux |= ux >> 2;
			ux |= ux >> 4;
			ux |= ux >> 8;
			ux |= ux >> 16;
			uint result = ux + 1;
			if (result > int.MaxValue) throw new OverflowException();
			return (int)result;
		}

	}

}
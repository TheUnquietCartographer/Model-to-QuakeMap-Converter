using System;

	public struct Vector2 {

		public double x = 0;
		public double y = 0;
    	public double sum {get{return x + y;}}
		public double magnitude {get{return Math.Sqrt(x*x + y*y);}}
		public double magnitudeSquared {get{return x*x + y*y;}}
		public Vector2 inverted {get{return new Vector2(-this.x, -this.y);}}
		public Vector2 normalized {get{
			double shrinkFactor = 1/this.magnitude;
			return new Vector2(this.x * shrinkFactor, this.y * shrinkFactor);
		}}
		public bool isFinite {get{return (double.IsFinite(this.x) && double.IsFinite(this.y));}}
		public bool isNaN {get{return (double.IsNaN(this.x) && double.IsNaN(this.y));}}
		public bool isInfiniteOrNaN {get{return (!this.isFinite || this.isNaN);}}

		public readonly static Vector2
			right = new Vector2 (1,0), up = new Vector2(0,1),
			left = new Vector2 (-1,0), down = new Vector2(0,-1),
			zero = new Vector2 (0,0)
		;
	//
	//	CONSTRUCTOR
	//
		public Vector2 (double x = 0, double y = 0) {
			this.x = x; this.y = y;
		}


	////////////////////////////////////////////////////////////////////////////////////////////////////
	//
	//	FUNCTIONS
	//
	////////////////////////////////////////////////////////////////////////////////////////////////////

	// Add
		public void Add (Vector2 other) {
			this.x += other.x; this.y += other.y;
		}
		public void Add (params double[] components) {
			switch (components.Length) {
				case >= 2: this.x += components[0]; this.y += components[1]; return;
				case 1: this.x += components[0]; return;
			}
		}
		public Vector2 Added (Vector2 other) {
			return new Vector2(this.x + other.x, this.y + other.y);
		}
		public Vector2 Added (params double[] components) {
			switch (components.Length) {
				case >= 2:  return new Vector2(this.x + components[0], this.y + components[1]);
				case 1:  return new Vector2(this.x + components[0], this.y);
			}
			return new Vector2(this.x, this.y);
		}

	//Sum
		public static Vector2 Combine (params Vector2[] vectors) {
			for (int i = 1; i < vectors.Length; i++) vectors[0].Add(vectors[i]);
			return vectors[0];
		}

	//Scale
		public void Scale (double scaleFactor) {
			this.x *= scaleFactor; this.y *= scaleFactor;
		}
		public void Shrink (double shrinkFactor) {
			shrinkFactor = shrinkFactor == 0 ? 0 : 1/shrinkFactor;
			this.x *= shrinkFactor; this.y *= shrinkFactor;
		}
		public void Scale (Vector2 other) {
			this.x += other.x; this.y += other.y;
		}
		public void Scale (params double[] components) {
			switch (components.Length) {
				case >= 2: this.x *= components[0]; this.y *= components[1]; return;
				case 1: this.x *= components[0]; return;
			}
		}

		public Vector2 Scaled (double scaleFactor) {
			return new Vector2(this.x * scaleFactor, this.y * scaleFactor);
		}
		public Vector2 Shrunk (double shrinkFactor) {
			shrinkFactor = shrinkFactor == 0 ? 0 : 1/shrinkFactor;
			return new Vector2(this.x * shrinkFactor, this.y * shrinkFactor);
		}
		public Vector2 Scaled (Vector2 other) {
			return new Vector2(this.x * other.x, this.y * other.y);
		}
		public Vector2 Scaled (params double[] components) {
			switch (components.Length) {
				case >= 2: return new Vector2(this.x * components[0], this.y * components[1]);
				case 1: return new Vector2(this.x * components[0], this.y);
			}
			return new Vector2(this.x, this.y);
		}

	//Invert
		public void Invert () {
			this.x = -this.x;
			this.y = -this.y;
		}
	//	public Vector2 Inverted () {
	//		return new Vector2(
	//			-this.x,
	//			-this.y,
	//		);
	//	}

	//Round
		public void Round (double rounding) {
			this.x = Math.Round(this.x / rounding) * rounding;
			this.y = Math.Round(this.y / rounding) * rounding;
		}
		public Vector2 Rounded (double rounding) {
			return new Vector2(
				Math.Round(this.x / rounding) * rounding,
				Math.Round(this.y / rounding) * rounding
			);
		}

	//Normalize
		public void Normalize () {
			double shrinkFactor = 1/this.magnitude;
			this.x *= shrinkFactor; this.y *= shrinkFactor;
		}
	//	public Vector2 Normalized () {
	//		double shrinkFactor = 1/this.magnitude;
	//		return new Vector2(this.x * shrinkFactor, this.y * shrinkFactor);
	//	}
/*
	//Cross product
		public static Vector2 Cross (Vector2 v1, Vector2 v2) {
			return new Vector2 (
				(v1.y * v2.z) - (v1.z * v2.y),
				(v1.z * v2.x) - (v1.x * v2.z),
				(v1.x * v2.y) - (v1.y * v2.x)
			);
		}
		public static Vector2 Cross_LeftHanded (Vector2 v1, Vector2 v2) {
       		return new Vector2 (
				(v1.z * v2.y) - (v1.y * v2.z),
				(v1.x * v2.z) - (v1.z * v2.x),
				(v1.y * v2.x) - (v1.x * v2.y)
        	);
    	}
*/
	//Dot product
		public static double Dot (Vector2 v1, Vector2 v2) {
			return (v1.x * v2.x) + (v1.y * v2.y);
		}

	//Vector to/from
		public Vector2 VectorTo (Vector2 destination) {
			return new Vector2 (
				destination.x - this.x,
				destination.y - this.y
			);
		}
		public Vector2 VectorFrom (Vector2 origin) {
			return new Vector2 (
				this.x - origin.x,
				this.y - origin.y
			);
		}
		public static Vector2 Vector (Vector2 origin, Vector2 destination) {
			return new Vector2 (
				destination.x - origin.x,
				destination.y - origin.y
			);
		}

	//SIGN VECTOR
		public Vector2 Sign (byte fidelity) {
		//FIDELITY 3: Sign all components.
			if (fidelity >= 3) {
				return new Vector2(
					Math.Sign(this.x),
					Math.Sign(this.y)
				);
			}
		//FIDELITY 1: Only sign the biggest component.
			Vector2 signedVector = new Vector2(0,0);
			if (Math.Abs(this.y) > Math.Abs(this.x)) {
				signedVector.y = Math.Sign(this.y);
			}
			else signedVector.x = Math.Sign(this.x);
			if (fidelity < 2) return signedVector;
		//FIDELITY 2: Get half the size of the biggest component. If the other components are > this value, sign them as well.
		//  Doing it this way means the return may have 2 OR 3 non-zero components, which may be useful when dealing with, for example, vertex or plane normals.
			double halfBig = Math.Abs(Vector2.Dot(this, signedVector))/2;
			if (Math.Abs(this.x) > halfBig) signedVector.x = Math.Sign(this.x);
			if (Math.Abs(this.y) > halfBig) signedVector.y = Math.Sign(this.y);
			return signedVector;
		}
		
	////////////////////////////////////////////////////////////////////////////////////////////////////

	//
	//  GET ANGLE BETWEEN VECTORS
	//

	//UNSIGNED
	//  Using law of cosines.
	//  Returns value in radians.
		public static double UnsignedAngle (Vector2 v1, Vector2 v2) {
			double mag_0_1 = v1.magnitude;
			double mag_0_2 = v2.magnitude;
			double mag_1_2 = Vector2.Vector(v1, v2).magnitude;
			return CosC(mag_0_1, mag_0_2, mag_1_2);
		}
	//  Get angle C
		private static double CosC (double lengthA, double lengthB, double lengthC) {
			return Math.Acos(
				(lengthA * lengthA + lengthB * lengthB - lengthC * lengthC)
				/ (2 * lengthA * lengthB)
			);
		}

	//SIGNED  
	//  Using atan2.
	//  Returns value in radians.
	//  Left-handed and right-handed variants.
		public static double SignedAngle_L (Vector2 v1, Vector2 v2) {
			double rad = Math.Atan2(v2.y, v2.x) - Math.Atan2(v1.y, v1.x);
			while (rad < -Math.PI) rad += Maff.MaffConst.pi2;
			while (rad > Math.PI) rad -= Maff.MaffConst.pi2;
			return rad;
		}
		public static double SignedAngle_R (Vector2 v1, Vector2 v2) {
			double rad = Math.Atan2(v1.y, v1.x) - Math.Atan2(v2.y, v2.x);
			while (rad < -Math.PI) rad += Maff.MaffConst.pi2;
			while (rad > Math.PI) rad -= Maff.MaffConst.pi2;
			return rad;
		}

	//ROTATE VECTOR
		public Vector2 Rotated_L (double angleInRadians) {
			double cos = Math.Cos(angleInRadians);
			double sin = Math.Sin(angleInRadians);
			return new Vector2(
				this.x*cos - this.y*sin,
				this.x*sin + this.y*cos
			);
		}

	//INTERSECTION
		public static Vector2? Intersection (Vector2 pointA, Vector2 vectorA, Vector2 pointB, Vector2 vectorB) {
		// Calculate the determinant of the matrix
			double determinant = vectorA.x * -vectorB.y - vectorA.y * -vectorB.x;
		//If lines are parallel return null
			if (determinant == 0) return null;
		// Solve the linear system to find t and s
			double t = (pointB.x - pointA.x) * (-vectorB.y) - (pointB.y - pointA.y) * (-vectorB.x);
			//double s = vectorA.x * (pointB.y - pointA.y) - vectorA.y * (pointB.x - pointA.x);
			t /= determinant;
			//s /= determinant;
		// Calculate the intersection point
			return new Vector2(pointA.x + t*vectorA.x, pointA.y + t*vectorA.y);
			//return new Vector2(pointB.x + s * vectorB.x, pointB.y + s * vectorB.y);
		}

	////////////////////////////////////////////////////////////////////////////////////////////////////

		public override string ToString () {
			return $"({this.x}, {this.y})";
		}

	//THANKS A.I.
		public bool Equals (Vector2 other) => Equals(other, Double.Epsilon);

		public bool Equals (Vector2 other, double epsilon) {
		// Use absolute difference; consider relative comparison if needed.
			if (
				Math.Abs(this.x - other.x) > epsilon
			||	Math.Abs(this.y - other.y) > epsilon
			) return false;
			return true;
		}

		public override bool Equals (object? obj) => obj is Vector2 other && Equals(other);

		public override int GetHashCode () {
			HashCode hashcode = new HashCode();
			hashcode.Add(BitConverter.DoubleToUInt64Bits(this.x));
			hashcode.Add(BitConverter.DoubleToUInt64Bits(this.y));
			// Use bit pattern to account for +0/-0 consistency
			return hashcode.ToHashCode();
		}

		public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);
		public static bool operator !=(Vector2 left, Vector2 right) => !left.Equals(right);

	}
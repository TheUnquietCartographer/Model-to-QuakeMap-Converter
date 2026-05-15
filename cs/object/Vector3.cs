using System;

	public struct Vector3 {

		public double x = 0;
		public double y = 0;
		public double z = 0;
    	public double sum {get{return x + y + z;}}
		public double magnitude {get{return Math.Sqrt(x*x + y*y + z*z);}}
		public double magnitudeSquared {get{return x*x + y*y + z*z;}}
		public Vector3 inverted {get{return new Vector3(-this.x, -this.y, -this.z);}}
		public Vector3 normalized {get{
			double shrinkFactor = 1/this.magnitude;
			return new Vector3(this.x * shrinkFactor, this.y * shrinkFactor, this.z * shrinkFactor);
		}}
		public Vector3 absolute {get{
			return new Vector3(Math.Abs(this.x), Math.Abs(this.y), Math.Abs(this.z));
		}}
		public bool isFinite {get{return (double.IsFinite(this.x) && double.IsFinite(this.y) && double.IsFinite(this.z));}}
		public bool isNaN {get{return (double.IsNaN(this.x) && double.IsNaN(this.y) && double.IsNaN(this.z));}}
		public bool isInfiniteOrNaN {get{return (!this.isFinite || this.isNaN);}}

		public readonly static Vector3
			right = new Vector3 (1,0,0), forward = new Vector3(0,1,0), up = new Vector3(0,0,1),
			left = new Vector3 (-1,0,0), back = new Vector3(0,-1,0), down = new Vector3(0,0,-1),
			zero = new Vector3 (0,0,0)
		;
	//
	//	CONSTRUCTOR
	//
		public Vector3 (double x = 0, double y = 0, double z = 0) {
			this.x = x; this.y = y; this.z = z;
		}


	////////////////////////////////////////////////////////////////////////////////////////////////////
	//
	//	FUNCTIONS
	//
	////////////////////////////////////////////////////////////////////////////////////////////////////

	//Add
		public void Add (Vector3 other) {
			this.x += other.x; this.y += other.y; this.z += other.z;
		}
		public void Add (double x, double y = 0, double z = 0) {
			this.x += x; this.y += y; this.z += z;
		}
		public Vector3 Added (Vector3 other) {
			return new Vector3(this.x + other.x, this.y + other.y, this.z + other.z);
		}
		public Vector3 Added (double x, double y = 0, double z = 0) {
			return new Vector3(this.x + x, this.y + y, this.z + z);
		}

	//Subtract
		public void Subtract (Vector3 other) {
			this.x -= other.x; this.y -= other.y; this.z -= other.z;
		}
		public void Subtract (double x, double y = 0, double z = 0) {
			this.x -= x; this.y -= y; this.z -= z;
		}
		public Vector3 Subtracted (Vector3 other) {
			return new Vector3(this.x - other.x, this.y - other.y, this.z - other.z);
		}
		public Vector3 Subtracted (double x, double y = 0, double z = 0) {
			return new Vector3(this.x - x, this.y - y, this.z - z);
		}

	//Combine
		public static Vector3 Combine (params Vector3[] vectors) {
			for (int i = 1; i < vectors.Length; i++) vectors[0].Add(vectors[i]);
			return vectors[0];
		}

	//Scale
		public void Scale (double scaleFactor) {
			this.x *= scaleFactor; this.y *= scaleFactor; this.z *= scaleFactor;
		}
		public void Shrink (double shrinkFactor) {
			shrinkFactor = shrinkFactor == 0 ? 0 : 1/shrinkFactor;
			this.x *= shrinkFactor; this.y *= shrinkFactor; this.z *= shrinkFactor;
		}
		public void Scale (Vector3 other) {
			this.x += other.x; this.y += other.y; this.z += other.z;
		}
		public void Scale (double x, double y = 1, double z = 1) {
			this.x *= x; this.y *= y; this.z *= z;
		}

		public Vector3 Scaled (double scaleFactor) {
			return new Vector3(this.x * scaleFactor, this.y * scaleFactor, this.z * scaleFactor);
		}
		public Vector3 Shrunk (double shrinkFactor) {
			shrinkFactor = shrinkFactor == 0 ? 0 : 1/shrinkFactor;
			return new Vector3(this.x * shrinkFactor, this.y * shrinkFactor, this.z * shrinkFactor);
		}
		public Vector3 Scaled (Vector3 other) {
			return new Vector3(this.x * other.x, this.y * other.y, this.z * other.z);
		}
		public Vector3 Scaled (double x, double y = 1, double z = 1) {
			return new Vector3(this.x * x, this.y * y, this.z * z);
		}

	//Invert
		public void Invert () {
			this.x = -this.x;
			this.y = -this.y;
			this.z = -this.z;
		}
	//	public Vector3 Inverted () {
	//		return new Vector3(
	//			-this.x,
	//			-this.y,
	//			-this.z
	//		);
	//	}

	//Round
		public void Round (double rounding) {
			this.x = Math.Round(this.x / rounding) * rounding;
			this.y = Math.Round(this.y / rounding) * rounding;
			this.z = Math.Round(this.z / rounding) * rounding;
		}
		public Vector3 Rounded (double rounding) {
			return new Vector3(
				Math.Round(this.x / rounding) * rounding,
				Math.Round(this.y / rounding) * rounding,
				Math.Round(this.z / rounding) * rounding
			);
		}

	//Normalize
		public void Normalize () {
			double shrinkFactor = 1/this.magnitude;
			this.x *= shrinkFactor; this.y *= shrinkFactor; this.z *= shrinkFactor;
		}
	//	public Vector3 Normalized () {
	//		double shrinkFactor = 1/this.magnitude;
	//		return new Vector3(this.x * shrinkFactor, this.y * shrinkFactor, this.z * shrinkFactor);
	//	}

	//Cross product
		public static Vector3 Cross (Vector3 v1, Vector3 v2) {
			return new Vector3 (
				(v1.y * v2.z) - (v1.z * v2.y),
				(v1.z * v2.x) - (v1.x * v2.z),
				(v1.x * v2.y) - (v1.y * v2.x)
			);
		}
		public static Vector3 Cross_LeftHanded (Vector3 v1, Vector3 v2) {
       		return new Vector3 (
				(v1.z * v2.y) - (v1.y * v2.z),
				(v1.x * v2.z) - (v1.z * v2.x),
				(v1.y * v2.x) - (v1.x * v2.y)
        	);
    	}

	//Dot product
		public static double Dot (Vector3 v1, Vector3 v2) {
			return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
		}

	//Vector to/from
		public Vector3 VectorTo (Vector3 destination) {
			return new Vector3 (
				destination.x - this.x,
				destination.y - this.y,
				destination.z - this.z
			);
		}
		public Vector3 VectorFrom (Vector3 origin) {
			return new Vector3 (
				this.x - origin.x,
				this.y - origin.y,
				this.z - origin.z
			);
		}
		public static Vector3 Vector (Vector3 origin, Vector3 destination) {
			return new Vector3 (
				destination.x - origin.x,
				destination.y - origin.y,
				destination.z - origin.z
			);
		}

	//Sign
		public Vector3 Signed (byte fidelity) {
		//FIDELITY 3: Sign all components.
			if (fidelity >= 3) {
				return new Vector3(
					Math.Sign(this.x),
					Math.Sign(this.y),
					Math.Sign(this.z)
				);
			}
		//FIDELITY 1: Only sign the biggest component.
			Vector3 signedVector = new Vector3(0,0,0);
			if (Math.Abs(this.y) > Math.Abs(this.x)) {
				if (Math.Abs(this.z) > Math.Abs(this.y)) {
					signedVector.z = Math.Sign(this.z);
				}
				else signedVector.y = Math.Sign(this.y);
			}
			else if (Math.Abs(this.z) > Math.Abs(this.x)) {
				signedVector.z = Math.Sign(this.z);
			}
			else signedVector.x = Math.Sign(this.x);
			if (fidelity < 2) return signedVector;
		//FIDELITY 2: Get half the size of the biggest component. If the other components are > this value, sign them as well.
			//Doing it this way means the return may have 2 OR 3 non-zero components, which may be useful when dealing with, for example, vertex or plane normals.
			double halfBig = Math.Abs(Vector3.Dot(this, signedVector))/2;
			if (Math.Abs(this.x) > halfBig) signedVector.x = Math.Sign(this.x);
			if (Math.Abs(this.y) > halfBig) signedVector.y = Math.Sign(this.y);
			if (Math.Abs(this.z) > halfBig) signedVector.z = Math.Sign(this.z);
			return signedVector;
		}

	////////////////////////////////////////////////////////////////////////////////////////////////////

	//
	//  GET ANGLE BETWEEN VECTORS
	//

	//UNSIGNED
	//  Using law of cosines.
	//  Returns value in radians.
		public static double UnsignedAngle (Vector3 v1, Vector3 v2) {
			double mag_0_1 = v1.magnitude;
			double mag_0_2 = v2.magnitude;
			double mag_1_2 = Vector3.Vector(v1, v2).magnitude;
			return CosC(mag_0_1, mag_0_2, mag_1_2);
		}
		public static double UnsignedAngle (Vector3 axis, Vector3 pt1, Vector3 pt2) {
			double mag_0_1 = axis.VectorTo(pt1).magnitude;
			double mag_0_2 = axis.VectorTo(pt2).magnitude;
			double mag_1_2 = pt1.VectorTo(pt2).magnitude;
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
		public static double SignedAngle_L (Vector3 axis, Vector3 v1, Vector3 v2) {
			axis = axis.normalized; v1 = v1.normalized; v2 = v2.normalized;
			return Math.Atan2(Vector3.Dot(Vector3.Cross(v1, v2), axis), Vector3.Dot(v1, v2));
		}
		public static double SignedAngle_R (Vector3 axis, Vector3 v1, Vector3 v2) {
			axis = axis.normalized; v1 = v1.normalized; v2 = v2.normalized;
			return Math.Atan2(Vector3.Dot(Vector3.Cross(v2, v1), axis), Vector3.Dot(v1, v2));
		}

	//ROTATE VECTOR
		public Vector3 Rotate_L_OlindeRodrigues (Vector3 axis, double angleInRadians) {
			return this.Scaled(Math.Cos(angleInRadians))
				.Added( Vector3.Cross(axis, this).Scaled(Math.Sin(angleInRadians)) )
				.Added( axis.Scaled(Vector3.Dot(axis, this)).Scaled(1-Math.Cos(angleInRadians)) );
		}

	////////////////////////////////////////////////////////////////////////////////////////////////////

		public override string ToString () {
			return $"({this.x}, {this.y}, {this.z})";
		}

	//THANKS A.I.
		public bool Equals (Vector3 other) => Equals(other, Double.Epsilon);

		public bool Equals (Vector3 other, double epsilon) {
		// Use absolute difference; consider relative comparison if needed.
			if (
				Math.Abs(this.x - other.x) > epsilon
			||	Math.Abs(this.y - other.y) > epsilon
			||	Math.Abs(this.z - other.z) > epsilon			
			) return false;
			return true;
		}

		public override bool Equals (object? obj) => obj is Vector3 other && Equals(other);

		public override int GetHashCode () {
			HashCode hashcode = new HashCode();
			hashcode.Add(BitConverter.DoubleToUInt64Bits(this.x));
			hashcode.Add(BitConverter.DoubleToUInt64Bits(this.y));
			hashcode.Add(BitConverter.DoubleToUInt64Bits(this.z));
			// Use bit pattern to account for +0/-0 consistency
			return hashcode.ToHashCode();
		}

		public static bool operator ==(Vector3 left, Vector3 right) => left.Equals(right);
		public static bool operator !=(Vector3 left, Vector3 right) => !left.Equals(right);

	}
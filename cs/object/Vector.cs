using System;

	public struct Vector {

		public double[] components;
		public double x {
			get {
				try {return components[0];}
				catch (IndexOutOfRangeException e) { Console.WriteLine($"{e.Message}"); }
				return 0;
			}
			set {
				try {components[0] = value;}
				catch (IndexOutOfRangeException e) { Console.WriteLine($"{e.Message}"); }
			}
		}
		public double y {
			get {
				try {return components[1];}
				catch (IndexOutOfRangeException e) { Console.WriteLine($"{e.Message}"); }
				return 0;
			}
			set {
				try {components[1] = value;}
				catch (IndexOutOfRangeException e) { Console.WriteLine($"{e.Message}"); }
			}
		}
		public double z {
			get {
				try {return components[2];}
				catch (IndexOutOfRangeException e) { Console.WriteLine($"{e.Message}"); }
				return 0;
			}
			set {
				try {components[2] = value;}
				catch (IndexOutOfRangeException e) { Console.WriteLine($"{e.Message}"); }
			}
		}
		public int dimensions {get{return this.components.Length;}}
		public double magnitude {get{
			double mag = 0;
			foreach (double d in this.components) mag += d*d;
			return Math.Sqrt(mag);
		}}


	//
	//	CONSTRUCTOR
	//
		public Vector (params double[] _components) {
			this.components = _components;
		}


	////////////////////////////////////////////////////////////////////////////////////////////////////
	//
	//	FUNCTIONS
	//
	////////////////////////////////////////////////////////////////////////////////////////////////////

	// Add
		public void Add (Vector other) {
			int c = Math.Min(this.components.Length, other.components.Length);
			for (int i = 0; i < c; i++) this.components[i] += other.components[i];
		}
		public void Add (params double[] components) {
			int c = Math.Min(this.components.Length, components.Length);
			for (int i = 0; i < c; i++) this.components[i] += components[i];
		}
		public Vector Added (Vector other) {
			Vector v = new Vector(this.components);
			int c = Math.Min(this.components.Length, other.components.Length);
			for (int i = 0; i < c; i++) v.components[i] += other.components[i];
			return v;
		}
		public Vector Added (params double[] components) {
			Vector v = new Vector(this.components);
			int c = Math.Min(this.components.Length, components.Length);
			for (int i = 0; i < c; i++) v.components[i] += components[i];
			return v;
		}

	//Sum
		public static Vector Combine (params Vector[] vectors) {
			for (int i = 1; i < vectors.Length; i++) vectors[0].Add(vectors[i]);
			return vectors[0];
		}

	//Scale
		public void Scale (double scaleFactor) {
			for (int i = 0; i < this.components.Length; i++) this.components[i] *= scaleFactor;
		}
		public void Shrink (double shrinkFactor) {
			shrinkFactor = shrinkFactor == 0 ? 0 : 1/shrinkFactor;
			for (int i = 0; i < this.components.Length; i++) this.components[i] *= shrinkFactor;
		}
		public void Scale (Vector other) {
			int c = Math.Min(this.components.Length, other.components.Length);
			for (int i = 0; i < c; i++) this.components[i] *= other.components[i];
		}
		public void Scale (params double[] components) {
			int c = Math.Min(this.components.Length, components.Length);
			for (int i = 0; i < c; i++) this.components[i] *= components[i];
		}
		
		public Vector Scaled (double scaleFactor) {
			Vector v = new Vector(this.components);
			for (int i = 0; i < v.components.Length; i++) v.components[i] *= scaleFactor;
			return v;
		}
		public Vector Shrunk (double shrinkFactor) {
			shrinkFactor = shrinkFactor == 0 ? 0 : 1/shrinkFactor;
			Vector v = new Vector(this.components);
			for (int i = 0; i < v.components.Length; i++) v.components[i] *= shrinkFactor;
			return v;
		}
		public Vector Scaled (Vector other) {
			Vector v = new Vector(this.components);
			int c = Math.Min(this.components.Length, other.components.Length);
			for (int i = 0; i < c; i++) v.components[i] *= other.components[i];
			return v;
		}
		public Vector Scaled (params double[] components) {
			Vector v = new Vector(this.components);
			int c = Math.Min(this.components.Length, components.Length);
			for (int i = 0; i < c; i++) v.components[i] *= components[i];
			return v;
		}

	//Round
		public void Round (double rounding) {
			for (int i = 0; i < this.components.Length; i++) this.components[i] = Math.Round(this.components[i] / rounding) * rounding;
		}
		public Vector Rounded (double rounding) {
			Vector v = new Vector(this.components);
			for (int i = 0; i < v.components.Length; i++) v.components[i] = Math.Round(v.components[i] / rounding) * rounding;
			return v;
		}

	//Normalize
		public void Normalize () {
			double shrinkFactor = 1/this.magnitude;
			for (int i = 0; i < this.components.Length; i++) this.components[i] *= shrinkFactor;
		}
		public Vector Normalized () {
			double shrinkFactor = 1/this.magnitude;
			Vector v = new Vector(this.components);
			for (int i = 0; i < this.components.Length; i++) v.components[i] *= shrinkFactor;
			return v;
		}

	/*
	//Cross product
		public static Vector3 Cross (Vector3 v1, Vector3 v2) {
			return new Vector3 (
				(v1._y * v2._z) - (v1._z * v2._y),
				(v1._z * v2._x) - (v1._x * v2._z),
				(v1._x * v2._y) - (v1._y * v2._x)
			);
		}

	//Dot product
		public static double Dot (Vector3 v1, Vector3 v2) {
			return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
		}

	//Direction to/from
		public Vector3 DirectionTo (Vector3 destination) {
			return new Vector3 (
				destination._x - this._x,
				destination._y - this._y,
				destination._z - this._z
			);
		}
		public Vector3 DirectionFrom (Vector3 origin) {
			return new Vector3 (
				this._x - origin._x,
				this._y - origin._y,
				this._z - origin._z
			);
		}
		public static Vector3 Direction (Vector3 origin, Vector3 destination) {
			return new Vector3 (
				destination._x - origin._x,
				destination._y - origin._y,
				destination._z - origin._z
			);
		}
	*/

	////////////////////////////////////////////////////////////////////////////////////////////////////

		public override string ToString () {
			string s = "(";
			if (this.components.Length > 0) {
				s += this.components[0];
				for (int i = 1; i < this.components.Length; i++) {
					s += $", {this.components[i]}";
				}
			}
			s += ")";
			return s;
		}

	//THANKS A.I.
		public bool Equals (Vector other) => Equals(other, Double.Epsilon);

		public bool Equals (Vector other, double epsilon) {
			if (ReferenceEquals(this.components, other.components)) return true; // same reference or both null
			if (this.dimensions != other.dimensions) return false;
		// Use absolute difference; consider relative comparison if needed.
			for (int i = 0; i < this.dimensions; i++) {
				if (Math.Abs(this.components[i] - other.components[i]) > epsilon) return false;
			}
			return true;
		}

		public override bool Equals (object? obj) => obj is Vector other && Equals(other);

		public override int GetHashCode () {
			if (this.components is null) return 0;
			// Combine a sample of elements for performance on large arrays while still using contents.
			// Here we combine all elements; for large arrays consider sampling.
			var hc = new HashCode();
			foreach (var v in this.components) hc.Add(BitConverter.DoubleToUInt64Bits(v)); // use bit pattern to account for +0/-0 consistency
			return hc.ToHashCode();
		}

		public static bool operator ==(Vector left, Vector right) => left.Equals(right);
		public static bool operator !=(Vector left, Vector right) => !left.Equals(right);

	}
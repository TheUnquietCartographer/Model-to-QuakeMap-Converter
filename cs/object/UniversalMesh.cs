using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DoubleExtensions;

	public class UniversalMesh {

	//
	//	FACE CLASS
	//
		public class Face {
		//PROPERTIES
			public int[] vertexIndices {get; private set;}
			public int[] vertexNormalIndices {get; private set;}
			public int[] textureVertexIndices {get; private set;}
			public int[] materialIndices {get; private set;}
		//CONSTRUCTOR
			public Face (int[] _vertexIndices, int[] _vertexNormalIndices, int[] _textureVertexIndices, int[] _materialIndices) {
				this.vertexIndices = _vertexIndices;
				this.vertexNormalIndices = _vertexNormalIndices;
				this.textureVertexIndices = _textureVertexIndices;
				this.materialIndices = _materialIndices;
			}
		//FUNCTIONS
			public bool IsDuplicateOf (in Face other) {
				if (this.vertexIndices.SequenceEqual(other.vertexIndices)
				&&	this.vertexNormalIndices.SequenceEqual(other.vertexNormalIndices)
				&&	this.textureVertexIndices.SequenceEqual(other.textureVertexIndices)
				&&	this.materialIndices.SequenceEqual(other.materialIndices)
				) return true;
				return false;
			}
			public void ReassignVertexIndices (in Dictionary<int, int> _changeVertexIndices) {
				this.vertexIndices = ReassignIndices(this.vertexIndices, _changeVertexIndices);
			}
			public void ReassignVertexNormalIndices (in Dictionary<int, int> _changeVertexNormalIndices) {
				this.vertexNormalIndices = ReassignIndices(this.vertexNormalIndices, _changeVertexNormalIndices);
			}
			public void ReassignTextureVertexIndices (in Dictionary<int, int> _changeTextureVertexIndices) {
				this.textureVertexIndices = ReassignIndices(this.textureVertexIndices, _changeTextureVertexIndices);
			}
			public void ReassignMaterialIndices (in Dictionary<int, int> _changeMaterialIndices) {
				this.materialIndices = ReassignIndices(this.materialIndices, _changeMaterialIndices);
			}
			private int[] ReassignIndices (int[] _oldIndices, Dictionary<int, int> _changeIndices) {
				int[] newIndices_ = new int[_oldIndices.Length];
				for (int i = 0; i < _oldIndices.Length; i++) {
					if (_changeIndices.TryGetValue(_oldIndices[i], out int newValue)) {
						newIndices_[i] = newValue;
					}
					else {
						newIndices_[i] = _oldIndices[i];
					}
				}
				return newIndices_;
			}
		//GETTERS
			public int vertexCount {get{return this.vertexIndices.Length;}}
			public int triangleCount {get{return this.vertexIndices.Length-2;}}

		//FUNCTIONS
			public override string ToString () {
				string s = "";
				int max = Math.Max(vertexIndices.Length, Math.Max(vertexNormalIndices.Length, textureVertexIndices.Length));
				for (int i = 0; i < max; i++) {
					s += (i < vertexIndices.Length) ? $"{vertexIndices[i]}/" : "-/";
					s += (i < vertexNormalIndices.Length) ? $"{vertexNormalIndices[i]}/" : "-/";
					s += (i < textureVertexIndices.Length) ? $"{textureVertexIndices[i]} " : "- ";
				}
				for (int i = 0; i < materialIndices.Length; i++) {
					s += $"{materialIndices[i]} ";
				}
				return s;
			}
		//End Face class
		}

	//
	//	PROPERTIES
	//
		public Vector3[] vertices {get; private set;}
		public Vector3[] vertexNormals {get; private set;}
		public Vector2[] textureVertices {get; private set;}
		public string[] materials {get; private set;}
		public Face[] faces {get; private set;}

		private bool usesTexels_;
		public bool usesTexels {get{return usesTexels_;}}
		public bool usesNormalizedUVs {get{return !usesTexels_;}}

		public int vertexCount {get{return this.vertices.Length;}}
		public int materialsCount {get{return this.materials.Length;}}
		public int facesCount {get{return this.faces.Length;}}

	//
	//	CONSTRUCTORS
	//
		public UniversalMesh (
			IEnumerable<Vector3> _vertices,
			IEnumerable<Vector3> _vertexNormals,
			IEnumerable<Vector2> _textureVertices,
			IEnumerable<string> _materials,
			IEnumerable<Face> _faces,
			bool usesNormalizedUVs
		) {
			this.vertices = _vertices.ToArray();
			this.vertexNormals = _vertexNormals.ToArray();
			this.textureVertices = _textureVertices.ToArray();
			this.materials = _materials.ToArray();
			this.faces = _faces.ToArray();
			this.usesTexels_ = !usesNormalizedUVs;
		}

	//CREATE A CONDENSED MESH
	// * All duplicate values are eliminated.
	// * Faces are modified to point to the non-duplicate vertices/normals/etc.
		public static UniversalMesh Condensed (
			IEnumerable<Vector3> _vertices,
			IEnumerable<Vector3> _vertexNormals,
			IEnumerable<Vector2> _textureVertices,
			IEnumerable<string> _materials,
			IEnumerable<Face> _faces,
			bool usesNormalizedUVs,
			bool removeDuplicateFaces
		) {
			return Condensed(
				_vertices.ToList(),
				_vertexNormals.ToList(),
				_textureVertices.ToList(),
				_materials.ToList(),
				_faces.ToList(),
				usesNormalizedUVs,
				removeDuplicateFaces
			);
		}
		public static UniversalMesh Condensed (
			List<Vector3> _vertices,
			List<Vector3> _vertexNormals,
			List<Vector2> _textureVertices,
			List<string> _materials,
			List<Face> _faces,
			bool usesNormalizedUVs,
			bool removeDuplicateFaces
		) {
		//Function to condense input list
		// * Eliminates duplicate values from the input list.
		// * Creates a dictionary for the list indices that have been eliminated, and the list indices that take their place.
			Dictionary<int, int> Condense<T> (ref List<T> _listParam) {
				Dictionary<int, int> changeIndices = new Dictionary<int, int>();
				List<T> newList = new List<T> {_listParam[0]};
				for (int i = 1; i < _listParam.Count; i++) {
					bool duplicateFound = false;
					for (int j = 0; j < newList.Count; j++) if (EqualityComparer<T>.Default.Equals(newList[j], _listParam[i])) {
							changeIndices.Add(i, j);
							duplicateFound = true;
							break;
					}
					if (!duplicateFound) {
						changeIndices.Add(i, newList.Count);
						newList.Add(_listParam[i]);
					}
				}
				_listParam = newList;
				return changeIndices;
			}
		//Condense vertices/vertex normals/texture vertices
			Dictionary<int, int> changeVertexIndices = Condense(ref _vertices);
			Dictionary<int, int> changeVertexNormalIndices = Condense(ref _vertexNormals);
			Dictionary<int, int> changeTextureVertexIndices = Condense(ref _textureVertices);
			Dictionary<int, int> changeMaterialIndices = Condense(ref _materials);
		//ITERATE FACES
		//Remove duplicate faces (expensive and potentially destructive)
			if (removeDuplicateFaces) for (int i = 0; i < _faces.Count; i++) {
					Face face_i = _faces[i];
					bool duplicateRemoved = false;
					for (int h = i-1; h > -1; h--) if (face_i.IsDuplicateOf(_faces[h])) {
						_faces.RemoveAt(i);
						duplicateRemoved = true;
						break;
					}
					if (duplicateRemoved) {
						Console.WriteLine($"Face removed");
						i--;
						continue;
					}
					face_i.ReassignVertexIndices(in changeVertexIndices);
					face_i.ReassignVertexNormalIndices(in changeVertexNormalIndices);
					face_i.ReassignTextureVertexIndices(in changeTextureVertexIndices);
					face_i.ReassignMaterialIndices(in changeMaterialIndices);
			}
		//Don't remove duplicate faces
			else for (int i = 0; i < _faces.Count; i++) {
					Face face_i = _faces[i];
					face_i.ReassignVertexIndices(in changeVertexIndices);
					face_i.ReassignVertexNormalIndices(in changeVertexNormalIndices);
					face_i.ReassignTextureVertexIndices(in changeTextureVertexIndices);
					face_i.ReassignMaterialIndices(in changeMaterialIndices);
			}
		//SET PROPERTIES
			return new UniversalMesh (
				_vertices,
				_vertexNormals,
				_textureVertices,
				_materials,
				_faces,
				usesNormalizedUVs
			);
		}

	//CREATE A SCALED (AND OPTIONALLY VERTEX-ROUNDED) MESH
		public UniversalMesh Scaled (double scaleFactor) {
			return new UniversalMesh(
				this.vertices.Select(v => v.Scaled(scaleFactor)),
				this.vertexNormals,
				this.textureVertices,
				this.materials,
				this.faces,
				this.usesNormalizedUVs
			);
		}
		public UniversalMesh Scaled (double scaleFactor, double rounding) {
			return new UniversalMesh(
				this.vertices.Select(v => v.Scaled(scaleFactor).Rounded(rounding)),
				this.vertexNormals,
				this.textureVertices,
				this.materials,
				this.faces,
				this.usesNormalizedUVs
			);
		}
		public UniversalMesh Scaled (double scaleFactor, double rounding, bool swapYZCoordinates) {
			return new UniversalMesh(
				(swapYZCoordinates) ? 
					this.vertices.Select(v => 
						new Vector3(
							v.x.Scaled(scaleFactor).Rounded(rounding),
							v.z.Scaled(scaleFactor).Rounded(rounding),
							v.y.Scaled(scaleFactor).Rounded(rounding)
						)
					).ToArray()
					: this.vertices.Select(v => v.Scaled(scaleFactor).Rounded(rounding)).ToArray()
				,
				this.vertexNormals,
				this.textureVertices,
				this.materials,
				this.faces,
				this.usesNormalizedUVs
			);
		}

	//
	//	FUNCTIONS
	//
		public void LogData () {
			Log.Multi(
				new Log.ColorLog("Vertices: ", ConsoleColor.White),
				new Log.ColorLog(this.vertices == null ? "null" : this.vertices.Length.ToString(), ConsoleColor.Yellow),
				new Log.ColorLog("; Vertex Normals: ", ConsoleColor.White),
				new Log.ColorLog(this.vertexNormals == null ? "null" : this.vertexNormals.Length.ToString(), ConsoleColor.DarkYellow),
				new Log.ColorLog("; Texture Vertices: ", ConsoleColor.White),
				new Log.ColorLog(this.textureVertices == null ? "null" : this.textureVertices.Length.ToString(), ConsoleColor.Cyan),
				new Log.ColorLog("; Materials: ", ConsoleColor.White),
				new Log.ColorLog(this.materials == null ? "null" : this.materials.Length.ToString(), ConsoleColor.Cyan),
				new Log.ColorLog("; Faces: ", ConsoleColor.White),
				new Log.ColorLog(this.faces == null ? "null" : this.faces.Length.ToString(), ConsoleColor.Magenta)
			);
		}

		public void WriteToFile (string pathToFile) {
			string
				str_vertices = "VERTICES:\r\n",
				str_vertexNormals = "VERTEX NORMALS:\r\n",
				str_textureVertices = "TEXTURE VERTICES:\r\n",
				str_materials = "MATERIALS:\r\n",
				str_faces = "FACES:\r\n"
			;
			for (int i = 0; i < this.vertices.Length; i++) {
				str_vertices += $"{i}: {this.vertices[i]}\r\n";
			}
			for (int i = 0; i < this.vertexNormals.Length; i++) {
				str_vertexNormals += $"{i}: {this.vertexNormals[i]}\r\n";
			}
			for (int i = 0; i < this.textureVertices.Length; i++) {
				str_textureVertices += $"{i}: {this.textureVertices[i]}\r\n";
			}
			for (int i = 0; i < this.materials.Length; i++) {
				str_materials += $"{i}: {this.materials[i]}\r\n";
			}
			for (int i = 0; i < this.faces.Length; i++) {
				str_faces += $"{i}: {this.faces[i]}\r\n";
			}
			File.WriteAllText(pathToFile, str_vertices);
			File.AppendAllText(pathToFile, str_vertexNormals);
			File.AppendAllText(pathToFile, str_textureVertices);
			File.AppendAllText(pathToFile, str_materials);
			File.AppendAllText(pathToFile, str_faces);
		}



	////////////////////////////////////////////////////////////////////////////////////////////////////
		public struct Edge {
			public int vertIndex;
			public Vector3 vector;
			public Edge (int vertexIndex, Vector3 vector) {
				this.vertIndex = vertexIndex;
				this.vector = vector;
			}
			public override string ToString () {return $"{this.vertIndex}: {this.vector.ToString()}";}
		}
		public List<Edge> GetEdges (Face _face) {
			List<Edge> edges = new List<Edge>(_face.vertexCount);
			int countMinusOne = _face.vertexCount - 1;
			for (int i = 0; i < countMinusOne; i++) {
				edges.Add(new Edge(_face.vertexIndices[i], Vector3.Vector(
					this.vertices[_face.vertexIndices[i]],
					this.vertices[_face.vertexIndices[i+1]]
				)));
			}
			edges.Add(new Edge(_face.vertexIndices[countMinusOne], Vector3.Vector(
				this.vertices[_face.vertexIndices[countMinusOne]],
				this.vertices[_face.vertexIndices[0]]
			)));
			return edges;
		}
	
		public Face[] GetTriangles (Face _face) {
		//STORE EDGES
			List<Edge> edges = this.GetEdges(_face);
		//TRIANGLIFY
			List<Face> triangles = new List<Face>(_face.triangleCount);
			int i = 0;
			while (edges.Count >= 3) {
				if (i >= edges.Count) i -= edges.Count;
				int j = i+1; if (j >= edges.Count) j -= edges.Count;
				int k = i+2; if (k >= edges.Count) k -= edges.Count;
			//Get a vector that is perpendicular to this edge and lies on the same plane as all the edges (hopefully)
				Vector3 cross = Vector3.Cross(edges[i].vector, edges[j].vector);
				Vector3 perpendicular = Vector3.Cross(cross, edges[i].vector);
				double dot = Vector3.Dot(perpendicular, edges[j].vector);
			//Iterate through pairs of edges to identify whether they encapsulate a convex part of the mesh.
				switch (dot) {
				//Angle is acute, these edges form a valid triangle
					case > 0:
					//Find where the edge vertIndices appear in the original face
						int indexOfFirstVertIndex = _face.vertexIndices.IndexOf(edges[i].vertIndex);
						int indexOfSecondVertIndex = _face.vertexIndices.IndexOf(edges[j].vertIndex);
						int indexOfThirdVertIndex = _face.vertexIndices.IndexOf(edges[k].vertIndex);
					//Add a new triangle
						triangles.Add(new Face(
							new int[] {_face.vertexIndices[indexOfFirstVertIndex], _face.vertexIndices[indexOfSecondVertIndex], _face.vertexIndices[indexOfThirdVertIndex]},
							new int[] {_face.vertexNormalIndices[indexOfFirstVertIndex], _face.vertexNormalIndices[indexOfSecondVertIndex], _face.vertexNormalIndices[indexOfThirdVertIndex]},
							new int[] {_face.textureVertexIndices[indexOfFirstVertIndex], _face.textureVertexIndices[indexOfSecondVertIndex], _face.textureVertexIndices[indexOfThirdVertIndex]},
							_face.materialIndices //Cloning unnecessary; will reference the same array though.
						));
					//Remove edges i & j.
					//Add a new edge that effectively cuts this triangle off from the face.
						edges[i] = new Edge(edges[i].vertIndex, Vector3.Vector(
							this.vertices[edges[i].vertIndex],
							this.vertices[edges[k].vertIndex]
						));
						edges.RemoveAt(j);
						break;
				//Edges are parallel, combine into one
					case 0:
						edges[i].vector.Add(edges[j].vector);
						edges.RemoveAt(j);
						break;
				//Angle is obtuse, triangle is invalid
					case < 0:
						break;
				}
				i++;
			}
			return triangles.ToArray();
		}

		//Vertex normals do not necessarily have any relation to the normal vector of the face, i.e. the one that is perpendicular to the plane.
		//As such, it is much safer to calculate the face normal from the cross product of two of its edges (or all of them, in this case).
		public Vector3 GetTrueFaceNormal (Face _face) {
			Vector3 prevSide, thisSide;
			Vector3 normal = new Vector3(0,0,0);
			prevSide = Vector3.Vector(this.vertices[_face.vertexIndices[0]], this.vertices[_face.vertexIndices[1]]);
			for (int i = 1; i < _face.vertexCount; i++) {
				int j = i+1;
				if (j == _face.vertexCount) j = 0;
				thisSide = Vector3.Vector(this.vertices[_face.vertexIndices[i]], this.vertices[_face.vertexIndices[j]]);
				normal.Add(Vector3.Cross(prevSide, thisSide));
				prevSide = thisSide;
			}
			normal.Normalize();
			return normal;
		}

		public Vector3 GetTriangleNormal (Face _triangle) {
			return Vector3.Cross(
				Vector3.Vector(this.vertices[_triangle.vertexIndices[0]], this.vertices[_triangle.vertexIndices[1]]),
				Vector3.Vector(this.vertices[_triangle.vertexIndices[1]], this.vertices[_triangle.vertexIndices[2]])
			).normalized;
		}

	}
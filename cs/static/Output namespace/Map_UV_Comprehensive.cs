using System;
using Maff;
using DoubleExtensions;
using System.Linq;

namespace Output {
	
	internal static class Map_UV_Comprehensive {

		public static string TranslateUVToMap (UniversalMesh _mesh, UniversalMesh.Face _face, int dbg_faceIndex, Map.Format _format, bool bakeAxes = false) {

		//TRANSFORM FACE VERTICES INTO 2D SPACE
			//Facing       Cross        Right vector	//Facing       Cross        Down vector
			//(-1, 0, 0) X (0, 0, -1) = (0, -1, 0)		//(-1, 0, 0) X (0, 1, 0)  = (0, 0, -1)
			//(1, 0, 0)  X (0, 0, -1) = (0, 1, 0)		//(1, 0, 0)  X (0, -1, 0) = (0, 0, -1)
			//(0, -1, 0) X (0, 0, -1) = (1, 0, 0)		//(0, -1, 0) X (-1, 0, 0) = (0, 0, -1)
			//(0, 1, 0)  X (0, 0, -1) = (-1, 0, 0)		//(0, 1, 0)  X (1, 0, 0)  = (0, 0, -1)
			//(0, 0, -1) X (0, -1, 0) = (-1, 0, 0)		//(0, 0, -1) X (-1, 0, 0) = (0, 1, 0)
			//(0, 0, 1)  X (0, -1, 0) = (1, 0, 0)		//(0, 0, 1)  X (-1, 0, 0) = (0, -1, 0)

			Vector3 _faceNormal = _mesh.GetTrueFaceNormal(_face); //Returns a normalized vector. 
			Vector3 dominantFacing = _faceNormal.Signed(1);
			Vector3 arbitraryTextureAxis_right, arbitraryTextureAxis_down;

		//Get face axes
		//If the face is on an angle of 45 degrees the priority goes z -> y -> x.
			if (dominantFacing.z != 0) {
				if (dominantFacing.z == 1) arbitraryTextureAxis_down = Vector3.Cross(_faceNormal, Vector3.left);
				else arbitraryTextureAxis_down = Vector3.Cross(_faceNormal, Vector3.right);
				arbitraryTextureAxis_right = Vector3.Cross(_faceNormal, arbitraryTextureAxis_down);
			}
			else if (dominantFacing.y != 0) {
				if (dominantFacing.y == 1) arbitraryTextureAxis_down = Vector3.Cross(_faceNormal, Vector3.right);
				else arbitraryTextureAxis_down = Vector3.Cross(_faceNormal, Vector3.left);
				arbitraryTextureAxis_right = Vector3.Cross(_faceNormal, arbitraryTextureAxis_down);
			}
			else if (dominantFacing.x != 0) {
				if (dominantFacing.x == 1) arbitraryTextureAxis_down = Vector3.Cross(_faceNormal, Vector3.back);
				else arbitraryTextureAxis_down = Vector3.Cross(_faceNormal, Vector3.forward);
				arbitraryTextureAxis_right = Vector3.Cross(_faceNormal, arbitraryTextureAxis_down);
			}
			else {
				return Map.DefaultTextureSettings(_format, _faceNormal);
			}

		//Transform vertices into 2D space
			Vector2[] _vertices_2D = new Vector2[_face.vertexCount];
			{
				Vector3 origin = _mesh.vertices[_face.vertexIndices[0]];
				for (int i = 0; i < _vertices_2D.Length; i++) {
					Vector3 originToVertex = origin.VectorTo(_mesh.vertices[_face.vertexIndices[i]]);
					_vertices_2D[i] = new Vector2(
						Vector3.Dot(originToVertex, arbitraryTextureAxis_right),
						Vector3.Dot(originToVertex, arbitraryTextureAxis_down)
					);
				}
			}

		//
		// * v_i and uv_i are the origin vertex/texture vertex respectively
			int i_ = -1, j_ = -1, k_ = -1;
			Vector2 v_i = Vector2.zero, v_ij = Vector2.zero, v_ik = Vector2.zero;
			double determinant = 0;
			void LogError_ZeroVector(int index1, int index2) {
				Log.Single($"{dbg_faceIndex}: Vertices {index1} {_vertices_2D[index1]} & {index2} {_vertices_2D[index2]} produce a zero vector.", ConsoleColor.Red);
			}
			for (i_ = 0; i_ < _vertices_2D.Length; i_++) {
				/*Vector2*/ v_i = _vertices_2D[i_];
				for (j_ = i_+1; j_ < _vertices_2D.Length; j_++) {
					Vector2 v_j = _vertices_2D[j_];
					/*Vector2*/ v_ij = Vector2.Vector(v_i, v_j);
					if (v_ij == Vector2.zero) {LogError_ZeroVector(i_, j_); continue;}
					for (k_ = j_+1; k_ < _vertices_2D.Length; k_++) {
						Vector2 v_k = _vertices_2D[k_];
						/*Vector2*/ v_ik = Vector2.Vector(v_i, v_k);
						if (v_ik == Vector2.zero) {LogError_ZeroVector(i_, k_); continue;}
						Vector2 v_jk = Vector2.Vector(v_j, v_k);
						if (v_jk == Vector2.zero) {LogError_ZeroVector(j_, k_); continue;}
						determinant = Vector2.Determinant(v_ij, v_ik);
						if (Math.Abs(determinant) < 1e-6d) {
							Log.Single($"{dbg_faceIndex}: Degenerate triangle for vertices {i_} {j_} {k_} {v_i} {v_j} {v_k}", ConsoleColor.Red);
							continue;
						}
						goto NEXT;
					}
				}
			}
			NEXT:
			Vector2 uv_i = _mesh.textureVertices[_face.textureVertexIndices[i_]];
			Vector2 uv_ij = Vector2.Vector(uv_i, _mesh.textureVertices[_face.textureVertexIndices[j_]]);
			Vector2 uv_ik = Vector2.Vector(uv_i, _mesh.textureVertices[_face.textureVertexIndices[k_]]);

		// Solve using inverse
			double inverseDeterminant = 1d / determinant;
			double A = ( uv_ij.x * v_ik.y - uv_ik.x * v_ij.y) * inverseDeterminant;
			double B = (-uv_ij.x * v_ik.x + uv_ik.x * v_ij.x) * inverseDeterminant;
			double C = ( uv_ij.y * v_ik.y - uv_ik.y * v_ij.y) * inverseDeterminant;
			double D = (-uv_ij.y * v_ik.x + uv_ik.y * v_ij.x) * inverseDeterminant;

			double uShift, vShift, scaleX, scaleY, rotation;

			switch (_format) {
				default:
				case Map.Format.Q1:
				//BASICALLY WORKS
				// Shift
					uShift = uv_i.x - (A * v_i.x + B * v_i.y);
					vShift = uv_i.y - (C * v_i.x + D * v_i.y);
				// Extract scales
					scaleX = Math.Sqrt(A*A + C*C);
					scaleY = Math.Sqrt(B*B + D*D);
				// Rotation (optional)
					rotation = Math.Atan2(C, A) * MaffConst.radToDeg;
				/*
				//SUPPOSED TO REMOVE SHEAR, BUT PRODUCES MORE NaNs
				//Orthogonalize (remove shear)
					Vector2 U = new Vector2(A, C);
					Vector2 V = new Vector2(B, D);
					scaleX = U.magnitude;
					U.Normalize();         //Normalize U
					double dot = Vector2.Dot(U, V);
					V.Add(U.Scaled(-dot)); //Orthogonalize V
					scaleY = V.magnitude;
					if (scaleY < 1e-8) {   //Handle NaN
						V = new Vector2(-U.y, U.x);
						scaleY = 0;
					}
					else V.Normalize();    //Normalize V
				//Rotation
					rotation = Math.Atan2(U.y, U.x) * MaffConst.radToDeg;
					rotation.Round(1);
				//Projection distortion compensation
					Vector3 uAxis_ = arbitraryTextureAxis_right.Added(_faceNormal.Scaled(-Vector3.Dot(arbitraryTextureAxis_right, _faceNormal)));
					Vector3 vAxis_ = arbitraryTextureAxis_down.Added(_faceNormal.Scaled(-Vector3.Dot(arbitraryTextureAxis_down, _faceNormal)));
					if (uAxis_.magnitude > 1e-6d) scaleX *= 1/uAxis_.magnitude;
					if (vAxis_.magnitude > 1e-6d) scaleY *= 1/vAxis_.magnitude;
				//Rescale axes
					U.Scale(scaleX);
					V.Scale(scaleY);
				//Shift
					uShift = uv_i.x - (U.x * v_i.x + U.y * v_i.y);
					vShift = uv_i.y - (V.x * v_i.x + V.y * v_i.y);
				*/
					return $"{uShift} {vShift} {rotation} {scaleX} {scaleY}";
				case Map.Format.Valve220: 
					Vector3 uAxis = arbitraryTextureAxis_right.Scaled(A).Added(arbitraryTextureAxis_down.Scaled(B));
					Vector3 vAxis = arbitraryTextureAxis_right.Scaled(C).Added(arbitraryTextureAxis_down.Scaled(D));
					if (uAxis.isNaN) {
						Console.WriteLine($"{dbg_faceIndex}: UAxis has NaN components.");
						return Map.DefaultTextureSettings(_format, _faceNormal);
					}
					if (vAxis.isNaN) {
						Console.WriteLine($"{dbg_faceIndex}: VAxis has NaN components.");
						return Map.DefaultTextureSettings(_format, _faceNormal);
					}
					Vector3 worldOrigin = _mesh.vertices[_face.vertexIndices[i_]];
					uShift = uv_i.x - Vector3.Dot(uAxis, worldOrigin);
					vShift = uv_i.y - Vector3.Dot(vAxis, worldOrigin);
					if (bakeAxes) {
						scaleX = 1;
						scaleY = 1;
					}
					else {
						scaleX = uAxis.magnitude;
						scaleY = vAxis.magnitude;
						uAxis.Normalize();
						vAxis.Normalize();
						if (uAxis.isNaN) {
							Log.Single($"{dbg_faceIndex}: UAxis has NaN components.", ConsoleColor.Red);
							return Map.DefaultTextureSettings(_format, _faceNormal);
						}
						if (vAxis.isNaN) {
							Log.Single($"{dbg_faceIndex}: VAxis has NaN components.", ConsoleColor.Red);
							return Map.DefaultTextureSettings(_format, _faceNormal);
						}
					}
					rotation = 0;
					return 
						$"[ {uAxis.x} {uAxis.y} {uAxis.z} {uShift} ]"
						+$" [ {vAxis.x} {vAxis.y} {vAxis.z} {vShift} ]"
						+$" {rotation} {scaleX} {scaleY}"
					;
			//End switch format
			}

		//End TranslateUVToMap()
		}

	}

}
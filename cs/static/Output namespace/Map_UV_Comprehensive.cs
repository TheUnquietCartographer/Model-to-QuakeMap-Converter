using System;
using Maff;

namespace Output {
	
	internal static class Map_UV_Comprehensive {

		public static string TranslateUVToMap (UniversalMesh _mesh, int faceIndex, Map.Format _format, bool bakeAxes = false) {

		//TRANSFORM FACE VERTICES INTO 2D SPACE
			//Facing       Cross        Right vector	//Facing       Cross        Down vector
			//(-1, 0, 0) X (0, 0, -1) = (0, -1, 0)		//(-1, 0, 0) X (0, 1, 0)  = (0, 0, -1)
			//(1, 0, 0)  X (0, 0, -1) = (0, 1, 0)		//(1, 0, 0)  X (0, -1, 0) = (0, 0, -1)
			//(0, -1, 0) X (0, 0, -1) = (1, 0, 0)		//(0, -1, 0) X (-1, 0, 0) = (0, 0, -1)
			//(0, 1, 0)  X (0, 0, -1) = (-1, 0, 0)		//(0, 1, 0)  X (1, 0, 0)  = (0, 0, -1)
			//(0, 0, -1) X (0, -1, 0) = (-1, 0, 0)		//(0, 0, -1) X (-1, 0, 0) = (0, 1, 0)
			//(0, 0, 1)  X (0, -1, 0) = (1, 0, 0)		//(0, 0, 1)  X (-1, 0, 0) = (0, -1, 0)

			UniversalMesh.Face _face = _mesh.faces[faceIndex];
			Vector3 _faceNormal = _mesh.GetTrueFaceNormal(faceIndex); //Returns a normalized vector. 
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

			//I believe this to be correct ^^^








		//Pick an origin from which to base further calculations
			Vector2 v_origin = _vertices_2D[0];
			Vector2 uv_origin = _mesh.textureVertices[_face.textureVertexIndices[0]];

		// Build system using two edges
			Vector2 dv1 = Vector2.Vector(v_origin, _vertices_2D[1]);
			Vector2 dv2 = Vector2.Vector(v_origin, _vertices_2D[2]);
			Vector2 duv1 = Vector2.Vector(uv_origin, _mesh.textureVertices[_face.textureVertexIndices[1]]);
			Vector2 duv2 = Vector2.Vector(uv_origin, _mesh.textureVertices[_face.textureVertexIndices[2]]);




		//Something something relying on triangles when we are getting the settings for the whole face.
		// We already know that triangles might be degen, so we need to be able to get one that isn't.
		//Should we pass in the triangle array gotten with Face.GetTriangles()?


		// Solve linear system for matrix [A B; C D]
			double determinant = dv1.x * dv2.y - dv1.y * dv2.x;	//Signed area of the parallelogram formed by dv1 and dv2
			if (Math.Abs(determinant) < 1e-8d) {
				Console.WriteLine($"{faceIndex}: Degenerate triangle");
				return Map.DefaultTextureSettings(_format, _faceNormal);
			}




			double inverseDeterminant = 1d / determinant;

		// Solve using inverse
			double A = ( duv1.x * dv2.y - duv2.x * dv1.y) * inverseDeterminant;
			double B = (-duv1.x * dv2.x + duv2.x * dv1.x) * inverseDeterminant;
			double C = ( duv1.y * dv2.y - duv2.y * dv1.y) * inverseDeterminant;
			double D = (-duv1.y * dv2.x + duv2.y * dv1.x) * inverseDeterminant;

		// Shift
			double uShift = uv_origin.x - (A * v_origin.x + B * v_origin.y);
			double vShift = uv_origin.y - (C * v_origin.x + D * v_origin.y);
			double scaleX, scaleY, rotation;

		//BRANCH 1 //Is this for original quake 1 format????
			switch (_format) {
				default:
				case Map.Format.Q1:		
				// Extract scales
					scaleX = Math.Sqrt(A*A + C*C);
					scaleY = Math.Sqrt(B*B + D*D);
				// Rotation (optional)
					rotation = Math.Atan2(C, A) * MaffConst.radToDeg;
					return $"{uShift} {vShift} {rotation} {scaleX} {scaleY}";
			//BRANCH 2 (Valve220 format)
				case Map.Format.Valve220: 
					Vector3 uAxis = arbitraryTextureAxis_right.Scaled(A).Added(arbitraryTextureAxis_down.Scaled(B));
					Vector3 vAxis = arbitraryTextureAxis_right.Scaled(C).Added(arbitraryTextureAxis_down.Scaled(D));
						if (uAxis.isNaN) {
							Console.WriteLine($"{faceIndex}: UAxis has NaN components.");
							return Map.DefaultTextureSettings(_format, _faceNormal);
						}
						if (vAxis.isNaN) {
							Console.WriteLine($"{faceIndex}: VAxis has NaN components.");
							return Map.DefaultTextureSettings(_format, _faceNormal);
						}

bakeAxes = true;

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
							Console.WriteLine($"{faceIndex}: UAxis has NaN components.");
							return Map.DefaultTextureSettings(_format, _faceNormal);
						}
						if (vAxis.isNaN) {
							Console.WriteLine($"{faceIndex}: VAxis has NaN components.");
							return Map.DefaultTextureSettings(_format, _faceNormal);
						}
					}
					rotation = 0;
					return 
						$"[ {uAxis.x} {uAxis.y} {uAxis.z} {uShift} ]"
						+$" [ {vAxis.x} {vAxis.y} {vAxis.z} {vShift} ]"
						+$" {rotation} {scaleX} {scaleY}"
					;
			}






		}

	}

}
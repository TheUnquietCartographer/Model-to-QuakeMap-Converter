using System;
using Maff;

namespace Output {

	//quake1 "0 0 0 1 1" //Xoffset Yoffset rotation Xscale Yscale
	//valve220 "[ 0 -1 0 -0 ] [ 0 0 -1 -0 ] -0 1 1" //[ Tx1 Ty1 Tz1 Toffset1 ] [ Tx2 Ty2 Tz2 Toffset2 ] rotation Xscale Yscale

	internal static class Map_UV_3 {

		public static string TranslateUVToMap (UniversalMesh _mesh, int faceIndex, Map.Format _format, bool bakeAxes = false) {

			UniversalMesh.Face _face = _mesh.faces[faceIndex];



			if (_face.textureVertexIndices[0] >= _mesh.textureVertices.Length) {
				Console.WriteLine($"{faceIndex} (0): Texture vertex index out of range {_face.textureVertexIndices[0]}/{_mesh.textureVertices.Length-1}");
				return Map.DefaultTextureSettings(_format, _mesh.GetTrueFaceNormal(faceIndex));
			}
			if (_face.textureVertexIndices[1] >= _mesh.textureVertices.Length) {
				Console.WriteLine($"{faceIndex} (1): Texture vertex index out of range {_face.textureVertexIndices[1]}/{_mesh.textureVertices.Length-1}");
				return Map.DefaultTextureSettings(_format, _mesh.GetTrueFaceNormal(faceIndex));
			}
			if (_face.textureVertexIndices[2] >= _mesh.textureVertices.Length) {
				Console.WriteLine($"{faceIndex} (2): Texture vertex index out of range {_face.textureVertexIndices[2]}/{_mesh.textureVertices.Length-1}");
				return Map.DefaultTextureSettings(_format, _mesh.GetTrueFaceNormal(faceIndex));
			}



			Vector3 N = _mesh.GetTrueFaceNormal(faceIndex);
			Vector3 absN = new Vector3(Math.Abs(N.x), Math.Abs(N.y), Math.Abs(N.z));

		// --- 1. Pick Hammer-style axes ---
			Vector3 uDir, vDir;
		// XY plane
			if (absN.z >= absN.x && absN.z >= absN.y) {
				uDir = new Vector3(0, -1, 0);
				vDir = new Vector3(1,  0, 0);
			}
		// XZ plane
			else if (absN.y >= absN.x) {
				uDir = new Vector3(1, 0, 0);
				vDir = new Vector3(0, 0, -1);
			}
		// YZ plane
			else {
				uDir = new Vector3(0, -1, 0);
				vDir = new Vector3(0, 0, -1);
			}

		// --- 2. Get 3 verts + UVs ---
			Vector3 P0 = _mesh.vertices[_face.vertexIndices[0]];
			Vector3 P1 = _mesh.vertices[_face.vertexIndices[1]];
			Vector3 P2 = _mesh.vertices[_face.vertexIndices[2]];

			Vector2 UV0 = _mesh.textureVertices[_face.textureVertexIndices[0]];
			Vector2 UV1 = _mesh.textureVertices[_face.textureVertexIndices[1]];
			Vector2 UV2 = _mesh.textureVertices[_face.textureVertexIndices[2]];

		// --- 3. Edges ---
			Vector3 E1 = Vector3.Vector(P0, P1);
			Vector3 E2 = Vector3.Vector(P0, P2);

			Vector2 dUV1 = Vector2.Vector(UV0, UV1);
			Vector2 dUV2 = Vector2.Vector(UV0, UV2);

		// --- 4. Project edges onto axes ---
			double du1 = Vector3.Dot(E1, uDir);
			double du2 = Vector3.Dot(E2, uDir);

			double dv1 = Vector3.Dot(E1, vDir);
			double dv2 = Vector3.Dot(E2, vDir);

		// --- 5. Solve scale ---
			double determinant = du1 * dv2 - du2 * dv1;

			if (Math.Abs(determinant) < 1e-8d) {
			//	Console.WriteLine($"{faceIndex}: Degenerate");
				return Map.DefaultTextureSettings(Map.Format.Valve220, N);
			}

			double inverseDeterminant = 1d / determinant;

			double scaleU = (dUV1.x * dv2 - dUV2.x * dv1) * inverseDeterminant;
			double scaleV = (dUV1.y * du2 - dUV2.y * du1) * inverseDeterminant;

		// --- 6. Build axes ---
			Vector3 uAxis = uDir.Scaled(scaleU);
			Vector3 vAxis = vDir.Scaled(scaleV);

		// --- 7. Shift ---
			double uShift = UV0.x - Vector3.Dot(uAxis, P0);
			double vShift = UV0.y - Vector3.Dot(vAxis, P0);

		// --- 8. Output ---
			return 
				$"[ {uAxis.x} {uAxis.y} {uAxis.z} {uShift} ] " +
				$"[ {vAxis.x} {vAxis.y} {vAxis.z} {vShift} ] 0 1 1"
			;
		}

	}
}


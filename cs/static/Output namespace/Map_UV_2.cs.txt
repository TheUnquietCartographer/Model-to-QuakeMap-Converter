using System;
using Maff;

namespace Output {

	//quake1 "0 0 0 1 1" //Xoffset Yoffset rotation Xscale Yscale
	//valve220 "[ 0 -1 0 -0 ] [ 0 0 -1 -0 ] -0 1 1" //[ Tx1 Ty1 Tz1 Toffset1 ] [ Tx2 Ty2 Tz2 Toffset2 ] rotation Xscale Yscale

	internal static class Map_UV_2 {

		public static string TranslateUVToMap (UniversalMesh _mesh, int faceIndex, Map.Format _format, bool bakeAxes = false) {

			UniversalMesh.Face _face = _mesh.faces[faceIndex];

			//Console.WriteLine($"{_mesh.textureVertices.Length} {_face.textureVertexIndices.Length} {_face.textureVertexIndices[0]} {_face.textureVertexIndices[1]} {_face.textureVertexIndices[2]}");
			if (_face.textureVertexIndices[0] > _mesh.textureVertices.Length || _face.textureVertexIndices[1] > _mesh.textureVertices.Length || _face.textureVertexIndices[2] > _mesh.textureVertices.Length) {
				Console.WriteLine($"{faceIndex}: Texture vertex index out of range");
				return Map.DefaultTextureSettings(_format, _mesh.GetTrueFaceNormal(faceIndex));
			}

			Vector3 P0 = _mesh.vertices[_face.vertexIndices[0]];
			Vector3 P1 = _mesh.vertices[_face.vertexIndices[1]];
			Vector3 P2 = _mesh.vertices[_face.vertexIndices[2]];

			Vector2 UV0 = _mesh.textureVertices[_face.textureVertexIndices[0]];
			Vector2 UV1 = _mesh.textureVertices[_face.textureVertexIndices[1]];
			Vector2 UV2 = _mesh.textureVertices[_face.textureVertexIndices[2]];

		// Edges in 3D
			Vector3 E1 = Vector3.Vector(P0, P1);
			Vector3 E2 = Vector3.Vector(P0, P2);

		// UV deltas
			Vector2 dUV1 = Vector2.Vector(UV0, UV1);
			Vector2 dUV2 = Vector2.Vector(UV0, UV2);

			Vector3 N = Vector3.Cross(E1, E2);
			double denom = Vector3.Dot(N, N);

			if (denom < 1e-8d) {
				// degenerate
				return Map.DefaultTextureSettings(_format, _mesh.GetTrueFaceNormal(faceIndex));
			}

		// U axis
			Vector3 uAxis = Vector3.Cross(E2, N).Scaled(dUV1.x).Added(
				Vector3.Cross(N, E1).Scaled(dUV2.x)
			).Shrunk(denom);
	

		// V axis
			Vector3 vAxis = Vector3.Cross(E2, N).Scaled(dUV1.y).Added(
				Vector3.Cross(N, E1).Scaled(dUV2.y)
			).Shrunk(denom);

			double uShift = UV0.x - Vector3.Dot(uAxis, P0);
			double vShift = UV0.y - Vector3.Dot(vAxis, P0);

bakeAxes = false;	//Temp, for testing

			double scaleX, scaleY;
			if (bakeAxes) {
				scaleX = 1;
				scaleY = 1;
			}
			else {
				scaleX = uAxis.magnitude;
				scaleY = vAxis.magnitude;
				uAxis.Normalize();
				vAxis.Normalize();
			}
			double rotation = 0;
			return 
				$"[ {uAxis.x} {uAxis.y} {uAxis.z} {uShift} ]"
				+$" [ {vAxis.x} {vAxis.y} {vAxis.z} {vShift} ]"
				+$" {rotation} {scaleX} {scaleY}"
			;
		}

	}
}


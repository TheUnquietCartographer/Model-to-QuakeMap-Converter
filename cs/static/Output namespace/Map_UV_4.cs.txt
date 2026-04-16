using System;
using Maff;

namespace Output {

	//quake1 "0 0 0 1 1" //Xoffset Yoffset rotation Xscale Yscale
	//valve220 "[ 0 -1 0 -0 ] [ 0 0 -1 -0 ] -0 1 1" //[ Tx1 Ty1 Tz1 Toffset1 ] [ Tx2 Ty2 Tz2 Toffset2 ] rotation Xscale Yscale

	internal static class Map_UV_4 {

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




		// 1. Face normal
			Vector3 N = _mesh.GetTrueFaceNormal(faceIndex).normalized; // ensure unit normal

			// 2. Choose stable tangent (uDir) and bitangent (vDir) from N
			Vector3 tangentBasisCandidate = (Math.Abs(N.x) < 0.9) ? new Vector3(1.0, 0.0, 0.0) : new Vector3(0.0, 1.0, 0.0);
			Vector3 uDir = Vector3.Cross(tangentBasisCandidate, N).normalized;
			// If cross produced (0,0,0) for some reason, fall back
			if (uDir.magnitudeSquared < 1e-12)
			{
				tangentBasisCandidate = new Vector3(0.0, 0.0, 1.0);
				uDir = Vector3.Cross(tangentBasisCandidate, N).normalized;
				if (uDir.magnitudeSquared < 1e-12)
					return Map.DefaultTextureSettings(Map.Format.Valve220, N);
			}
			Vector3 vDir = Vector3.Cross(N, uDir).normalized; // already orthonormal with uDir

			// 3. Get verts + UVs
			Vector3 P0 = _mesh.vertices[_face.vertexIndices[0]];
			Vector3 P1 = _mesh.vertices[_face.vertexIndices[1]];
			Vector3 P2 = _mesh.vertices[_face.vertexIndices[2]];

			Vector2 UV0 = _mesh.textureVertices[_face.textureVertexIndices[0]];
			Vector2 UV1 = _mesh.textureVertices[_face.textureVertexIndices[1]];
			Vector2 UV2 = _mesh.textureVertices[_face.textureVertexIndices[2]];

			// 4. Edges in 3D and UV space
			Vector3 E1 = Vector3.Vector(P0, P1);
			Vector3 E2 = Vector3.Vector(P0, P2);

			Vector2 dUV1 = new Vector2(UV1.x - UV0.x, UV1.y - UV0.y);
			Vector2 dUV2 = new Vector2(UV2.x - UV0.x, UV2.y - UV0.y);

			// Check UV degeneracy
			const double epsUV = 1e-12;
			if (dUV1.magnitudeSquared < epsUV && dUV2.magnitudeSquared < epsUV)
				return Map.DefaultTextureSettings(Map.Format.Valve220, N);

			// 5. Project edges onto tangent basis
			double du1 = Vector3.Dot(E1, uDir);
			double du2 = Vector3.Dot(E2, uDir);
			double dv1 = Vector3.Dot(E1, vDir);
			double dv2 = Vector3.Dot(E2, vDir);

			// 6. Solve 2x2 for scale factors robustly
			double det = du1 * dv2 - du2 * dv1;
			const double detThreshold = 1e-6; // looser threshold to avoid huge inverses
			if (Math.Abs(det) < detThreshold)
			{
				// Face is nearly aligned with one axis in this basis; fallback to default
				return Map.DefaultTextureSettings(Map.Format.Valve220, N);
			}
			double invDet = 1.0 / det;
			double scaleU = (dUV1.x * dv2 - dUV2.x * dv1) * invDet;
			double scaleV = (dUV1.y * du2 - dUV2.y * du1) * invDet;

			// Clamp runaway scales (optional safety)
			const double maxReasonableScale = 1e6;
			if (double.IsInfinity(scaleU) || double.IsNaN(scaleU) || Math.Abs(scaleU) > maxReasonableScale) return Map.DefaultTextureSettings(Map.Format.Valve220, N);
			if (double.IsInfinity(scaleV) || double.IsNaN(scaleV) || Math.Abs(scaleV) > maxReasonableScale) return Map.DefaultTextureSettings(Map.Format.Valve220, N);

			// 7. Build axes (uAxis maps world positions to U coordinate when dotted)
			Vector3 uAxis = uDir.Scaled(scaleU);
			Vector3 vAxis = vDir.Scaled(scaleV);

			// 8. Compute shifts
			double uShift = UV0.x - Vector3.Dot(uAxis, P0);
			double vShift = UV0.y - Vector3.Dot(vAxis, P0);

			// 9. Output (Valve texture axes format)
			return $"[ {uAxis.x} {uAxis.y} {uAxis.z} {uShift} ] [ {vAxis.x} {vAxis.y} {vAxis.z} {vShift} ] 0 1 1";


		}
	}
}


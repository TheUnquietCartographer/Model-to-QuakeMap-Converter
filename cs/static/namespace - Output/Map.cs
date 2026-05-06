using System;
using System.Linq;
using DoubleExtensions;
using CollectionExtensions;

namespace Output {

	public static class Map {

		public enum Format {
			Q1, Q2, Valve220, JACK
		}

		public static string Output (
			UniversalMesh _mesh,
			double scaleFactor,
			double rounding,
			int brushThickness,
			bool reverseVertexOrder,
			bool invertFaceNormals,
			bool trianglifyFaces,
			bool swapYZCoordinates,
			Format format,
			string q2TextureDirectory = ""
		) {
		//SCALE MESH VERTICES
			UniversalMesh scaledMesh = _mesh.Scaled(scaleFactor, rounding, swapYZCoordinates);	

		//LOCAL FUNCTIONS & DELEGATES
			Vector3 GetFaceNormal(UniversalMesh.Face _face) {
				Vector3 normal_ = scaledMesh.GetTrueFaceNormal(_face);
				normal_.Round(1);
				if (invertFaceNormals) normal_.Invert();
				normal_.Scale(brushThickness);
				return normal_;
			}
			string GetTexturePath(UniversalMesh.Face _face) {
				string textureName = scaledMesh.materials[_face.materialIndices[0]];
				textureName = textureName.Substring(0, textureName.IndexOf('.'));
				if (format == Format.Q2) return $"{q2TextureDirectory}/{textureName}";
				return textureName;
			}

		//BEGIN OUTPUT
			string output = "{\r\n\t\"classname\" \"worldspawn\"\r\n";

		//SWITCH .MAP FORMATS
			switch (format) {
				default:
				case Format.Q1:
					output += $"\t\"wad\" \"{q2TextureDirectory.Trim('/')}.wad\"\r\n";
					q2TextureDirectory = "";
					break;
				case Format.Valve220:
					output += $"\t\"mapversion\" \"220\"\r\n";
					output += $"\t\"wad\" \"{q2TextureDirectory.Trim('/')}.wad\"\r\n";
					q2TextureDirectory = "";
					break;
				case Format.Q2:
					break;
				case Format.JACK:
					break;
			}

		//BRUSHES (TRIANGLIFICATION)
		// * Gets face normal per-triangle
		// * Uses scaled & rounded vertices
		// * Inverts face normals
		// * Reverses vertex order
		// * Applies correct brush thickness
			if (trianglifyFaces) {
				//int DEBUGCOUNTER = 0;
				for (int i = 0; i < scaledMesh.faces.Length; i++) {
				//Get useful data
					UniversalMesh.Face _face = scaledMesh.faces[i];
					string texturePath = GetTexturePath(_face);
				//Get triangles
					UniversalMesh.Face[] triangles = scaledMesh.GetTriangles(_face);
					//int FIRST_THISFACE = DEBUGCOUNTER;
				//Iterate triangles
					for (int j = 0; j < triangles.Length; j++) {
					//Get brush vertices
						Vector3[] brushVertices = new Vector3[6];
					//Reverse vertex order?
						if (reverseVertexOrder) {
							brushVertices[0] = scaledMesh.vertices[triangles[j].vertexIndices[2]];
							brushVertices[1] = scaledMesh.vertices[triangles[j].vertexIndices[1]];
							brushVertices[2] = scaledMesh.vertices[triangles[j].vertexIndices[0]];
						}
						else {
							brushVertices[0] = scaledMesh.vertices[triangles[j].vertexIndices[0]];
							brushVertices[1] = scaledMesh.vertices[triangles[j].vertexIndices[1]];
							brushVertices[2] = scaledMesh.vertices[triangles[j].vertexIndices[2]];
						}
					//Check degenerate triangles and discard
					// * Triangle is degenerate if two of the edges are parallel or anti-parallel.
					// * This can occur as a result of rounding and should not leave any gaps in the geometry.
						if (Vector3.Cross(
							Vector3.Vector(brushVertices[0], brushVertices[1]),
							Vector3.Vector(brushVertices[1], brushVertices[2])
						) == Vector3.zero) {
							Log.Single("Degenerate triangle discarded.", ConsoleColor.Red);
							continue;
						}
					//Get remaining useful data
						Vector3 triangleNormal = scaledMesh.GetTriangleNormal(triangles[j]).Scaled(brushThickness);
						string textureSettings = Map_UV.TranslateUVToMap(scaledMesh, triangles[j], i, format);
					//Create backface vertices
						brushVertices[3] = brushVertices[0].Added(triangleNormal);
						brushVertices[4] = brushVertices[1].Added(triangleNormal);
						brushVertices[5] = brushVertices[2].Added(triangleNormal);
					//Write brush
						string brush = "\t{\r\n";
						//brush += $"//{DEBUGCOUNTER}: First in this face is {FIRST_THISFACE}\r\n";
						brush += $"\t\t{WriteBrushPlane(brushVertices[0], brushVertices[1], brushVertices[2], texturePath, textureSettings)}\r\n"; //Front
						brush += $"\t\t{WriteBrushPlane(brushVertices[3], brushVertices[5], brushVertices[4], DefaultTexturePath(format), DefaultTextureSettings(format, triangleNormal))}\r\n"; //Rear
						brush += $"\t\t{WriteBrushPlane(brushVertices[0], brushVertices[3], brushVertices[1], DefaultTexturePath(format), DefaultTextureSettings(format, triangleNormal))}\r\n"; //Side a
						brush += $"\t\t{WriteBrushPlane(brushVertices[1], brushVertices[4], brushVertices[2], DefaultTexturePath(format), DefaultTextureSettings(format, triangleNormal))}\r\n"; //Side b
						brush += $"\t\t{WriteBrushPlane(brushVertices[2], brushVertices[5], brushVertices[0], DefaultTexturePath(format), DefaultTextureSettings(format, triangleNormal))}\r\n"; //Side c
						brush += "\t}\r\n";
						output += brush;
						//DEBUGCOUNTER++;
					//End iterating triangles
					}
				//End iterating faces
				}
				output += "}\r\n";
				return output;
			//End trianglify faces
			}

		//BRUSHES (NO TRIANGLIFICATION)
		// * Uses scaled & rounded vertices
		// * Inverts face normals
		// * Reverses vertex order
		// * Applies correct brush thickness
			else {
				for (int i = 0; i < scaledMesh.faces.Length; i++) {
				//Get useful data
					UniversalMesh.Face _face = scaledMesh.faces[i];
					Vector3 faceNormal = GetFaceNormal(_face);
					string texturePath = GetTexturePath(_face);
					string textureSettings = Map_UV.TranslateUVToMap(scaledMesh, _face, i, format);
				//Get brush vertices
					Vector3[] brushVertices = new Vector3[_face.vertexCount * 2];
				//Reverse vertex order?
				//Create backface vertices
					int countMinusOne = _face.vertexCount-1;
					if (reverseVertexOrder) {
						for (int j = 0; j < _face.vertexCount; j++) {
							int _j = (countMinusOne - j) * 2;
							brushVertices[_j] = scaledMesh.vertices[_face.vertexIndices[j]];
							brushVertices[_j+1] = brushVertices[_j].Added(faceNormal);
						}
					}
					else {
						for (int j = 0; j < _face.vertexCount; j++) {
							int j2 = j*2;
							brushVertices[j2] = scaledMesh.vertices[_face.vertexIndices[j]];
							brushVertices[j2+1] = brushVertices[j2].Added(faceNormal);
						}
					}
				//WRITE BRUSH
					string brush = "\t{\r\n";
				//Front and back
					brush += $"\t\t{WriteBrushPlane(brushVertices[0], brushVertices[2], brushVertices[4], texturePath, textureSettings)}\r\n";
					brush += $"\t\t{WriteBrushPlane(brushVertices[1], brushVertices[5], brushVertices[3], DefaultTexturePath(format), DefaultTextureSettings(format, faceNormal))}\r\n";
				//Sides
					for (int j = 0; j < countMinusOne; j++) {
						brush += $"\t\t{WriteBrushPlane(brushVertices[j*2], brushVertices[j*2 +1], brushVertices[(j+1)*2], DefaultTexturePath(format), DefaultTextureSettings(format, faceNormal))}\r\n";
					}
				//Final side
					brush += $"\t\t{WriteBrushPlane(brushVertices[countMinusOne*2], brushVertices[countMinusOne*2 +1], brushVertices[0], DefaultTexturePath(format), DefaultTextureSettings(format, faceNormal))}\r\n";
				//End
					brush += "\t}\r\n";
					output += brush;
				//End iterating faces
				}
				output += "}\r\n";
				return output;
			//End don't trianglify faces
			}

		//End Map.Output()
		}



	//////////////////////////////////////////////////
	//
	//	WRITE A BRUSH PLANE DEFINITION
	//
	//////////////////////////////////////////////////
	//Format.Q1
		//Textures are stored in .wad files.
		//( x1 y1 z1 ) ( x2 y2 z2 ) ( x3 y3 z3 ) TEXTURE Xoffset Yoffset rotation Xscale Yscale
	//Format.Valve220
		//Textures are stored in .wad files.
		//( x1 y1 z1 ) ( x2 y2 z2 ) ( x3 y3 z3 ) TEXTURE [ Tx1 Ty1 Tz1 Toffset1 ] [ Tx2 Ty2 Tz2 Toffset2 ] rotation Xscale Yscale
	//Format.Q2
		//Textures are given relative paths from the textures/ directory.
		//( x1 y1 z1 ) ( x2 y2 z2 ) ( x3 y3 z3 ) PATH/TEXTURE Uoffset Voffset rotation Uscale Vscale SurfaceFlag ContentsFlag Value
	//Format.JACK
		//Textures are given relative paths from the textures/ directory.
		//( x1 y1 z1 ) ( x2 y2 z2 ) ( x3 y3 z3 ) PATH/TEXTURE [ Tx1 Ty1 Tz1 Toffset1 ] [ Tx2 Ty2 Tz2 Toffset2 ] rotation Uscale Vscale SurfaceFlag ContentsFlag Value

	//WRITE BRUSH PLANE
		private static string WriteBrushPlane(Vector3 h, Vector3 i, Vector3 j, string texturePath, string textureSettings) {
			return ($"{WriteCoord(h)} {WriteCoord(i)} {WriteCoord(j)} {texturePath} {textureSettings}");
		}
		private static string WriteCoord(Vector3 vertex) {
			return $"( {vertex.x} {vertex.y} {vertex.z} )";
		}


	//DEFAULT TEXTURE PATH
		internal static string DefaultTexturePath (Map.Format _format) {
			switch (_format) {
				default:
				case Map.Format.Q1: return "skip";
				case Map.Format.Valve220: return "skip";
				case Map.Format.Q2: return "common/caulk";
				case Map.Format.JACK: return "skip";
			}
		}
	//DEAFAULT TEXTURE SETTINGS
		internal static string DefaultTextureSettings (Map.Format _format, Vector3 _faceNormal) {
			switch (_format) {
				default:
				case Map.Format.Q1: return "0 0 0 1 1";
				case Map.Format.Valve220: return DefaultTextureSettings_Valve220(_faceNormal);
			}
		}
		internal static string DefaultTextureSettings_Valve220 (Vector3 _faceNormal) {
			Vector3 dominantFacing = _faceNormal.Signed(1);
			if (dominantFacing.z == -1) return "[ -1 0 0 0 ] [ 0 1 0 0 ] 0 1 1";
			else if (dominantFacing.z == 1) return "[ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1";
			else if (dominantFacing.y == -1) return "[ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1";
			else if (dominantFacing.y == 1) return "[ -1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1";
			else if (dominantFacing.x == -1) return "[ 0 -1 0 0 ] [ 0 0 -1 0 ] 0 1 1";
			//else if (dominantFacing.x == 1)
			else return "[ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1";
		}


	}

}
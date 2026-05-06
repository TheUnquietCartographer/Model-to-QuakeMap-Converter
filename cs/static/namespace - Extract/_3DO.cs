using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CollectionExtensions;

namespace Extract {

/*
*	.3DO files use 0-based indexing.

*	A typical face in a 3DO file is formatted like so:
		1309:		9					0x0	4 3 1 0			5				1176, 1443 1188, 2314 1455, 2247 1450, 2248 1171, 2310
		^			^						^^^				^										^^^
		Face index	Material index			???				# vertices					vertex/normal_index uv_index

*	Vertices and vertex normals are tied together. Every vertex has an associated vertex normal, and they are referred to by the same index.
*	Texture vertices are separate from vertices/vertex normals. Texture vertices do not need to share an index with vertices/vertex normals.

*	.3DO files record both face normals and vertex normals. I'm not sure why.
*	*	A vertex normal represents the average of the face normals, of the faces that share a given vertex.
*	*	If using vertex normals to calculate lighting, your mesh will appear to have smooth edges (useful for modelling rounded or organic shapes).
*	*	Using face normals to calculate lighting is preferred for geometric shapes.

*	Characters in Jedi Knight are approximately 0.25 units high.
*	*	Since character controllers in Unity are usually around 2 units (2 meters) in height, recommended scale factor for level geometry is 8.
*	*	Recommended scale factor for converting to Quake or Source engines is 320 (40 if converting from Unity scale).

*	UVs need to be scaled with a factor of (1/[textureWidth], 1/[textureHeight]);
*	*	Textures vertices use a pixel offset rather than a normalized scalar offset as with more modern formats like .OBJ.
*	*	In order to modernize the format (normalize the texture vertices) we will need to make the materials available to the extractor.
*/

	public static class _3DO {

		private struct Section {
			public int i_firstEntry = -1;
			public int entries = 0;
			public Section () {}
			public Section (int _i_firstEntry, int _entries) {
				this.i_firstEntry = _i_firstEntry;
				this.entries = _entries;
			}
		}

		public static UniversalMesh? Extract (in Input_PlainText _input, bool useFaceNormals = true, bool normalizeTextureVertices = false) {
			if (_input.streamReader is null) return null;
			string[] fileContents = _input.fileContents;
			
			Dictionary<string, Section> sections = new Dictionary<string, Section>();
			List<string> searchTerms = new List<string>() {
				"VERTICES", "TEXTURE VERTICES", "FACES", "MATERIALS", "VERTEX NORMALS", "FACE NORMALS"
			};
		//
		//	GET SECTION DATA
		//
			string? recordingSection = null;	//The current section being recorded.
			for (int i = 0; i < fileContents.Length; i++) {
			//If not recording a section...
				if (recordingSection == null) {
				//Try and identify a section
					for (int j = searchTerms.Count-1; j > -1; j--) if (fileContents[i].Trim().StartsWith(searchTerms[j])) {
						sections.Add(searchTerms[j], new Section());
						recordingSection = searchTerms[j];
						searchTerms.Remove(searchTerms[j]);
					}
				}
			//Else...
				else {
					string[] line = fileContents[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
				//If line has non-zero length...
					if (line.Length > 0) {
					// Ignore comments
						if (line[0][0] == '#') continue;
					//... and it starts with a number...
						if (Char.IsDigit(line[0][0])) {
							if (sections[recordingSection!].i_firstEntry < 0) {
								sections[recordingSection!] = new Section(i, 1);
								continue;
							}
							Section _s = sections[recordingSection!];
							sections[recordingSection!] = new Section(_s.i_firstEntry, _s.entries+1);
						}
					//... and it DOESN'T start with a number
						else {
							recordingSection = null;
							i--;
							//We will reiterate this line to check if it's a new section header.
						} 
					}
				}

			//End for
			}

			Console.WriteLine($"VERTICES: {sections["VERTICES"].entries} @ line {sections["VERTICES"].i_firstEntry}");
			Console.WriteLine($"VERTEX NORMALS: {sections["VERTEX NORMALS"].entries} @ line {sections["VERTEX NORMALS"].i_firstEntry}");
			Console.WriteLine($"TEXTURE VERTICES: {sections["TEXTURE VERTICES"].entries} @ line {sections["TEXTURE VERTICES"].i_firstEntry}");
			Console.WriteLine($"MATERIALS: {sections["MATERIALS"].entries} @ line {sections["MATERIALS"].i_firstEntry}");

		////////////////////////////////////////////////////////////////////////////////////////////////////

			int i_offset;
		//
		//	GET VERTEX DATA
		//
			Vector3[] vertices = new Vector3[sections["VERTICES"].entries];
			i_offset = sections["VERTICES"].i_firstEntry;
			for (int i = 0; i < vertices.Length; i++) {
				string[] line = fileContents[i_offset+i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
				vertices[i] = new Vector3(
					double.Parse(line[1]),
					double.Parse(line[2]),
					double.Parse(line[3])
				);
			}

		//
		//	GET VERTEX NORMAL DATA
		//
			Vector3[] vertexNormals;
			if (useFaceNormals) {
				vertexNormals = new Vector3[sections["FACE NORMALS"].entries];
				i_offset = sections["FACE NORMALS"].i_firstEntry;
				for (int i = 0; i < vertexNormals.Length; i++) {
					string[] line = fileContents[i_offset+i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
					vertexNormals[i] = new Vector3(
						double.Parse(line[1]),
						double.Parse(line[2]),
						double.Parse(line[3])
					);
				}
			}
			else {
				vertexNormals = new Vector3[sections["VERTEX NORMALS"].entries];
				i_offset = sections["VERTEX NORMALS"].i_firstEntry;
				for (int i = 0; i < vertexNormals.Length; i++) {
					string[] line = fileContents[i_offset+i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
					vertexNormals[i] = new Vector3(
						double.Parse(line[1]),
						double.Parse(line[2]),
						double.Parse(line[3])
					);
				}
			}

		//
		//	GET TEXTURE VERTEX DATA
		//
			Vector2[] textureVertices = new Vector2[sections["TEXTURE VERTICES"].entries];
			i_offset = sections["TEXTURE VERTICES"].i_firstEntry;
			for (int i = 0; i < textureVertices.Length; i++) {
				string[] line = fileContents[i_offset+i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
				textureVertices[i] = new Vector2(
					double.Parse(line[1]),
					double.Parse(line[2])
				);
			}

		//
		//	GET MATERIAL DATA
		//
			string[] materials = new String[sections["MATERIALS"].entries];
			i_offset = sections["MATERIALS"].i_firstEntry;
			for (int i = 0; i < materials.Length; i++) {
				materials[i] = fileContents[i_offset+i].Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
			}



		//
		//	GET FACE DATA
		//
			UniversalMesh.Face[] faces = new UniversalMesh.Face[sections["FACES"].entries];
			i_offset = sections["FACES"].i_firstEntry;

		//HELPER FUNCTIONS
		//Parse a line of data that defines a face.
			int[] Parse_Face (string _line) {
				return _line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select((x, j)=>{
					if (j > 7 && j%2 == 0) return int.Parse(x.TrimEnd(','));	//Vertex/vertex normal index. Texture vertex indices come after the ','.
					if (j == 0) return int.Parse(x.TrimEnd(':'));				//Face entry number in the file.
					if (j == 2) return Convert.ToInt32(x, 16);					//IDK what values 2-6 are but the first one is in hex (I think).
					return (int)double.Parse(x);								//Any other value that doesn't require complex parsing. Includes material index and texture vertex indices. May be non-integer, which is annoying, but we're not using these values.
				}).ToArray();
			}
		//Get vertex indices
			int[] Get_VertexIndices (int[] _line) {
				return _line.Where( (x, j)=>{ return (j > 7 && j%2 == 0) ? true : false; }).ToArray();
			}
		//Get vertex normal indices (use face normals?)
			Func<int[], int[]> Get_VertexNormalIndices;
			if (useFaceNormals) {
				Get_VertexNormalIndices = (_line => {
					int vertexCount = (_line.Length - 7) / 2;
					int[] vertexNormalIndices_ = new int[vertexCount];
					Array.Fill(vertexNormalIndices_, _line[0]);
					return vertexNormalIndices_;
				});
			}
			else {
				Get_VertexNormalIndices = (_line => {
					return _line.Where( (x, j) => { return (j > 7 && j%2 == 0) ? true : false; } ).ToArray();
				});
			}
		//Get texture vertex indices
			int[] Get_TextureVertexIndices (int[] _line) {
				return _line.Where( (x, j)=>{ return (j > 7 && j%2 == 1) ? true : false; }).ToArray();
			}
		//Get material index
			int Get_MaterialIndex (int[] _line) {
				return _line[1];
			}

		//NORMALIZE TEXTURE VERTICES?
			//.3DO uses pixel offsets for the texture vertices.
			//We can update the texture vertices to use a more modern approach where the values are normalized,
			// i.e. (0,0) is the bottom-left corner of the texture and (1,1) is the top-right corner regardless of the size of the texture.
			string materialDirectory = $"{_input.path}/{_input.filename}";
			bool materialsFound = Directory.Exists(materialDirectory);

		//IF MATERIALS ARE PRESENT...
			if (normalizeTextureVertices && materialsFound) {
			//Get scalars (we will scale the texture coordinates by 1/textureSize to get the normalized values).
				Vector2[] materialScalars = materials.Select((_materialName)=>{
					_materialName = _materialName.Substring(0, _materialName.LastIndexOf('.'))+".bmp";
					using (Input_Binary _bmp = new Input_Binary($"{materialDirectory}/{_materialName}")) {
						uint x, y;
						if (!BMP.TryGetWidthHeight(_bmp, out x, out y)) return new Vector2(1,1);
						Log.Multi(
							new Log.ColorLog("> ", ConsoleColor.White),
							new Log.ColorLog($"Scalar for {_materialName} is ({1d/x}, {1d/y})", ConsoleColor.Green)
						);
						return new Vector2(1d/x, 1d/y);
					}
				}).ToArray();
				List<Vector2> normalizedTextureVertices = new List<Vector2>(textureVertices.Length);
			//Iterate faces...
				for (int i = 0; i < faces.Length; i++) {
					int[] line = Parse_Face(fileContents[i_offset+i]);
					int[] textureVertexIndices = Get_TextureVertexIndices(line);
					int materialIndex = Get_MaterialIndex(line);
				//Normalize texture vertices associated with this face and store them in the new list.
				//Update texture vertex indices to reference the new list.
					for (int j = 0; j < textureVertexIndices.Length; j++) {
						normalizedTextureVertices.Add(
							textureVertices[textureVertexIndices[j]].Scaled(materialScalars[materialIndex])
						);
						textureVertexIndices[j] = normalizedTextureVertices.Count-1;
					}
				//Define face
					faces[i] = new UniversalMesh.Face (
						Get_VertexIndices(line),
						Get_VertexNormalIndices(line),
						textureVertexIndices,
						new int[] {materialIndex}
					);
				}
			//Overwrite texture vertices
				textureVertices = normalizedTextureVertices.ToArray();
			}

		//IF MATERIALS ARE NOT PRESENT...
			else {
				if (normalizeTextureVertices) {
					Log.Multi(
						new Log.ColorLog("> ", ConsoleColor.White),
						new Log.ColorLog("Materials directory not found!", ConsoleColor.Red)
					);
					normalizeTextureVertices = false;
				}
			//Iterate faces
				for (int i = 0; i < faces.Length; i++) {
					int[] line = Parse_Face(fileContents[i_offset+i]);
				//Define face
					faces[i] = new UniversalMesh.Face (
						Get_VertexIndices(line),
						Get_VertexNormalIndices(line),
						Get_TextureVertexIndices(line),
						new int[] {Get_MaterialIndex(line)}
					);
				}
			}


		////////////////////////////////////////////////////////////////////////////////////////////////////

		//
		//	MAKE UNIVERSAL MESH
		//
			//UniversalMesh universalMesh = new UniversalMesh(
			//	vertices, vertexNormals, textureVertices, materials, faces
			//);
			UniversalMesh universalMesh = UniversalMesh.Condensed (
				vertices, vertexNormals, textureVertices, materials, faces, normalizeTextureVertices, false
			);
			return universalMesh;

		}
		
	}

}

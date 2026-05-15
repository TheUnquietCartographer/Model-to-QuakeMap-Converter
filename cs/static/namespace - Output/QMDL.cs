using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using CollectionExtensions;
using StringExtensions;

namespace Output {

//Health packs, ammo packs, explosive boxes, use .bsp (very simple models, all cubular, no animation)
//So far as I can tell, all other objects use .mdl, including enemies, projectiles, powerups and armor.

//Palettes in Quake .MDL v6 use 3 bytes per color.

//Maximum number of triangles: 2048
//Maximum number of vertices: 1024
//Maximum number of texture coordinates: 1024
//Maximum number of frames: 256
//Number of precalculated normal vectors: 162

///TODO: 
// * Get image as bytes (currently just creating an empty byte array called imageAsBytes)
// * Atlas multiple textures (have option in function to animate skins or combine textures (bool isAnimated))
// * Enforce limits.

	public static class QMDL {

		public Enum SkinMode {SingleSkin, MultipleSingleSkins, SingleAnimatedGroup, SingleAtlased}

		public static void Output (
			Input _input,
			UniversalMesh _mesh,
			double scaleFactor,
			double rounding,
			bool reverseVertexOrder,
			bool invertFaceNormals,
			bool swapYZCoordinates,
			MaterialDirectory _materialDirectory,
			SkinMode skinMode
		) {
		//////////////////////////////////////////////////
		//
		//	COMPUTE VALUES
		//
		//////////////////////////////////////////////////
		//Scale mesh vertices
			UniversalMesh scaledMesh = _mesh.Scaled(scaleFactor, rounding, swapYZCoordinates);	
		//Bounding box
			Vector3 min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
			Vector3 max = new Vector3(double.MinValue, double.MinValue, double.MinValue);
			foreach (Vector3 v in scaledMesh.vertices) {
				if (v.x < min.x) min.x = v.x;	if (v.x > max.x) max.x = v.x;
				if (v.y < min.y) min.y = v.y;	if (v.x > max.y) max.y = v.y;
				if (v.z < min.z) min.z = v.z;	if (v.x > max.z) max.z = v.z;
			}
			Vector3 size = max.Subtracted(min);
			Vector3 quantizationScale = size.Shrunk(255f);
			Vector3 translation = min;
		//Radius
			double radius = double.MinValue;
			for (int i = 0; i < scaledMesh.vertexCount; i++) {
				Vector3 abs = scaledMesh.vertices[i].absolute;
				if (abs.x > radius) radius = abs.x;
				if (abs.y > radius) radius = abs.y;
				if (abs.z > radius) radius = abs.z;
			}
		//Skins
			UniversalImage[] skins;
			Input[] _imageFiles = _materialDirectory.ExistingFiles().Where(x => x.extension.ToLower() == ".bmp").ToArray(); 

			//If there are missing image files, you at the very least need to log it. Perhaps even fuck off the whole operation.




			int skinCount;
			int skinWidth, skinHeight;
			switch (skinMode) {
				default:
				case SingleSkin:
					skins = new UniversalImage[]{
						UniversalImage.FromInput(_imageFiles[0]);
					};
					skinCount = 1;
					skinWidth = skins[0].width;
					skinHeight = skins[0].height;
					break;
				case MultipleSingleSkins:
					int w_ = int.MinValue, h_ = int.MaxValue;
					skins = new UniversalImage[_imageFiles.Length];
					for(int i = 0; i < _imageFiles.Length; i++) {#
						skins[i] = UniversalImage.FromInput(_imageFiles[i]);
						if (skins[i].width > w_) w_ = skins[i].width;
						if (skins[i].height > h_) h_ = skins[i].height;
					}
					for(int i = 0; i < skins.Length; i++) {
						if (skins[i].width < w_) skins[i].PadWidth(w_ - skins[i].width);
						if (skins[i].height < h_) skins[i].PadWidth(h_ - skins[i].height);
						//Consider extruding as well. But then do you extrude all sides? That means offsetting the image and modifying the UVs as well...
					}
					skinCount = skins.Length;
					skinWidth = w_;
					skinHeight = h_;
					break;
				case SingleAnimatedGroup:
					int w_ = int.MinValue, h_ = int.MaxValue;
					skins = new UniversalImage[_imageFiles.Length];
					for(int i = 0; i < _imageFiles.Length; i++) {
						skins[i] = UniversalImage.FromInput(_imageFiles[i]);
						if (skins[i].width > w_) w_ = skins[i].width;
						if (skins[i].height > h_) h_ = skins[i].height;
					}
					for(int i = 0; i < skins.Length; i++) {
						if (skins[i].width < w_) skins[i].PadWidth(w_ - skins[i].width);
						if (skins[i].height < h_) skins[i].PadWidth(h_ - skins[i].height);
						//Consider extruding as well. But then do you extrude all sides? That means offsetting the image and modifying the UVs as well...
					}
					skinCount = 1;
					skinWidth = w_;
					skinHeight = h_;
					break;
				case SingleAtlased:
					UniversalImage[] images_ = _imageFiles.Select(x => UniversalImage.FromInput(x)).ToArray();
					RectAtlas rectAtlas_;
					skins = new UniversalImage[] {
						UniversalImage.CreateAtlas(_images, 256, 256, padding, out rectAtlas_);
					}
				//Shrink atlas to smallest possible PO2 size
					int w_ = int.MinValue, h_ = int.MaxValue;
					foreach (Rect r in rectAtlas_) {
						if (r.xExt > w_) w_ = r.xExt;
						if (r.yExt > h_) h_ = r.yExt;
					}
					w_ = Maff.MaffFunc.NextPow2(w_);
					h_ = Maff.MaffFunc.NextPow2(h_);
					skins[0].Crop(skins[0].width - w_, skins[0] - h_);

				
				//Need to go through scaledMesh.UVs and make sure they are not shared by faces using more than one texture;
				// if they are, duplicate them, and offset them according to the rect that mateches the image in images_
				// Need to offset them anyway if using padding. So even if the material index on the face is 0, still need to adjust the UV by padding.
					//Should really be saying, if UV is not this int AND material, so the int AND the material needs to be the key; if the material is different, needs to create a new key and then the value would be the new index.

					Dictionary<string, int>UVMaterialLookup = new Dictionary<string, int>();

					List<Vector2>newTextureVertices = scaledMesh.textureVertices.ToList();

					for (int i = 0; i < scaledMesh.faceCount; i++) {
						UniversalMesh.Face _face = scaledMesh.faces[i];
						for (int j = 0; j < _face.textureVertexIndices; j++) {
							string key = $"{_face.textureVertexIndices[j]} {_face.materials[0]}";
							if (UVMaterialLookup.ContainsKey(key)) {
								_face.textureVertexIndices[j] = UVMaterialLookup[key];
							};
							else {
								UVMaterialLookup.Add(key, newTextureVertices.Count);
							//separate UV index from material name / material index......
							// no dont need material index because its an atlas, 
							// wait yes we do because we need to find where the material is on the atlas so we know how to offset it


							//Duplicate original UV
								Vector2 newUV = _scaledMesh.textureVertices[_face.textureVertexIndices[j]];
								//Consider changing UniveresalMesh to record materials as Input...
									//Then the question beecomes can I record it as an Input when no file exists?????

								string _materialName = _face.materials[0].Substring(0, _face.materials[0].LastIndexOf('.'))+".bmp";	//<== What if it doesn't have an extension to begin with?

								int rectIndex = _imageFiles.IndexOf(x => x.filename == {_face.materials[0]} || x.fullname == {face.materials[0]});

							//Use the index of the material in _imageFiles to get the offset from the equivalent Rect in the RectAtlas
								//==> Modify UV by Rect position / padding
							
								_face.textureVertexIndices[j] = newTextureVertices.Count;
								newTextureVertices.Add(newUV);

							}
						//End iterate _face.textureVertexIndices
						}
					//End iterate scaledMesh.faces;
					}
					skinCount = 1;
					skinWidth = w_;
					skinHeight = h_;
					break;
			//End switch
			}

		//Triangles/triangle count
			UniversalMesh.Face[] triangles = scaledMesh.faces.SelectMany(x => scaledMesh.GetTriangles(x)).ToArray();

		//UVs
			QMDL_UVTranslation UVs = new QMDL_UVTranslation(scaledMesh, skinWidth, in triangles);

			//Foreach triangle
			//Link the material to the rect in imageOffsets[]
			//Then for all the UVs associated with that face, modify them by the image offset.
			//BUT only do it once per texture vertex.
			//Would also need to ensure that texture vertices are not shared between faces of different materials.


		//////////////////////////////////////////////////
		//
		//	WRITE FILE
		//
		//////////////////////////////////////////////////
			using (BinaryWriter binaryWriter = new BinaryWriter(
				new FileStream("output/filename.mdl", FileMode.Create, FileAccess.Write)
			)) {
			//Header
				binaryWriter.Write(Header(
					quantizationScale,
					translation,
					radius,
					skinCount,
					skinWidth,
					skinHeight,
					scaledMesh.vertexCount,
					triangles.Length
				));
			//Skin(s)
				switch (skinMode) {
					default:
					case SingleSkin:
						binaryWriter.Write(SingleSkin(skins[0]));
						break;
					case MultipleSingleSkins:
						foreach(UniversalImage i in skins)binaryWriter.Write(SingleSkin(i));
						break;
					case SingleAnimatedGroup:
						float[] intervals = new float[skins.Length];
						Array.Fill(intervals, 0.1f);
						binaryWriter.Write(GroupSkin(skins, intervals))
						break;
					case SingleAtlased:
						binaryWriter.Write(SingleSkin(skins[0]));
						break;
				}
			//UVs

			//Triangles

			//Frames


			}

		}

		public static byte[] Header (
			Vector3 _quantizationScale,
			Vector3 _translation,
			float _radius,
			int _skinCount,
			int _skinWidth,
			int _skinHeight,
			int _vertexCount,
			int _triangleCount
		) {
			List<byte> bytes = new List<byte>(84);
			bytes.AddRange( BitConverter.GetBytes((int)0x4F504449) ); //Ident ("IDPO", little-endian)
			bytes.AddRange( BitConverter.GetBytes((int)6) ); //Version (Quake .MDL v6)
			bytes.AddRange( BitConverter.GetBytes((float)_quantizationScale.x) ); //Scale X (quantization)
			bytes.AddRange( BitConverter.GetBytes((float)_quantizationScale.y) ); //Scale Y (quantization)
			bytes.AddRange( BitConverter.GetBytes((float)_quantizationScale.z) ); //Scale Z (quantization)
			bytes.AddRange( BitConverter.GetBytes((float)_translation.x) ); //Translate X (origin)
			bytes.AddRange( BitConverter.GetBytes((float)_translation.y) ); //Translate Y (origin)
			bytes.AddRange( BitConverter.GetBytes((float)_translation.z) ); //Translate Z (origin)
			bytes.AddRange( BitConverter.GetBytes((float)_radius) ); //Bounding radius (prefer furthest coordinate on a single axis)
			bytes.AddRange( BitConverter.GetBytes((float)0.0f) ); //Eyeposition X (offset for first-person models or camera attachments)
			bytes.AddRange( BitConverter.GetBytes((float)0.0f) ); //Eyeposition Y (offset for first-person models or camera attachments)
			bytes.AddRange( BitConverter.GetBytes((float)0.0f) ); //Eyeposition Z (offset for first-person models or camera attachments)
			bytes.AddRange( BitConverter.GetBytes((int)_skinCount) ); //Skin count (# textures - assume a single skin or group (see Skins section for more details))
			bytes.AddRange( BitConverter.GetBytes((int)_skinWidth) ); //Texture width (while all textures should be the same size, prefer largest)
			bytes.AddRange( BitConverter.GetBytes((int)_skinHeight) ); //Texture Height (while all textures should be the same size, prefer largest)
			bytes.AddRange( BitConverter.GetBytes((int)_vertexCount) ); //Vertex count
			bytes.AddRange( BitConverter.GetBytes((int)_triangleCount) ); //Triangle count
			bytes.AddRange( BitConverter.GetBytes((int)1) ); //Frame count (assume 1 unless passing in multiple meshes)
			bytes.AddRange( BitConverter.GetBytes((int)0) ); //Sync type (0 = sync, 1 = random; prefer sync because why would you have random animation?)
			bytes.AddRange( BitConverter.GetBytes((int)0) ); //Flags (detailed below, order assumed from QME interface)
				//1: Rotate
				//2: Rocket smoke trail
				//4: Grenade smoke trail
				//8: Gib long blood trail
				//16: Gib short blood trail
				//32: Wizard green tracer
				//64: Hell Knight yellow tracer
				//128: Vore purple tracer
			bytes.AddRange( BitConverter.GetBytes((float)1.0f) ); //Size, possibly the average triangle size, ultimately left unused by the Quake engine.
			return bytes.ToArray();
		}


	// * Skins can either be single or in a group. Skins in a group may be animated.
	// * Assume a single skin or skin group (multiple textures may be animated in a single group, or compiled into an atlas).
	// * The most prominent use for multiple skins in Quake is to depict damage on enemy models.
	// * I don't know of any examples in Quake where a model uses an animated skin (check progs?).
		public static byte[] SingleSkin (UniversalImage _image) {
			List<byte> bytes = new List<byte>(4 + _images[0].size*4);
			bytes.AddRange( BitConverter.GetBytes((int)0) ); //Is group skin? 0 = single, 1 = group
			bytes.AddRange( ImageAsBytes(_images[0]) ); //[Skin width * skin height] bytes, each an int index [0-255] of a color on the Quake palette.
			return bytes.ToArray();
		}
		public static byte[] GroupSkin (UniversalImage[] _images, float[] animationIntervals) {
			List<byte> bytes = new List<byte>(4 + 4 + 4*_images.Length + 4*_images[0].width*_images[0].height);
			bytes.AddRange( BitConverter.GetBytes((int)1) ); //Is group skin? 0 = single, 1 = group
			bytes.AddRange( BitConverter.GetBytes((int)_images.Length) ); //Number of textures used in the skin group
			for (int i = 0; i < _images.Length; i++) {
				bytes.AddRange( BitConverter.GetBytes((float)0.1f) ); //The time for each frame to display during animation
			}
			for (int i = 0; i < _images.Length; i++) {
				bytes.AddRange( ImageAsBytes(_images[i]) ); //[Skin width * skin height] bytes, each an int index [0-255] of a color on the Quake palette.
			}
			return bytes.ToArray();
		}




			//////////////////////////////////////////////////
			//
			//	TEXTURE VERTICES, TRIANGLES
			//
			//////////////////////////////////////////////////

			//TEXTURE VERTICES
				for (int i = 0; i < UVs.QMDL_UVs.Count; i++) {
					binaryWriter.Write(UVs.QMDL_UVs[i].onSeam ? 1 : 0);
					binaryWriter.Write(UVs.QMDL_UVs[i].x);
					binaryWriter.Write(UVs.QMDL_UVs[i].y);
				}

			//TRIANGLES
				for (int i = 0; i < triangles.Length; i++) {
					binaryWriter.Write(UVs.QMDL_facesFront[i] ? 1 : 0);
					binaryWriter.Write(triangles[i].vertexIndices[0]);
					binaryWriter.Write(triangles[i].vertexIndices[1]);
					binaryWriter.Write(triangles[i].vertexIndices[2]);
					binaryWriter.Write(triangles[i].textureVertexIndices[0]);
					binaryWriter.Write(triangles[i].textureVertexIndices[1]);
					binaryWriter.Write(triangles[i].textureVertexIndices[2]);
				}

			//////////////////////////////////////////////////
			//
			//	WRITE A SINGLE FRAME
			//
			//////////////////////////////////////////////////
			// * Frames contain vertices
			// * Models comprised of multiple frames can be combined from single-frame models at a later stage.
			//HELPER FUNCTIONS
				byte[] CompressedVertex (Vector3 _vertex, int normalIndex) {
					byte x = (quantizationScale.x == 0) ? (byte)0
						: (byte)Math.Clamp((int)Math.Round((_vertex.x - translation.x) / quantizationScale.x), 0, 255)
					;
					byte y = (quantizationScale.y == 0) ? (byte)0
						: (byte)Math.Clamp((int)Math.Round((_vertex.y - translation.y) / quantizationScale.y), 0, 255)
					;
					byte z = (quantizationScale.z == 0) ? (byte)0
						: (byte)Math.Clamp((int)Math.Round((_vertex.z - translation.z) / quantizationScale.z), 0, 255)
					;
					return new byte[] {x, y, z, (byte)normalIndex};
				}
				byte[] CompressedVertices (UniversalMesh __mesh, UniversalMesh.Face[] __triangles) {
				//Get normals per each vertex
					Vector3[] normalsPerVertex = new Vector3[__mesh.vertexCount];
					for (int i = 0; i < __triangles.Length; i++) {
						normalsPerVertex[__triangles[i].vertexIndices[0]].Add(__mesh.vertexNormals[__triangles[i].vertexNormalIndices[0]]);
						normalsPerVertex[__triangles[i].vertexIndices[1]].Add(__mesh.vertexNormals[__triangles[i].vertexNormalIndices[1]]);
						normalsPerVertex[__triangles[i].vertexIndices[2]].Add(__mesh.vertexNormals[__triangles[i].vertexNormalIndices[2]]);
					}
				//Write vertices (X, Y, Z, normal index)
					byte[] compressedVertices = new byte[__mesh.vertexCount * 4];
					for (int i = 0; i < __mesh.vertexCount; i++) {
					//Choose a vertex normal
						Vector3 normal_thisVertex = normalsPerVertex[i].normalized;
						if (normal_thisVertex.isNaN) normal_thisVertex = Vector3.zero;
						int index_ = -1;
						double dot_ = double.NegativeInfinity;
						for (int j = 0; j < QMDL_normals.Length; j++) {
							double thisDot = Vector3.Dot(normal_thisVertex, QMDL_normals[j]);
							if (thisDot > dot_) {
								dot_ = thisDot;
								index_ = j;
							}
						}
						byte[] compressedVertex = CompressedVertex(__mesh.vertices[i], index_);
						Array.Copy(compressedVertex, 0, compressedVertices, i*4, 4);
					}
					return compressedVertices;
				}
			//WRITE BYTES
			bool singleFrame = true;
				if (singleFrame) {
					binaryWriter.Write(0); //0 = single frame, 1 = group frame
				//Bounding box max/min (uses the same structure as the vertices -> 3 bytes compresssed vertex components, 1 byte normal index(unused))
					binaryWriter.Write(CompressedVertex(min, 0));
					binaryWriter.Write(CompressedVertex(max, 0));
				//Name is 16 bytes long (fixed)
					binaryWriter.Write(_input.filename.AsBytes_ASCII(16));
				//Finally the vertices
					binaryWriter.Write(CompressedVertices(scaledMesh, triangles));
				}

			//End using()
			}
		//End Output()
		}

	/*
	*	anorms.h - header file
	*/
		private static Vector3[] QMDL_normals = new Vector3[] {
			new Vector3(-0.525731f,  0.000000f,  0.850651f), 
			new Vector3(-0.442863f,  0.238856f,  0.864188f), 
			new Vector3(-0.295242f,  0.000000f,  0.955423f), 
			new Vector3(-0.309017f,  0.500000f,  0.809017f), 
			new Vector3(-0.162460f,  0.262866f,  0.951056f), 
			new Vector3( 0.000000f,  0.000000f,  1.000000f), 
			new Vector3( 0.000000f,  0.850651f,  0.525731f), 
			new Vector3(-0.147621f,  0.716567f,  0.681718f), 
			new Vector3( 0.147621f,  0.716567f,  0.681718f), 
			new Vector3( 0.000000f,  0.525731f,  0.850651f), 
			new Vector3( 0.309017f,  0.500000f,  0.809017f), 
			new Vector3( 0.525731f,  0.000000f,  0.850651f), 
			new Vector3( 0.295242f,  0.000000f,  0.955423f), 
			new Vector3( 0.442863f,  0.238856f,  0.864188f), 
			new Vector3( 0.162460f,  0.262866f,  0.951056f), 
			new Vector3(-0.681718f,  0.147621f,  0.716567f), 
			new Vector3(-0.809017f,  0.309017f,  0.500000f), 
			new Vector3(-0.587785f,  0.425325f,  0.688191f), 
			new Vector3(-0.850651f,  0.525731f,  0.000000f), 
			new Vector3(-0.864188f,  0.442863f,  0.238856f), 
			new Vector3(-0.716567f,  0.681718f,  0.147621f), 
			new Vector3(-0.688191f,  0.587785f,  0.425325f), 
			new Vector3(-0.500000f,  0.809017f,  0.309017f), 
			new Vector3(-0.238856f,  0.864188f,  0.442863f), 
			new Vector3(-0.425325f,  0.688191f,  0.587785f), 
			new Vector3(-0.716567f,  0.681718f, -0.147621f), 
			new Vector3(-0.500000f,  0.809017f, -0.309017f), 
			new Vector3(-0.525731f,  0.850651f,  0.000000f), 
			new Vector3( 0.000000f,  0.850651f, -0.525731f), 
			new Vector3(-0.238856f,  0.864188f, -0.442863f), 
			new Vector3( 0.000000f,  0.955423f, -0.295242f), 
			new Vector3(-0.262866f,  0.951056f, -0.162460f), 
			new Vector3( 0.000000f,  1.000000f,  0.000000f), 
			new Vector3( 0.000000f,  0.955423f,  0.295242f), 
			new Vector3(-0.262866f,  0.951056f,  0.162460f), 
			new Vector3( 0.238856f,  0.864188f,  0.442863f), 
			new Vector3( 0.262866f,  0.951056f,  0.162460f), 
			new Vector3( 0.500000f,  0.809017f,  0.309017f), 
			new Vector3( 0.238856f,  0.864188f, -0.442863f), 
			new Vector3( 0.262866f,  0.951056f, -0.162460f), 
			new Vector3( 0.500000f,  0.809017f, -0.309017f), 
			new Vector3( 0.850651f,  0.525731f,  0.000000f), 
			new Vector3( 0.716567f,  0.681718f,  0.147621f), 
			new Vector3( 0.716567f,  0.681718f, -0.147621f), 
			new Vector3( 0.525731f,  0.850651f,  0.000000f), 
			new Vector3( 0.425325f,  0.688191f,  0.587785f), 
			new Vector3( 0.864188f,  0.442863f,  0.238856f), 
			new Vector3( 0.688191f,  0.587785f,  0.425325f), 
			new Vector3( 0.809017f,  0.309017f,  0.500000f), 
			new Vector3( 0.681718f,  0.147621f,  0.716567f), 
			new Vector3( 0.587785f,  0.425325f,  0.688191f), 
			new Vector3( 0.955423f,  0.295242f,  0.000000f), 
			new Vector3( 1.000000f,  0.000000f,  0.000000f), 
			new Vector3( 0.951056f,  0.162460f,  0.262866f), 
			new Vector3( 0.850651f, -0.525731f,  0.000000f), 
			new Vector3( 0.955423f, -0.295242f,  0.000000f), 
			new Vector3( 0.864188f, -0.442863f,  0.238856f), 
			new Vector3( 0.951056f, -0.162460f,  0.262866f), 
			new Vector3( 0.809017f, -0.309017f,  0.500000f), 
			new Vector3( 0.681718f, -0.147621f,  0.716567f), 
			new Vector3( 0.850651f,  0.000000f,  0.525731f), 
			new Vector3( 0.864188f,  0.442863f, -0.238856f), 
			new Vector3( 0.809017f,  0.309017f, -0.500000f), 
			new Vector3( 0.951056f,  0.162460f, -0.262866f), 
			new Vector3( 0.525731f,  0.000000f, -0.850651f), 
			new Vector3( 0.681718f,  0.147621f, -0.716567f), 
			new Vector3( 0.681718f, -0.147621f, -0.716567f), 
			new Vector3( 0.850651f,  0.000000f, -0.525731f), 
			new Vector3( 0.809017f, -0.309017f, -0.500000f), 
			new Vector3( 0.864188f, -0.442863f, -0.238856f), 
			new Vector3( 0.951056f, -0.162460f, -0.262866f), 
			new Vector3( 0.147621f,  0.716567f, -0.681718f), 
			new Vector3( 0.309017f,  0.500000f, -0.809017f), 
			new Vector3( 0.425325f,  0.688191f, -0.587785f), 
			new Vector3( 0.442863f,  0.238856f, -0.864188f), 
			new Vector3( 0.587785f,  0.425325f, -0.688191f), 
			new Vector3( 0.688191f,  0.587785f, -0.425325f), 
			new Vector3(-0.147621f,  0.716567f, -0.681718f), 
			new Vector3(-0.309017f,  0.500000f, -0.809017f), 
			new Vector3( 0.000000f,  0.525731f, -0.850651f), 
			new Vector3(-0.525731f,  0.000000f, -0.850651f), 
			new Vector3(-0.442863f,  0.238856f, -0.864188f), 
			new Vector3(-0.295242f,  0.000000f, -0.955423f), 
			new Vector3(-0.162460f,  0.262866f, -0.951056f), 
			new Vector3( 0.000000f,  0.000000f, -1.000000f), 
			new Vector3( 0.295242f,  0.000000f, -0.955423f), 
			new Vector3( 0.162460f,  0.262866f, -0.951056f), 
			new Vector3(-0.442863f, -0.238856f, -0.864188f), 
			new Vector3(-0.309017f, -0.500000f, -0.809017f), 
			new Vector3(-0.162460f, -0.262866f, -0.951056f), 
			new Vector3( 0.000000f, -0.850651f, -0.525731f), 
			new Vector3(-0.147621f, -0.716567f, -0.681718f), 
			new Vector3( 0.147621f, -0.716567f, -0.681718f), 
			new Vector3( 0.000000f, -0.525731f, -0.850651f), 
			new Vector3( 0.309017f, -0.500000f, -0.809017f), 
			new Vector3( 0.442863f, -0.238856f, -0.864188f), 
			new Vector3( 0.162460f, -0.262866f, -0.951056f), 
			new Vector3( 0.238856f, -0.864188f, -0.442863f), 
			new Vector3( 0.500000f, -0.809017f, -0.309017f), 
			new Vector3( 0.425325f, -0.688191f, -0.587785f), 
			new Vector3( 0.716567f, -0.681718f, -0.147621f), 
			new Vector3( 0.688191f, -0.587785f, -0.425325f), 
			new Vector3( 0.587785f, -0.425325f, -0.688191f), 
			new Vector3( 0.000000f, -0.955423f, -0.295242f), 
			new Vector3( 0.000000f, -1.000000f,  0.000000f), 
			new Vector3( 0.262866f, -0.951056f, -0.162460f), 
			new Vector3( 0.000000f, -0.850651f,  0.525731f), 
			new Vector3( 0.000000f, -0.955423f,  0.295242f), 
			new Vector3( 0.238856f, -0.864188f,  0.442863f), 
			new Vector3( 0.262866f, -0.951056f,  0.162460f), 
			new Vector3( 0.500000f, -0.809017f,  0.309017f), 
			new Vector3( 0.716567f, -0.681718f,  0.147621f), 
			new Vector3( 0.525731f, -0.850651f,  0.000000f), 
			new Vector3(-0.238856f, -0.864188f, -0.442863f), 
			new Vector3(-0.500000f, -0.809017f, -0.309017f), 
			new Vector3(-0.262866f, -0.951056f, -0.162460f), 
			new Vector3(-0.850651f, -0.525731f,  0.000000f), 
			new Vector3(-0.716567f, -0.681718f, -0.147621f), 
			new Vector3(-0.716567f, -0.681718f,  0.147621f), 
			new Vector3(-0.525731f, -0.850651f,  0.000000f), 
			new Vector3(-0.500000f, -0.809017f,  0.309017f), 
			new Vector3(-0.238856f, -0.864188f,  0.442863f), 
			new Vector3(-0.262866f, -0.951056f,  0.162460f), 
			new Vector3(-0.864188f, -0.442863f,  0.238856f), 
			new Vector3(-0.809017f, -0.309017f,  0.500000f), 
			new Vector3(-0.688191f, -0.587785f,  0.425325f), 
			new Vector3(-0.681718f, -0.147621f,  0.716567f), 
			new Vector3(-0.442863f, -0.238856f,  0.864188f), 
			new Vector3(-0.587785f, -0.425325f,  0.688191f), 
			new Vector3(-0.309017f, -0.500000f,  0.809017f), 
			new Vector3(-0.147621f, -0.716567f,  0.681718f), 
			new Vector3(-0.425325f, -0.688191f,  0.587785f), 
			new Vector3(-0.162460f, -0.262866f,  0.951056f), 
			new Vector3( 0.442863f, -0.238856f,  0.864188f), 
			new Vector3( 0.162460f, -0.262866f,  0.951056f), 
			new Vector3( 0.309017f, -0.500000f,  0.809017f), 
			new Vector3( 0.147621f, -0.716567f,  0.681718f), 
			new Vector3( 0.000000f, -0.525731f,  0.850651f), 
			new Vector3( 0.425325f, -0.688191f,  0.587785f), 
			new Vector3( 0.587785f, -0.425325f,  0.688191f), 
			new Vector3( 0.688191f, -0.587785f,  0.425325f), 
			new Vector3(-0.955423f,  0.295242f,  0.000000f), 
			new Vector3(-0.951056f,  0.162460f,  0.262866f), 
			new Vector3(-1.000000f,  0.000000f,  0.000000f), 
			new Vector3(-0.850651f,  0.000000f,  0.525731f), 
			new Vector3(-0.955423f, -0.295242f,  0.000000f), 
			new Vector3(-0.951056f, -0.162460f,  0.262866f), 
			new Vector3(-0.864188f,  0.442863f, -0.238856f), 
			new Vector3(-0.951056f,  0.162460f, -0.262866f), 
			new Vector3(-0.809017f,  0.309017f, -0.500000f), 
			new Vector3(-0.864188f, -0.442863f, -0.238856f), 
			new Vector3(-0.951056f, -0.162460f, -0.262866f), 
			new Vector3(-0.809017f, -0.309017f, -0.500000f), 
			new Vector3(-0.681718f,  0.147621f, -0.716567f), 
			new Vector3(-0.681718f, -0.147621f, -0.716567f), 
			new Vector3(-0.850651f,  0.000000f, -0.525731f), 
			new Vector3(-0.688191f,  0.587785f, -0.425325f), 
			new Vector3(-0.587785f,  0.425325f, -0.688191f), 
			new Vector3(-0.425325f,  0.688191f, -0.587785f), 
			new Vector3(-0.425325f, -0.688191f, -0.587785f), 
			new Vector3(-0.587785f, -0.425325f, -0.688191f), 
			new Vector3(-0.688191f, -0.587785f, -0.425325f)
		};
	}

}

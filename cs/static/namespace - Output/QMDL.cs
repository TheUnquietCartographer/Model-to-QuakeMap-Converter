using System;
using System.IO;
using System.Text;
using System.Linq;
using CollectionExtensions;

namespace Output {

//Health packs, ammo packs, explosive boxes, use .bsp (very simple models, all cubular, no animation)
//So far as I can tell, all other objects use .mdl, including enemies, projectiles, powerups and armor.

//Palettes in Quake .MDL v6 use 3 bytes per color.

				// float == System.Single == 32 bits == 4 bytes
				// int == System.Int32 == 32 bits == 4 bytes
				// Calling BinaryReader.Write() will convert your 32-bit value to 4 bytes or whatever.

//Maximum number of triangles: 2048
//Maximum number of vertices: 1024
//Maximum number of texture coordinates: 1024
//Maximum number of frames: 256
//Number of precalculated normal vectors: 162


///TODO: 
// * Get image as bytes (currently just creating an empty byte array called imageAsBytes)
// * Force vertex splitting when checking for onseam texture vertices.

	public static class QMDL {

		public static void Output (
			Input _input,
			UniversalMesh _mesh,
			double scaleFactor,
			double rounding,
			bool reverseVertexOrder,
			bool invertFaceNormals,
			bool swapYZCoordinates,
			MaterialDirectory _materialDirectory
		) {
			using (BinaryWriter binaryWriter = new BinaryWriter(
				new FileStream("output/filename.mdl", FileMode.Create, FileAccess.Write)
			)) {

			//SCALE MESH VERTICES
				UniversalMesh scaledMesh = _mesh.Scaled(scaleFactor, rounding, swapYZCoordinates);	

			//////////////////////////////////////////////////
			//
			//	HEADER
			//
			//////////////////////////////////////////////////
			// * Fixed size
				binaryWriter.Write(0x4F504449); //Identifier ("IDPO", little-endian)
				binaryWriter.Write(6); //Version (Quake .MDL v6)
				binaryWriter.Write(1.0f); //Scale X (using scaledMesh so set to 1)
				binaryWriter.Write(1.0f); //Scale Y (using scaledMesh so set to 1)
				binaryWriter.Write(1.0f); //Scale Z (using scaledMesh so set to 1)
				binaryWriter.Write(0.0f); //Translate X (origin)
				binaryWriter.Write(0.0f); //Translate Y (origin)
				binaryWriter.Write(0.0f); //Translate Z (origin)
				{
					double radius = double.MinValue;
					for (int i = 0; i < scaledMesh.vertexCount; i++) {
						Vector3 abs = scaledMesh.vertices[i].absolute;
						if (abs.x > radius) radius = abs.x;
						if (abs.y > radius) radius = abs.y;
						if (abs.z > radius) radius = abs.z;
					}
					binaryWriter.Write((float)radius); //Bounding radius (prefer furthest coordinate on a single axis)
				}
				binaryWriter.Write(0.0f); //Eyeposition X (offset for first-person models or camera attachments)
				binaryWriter.Write(0.0f); //Eyeposition Y (offset for first-person models or camera attachments)
				binaryWriter.Write(0.0f); //Eyeposition Z (offset for first-person models or camera attachments)
				binaryWriter.Write(1); //Skin count (# textures - assume a single skin or group (see Skins section for more details))
				int width, height;
				{
					foreach (MaterialDirectory.MaterialData md in _materialDirectory) {
						int _width = (int)md.width; int _height = (int)md.height;
						if (_width > width) width = _width;
						if (_height > height) height = _height;
					}
					binaryWriter.Write(width); //Texture width (while all textures should be the same size, prefer largest)
					binaryWriter.Write(height); //Texture height (wile all textures should be the same size, prefer largest)
				}
				binaryWriter.Write((int)scaledMesh.vertexCount); //Vertex count
				{
					int tris_ = 0;
					for (int i = 0; i < _mesh.facesCount; i++) {
						tris_ += _mesh.faces[i].triangleCount;
					}
					binaryWriter.Write(tris_); //Triangle count
				}
				binaryWriter.Write(1); //Frame count (assume 1 unless passing in multiple meshes)
				binaryWriter.Write(0); //Sync type (0 = sync, 1 = random; prefer sync because why would you have random animation?)
				binaryWriter.Write(0); //Flags (detailed below, order assumed from QME interface)
				//1: Rotate
				//2: Rocket smoke trail
				//4: Grenade smoke trail
				//8: Gib long blood trail
				//16: Gib short blood trail
				//32: Wizard green tracer
				//64: Hell Knight yellow tracer
				//128: Vore purple tracer
				{
					binaryWriter.Write(1.0f); //Size, perhaps some kind of scalar, perhaps average triangle size...
				}

			//////////////////////////////////////////////////
			//
			//	SKINS
			//
			//////////////////////////////////////////////////
			// * Skins can either be single or in a group. Skins in a group may be animated.
			// * Assume a single skin or skin group (i.e. assume that multiple materials equals animated texture, as opposed to state-dependent skins).
			// * The most prominent use for multiple skins in Quake is to depict damage on enemy models.
			// * I don't know of any examples in Quake where a model uses an animated (group) skin (check progs?).
			//SKIN GROUP
				if (_mesh.materialsCount > 1) {
					binaryWriter.Write(1); //Is group skin? 0 = single, 1 = group
					binaryWriter.Write((int)_mesh.materialsCount); //Number of textures used in the skin group
					for (int i = 0; i < _mesh.materialsCount; i++) {
						binaryWriter.Write(0.1f); //The time for each frame to display during animation
					}
					for (int i = 0; i < _mesh.materialsCount; i++) {
						byte[] imageAsBytes = new byte[width * height]; //[Skin width * skin height] bytes, each an int index [0-255] of a color on the Quake palette.
						binaryWriter.Write(imageAsBytes); //Record each texture/frame
					}
				}
			//SINGLE SKIN, ON IT'S OWN, NO GROUP
				else {
					binaryWriter.Write(0); //Is group skin? 0 = single, 1 = group
					byte[] imageAsBytes = new byte[width * height]; //[Skin width * skin height] bytes, each an int index [0-255] of a color on the Quake palette.
					binaryWriter.Write(imageAsBytes); //Record each texture/frame
				}

			//////////////////////////////////////////////////
			//
			//	TEXTURE VERTICES
			//
			//////////////////////////////////////////////////
				QMDL_UVTranslation UVs = new QMDL_UVTranslation(_scaledMesh, width);
				for (int i = 0; i < UVs.QMDL_UVs.Count; i++) {
					binaryWriter.Write(UVs.QMDL_UVs[i].onSeam ? 1 : 0);
					binaryWriter.Write(UVs.QMDL_UVs[i].x);
					binaryWriter.Write(UVs.QMDL_UVs[i].y);
				}





			//////////////////////////////////////////////////
			//
			//	UNUSED (WRONG INFORMATION?)
			//
			//////////////////////////////////////////////////

			/*
			//Name
				{
					byte[] headerName_ = new byte[64];
					byte[] nameInBytes = Encoding.Unicode.GetBytes(modelName);
					Array.Copy(nameInBytes, 0, headerName_, 0, Math.Min(nameInBytes.Length, 64));	//<== Should we use Encoding.ASCII instead, since it's an old file format?
					binaryWriter.Write(headerName_);
				}
			//Length (placeholder) (why a placeholder?)
				binaryWriter.Write(0);
			//BOUNDING BOXES
				{
					Vector3 min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue), max = new Vector3(double.MinValue, double.MinValue, double.MinValue);
					for (int i = 0; i < scaledMesh.vertexCount; i++) {
						Vector3 v = scaledMesh.vertices[i];
						if (v.x < min.x) min.x = v.x;	if (v.x > max.x) max.x = v.x;
						if (v.y < min.y) min.y = v.y;	if (v.x > max.y) max.y = v.y;
						if (v.z < min.z) min.z = v.z;	if (v.x > max.z) max.z = v.z;
					}
					byte[] _min = new byte[]{(byte)min.x, (byte)min.y, (byte)min.z};	byte[] _max = new byte[]{(byte)max.x, (byte)max.y, (byte)max.z};
				//Min/max (axis-aligned bounding box usually representing a single frame (like the default pose) of the model)
				// * "...used by rendering code and some per-frame tests".
					binaryWriter.Write(_min);
					binaryWriter.Write(_max);

				//BBMin/max (axis-aligned bounding box encompasing ALL frames of the model, think a collision hull)
					binaryWriter.Write(_min);
					binaryWriter.Write(_max);
				}
				*/
			}

		}

	
	}


}

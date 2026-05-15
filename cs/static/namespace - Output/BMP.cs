using System;
using System.Collections.Generic;
using System.Numerics;

namespace Output {

//Forced 8-bit for now.
// * !8-bit will require the dilesize and pixel offset in the header to change, as the size of the palette will be different.
// * Will also require BPP to be changed in the DIBHeader. Other values too since we are basically hard-coding it for Quake at this time.
//Little-endian

	public static class BMP {

		public static void Output (UniversalImage _image, Palette palette) {
			using (BinaryWriter binaryWriter = new BinaryWriter(
				new FileStream("output/filename.mdl", FileMode.Create, FileAccess.Write)
			)) {
				binaryWriter.Write(Header());
				binaryWriter.Write(DIBHeader());
				binaryWriter.Write(Palette(palette));
			}
		}

	//HEADER (14 BYTES)
		private static byte[] Header (int size) {
			List<byte> bytes = new List<byte>(14);
			bytes.AddRange( BitConverter.GetBytes((ushort)0x4D42) ); //Ident ("BM", little-endian)
			bytes.AddRange( BitConverter.GetBytes((int)1078+size) ); //FileSize (14 + 40 + 256*4 + width*height [header + DIB header + palette + pixels])
			bytes.AddRange( BitConverter.GetBytes((ushort)0) ); //Reserved (0 value not '0' character!)
			bytes.AddRange( BitConverter.GetBytes((ushort)0) ); //Reserved (0 value not '0' character!)
			bytes.AddRange( BitConverter.GetBytes((int)1078) ); //Pixel offset (14 + 40 + 256*4 [header + DIB header + palette])
			return bytes.ToArray();
		}

	//DIB HEADER
		protected static virtual byte[] DIBHeader (int width, int height) {
			if (height > 0) height = -height; //Force storing pixels top-down (mirrors UniversalImage)
			List<byte> bytes = new List<byte>(40);
			bytes.AddRange( BitConverter.GetBytes((int)40) ); //DIB header size (40)
			bytes.AddRange( BitConverter.GetBytes((int)width) ); //Width (pixels)
			bytes.AddRange( BitConverter.GetBytes((int)height) ); //Height (positive for bottom-up, negative for top-down)
			bytes.AddRange( BitConverter.GetBytes((ushort)1) ); //Planes (legacy - just put 1)
			bytes.AddRange( BitConverter.GetBytes((ushort)8) ); //Bits-per-pixel (force 8 for now)
			bytes.AddRange( BitConverter.GetBytes((int)0) ); //Compression level (Quake uses 0)
			bytes.AddRange( BitConverter.GetBytes((int)0) ); //Compressed image size (don't know how to calculate)
			bytes.AddRange( BitConverter.GetBytes((int)2835) ); //X pixels-per-meter (physical/print size)
			bytes.AddRange( BitConverter.GetBytes((int)2835) ); //Y pixels-per-meter (physical/print size)
			bytes.AddRange( BitConverter.GetBytes((int)256) ); //Colors in the palette (0 = default for the current bit-depth)
			bytes.AddRange( BitConverter.GetBytes((int)0) ); //Important colors (legacy - 0 = all)
			return bytes.ToArray(); 
		}

	//PALETTE
		private static byte[] Palette (Palette palette) {
			if (palette.Length == 1024) return palette.ToByte4_BGR0();
			byte[] bytes = new byte[1024];
			byte[] paletteBytes = palette.ToByte4_BGR0();
			Array.Copy(paletteBytes, 0, bytes, 0, Math.Min(paletteBytes.Length, 1024));
			return bytes;
		}
	}

}
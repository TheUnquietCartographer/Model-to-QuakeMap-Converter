using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

	public struct Palette : IEnumerable<Color> {
		
		private readonly Color[] colors_;

	//CONSTRUCTORS
		public Palette (params Color[] colors) {
			colors_ = colors ?? Array.Empty<Color>();
		}
		public Palette (params string[] hexCodes) {
			if (hexCodes == null) {
				colors_ = Array.Empty<Color>();
				return;
			}
			colors_ = new Color[hexCodes.Length];
			for (int i = 0; i < hexCodes.Length; i++) {
				colors_[i] =  ColorTranslator.FromHtml(hexCodes[i]);
			}
		}




/*
//Create a 256-colour palette???????? Make this a constructor in Palette?????
//I think the idea is to exchange colors for ints like in the UniversalImage class... would also have to extract the colour from the int.
		HashSet<int> colorsAsInt = new HashSet<int>();
		for(int y = 0; y < _image.height; y++) {
			for (int x = 0; x < _image.width; x++) {
				int i = y * width + x;
				int argb = (_image[i].A << 24) | (_image[i].R << 16) | (_image[i].G << 8) | _image[i].B;
				int ARGB = Color.ToArbg(_image[i]);
				colorsAsInt.Add(ARGB);
			}
		}
*/




	//INDEXING
		public int Length => colors_?.Length ?? 0;
		public Color this[int index] => (uint)index < (uint)colors_.Length ? colors_[index] : throw new IndexOutOfRangeException();
		public IEnumerator<Color> GetEnumerator() => ((IEnumerable<Color>)colors_).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => colors_.GetEnumerator();

	//FUNCTIONS
		public byte[] ToByte3_RGB () {
			byte[] bytes = new byte[this.Length * 3];
			for (int i = 0; i < this.Length; i++) {
				int i3 = i*3;
				bytes[i3] = this[i].R;
				bytes[i3+1] = this[i].G;
				bytes[i3+2] = this[i].B;
			}
			return bytes;
		}
		public byte[] ToByte4_RGBA () {
			byte[] bytes = new byte[this.Length * 4];
			for (int i = 0; i < this.Length; i++) {
				int i4 = i*4;
				bytes[i4] = this[i].R;
				bytes[i4+1] = this[i].G;
				bytes[i4+2] = this[i].B;
				bytes[i4+3] = this[i].A;
			}
			return bytes;
		}
		public byte[] ToByte4_BGR0 () { //Used by Quake 8-bit BMP
			byte[] bytes = new byte[this.Length * 4];
			for (int i = 0; i < this.Length; i++) {
				int i4 = i*4;
				bytes[i4] = this[i].B;
				bytes[i4+1] = this[i].G;
				bytes[i4+2] = this[i].R;
				bytes[i4+3] = (byte)0;
			}
			return bytes;
		}

	}
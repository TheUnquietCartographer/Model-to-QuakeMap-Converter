using System;
using System.IO;

namespace Extract {

	internal static class BMP {

		public struct ByteData {
			public uint index;
			public uint size;
			private bool isLittleEndian_;
			public bool isLittleEndian {get => isLittleEndian_; set => isLittleEndian_ = value;}
			public bool isBigEndian {get => !isLittleEndian_; set => isLittleEndian_ = !value;}
			public ByteData (uint _index, uint _size, bool littleEndian = true) {
				this.index = _index;
				this.size = _size;
				this.isLittleEndian_ = littleEndian;
			}
		}
		public static readonly ByteData byteData_byteOffset_pixels = new ByteData(10, 4, true),
			byteData_pixelWidth = new ByteData(18, 4, true),
			byteData_pixelHeight = new ByteData(22, 4, true),
			byteData_bitsPerPixel = new ByteData(28, 2, true)
		;
		public static uint BytesPerRow (uint _pixelWidth, uint _bitsPerPixel) {
			return (_pixelWidth * _bitsPerPixel + 7) / 8;
		}
		public static uint BytesPerRow_Padded (uint _pixelWidth, uint _bitsPerPixel) {
			return (BytesPerRow(_pixelWidth, _bitsPerPixel) + 3) & ~3u;
		}
/*

	# Read palette if needed
		if ((src_bitsPerPixel < 16)); then
			ReadBMPPalette $src_bitsPerPixel
		fi
# Detect and read BMP palette for indexed images
# Palettes will be recorded in hexidecimal format (this is so the Konsole can read the colours)
	sourceBMPPalette=()		#I would rather pass this as an argument but it was causing me gyp so I left it global.
	ReadBMPPalette() {
		sourceBMPPalette=()
		local _bitsPerPixel=$1
		local numColors=$(od -An -t u4 -j 46 -N 4 "$arg_input" | tr -d ' ')
		if (( numColors == 0 )); then
			numColors=$((1 << _bitsPerPixel))
		fi
		for ((i=0;i<$numColors;i++)); do
			offset=$((14 + 40 + i*4))   # BMP header + DIB header + palette entry
			b=$(od -An -t u1 -j $offset -N 1 "$arg_input" | tr -d ' ')
			g=$(od -An -t u1 -j $((offset+1)) -N 1 "$arg_input" | tr -d ' ')
			r=$(od -An -t u1 -j $((offset+2)) -N 1 "$arg_input" | tr -d ' ')
		#Fallback to 0 if empty
			r=$(ToHex ${r:-224})
			g=$(ToHex ${g:-0})
			b=$(ToHex ${b:-224})
			sourceBMPPalette+=("#$r$g$b")
		done
	#Logging the palette for debugging
		if [ $logging = true ]; then
			echo "Palette:"
			LogPalette "${sourceBMPPalette[@]}"
		fi
	}


*/




	//	public Vector GetComponents (Input_BinaryReader _input) {

	//	}

	}

}
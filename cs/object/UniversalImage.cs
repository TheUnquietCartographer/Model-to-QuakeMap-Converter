using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

	public struct UniversalImage : IEnumerable<int> {
		
		public int height;
		public int width;
		private readonly int[] pixelsAsInt_;

	//CONSTRUCTORS
		public UniversalImage();
		public UniversalImage (int width, int height, params Color[] pixels) {
			this.width = width;
			this.height = height;
			this.pixels_ = pixels.Select(x => x.ToArbg()).ToArray() ?? Array.Empty<int>();
		}
	/*
		public UniversalImage (uint width, uint height, params string[] hexCodes) {
			this.width = width;
			this.height = height;	
			if (hexCodes == null) {
				this.pixels_ = Array.Empty<int>();
				return;
			}
			this.pixels_ = new Color[hexCodes.Length];
			for (int i = 0; i < hexCodes.Length; i++) {
				this.pixels_[i] = ColorTranslator.FromHtml(hexCodes[i]);
			}
		}
	*/
		public static UniversalImage FromInput (Input input) {
			switch (input.extension.ToLower()) {
				default: return new UniversalImage();
				case ".bmp":
					using (Input_Binary bmp = new Input_Binary(input)) 
						return UniversalImage.FromBMP(bmp);
					break;
			}
		}

		public static UniversalImage FromBMP (Input_Binary _bmp) {

		}


		public static UniversalImage CreateAtlas (UniversalImage[] images, int maxWidth, int maxHeight, int padding, out RectAtlas rectAtlas_) {
			Array.Sort(images, (a,b) => (b.width * b.height).CompareTo(a.width * a.height));
			int[] oldWidth = new int[images.Length];
			int[] oldHeight = new int[images.Length];
			rectAtlas_ = new RectAtlas(maxWidth, maxHeight);
		//Pad images (2px each side)
			int TEMP_PADDING = 2;
			for (int i = 0; i < images.Length; i++) {
				oldWidth[i] = images[i].width;
				oldHeight[i] = images[i].height;
				images[i].PadWidth(oldWidth[i]+TEMP_PADDING*2, TEMP_PADDING);
				images[i].ExtrudeLeft(TEMP_PADDING);
				images[i].ExtrudeRight(TEMP_PADDING);
				images[i].PadHeight(oldHeight[i]+TEMP_PADDING*2, TEMP_PADDING);
				images[i].ExtrudeTop(TEMP_PADDING);
				images[i].ExtrudeBottom(TEMP_PADDING);
			//Map image
				if (!rectAtlas_.Add(images[i].width, images[i].height)) {
					//Log error, break operation???
				}
				else {
					//Access image position with rectMap.rects[i].xPos/yPos
					//i corresponds with the order in which the images/rects were added.

					//We have padding, so we could just output the rectatlas and use a combination of padding and rectatlas to modify the UVs.
					
					//We would have to add a rotation field to the Rect structs in the case of rotated rects but that's not too bad.
				}
			}
		}

	//INDEXING
		public int size => pixels_?.Length ?? 0;
		public Color this[int index] {
			get { ((uint)index < (uint)pixels_.Length) ? pixels_[index].FromArbg() : throw new IndexOutOfRangeException(); }
			set { ((uint)index < (uint)pixels_.Length) ? pixels_[index] = value.ToArbg() : throw new IndexOutOfRangeException(); }
		}
		public Color this[int x, int y] {
			get { ((uint)x < width && (uint)y < height) ? pixels_[y * width + x].FromArbg() : throw new IndexOutOfRangeException(); }
			set { ((uint)x < width && (uint)y < height) ? pixels_[y * width + x] = value.ToArbg() : throw new IndexOutOfRangeException(); }
		}
		public IEnumerator<Color> GetEnumerator() => ((IEnumerable<Color>)pixels_).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => pixels_.GetEnumerator();



	//FUNCTIONS
		public void PadWidth(int newWidth, int offset = 0) {
			if (newWidth <= 0) throw new ArgumentOutOfRangeException(nameof(newWidth));
		// Prevent multiplication overflow
			int totalSize;
			checked {totalSize = this.height * newWidth;}
			int[] newArray = new int[totalSize];
		// Handle the int.MinValue edge case explicitly [Abs(int.MinValue) > int.MaxValue so the math fails]
			if (offset == int.MinValue) throw new OverflowException(nameof(offset));
		// Split offset into direction-safe components
			int srcOffset = 0;
			int dstOffset = 0;
			if (offset < 0) srcOffset = -offset;
			else dstOffset = offset;
		//Transfer within safe bounds
			int transfer = Math.Min(this.width - srcOffset, newWidth - dstOffset);
			if (transfer > 0) for (int y = 0; y < this.height; y++) {
				Array.Copy(this.pixels_, (y * this.width) + srcOffset, newArray, (y * newWidth) + dstOffset, transfer);
			}
			this.pixels_ = newArray;
			this.width = newWidth;
		}

		public void PadHeight(int newHeight, int offset = 0) {
			if (newHeight <= 0) throw new ArgumentOutOfRangeException(nameof(newHeight));
		// Prevent multiplication overflow
			int totalSize;
			checked {totalSize = newHeight * this.width;}
			int[] newArray = new int[totalSize];
		// Handle the int.MinValue edge case explicitly [Abs(int.MinValue) > int.MaxValue so the math fails]
			if (offset == int.MinValue) throw new OverflowException(nameof(offset));
		// Split offset into direction-safe components
			int srcOffset = 0;
			int dstOffset = 0;
			if (offset < 0) srcOffset = -offset;
			else dstOffset = offset;
		// Transfer within safe bounds (rows instead of columns)
			int transfer = Math.Min(this.height - srcOffset, newHeight - dstOffset);
			if (transfer > 0) for (int y = 0; y < transfer; y++) {
				Array.Copy(this.pixels_, (y + srcOffset) * this.width, newArray, (y + dstOffset) * this.width, this.width);
			}
			this.pixels_ = newArray;
			this.height = newHeight;
		}

		public void ExtrudeLeft (int distance) {
			for (int i = distance; i < this.size; i+= this.width) {
				for (int j = 0; j < distance; j++) this.pixels_[i-j-1] = this.pixels_[i];
			}
		}
		public void ExtrudeRight (int distance) {
			for (int i = this.width-distance-1; i < this.size; i+= this.width) {
				for (int j = 0; j < distance; j++) this.pixels_[i+j+1] = this.pixels_[i];
			}
		}
		public void ExtrudeTop (int distance) {
			for (int j = 0; j < distance; j++) {
				Array.Copy(this.pixels_, distance*this.width, this.pixels_, (distance-j-1)*this.width, this.width);
			}
		}
		public void ExtrudeBottom (int distance) {
			for (int j = 0; j < distance; j++) {
				Array.Copy(this.pixels_, this.size-(distance+1)*this.width, this.pixels_, this.size-(distance-j)*this.width, this.width);
			}
		}

	}
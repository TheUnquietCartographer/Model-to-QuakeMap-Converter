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
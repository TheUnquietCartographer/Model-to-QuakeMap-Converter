using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

	public class MaterialDirectory : IEnumerable<KeyValuePair<string, MaterialDirectory.MaterialData>> {

		//Should this be external to MaterialDirectory class?
		public struct MaterialData {
			private uint width_, height_;
			public uint width {get{return width_;}}
			public uint height {get{return height_;}}
			public MaterialData (uint width, uint height) {
				this.width_ = width; this.height_ = height;
			}
		}

		private readonly Dictionary<string, MaterialData> dict_;

		private string path_;
		public string path {get{return path_;}}

		private bool exists_;
		public bool exists {get{return exists_;}}

	//CONSTRUCTOR
		//If not using subdirectory, assume materials are next to the input file.
		//If using subdirectory, assume materials are in a folder next to the input file with the same name as the input file.
		public MaterialDirectory (Input _input, bool usingSubdirectory = true) {
			this.path_ = usingSubdirectory ? $"{input.path}/{input.filename}" : $"{input.path}";
			this.exists_ = Directory.Exists(this.path_);
			this.dict_ = new Dictionary<string, MaterialData>();
			if (this.exists_) {
				string[] files = Directory.GetFiles(this.path_);
				foreach (string s in files) {
					int i = s.LastIndexOf('.');
					if (s.Substring(i).ToLower() != ".bmp") continue;
					using (Input_Binary _bmp = new Input_Binary(s)) {
						uint x, y;
						if (!Extract.BMP.TryGetWidthHeight(_bmp, out x, out y)) continue;
						int h = s.LastIndexOf('/')+1;	//Should also work for -1.
						dict_.Add(s.Substring(h, i-h), new MaterialData(
							x, y
						));
					}
				}
			}
		}

	//INTERFACE GETTERS & FUNCTIONS
		public int Count => dict_.Count;
		public bool IsReadOnly => false;
		public IEnumerable<string> Keys => dict_.Keys;
		public IEnumerable<MaterialData> Values => dict_.Values;
		public MaterialData this[string key] {
			get => dict_[key];
			set => dict_[key] = value;
		}

		public void Add(string key, MaterialData value) => dict_.Add(key, value);
		public bool Remove(string key) => dict_.Remove(key);
		public bool ContainsKey(string key) => dict_.ContainsKey(key);
		//public bool Contains(KeyValuePair<string, MaterialData> kvp) => dict_.Contains(kvp);
		public bool TryGetValue(string key, out MaterialData value) => dict_.TryGetValue(key, out value);
		public void Clear() => dict_.Clear();
		public IEnumerator<KeyValuePair<string, MaterialDirectory.MaterialData>> GetEnumerator() => dict_.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public bool TryAdd(string key, MaterialData value) {
			if (dict_.ContainsKey(key)) return false;
			dict_.Add(key, value);
			return true;
		}
		public bool TryUpdate(string key, MaterialData newValue) {
			if (!dict_.ContainsKey(key)) return false;
			dict_[key] = newValue;
			return true;
		}

	}
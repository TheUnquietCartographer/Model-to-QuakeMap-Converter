using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CollectionExtensions;
using System.IO;

	public class MaterialDirectory : IEnumerable<KeyValuePair<string, Input>> {

		private readonly List<KeyValuePair<string, Input>> list_;

		private string path_;
		public string path {get{return path_;}}

	//CONSTRUCTOR
		//If not using subdirectory, assume materials are next to the input file.
		//If using subdirectory, assume materials are in a folder next to the input file with the same name as the input file.
		public MaterialDirectory (Input _input, bool usingSubdirectory = true) {
			this.path_ = usingSubdirectory ? $"{_input.path}/{_input.filename}" : $"{_input.path}";
			this.list_ = new List<KeyValuePair<string, Input>>();
			if (this.Exists()) {
				string[] files = Directory.GetFiles(this.path_);
				foreach (string filepath in files) {
					int i = filepath.LastIndexOf('.');
					string extension = filepath.Substring(i).ToLower();

					if (extension != ".bmp") continue; //CURRENTLY FORCES .BMP

					int h = filepath.LastIndexOf('/')+1;	//Should also work for -1.
					string key = filepath.Substring(h, i-h);
					list_.Add(new KeyValuePair(key, new Input(filepath)));
				}
			}
		}

	//GETTERS
		public bool Exists () {
			return Directory.Exists(this.path_);
		}
		public bool FileExists (string key) {
			return File.Exists(list_[key].fullpath);
		}
		public string[] KeysWithExistingFiles () {
			if (!DirectoryExists()) return Array.Empty<string>();
			return list_.Where(x => FileExists(x.key)).Select(x => x.key).ToArray();
		}
		public Input[] ExistingFiles () {
			if (!DirectoryExists()) return Array.Empty<Input>();
			return list_.Where(x => FileExists(x.key)).Select(x => x.value).ToArray();
		}



	//IENUMERABLE
		public int Count => list_.Count;
		public bool IsReadOnly => false;
	//Get/set
		public IEnumerable<string> Keys => list_.Select(x => x.key);
		public IEnumerable<Input> Values => list_.Select(x => x.value);
		public Input this[string key] {
			get {
				int i = list_.FindIndex(x => string.Equals(x.key, key, StringComparison.Ordinal));
				if (i == -1) throw new KeyNotFoundException(nameof(key));
				return list_[i].value;
			}
			set {
				int i = list_.FindIndex(x => string.Equals(x.key, key, StringComparison.Ordinal));
				if (i == -1) {
					list_.Add(new KeyValuePair<string, Input>(key, value));
					return;
				}
				list_[i] = new KeyValuePair<string, Input>(list_[i].Key, value);
			}
		}
		//public KeyValuePair ElementAt(int index) => (uint)index < list_.Count ? list_.ElementAt(index) : throw new IndexOutOfRangeException();
		public KeyValuePair<string, Input> this[int index] {
			get => (uint)index < list_.Count ? list_[index] : throw new IndexOutOfRangeException();
			set => (uint)index < list_.Count ? list_[index] = value : throw new IndexOutOfRangeException();
		}
	//Add/remove
		public void Add(string key, Input value) => list_[key] = value;
		public void Remove(string key) {
			for (int i = 0; i < list_.Count; i++) {
				if (string.Equals(list_[i].key, key, StringComparison.Ordinal)) {
					list_.RemoveAt(i);
					return;
				}
			}
			throw new KeyNotFoundException(nameof(key));
		}
		public void RemoveAt(int index) {
			(uint)index < list_.Count ? list_.RemoveAt(index) : throw new IndexOutOfRangeException();
		} 
	//Contains
		public bool ContainsKey(string key) {
			for (int i = 0; i < list_.Count; i++) {
				if (string.Equals(list_[i].key, key, StringComparison.Ordinal)) return true;
			}
			return false;
		}
		//public bool Contains(KeyValuePair<string, Input> kvp) => dict_.Contains(kvp);
		//public bool TryGetValue(string key, out Input value) => dict_.TryGetValue(key, out value);
		///public void Clear() => dict_.Clear();
	//Enumerator
		public IEnumerator<KeyValuePair<string, Input>> GetEnumerator() => list_.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


	}
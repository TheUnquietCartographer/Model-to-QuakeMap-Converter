using System;
using System.IO;

	public class Input {

	//
	//	PROPERTIES
	//
		//Compiler fails to recognise that the setters in the following properties will always assign a non-null value:
#pragma warning disable CS9264
	//Path
		protected string path_ = "";
		public string/*?*/ path {
			get => path_;
			set {
			//If null assign an empty string?
				if (value == null) {
					path_ = "";
				}
			//If 0-length assign an empty string
				else if (value.Length == 0) {
					path_ = "";
				}
			//Else assign value
				else {
					path_ = value;
				}
			}
		}
	//Filename
		protected string filename_ = "";
		public string/*?*/ filename {
			get => filename_;
			set {
			//If null assign an empty string?
				if (value == null) {
					filename_ = "";
				}
			//If 0-length assign an empty string
				else if (value.Length == 0) {
					filename_ = "";
				}
			//Else assign value
				else {
					if (value[0] == '/') {
						filename_ = value.Substring(1);
					}
					else {
						filename_ = value;
					}
				}
			}
		}
	//Extension
		protected string extension_ = "";
		public string/*?*/ extension {
			get => extension_;
			set {
			//If null assign an empty string?
				if (value == null) {
					extension_ = "";
				}
			//If 0-length assign an empty string
				else if (value.Length == 0) {
					extension_ = "";
				}
			//Else assign value
				else {
					if (value[0] != '.') {
						extension_ = '.'+value;
					}
					else {
						extension_ = value;
					}
				}
			}
		}
		//We explicitly declare our backing fields because otherwise they would default to null.
		//We are avoiding null because we want to avoid the presumption that a lack of data makes an instance of this class invalid.
#pragma warning restore CS9264

	//
	//	CONSTRUCTOR
	//
		public Input () {}
		public Input (string fullpath) {
			fullpath = fullpath.Trim('"');
			int i_extension = fullpath.LastIndexOf('.');
			int i_filename = fullpath.LastIndexOf('/');
		//Yes extension
			if (i_extension != -1) {
				while (i_filename > i_extension) i_filename = fullpath.LastIndexOf('/', i_filename);
				this.extension = fullpath.Substring(i_extension);
			//Yes filename & path
				if (i_filename != -1) {
					this.filename = fullpath.Substring(i_filename, i_extension - i_filename);
					this.path = fullpath.Substring(0, i_filename);
				}
			//No filename, yes path
				else {
					this.path = fullpath.Substring(0, i_extension);
				}
			}
		//No extension, yes filename, yes path
			else if (i_filename != -1) {
				this.filename = fullpath.Substring(i_filename);
				this.path = fullpath.Substring(0, i_filename);
			}
		//No extension, no filename, path only
			else {
				this.path = fullpath;
			}
		}

	//
	//	GETTERS
	//
		public string fullname {get{return filename+extension;}}
		public string fullpath {get{return path+'/'+filename+extension;}}
		
	//
	//	FUNCTIONS
	//		
	//LOG DATA
		public virtual void LogData () {
			Log.Multi(
				new Log.ColorLog("Path: ", ConsoleColor.White),
				new Log.ColorLog(this.path, ConsoleColor.Yellow),
				new Log.ColorLog("; Name: ", ConsoleColor.White),
				new Log.ColorLog(this.filename, ConsoleColor.Yellow),
				new Log.ColorLog("; Extension: ", ConsoleColor.White),
				new Log.ColorLog(this.extension, ConsoleColor.Yellow),
				new Log.ColorLog("; Fullname: ", ConsoleColor.White),
				new Log.ColorLog(this.fullname, ConsoleColor.Cyan),
				new Log.ColorLog("; fullpath: ", ConsoleColor.White),
				new Log.ColorLog(this.fullpath, ConsoleColor.Green)
			);
		}

		public static FileStream? OpenOrThrow (string fullpath) {
			try { return File.Open(fullpath, FileMode.Open); }
			catch (FileNotFoundException e) { Console.WriteLine($"File not found: {e.Message}"); }
			catch (DirectoryNotFoundException e) { Console.WriteLine($"Directory not found: {e.Message}"); }
			catch (UnauthorizedAccessException e) { Console.WriteLine($"Access denied: {e.Message}"); }
			catch (Exception e) { Console.WriteLine($"An unexpected error occurred: {e.Message}"); }
			return null;
		}

	}


////////////////////////////////////////////////////////////////////////////////////////////////////


	public abstract class Input_Derived : Input, IDisposable {

		public FileStream? fileStream = null;

		public Input_Derived () : base () {}
		public Input_Derived (string fullpath) : base (fullpath) {}

	//SET STREAM READER OR BINARY READER (DERIVED CLASSES ONLY)
		protected virtual void TrySetFileStream (string _fullpath) {
			this.fileStream = OpenOrThrow(_fullpath);
		}

		public virtual void Dispose () {
			this.fileStream?.Dispose();
		}

	}

//////////////////////////////////////////////////
//
//	PLAIN TEXT INPUT
//
//////////////////////////////////////////////////
	public class Input_PlainText : Input_Derived {

	//
	//	PROPERTIES
	//
		public StreamReader? streamReader = null;

	//
	//	CONSTRUCTOR
	//
		public Input_PlainText (Input _input) : base() {
		//Inherit property (backing field) values
			this.path_ = _input.path;
			this.filename_ = _input.filename;
			this.extension_ = _input.extension;
		//Initialise new properties
			TrySetFileStream(_input.fullpath);
		}
		public Input_PlainText (string fullpath) : base(fullpath) {
			//Calls base constructor
			TrySetFileStream(fullpath);
		}

	//
	//	FUNCTIONS
	//
		protected override void TrySetFileStream (string _fullpath) {
			base.TrySetFileStream(_fullpath);
			if (this.fileStream == null) {
				this.streamReader = null;
				return;
			}
			this.streamReader = new StreamReader(this.fileStream);
		}
		public override void Dispose () {
			this.fileStream?.Dispose();
			this.streamReader?.Dispose();
		}
		public string[] fileContents {get{
				if (this.streamReader == null) return Array.Empty<string>();
				string[] fileContents = this.streamReader!.ReadToEnd().Split('\n');
				for (int i = 0; i < fileContents.Length; i++) fileContents[i] = fileContents[i].Trim();
				return fileContents;
		}}

	}


//////////////////////////////////////////////////
//
//	BINARY INPUT
//
//////////////////////////////////////////////////
	public class Input_Binary : Input_Derived {

	//
	//	PROPERTIES
	//
		public BinaryReader? binaryReader = null;

	//
	//	CONSTRUCTOR
	//
		public Input_Binary (Input _input) : base() {
		//Inherit property (backing field) values
			this.path_ = _input.path;
			this.filename_ = _input.filename;
			this.extension_ = _input.extension;
		//Initialise new properties
			TrySetFileStream(_input.fullpath);
		}
		public Input_Binary (string fullpath) : base(fullpath) {
			//Calls base constructor
			TrySetFileStream(fullpath);
		}

	//
	//	FUNCTIONS
	//
		protected override void TrySetFileStream (string _fullpath) {
			base.TrySetFileStream(_fullpath);
			if (this.fileStream == null) {
				this.binaryReader = null;
				return;
			}
			this.binaryReader = new BinaryReader(this.fileStream);
		}
		public override void Dispose () {
			this.fileStream?.Dispose();
			this.binaryReader?.Dispose();
		}

	}
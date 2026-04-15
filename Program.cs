using System;
using System.Collections.Generic;
using System.IO;
using CollectionExtensions;

	//Declare top-level statements here
	/*
	*	Top-level statements Will act as the Main() function.
	*	Only one Main() can exist.
	*	Better to explicitly declare Main(). If declared inside a class, it will be called automatically.
	*	If not declared inside a class, will throw a warning and have to be called manually.
	*/

namespace Program {

	internal class Program {

		private static void Main () {

		//Wait for input
			while (true) {
				Console.WriteLine("Waiting for input... [\"path/to/file.extension\"]");
				string _userInput = Console.ReadLine()?.Trim() ?? "";
				string[] userInput;
				{
					List<string> userInput_ = new List<string>();
					bool inQuotes = false;
					bool inSubstring = true;
					int substringStart = 0;
					for (int i = 0; i < _userInput.Length; i++) {
						if (_userInput[i] == ' ') {
							if (inQuotes) continue;
							if (!inSubstring) continue;		//In the case where there are multiple spaces.
							userInput_.Add(_userInput.Substring(substringStart, i-1));
							inSubstring = false;
						}
						else {
							if (!inSubstring) {
								substringStart = i;
								inSubstring = true;
							}
							if (_userInput[i] == '\"') {
								inQuotes = !inQuotes;
							}
						}
					}
					if (inSubstring == true) userInput_.Add(_userInput.Substring(substringStart, _userInput.Length));
					userInput = userInput_.ToArray();
				}
				if (userInput.Length == 0) continue;

			//
			//	VALIDATE INPUT FILE
			//
			//0: PATH TO FILE
				Input thisInput = new Input(userInput[0]);
				thisInput.LogData();
				if (thisInput.path == "" || thisInput.filename == "" || thisInput.extension == "") {
					Console.WriteLine("Invalid path.");
					continue;
				}
				if (!File.Exists(thisInput.fullpath)) {
					Console.WriteLine("No such file exists.");
					continue;
				}

			//
			//	TAKE ACTION DEPENDING ON FILETYPE
			//
				switch (thisInput.extension.ToLower()) {
					default:
						thisInput = new Input_PlainText(thisInput);
						Console.WriteLine($"File extension {thisInput.extension} not currently supported.");
						continue;
					case ".3do":
						Input_PlainText _thisInput = new Input_PlainText(thisInput);
						if (!Verify._3DO(in _thisInput)) {
							Console.WriteLine($"{_thisInput.extension} failed verification.");
							continue;
						};
						UniversalMesh? UM = Extract._3DO.Extract(_thisInput);
						if (UM != null) {
							Console.Write("New universal mesh: ");
							UM.LogData();
							Console.WriteLine($"\"{UM.materials.Stringify("\" \"")}\"");
							UM.WriteToFile("test/workingMesh.txt");
							File.WriteAllText("test/output.map", Output.Map.Output(
								UM,		//UniversalMesh _mesh
								320,	//float scaleFactor
								1,		//float rounding
								8,		//int brushThickness
								false,	//bool reverseVertexOrder
								false,	//bool invertFaceNormals
								true,	//bool trianglifyFaces
								false,	//bool swapYZCoordinates
								Output.Map.Format.Valve220,	//Map.Format format
								$"{_thisInput.filename}/"	//string textureDirectory
							));
						}
						break;
				}

			//End waiting for input
			}

		//End Main()
		}

	}

}
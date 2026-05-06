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
		/*
			Console.WriteLine(Translate.JKL_Entities_ToMap.Translate(
				new string[] {
					"0: walkplayer walkplayer -3.044696 2.421065 1.120114 .019783 -134.988342 0 4",
					"1: walkplayer walkplayer 2.39995 .775153 1.120114 0 0 0 41 thingflags=0X20080501",
					"2: walkplayer walkplayer .99995 .878893 -.879886 0 0 0 3 thingflags=0X20080501",
					"3: walkplayer walkplayer -3.20005 -4.78012 .120114 0 0 0 39 thingflags=0X20080501",
					"4: walkplayer walkplayer -2.75005 -2.6 1.120114 0 0 0 5 thingflags=0X20080501",
					"5: ghost ghost -.4 -.7 2 0 0 0 0",
					"6: m10column m10column -.1 .9 .450001 0 0 0 12",
					"7: m10column m10column -.1 -.1 .450001 0 0 0 12",
					"8: m10column m10column -.1 -1.1 .450001 0 0 0 12",
					"9: m10column m10column -.1 -2.1 .450001 0 0 0 12",
					"10: repeatergun repeatergun -2.15931 -.931726 -1.671954 0 0 0 36 thingflags=0X2000400",
					"11: railgun railgun 3.00136 -.113613 -1.75558 0 0 0 40 thingflags=0X2000400",
					"12: crossbow crossbow -1.816637 3.298789 -1.796565 0 0 0 37 thingflags=0X2000400",
					"13: detonator detonator -.746561 -3.61883 -1.8 0 0 0 34 thingflags=0X2000400",
					"14: bactatank bactatank -.199967 -2.800075 1.028885 0 0 0 0",
					"15: bactatank bactatank -.199967 -1.000075 1.028885 0 0 0 0",
					"16: bactatank bactatank -.199967 .999925 1.028885 0 0 0 0",
					"17: bactatank bactatank -.199967 2.999925 1.028885 0 0 0 6",
					"18: shieldrecharge shieldrecharge -.1 1.492434 .021618 0 0 0 12",
					"19: shieldrecharge shieldrecharge -.1 .392434 .021618 0 0 0 12",
					"20: shieldrecharge shieldrecharge -.1 -.607566 .021618 0 0 0 12",
					"21: shieldrecharge shieldrecharge -.1 -1.607566 .021618 0 0 0 12",
					"22: railgun railgun -3.05 -4.378177 .14442 0 0 0 57",
					"23: railcharges railcharges -3.05 -3.778917 .116307 0 0 0 57",
					"24: railcharges railcharges -3.05 -3.978917 .116307 0 0 0 57",
					"25: railcharges railcharges -3.05 -4.178917 .116307 0 0 0 57",
					"26: seqcharge seqcharge -.703957 4.44834 1.000895 0 0 0 30",
					"27: seqcharge seqcharge .296043 4.44834 1.000895 0 0 0 30",
					"28: seqcharge seqcharge 1.29604 4.44834 1.000895 0 0 0 30",
					"29: concrifle concrifle 2.399664 2.053292 1.124753 0 0 0 58",
					"30: powercell powercell -1.194521 -4.782064 -.170546 0 0 0 19",
					"31: energycell energycell -.072319 -4.407506 -.367241 0 0 0 20",
					"32: powercell powercell 1.112549 -3.609924 -.582132 0 0 0 21",
					"33: energycell energycell 2.010551 -2.264506 -.77141 0 0 0 14",
					"34: repeatergun repeatergun 1.95 2.64463 -.871954 0 0 0 59",
					"35: strifle strifle 1.949995 3.344811 -.864823 0 0 0 59",
					"36: crossbow crossbow 1.950273 2.994499 -.896565 0 0 0 59",
					"37: powercell powercell 1.949839 2.794646 -.882132 0 0 0 59",
					"38: energycell energycell 1.949801 3.195774 -.877565 0 0 0 59",
					"39: healthpack healthpack -3.075 1.06 .773701 0 0 0 56",
					"40: healthpack healthpack -3.075 .96 .773701 0 0 0 56",
					"41: healthpack healthpack -3.075 .86 .773701 0 0 0 56",
					"42: fullshield fullshield -.642131 3.494543 .611804 0 0 0 63",
					"43: bactatank bactatank .370361 3.494535 .028885 0 0 0 62",
					"44: powercell powercell 1.382629 3.494646 -.482132 0 0 0 61",
					"45: ghost ghost -.71409 4.03916 .681998 .019783 -169.999985 0 64 thingflags=0X400",
					"46: ghost ghost .273599 4.18559 .099999 .019783 -169.999985 0 65 thingflags=0X400",
					"47: ghost ghost 1.26129 4.34202 -.400001 .019783 -169.999985 0 66 thingflags=0X400",
					"48: lightside lightside -2.05992 3.2351 .2 0 0 0 67",
					"49: walkplayer walkplayer -1.51005 -.1 2.320114 0 0 0 0 thingflags=0X20080501",
					"50: walkplayer walkplayer -2.69 2.69005 .120114 0 -90 0 31 thingflags=0X20080501",
					"51: walkplayer walkplayer -1.37977 4.2946 1.120113 0 -90 0 30 thingflags=0X20080501",
					"52: walkplayer walkplayer 1.02774 4.37207 1.120114 0 90 0 30 thingflags=0X20080501",
					"53: walkplayer walkplayer 2.30005 4.03499 -.879886 0 180 0 18 thingflags=0X20080501",
					"54: walkplayer walkplayer 2.3 1.53494 -.879886 0 90 0 18 thingflags=0X20080501",
					"55: walkplayer walkplayer 1.90509 -2.18325 -.673731 0 90 0 14 thingflags=0X20080501",
					"56: walkplayer walkplayer 1.04957 -3.49414 -.479886 0 0 0 21 thingflags=0X20080501",
					"57: walkplayer walkplayer -.031203 -4.34944 -.269562 0 0 0 20 thingflags=0X20080501",
					"58: walkplayer walkplayer -3.3 -3.28007 .120114 0 -90 0 39 thingflags=0X20080501",
					"59: walkplayer walkplayer -3.3 -1.58007 .120114 0 -90 0 39 thingflags=0X20080501",
					"60: manaboost manaboost 2.500115 1.000189 1.030601 0 0 0 41",
					"61: manaboost manaboost -3.441625 -1.144801 1.030601 0 0 0 4",
					"62: manaboost manaboost -.749129 -3.441881 -1.569399 0 0 0 34 thingflags=0X2000400",
					"63: manaboost manaboost -1.746195 2.981219 -1.569399 0 0 0 37 thingflags=0X2000400",
					"64: manaboost manaboost -1.509885 -2.499811 1.030601 0 0 0 0",
					"65: darkside darkside 3.22359 -.07918 .380462 0 0 0 68 thingflags=0X2000400"
				},
				320, 1, false
			));
			return;
		*/
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
						Console.WriteLine($"File extension {thisInput.extension} not currently supported.");
						continue;

				//3DO
				//Extract from 3DO - Recommend force using texels. Write to UMesh - Recommend forcing Valve220 format.
				//Eventually we're going to have some global settings. Instead of passing all the options into the function. If we do that we should be able to force certain settings, while leaving the option for further expansion of those settings in the future.
					case ".3do":
						using (Input_PlainText _thisInput = new Input_PlainText(thisInput)) {
							if (!Verify._3DO(in _thisInput)) {
								Console.WriteLine($"{_thisInput.extension} failed verification.");
								continue;
							};
							UniversalMesh? UM = Extract._3DO.Extract(_thisInput, true, false);
							if (UM != null) {
								Console.Write("New universal mesh: ");
								UM.LogData();
								Console.WriteLine($"\"{UM.materials.Stringify("\" \"")}\"");
								//UM.WriteToFile("output/workingMesh.txt");
								File.WriteAllText($"output/{_thisInput.filename}.map", Output.Map.Output(
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
						}
						break;

				//JKL

				}

			//End waiting for input
			}

		//End Main()
		}

	}

}
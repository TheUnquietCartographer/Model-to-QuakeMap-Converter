using System.Collections.Generic;
using DoubleExtensions;

namespace Translate {

	public static class JKL_Entities_ToMap {
/*
		private static Dictionary<string, string> quakeCounterparts = new Dictionary<string, string>() {
			{"walkplayer", "info_player_deathmatch"},
			{"strifle", "nailgun"},
			{"crossbow", "supershotgun"},
			{"repeatergun", "supernailgun"},
			{"detonator",},
			{"seqcharge",},
			{"railgun",},
			{"concrifle",},
		};
*/

		public static string Translate (string[] input, double scaleFactor, double rounding, bool swapYZcoordinates) {
			List<Dictionary<string, string>> entities_ = new List<Dictionary<string, string>>();

			string output = "";

			string ParseCoord (string s) {
				return double.Parse(s).Scaled(scaleFactor).Rounded(rounding).ToString();
			}
			string ParseD (string s) {
				return double.Parse(s).ToString();
			}

			for (int i = 0; i < input.Length; i++) {
				string[] line = input[i].Trim().Split(' ');
				entities_.Add(new Dictionary<string, string>());
				entities_[i].Add("classname", line[1]);
				entities_[i].Add("origin", $"{ParseCoord(line[3])} {ParseCoord(line[4])} {ParseCoord(line[5])}");
			//Yaw only
				entities_[i].Add("angle", ParseD(line[7]));
			//Pitch yaw roll
				entities_[i].Add("angles", $"{ParseD(line[6])} {ParseD(line[7])} {ParseD(line[8])}");
			}

			for (int i = 0; i < entities_.Count; i++) {
				output += $"{{\r\n";
				output += $"\t\"classname\" \"{entities_[i]["classname"]}\"\r\n";
				output += $"\t\"origin\" \"{entities_[i]["origin"]}\"\r\n";
				output += $"\t\"angle\" \"{entities_[i]["angle"]}\"\r\n";
				output += $"\t\"angles\" \"{entities_[i]["angles"]}\"\r\n";
				output += $"}}\r\n";
			}

			return output;
		}

	}

} 
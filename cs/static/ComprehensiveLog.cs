using System.IO;

	public static class ComprehensiveLog {

		const string path = "test/comprehensiveLog.txt";

		public static void Reset () {
			File.WriteAllText(path, "");
		}

		public static void Add (params string[] strings) {
			foreach (string s in strings) File.AppendAllText(path, s+"\r\n");
		}

	}
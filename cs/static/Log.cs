using System;

	public static class Log {

		public static bool enable_devLog = true;

		public struct ColorLog {
			public string string_;
			public ConsoleColor color_;
			public ColorLog (string _string, ConsoleColor _color) {
				this.string_ = _string;
				this.color_ = _color;
			}
		}
		public static void Single (string _string, ConsoleColor _color = ConsoleColor.White) {
				Console.ForegroundColor = _color;
				Console.WriteLine(_string);
				Console.ResetColor();
		}
		public static void Multi (params ColorLog[] cLogs) {
			for (int i = 0; i < cLogs.Length; i++) {
				Console.ForegroundColor = cLogs[i].color_;
				Console.Write(cLogs[i].string_);
			}
			Console.ResetColor();
			Console.WriteLine();
		}

	}

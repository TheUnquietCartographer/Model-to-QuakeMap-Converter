using System;
using System.Text;

namespace StringExtensions {

	public static class StringExtensions {

		public static byte[] AsBytes_ASCII(this string s, int length) {
			byte[] output_ = new byte[length];
			byte[] nameInBytes = Encoding.ASCII.GetBytes(s);
			Array.Copy(nameInBytes, 0, output_, 0, Math.Min(nameInBytes.Length, length));
			return output_;
		}

	}

}
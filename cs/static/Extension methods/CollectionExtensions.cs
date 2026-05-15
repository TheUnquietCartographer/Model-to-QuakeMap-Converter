using System.Collections.Generic;
using System.Linq;

namespace CollectionExtensions {

	public static class CollectionExtensions {

		public static bool Contains_OutIndex<T> (this IEnumerable<T> collection, T value, out int index) {
			int i = -1;
			foreach (T t in collection) {
				i++;
				if (EqualityComparer<T>.Default.Equals(t, value)) {
					index = i;
					return true;
				}
			}
			index = -1;
			return false;
		}

		public static string Stringify<T> (this IEnumerable<T>? collection, string separator = ", ") {
			if (collection is null) return "NULL";
			int c = collection.Count();
			if (c == 0) return "";
			string s = collection.ElementAt(0)?.ToString() ?? "null";
			for (int i = 1; i < c; i++) {
				s += $"{separator}{collection.ElementAt(i)?.ToString() ?? "null"}";
			}
			return s;
		}

	}
	
}
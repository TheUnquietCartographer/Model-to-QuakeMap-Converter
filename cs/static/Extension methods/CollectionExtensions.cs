using System.Collections.Generic;
using System.Linq;

namespace CollectionExtensions {

	public static class CollectionExtensions {

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
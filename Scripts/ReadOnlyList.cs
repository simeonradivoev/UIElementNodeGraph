using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NodeEditor
{
	public struct ReadOnlyList<T> : IEnumerable<T>
	{
		private readonly List<T> collection;

		public ReadOnlyList(List<T> collection)
		{
			this.collection = collection;
		}

		public int Count => collection?.Count ?? 0;

		public T this[int index] => collection != null ? collection[index] : default;

		public T First()
		{
			return collection[0];
		}

		public T FirstOrDefault()
		{
			if(collection != null && collection.Count > 0)
				return collection[0];
			return default;
		}

		public bool Any()
		{
			return collection != null && collection.Count > 0;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			if (collection == null) return Enumerable.Empty<T>().GetEnumerator();
			return collection.GetEnumerator();
		}

		public List<T>.Enumerator GetEnumerator()
		{
			return collection.GetEnumerator();
		}
	}
}
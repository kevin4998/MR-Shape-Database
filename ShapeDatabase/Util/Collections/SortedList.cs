using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Properties;

namespace ShapeDatabase.Util.Collections {

	/// <summary>
	/// Represents a specific list where all the entries are sorted in increasing order.
	/// </summary>
	/// <typeparam name="T">The type of objects stored in the list.
	/// All objects in the list shoud implement the <see cref="System.IComparable{T}"/>
	/// interface to allow for ordering.</typeparam>
	public sealed class SortedList<T> : IList<T>, IList, IReadOnlyList<T> 
		where T : System.IComparable<T> {

		#region --- Properties ---

		private const int STARTING_SIZE = 8;

		private T[] array;

		public T this[int index] {
			get => array[index];
			set { if (value is T item) Add(item); }
		}
		object IList.this[int index] {
			get {
				return this[index];
			}
			set {
				if (value != null && value is T)
					this[index] = (T) value;
			}
		}

		public int Count { get; private set; } = 0;
		public bool IsReadOnly {get; private set; }
		public bool IsFixedSize => false;

		public object SyncRoot { get; } = new object();
		public bool IsSynchronized => false;

		#endregion

		#region --- Constructor Methods ---

		public SortedList() : this(STARTING_SIZE) { }

		public SortedList(int capacity) {
			if (capacity < 0)
				throw new ArgumentException(Resources.EX_ExpPosValue, nameof(capacity));

			array = new T[capacity];
		}

		public SortedList(IEnumerable<T> collection) {
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			if (collection is ICollection<T> col) {
				Count = col.Count;
				array = new T[Count];
				col.CopyTo(array, 0);
			} else {
				array = new T[STARTING_SIZE];
				using (IEnumerator<T> enumerator = collection.GetEnumerator()) {
					while (enumerator.MoveNext())
						Add(enumerator.Current);
				}
			}
		}

		#endregion

		#region --- Instance Methods ---

		private void ExpandArray() {
			T[] newArray = new T[array.Length * 2];
			array.CopyTo(newArray, 0);
			array = newArray;
		}

		public void Add(T item) {
			int position = Array.BinarySearch(array, item);
			if (position < 0) position = ~position;

			if (Count == array.Length)
				ExpandArray();

			for(int newPos = Count - 1; newPos > position; /*Incrementation in code*/)
				array[newPos] = array[--newPos];
			array[position] = item;

			Count++;
		}

		int IList.Add(object value) {
			if (value is T item) { 
				Add(item);
				return IndexOf(value);
			}
			return -1;
		}


		public void Clear() {
			Count = 0;
			// Do not keep too much space occupied.
			if (array.Length > STARTING_SIZE)
				array = new T[STARTING_SIZE];
		}

		public bool Contains(T item) => Array.BinarySearch(array, item) != -1;

		public bool Contains(object value) => value is T && Contains((T) value);


		public void CopyTo(T[] array, int arrayIndex) => this.array.CopyTo(array, arrayIndex);

		public void CopyTo(Array array, int arrayIndex) => this.array.CopyTo(array, arrayIndex);


		public IEnumerator<T> GetEnumerator() {
			for (int i = 0; i < Count; i++)
				yield return array[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


		public int IndexOf(T item) => Array.BinarySearch(array, item);

		public int IndexOf(object value) => (value is T) ? IndexOf((T) value) : -1;


		void IList<T>.Insert(int index, T item) => Add(item);

		void IList.Insert(int index, object value) {
			if (value is T item) Add(item);
		}


		public bool Remove(T item) {
			int index = IndexOf(item);
			if (index == -1)
				return false;

			RemoveAt(index);
			return true;
		}

		public void Remove(object value) {
			if (value is T item) Remove(item);
		}

		public void RemoveAt(int index) {
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			int length = --Count;
			for (int i = index; i < length; /*Incrementation in expression*/)
				array[i] = array[++i];
		}

		#endregion

	}

}

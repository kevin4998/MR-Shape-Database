using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
	[DebuggerDisplay("Count = {Count}")]
	public class SortedList<T> : IList<T>, IList, IReadOnlyList<T>, IEquatable<SortedList<T>> where T : System.IComparable<T> {

		#region --- Properties ---

		/// <summary>
		/// The minimum array size which should be used for storing values.
		/// </summary>
		protected const int StartingSize = 8;

		private T[] array;
		private readonly IComparer<T> comparer;

		public virtual T this[int index] {
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

		public virtual int Count { get; private set; } = 0;
		public virtual bool IsReadOnly { get; private set; }
		public virtual bool IsFixedSize => false;

		public virtual object SyncRoot { get; } = new object();
		public virtual bool IsSynchronized => false;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new list where all elements are sorted with a size of 8.
		/// </summary>
		public SortedList() : this(StartingSize) { }

		/// <summary>
		/// Initialises a new list where all elements are sorted with a size of 8.
		/// </summary>
		/// <param name="comparer">The method to compare two values with each other
		/// for sorting.</param>
		public SortedList(IComparer<T> comparer) : this(StartingSize, comparer) { }

		/// <summary>
		/// Initialises a new list where all elements are sorted with the given size.
		/// </summary>
		/// <param name="capacity">The amount of starting space in the list.</param>
		/// <exception cref="ArgumentException">If the given capactiy is below 0.
		/// You can't create an list with negative capacity.</exception>
		public SortedList(int capacity) : this(capacity, Comparer<T>.Default) { }

		/// <summary>
		/// Initialises a new list where all elements are sorted with the given size.
		/// </summary>
		/// <param name="capacity">The amount of starting space in the list.</param>
		/// <param name="comparer">The method to compare two values with each other
		/// for sorting.</param>
		/// <exception cref="ArgumentException">If the given capactiy is below 0.
		/// You can't create an list with negative capacity.</exception>
		public SortedList(int capacity, IComparer<T> comparer) { 
			if (capacity< 0)
				throw new ArgumentException(Resources.EX_ExpPosValue, nameof(capacity));

			array = new T[capacity];
			this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
		}

		/// <summary>
		/// Initialises a new list where all elements are sorted
		/// using the given collection for initial population.
		/// </summary>
		/// <param name="collection">All the elements which should be present
		/// in this sorted list.</param>
		/// <exception cref="ArgumentNullException">If the given collection is
		/// <see langword="null"/> or in other words, does not exist.</exception>
		public SortedList(IEnumerable<T> collection)
			: this(collection, Comparer<T>.Default) { }

		/// <summary>
		/// Initialises a new list where all elements are sorted
		/// using the given collection for initial population.
		/// </summary>
		/// <param name="collection">All the elements which should be present
		/// in this sorted list.</param>
		/// <param name="comparer">The method to compare two values with each other
		/// for sorting.</param>
		/// <exception cref="ArgumentNullException">If the given collection is
		/// <see langword="null"/> or in other words, does not exist.</exception>
		public SortedList(IEnumerable<T> collection, IComparer<T> comparer) {
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			if (collection is ICollection<T> col) {
				Count = col.Count;
				array = new T[Count];
				col.CopyTo(array, 0);
			} else {
				array = new T[StartingSize];
				using (IEnumerator<T> enumerator = collection.GetEnumerator()) {
					while (enumerator.MoveNext()) {
						if (Count == array.Length) {
							T[] newArray = new T[array.Length << 1];
							array.CopyTo(newArray, 0);
							array = newArray;
						}
						array[Count++] = enumerator.Current;
					}
				}
			}
			this.comparer = comparer;
			Array.Sort(array, comparer);
		}

		#endregion

		#region --- Instance Methods ---

		#region -- Interface Methods --

		/// <summary>
		/// Dubbles the size of the internal array for storing values.
		/// All previous values will be copied to the new array.
		/// </summary>
		protected virtual void ExpandArray() {
			T[] newArray = new T[array.Length << 1];
			array.CopyTo(newArray, 0);
			array = newArray;
		}

		public virtual void Add(T item) {
			int position = Array.BinarySearch(array, item, comparer);
			if (position < 0) position = ~position;
			Insert(position, item);
		}

		int IList.Add(object value) {
			if (value is T item) { 
				Add(item);
				return IndexOf(value);
			}
			return -1;
		}


		public virtual void Clear() {
			Count = 0;
			// Do not keep too much space occupied.
			if (array.Length > StartingSize)
				array = new T[StartingSize];
		}

		public virtual bool Contains(T item) => IndexOf(item) < 0;

		public virtual bool Contains(object value) => value is T && Contains((T) value);


		public virtual void CopyTo(T[] array, int arrayIndex) => this.array.CopyTo(array, arrayIndex);

		public virtual void CopyTo(Array array, int arrayIndex) => this.array.CopyTo(array, arrayIndex);


		public virtual IEnumerator<T> GetEnumerator() {
			for (int i = 0; i < Count; i++)
				yield return array[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


		public virtual int IndexOf(T item) => Array.BinarySearch(array, item, comparer);

		public virtual int IndexOf(object value) => (value is T) ? IndexOf((T) value) : -1;


		/// <summary>
		/// Places the provided element at the given position,
		/// assuming that the provided position keeps the array sorted.
		/// </summary>
		/// <param name="index">The place to put this item in the array to keep
		/// it sorted.</param>
		/// <param name="item">The object value which needs to be placed in the array.
		/// </param>
		protected void Insert(int index, T item) {
			if (index < 0)
				throw new ArgumentException(Resources.EX_ExpPosValue, nameof(index));
			if (index > Count)
				index = Count;

			if (Count == array.Length)
				ExpandArray();

			Array.Copy(array, index, array, index + 1, Count - index);
			array[index] = item;

			++Count;
		}

		void IList<T>.Insert(int index, T item) => Add(item);

		void IList.Insert(int index, object value) {
			if (value is T item) Add(item);
		}


		public virtual bool Remove(T item) {
			int index = IndexOf(item);
			if (index < 0)
				return false;

			RemoveAt(index);
			return true;
		}

		public virtual void Remove(object value) {
			if (value is T item) Remove(item);
		}

		public virtual void RemoveAt(int index) {
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			Array.Copy(array, index + 1, array, index, --Count - index);
		}

		#endregion

		#region -- Object Methods --

		public override bool Equals(object obj) {
			return Equals(obj as SortedList<T>);
		}

		public bool Equals(SortedList<T> other) {
			return other != null && array.Equals(other.array);
		}

		public override int GetHashCode() {
			return array.GetHashCode();
		}

		#endregion

		#region -- Operators --

		public static bool operator ==(SortedList<T> left, SortedList<T> right) {
			return (left == null)
				 ? right == null
				 : left.Equals(right);
		}

		public static bool operator !=(SortedList<T> left, SortedList<T> right) {
			return !(left == right);
		}

		#endregion

		#endregion

	}

}

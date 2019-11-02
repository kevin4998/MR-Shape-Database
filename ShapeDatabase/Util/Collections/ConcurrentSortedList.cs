using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Util.Collections {

	/// <summary>
	/// A concurrent list where all the entries are sorted.
	/// This list makes use of locks and other blocking methods to guarantee
	/// that everything is in order.
	/// </summary>
	/// <typeparam name="T">The type of objects stored in the list.
	/// All objects in the list shoud implement the <see cref="System.IComparable{T}"/>
	/// interface to allow for ordering.</typeparam>
	[DebuggerDisplay("Count = {Count}")]
	public class ConcurrentSortedList<T> : SortedList<T>
		where T : System.IComparable<T> {

		#region --- Properties ---

		public override bool IsSynchronized => true;

		public override T this[int index] {
			get {
				lock (SyncRoot) {
					return base[index];
				}
			}
			set {
				lock (SyncRoot) {
					base[index] = value;
				}
			}
		}

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new list where all elements are sorted with a size of 8.
		/// </summary>
		public ConcurrentSortedList() : base() { }

		/// <summary>
		/// Initialises a new list where all elements are sorted with a size of 8.
		/// </summary>
		/// <param name="comparer">The method to compare two values with each other
		/// for sorting.</param>
		public ConcurrentSortedList(IComparer<T> comparer) : base(comparer) { }

		/// <summary>
		/// Initialises a new list where all elements are sorted with the given size.
		/// </summary>
		/// <param name="capacity">The amount of starting space in the list.</param>
		/// <exception cref="ArgumentException">If the given capactiy is below 0.
		/// You can't create an list with negative capacity.</exception>
		public ConcurrentSortedList(int capacity) : base(capacity) { }

		/// <summary>
		/// Initialises a new list where all elements are sorted with the given size.
		/// </summary>
		/// <param name="capacity">The amount of starting space in the list.</param>
		/// <param name="comparer">The method to compare two values with each other
		/// for sorting.</param>
		/// <exception cref="ArgumentException">If the given capactiy is below 0.
		/// You can't create an list with negative capacity.</exception>
		public ConcurrentSortedList(int capacity, IComparer<T> comparer)
			: base(capacity, comparer) { }

		/// <summary>
		/// Initialises a new list where all elements are sorted
		/// using the given collection for initial population.
		/// </summary>
		/// <param name="collection">All the elements which should be present
		/// in this sorted list.</param>
		/// <exception cref="ArgumentNullException">If the given collection is
		/// <see langword="null"/> or in other words, does not exist.</exception>
		public ConcurrentSortedList(IEnumerable<T> collection) : base(collection) { }

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
		public ConcurrentSortedList(IEnumerable<T> collection, IComparer<T> comparer)
			: base(collection, comparer) { }

		#endregion

		#region --- Instance Methods ---

		public override void Add(T item) {
			lock (SyncRoot) {
				base.Add(item);
			}
		}

		public override void Clear() {
			lock (SyncRoot) {
				base.Clear();
			}
		}

		public override bool Contains(T item) {
			lock (SyncRoot) {
				return base.Contains(item);
			}
		}

		public override void CopyTo(T[] array, int arrayIndex) {
			lock (SyncRoot) {
				base.CopyTo(array, arrayIndex);
			}
		}

		public override void CopyTo(Array array, int arrayIndex) {
			lock (SyncRoot) {
				base.CopyTo(array, arrayIndex);
			}
		}

		public override IEnumerator<T> GetEnumerator() {
			lock (SyncRoot) {
				return base.GetEnumerator();
			}
		}

		public override int IndexOf(T item) {
			lock (SyncRoot) {
				return base.IndexOf(item);
			}
		}

		public override bool Remove(T item) {
			lock (SyncRoot) {
				return base.Remove(item);
			}
		}

		public override void RemoveAt(int index) {
			lock (SyncRoot) {
				base.RemoveAt(index);
			}
		}

		#endregion

	}

}

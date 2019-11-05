using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ShapeDatabase.Properties;

namespace ShapeDatabase.Util.Collections {

	/// <summary>
	/// An implementation of <see cref="IWeightedCollection{T}"/> which makes use
	/// of a single array which has roughly the same weights but averages to an integer
	/// value.
	/// <para>
	/// This class makes use of an internal array where the weight of a value
	/// refers to the amount of occurances of this item in the array. To achieve
	/// this the weights will be rounded to the nearest integer value.
	/// </para>
	/// <para>
	/// This collection makes use of O(1) access and O(1) addition.
	/// </para>
	/// </summary>
	/// <typeparam name="T">The type of random object to get.</typeparam>
	public class ArrayWC<T> : IWeightedCollection<T> {

		#region --- Properties ---

		private T[] array = null;
		private List<(T, double)> list = new List<(T, double)>(Settings.RefineVertexNumber);
		private bool readMode = false;

		public int Count => readMode ? list.Count : array.Length;
		public bool IsReadOnly => false;
		public double TotalWeight { get; private set; }

		public bool IsSynchronized => false;
		public object SyncRoot { get; } = new object();

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new empty weighted collection which makes use of an internal
		/// array for O(1) access.
		/// </summary>
		public ArrayWC() { }

		/// <summary>
		/// Initialises a new weighted collection with the specified items, all containing
		/// the same weights.
		/// </summary>
		/// <param name="collection">The collection of items to add to the collection.
		/// </param>
		public ArrayWC(IEnumerable<T> collection)
			: this(collection, Enumerators.Infinite(1d)) { }

		/// <summary>
		/// Initialises a new weighted collection with the specified items,
		/// containing the given weights calculated from the function.
		/// </summary>
		/// <param name="collection">The collection of items to add to the collection.
		/// </param>
		/// <param name="weightFunction">The function to calculate the weight of
		/// a specified item.</param>
		public ArrayWC(IEnumerable<T> collection, Converter<T, double> weightFunction) 
			: this(collection, collection.ConvertTo(weightFunction)) { }

		/// <summary>
		/// Initialises a new weighted collection with the specified items,
		/// containing the given weights presented in the second collection.
		/// </summary>
		/// <param name="collection">The collection of items to add to the collection.
		/// </param>
		/// <param name="weights">The collection of weights for each item. It is assumed
		/// that the items and weights are ordered and therefor each item from the
		/// provided collection has a weight at the same position in the weight
		/// enumerable.</param>
		public ArrayWC(IEnumerable<T> collection, IEnumerable<double> weights) {
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			if (weights == null)
				throw new ArgumentNullException(nameof(weights));

			IEnumerator<T> itemEnumerator = collection.GetEnumerator();
			IEnumerator<double> weightEnumerator = weights.GetEnumerator();

			while (itemEnumerator.MoveNext() && weightEnumerator.MoveNext())
				Add(itemEnumerator.Current, weightEnumerator.Current);
		}

		#endregion

		#region --- Instance Methods ---

		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		private void AllowModifications() {
			if (!readMode) return;
			throw new InvalidOperationException();
		}

		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		private void AllowRetrieval() {
			if (readMode) return;
			lock(SyncRoot) {
				if (readMode) return;

				array = new T[Settings.WeightedVertexArraySize];

				double currentTotal = 0;
				foreach((T, double) item in list)
				{
					double endTotal = currentTotal + item.Item2 / TotalWeight * (Settings.WeightedVertexArraySize - 1);
					for (int j = (int)Math.Ceiling(currentTotal); j < endTotal; j++)
					{
						array[j] = item.Item1;
					}
					currentTotal = endTotal;
				}

				list = null;
				readMode = true;
			}
		}

		public void Add(T item, double weight) {
			if (weight < 0)
				throw new ArgumentException(
					string.Format(
						Settings.Culture,
						Resources.EX_ExpPosValue,
						weight
					), nameof(weight)
				);

			AllowModifications();

			TotalWeight += weight;
			list.Add((item, weight));
		}
		public void Add(T item) => Add(item, 0);
		public bool AddWeight(T item, double weight) {
			Add(item, weight);
			return !readMode;
		}

		public void Clear() {
			AllowModifications();
			list.Clear();
		}
		public bool Contains(T item) {
			if (readMode) return Array.FindIndex(array, x => Equals(x, item)) != -1;
			else		  return list.Find(x => x.Item1.Equals(item)).Equals(default);
		}

		public void CopyTo(T[] array, int index) => CopyTo((Array) array, index);
		public void CopyTo(Array array, int index) {
			if (readMode) Array.Copy(this.array, 0, array, index, Count);
			else		  ((ICollection) list).CopyTo(array, index);
		}

		public T GetElement(Random random) {
			AllowRetrieval();
			return array[random.Next(array.Length)];
		}
		public bool Remove(T item) {
			AllowModifications();
			return list.RemoveAll(x => Equals(item, x)) != 0;
		}

		public IEnumerator<T> GetEnumerator() {
			AllowRetrieval();
			foreach (T item in array)
				yield return item;
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

	}
}

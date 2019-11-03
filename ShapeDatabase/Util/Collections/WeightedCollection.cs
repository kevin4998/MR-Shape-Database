using System;
using System.Collections;
using System.Collections.Generic;
using ShapeDatabase.Properties;

namespace ShapeDatabase.Util.Collections {

	/// <summary>
	/// An implementation of <see cref="IWeightedCollection{T}"/> which makes use of
	/// an internal array and list to contain the weights and item respectively.
	/// The array will always be exact size of the items in this collection and
	/// therefor all <see cref="Add(T)"/> methos run in O(n) item. while the
	/// <see cref="GetElement(Random)"/> method makes use of O(log(n)) time to retrieve
	/// the elements from the array.
	/// </summary>
	/// <typeparam name="T">The type of elements which are stored in the collection.
	/// </typeparam>
	public class WeightedCollection<T> : IWeightedCollection<T> {

		#region --- Properties ---

		private double[] weights;
		private readonly IList<T> collection;

		public int Count => weights.Length;
		public double TotalWeight => weights[weights.Length - 1];

		public object SyncRoot { get; } = new object();
		public bool IsReadOnly => false;
		public bool IsSynchronized => false;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new empty weighted collection.
		/// </summary>
		public WeightedCollection() {
			collection = new List<T>();
			weights = Array.Empty<double>();
		}

		/// <summary>
		/// Initialises a new weighted collection with the provided items,
		/// all having the same weight of 1.
		/// </summary>
		/// <param name="collection">The items which should be part of the collection.
		/// </param>
		/// <exception cref="ArgumentNullException">If the given collection does not
		/// exist.</exception>
		public WeightedCollection(IEnumerable<T> collection) 
			: this(collection, Enumerators.Infinite(0d)) { }

		/// <summary>
		/// Initialises a new weighted collection with the provided items,
		/// the weight of the items is determined by the given function.
		/// </summary>
		/// <param name="collection">The items which should be part of the collection.
		/// </param>
		/// <param name="weightFunction">The function to calculate the weight of
		/// an individual item.</param>
		/// <exception cref="ArgumentNullException">If the given collection or
		/// weight function does not exist.</exception>
		public WeightedCollection(IEnumerable<T> collection, Converter<T, double> weightFunction)
			: this(collection, collection.ConvertTo(weightFunction)) { }

		/// <summary>
		/// Initialises a new weighted collection with the provided items,
		/// the weight of the items is determined by the given enumerable.
		/// </summary>
		/// <param name="collection">The items which should be part of the collection.
		/// </param>
		/// <param name="weights">The collection of weights for each item. It is assumed
		/// that the items and weights are ordered and therefor each item from the
		/// provided collection has a weight at the same position in the weight
		/// enumerable.</param>
		/// <exception cref="ArgumentNullException">If any of the given collections
		/// does not exist.</exception>
		public WeightedCollection(IEnumerable<T> collection, IEnumerable<double> weights) {
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			if (weights == null)
				throw new ArgumentNullException(nameof(weights));

			IEnumerator<T> itemEnumerator = collection.GetEnumerator();
			IEnumerator<double> weightEnumerator = weights.GetEnumerator();

			List<T>		 itemList   = new List<T>();
			List<double> weightList = new List<double>();
			uint lastWeight = 0;

			while (itemEnumerator.MoveNext() && weightEnumerator.MoveNext()) {
				itemList  .Add(itemEnumerator.Current);
				weightList.Add(lastWeight += (uint) weightEnumerator.Current);
			}

			this.collection = itemList;
			this.weights	= weightList.ToArray();
		}

		#endregion

		#region --- Instance Methods ---

		public void Add(T item) => Add(item, 1);

		public void Add(T item, double weight) {
			collection.Add(item);
			int length = Count;
			if (length == 0) {
				weights = new double[] { weight };
			} else {
				double[] newWeights = new double[length + 1];
				Array.Copy(weights, 0, newWeights, 0, length);
				newWeights[length] = newWeights[length - 1] + weight;
				weights = newWeights;
			}

		}

		public bool AddWeight(T item, double weight) {
			int index = collection.IndexOf(item);
			// If it is not present add the item.
			if (index == -1) {
				Add(item, weight);
				return true;
			// If it is present then increase its weight.
			} else {
				while (index < weights.Length)
					weights[index++] += weight;
				return true;
			}
		}


		public T GetElement(Random random) {
			if (random == null)
				throw new ArgumentNullException(nameof(random));

			double value = random.NextDouble() * TotalWeight;
			int pos = Array.BinarySearch(weights, value);
			if (pos < 0)
				pos = ~pos;

			return collection[pos];
		}


		public bool Contains(T item) => collection.Contains(item);

		public bool Remove(T item) {
			int index = collection.IndexOf(item);
			if (index == -1) return false;

			collection.RemoveAt(index);
			int length = Count;
			double[] newWeights = new double[length - 1];
			Array.Copy(weights, 0, newWeights, 0, index++);	// +1 To ignore this element.
			Array.Copy(weights, index, newWeights, index, length - index);
			weights = newWeights;
			return true;
		}


		public void Clear() {
			collection.Clear();
			weights = Array.Empty<double>();
		}


		public void CopyTo(T[] array, int index) => collection.CopyTo(array, index);
		public void CopyTo(Array array, int index) {
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (index < 0)
				throw new ArgumentException(
					string.Format(Settings.Culture,
								  Resources.EX_ExpPosValue,
								  nameof(index)
								  ),
					nameof(index));

			foreach (T item in collection)
				array.SetValue(item, index++);
		}


		public IEnumerator<T> GetEnumerator() => collection.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

	}

}

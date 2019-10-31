using System;
using System.Collections;
using System.Collections.Generic;
using ShapeDatabase.Properties;

namespace ShapeDatabase.Util.Collections {

	/// <summary>
	/// A collection where each item has a specific weight,
	/// meaning that retrieving items with <see cref="GetElement(Random)"/>
	/// has a higher chance of providing items with higher weights than others.
	/// </summary>
	/// <remarks>
	/// A weight is not the same as the chance of retrieving an item.
	/// </remarks>
	/// <example>
	/// Assume that the collection has 3 items:
	/// <list type="bullet">
	///		<item><description>A with weight 1</description></item>
	///		<item><description>B with weight 1 and</description></item>
	///		<item><description>C with weight 3</description></item>
	/// </list>
	/// if you would retrieve one of these items with <see cref="GetElement(Random)"/>,
	/// then you would have a 20% chance of getting item A, 20% for B and 60% for item C.
	/// The percentage is determined by their individual weight divided by the total weight.
	/// </example>
	/// <typeparam name="T">The type of item in the collection.</typeparam>
	public interface IWeightedCollection<T> : ICollection<T>, ICollection {

		/// <summary>
		/// A combination of all the weights of the items in this collection.
		/// This can be used for retrieving items with <see cref="Random"/>.
		/// </summary>
		uint TotalWeight { get; }

		/// <summary>
		/// Provides a new item to the collection with the specified weight.
		/// </summary>
		/// <param name="item">The collection item which can be retrieved with
		/// the specified weight.</param>
		/// <param name="weight">The weight for which this item will be retrieved
		/// from the collection.</param>
		/// <exception cref="ArgumentException">If the value of the weight is below 0.
		/// Weights may only have positive values.</exception>
		void Add(T item, uint weight);

		/// <summary>
		/// Increments the weight of the specified item.
		/// If this item is not present then it will be added to the collection.
		/// </summary>
		/// <param name="item">The collection item which can be retrieved with
		/// the specified weight.</param>
		/// <param name="weight">The amount for how much the weight for which this item
		/// will be retrieved from the collection should be increased.</param>
		/// <returns><see langword="true"/> if the value was incremented and
		/// <see langword="false"/> if it was newly added.</returns>
		/// <exception cref="ArgumentException">If the value of the weight is below 0.
		/// Weights may only have positive values.</exception>
		bool AddWeight(T item, uint weight);

		/// <summary>
		/// Provides a random item from the collection where return chance is based
		/// on their weights.
		/// </summary>
		/// <param name="random">The randomizer which helps to determine the item
		/// which can be returned.</param>
		/// <returns>A random item from the collection.</returns>
		/// <exception cref="ArgumentNullException">If no randomiser is provided.
		/// </exception>
		T GetElement(Random random);

	}

	/// <summary>
	/// A class to provide extra functionality for the <see cref="IWeightedCollection{T}"/>.
	/// </summary>
	public static class WeightedListEx {

		/// <summary>
		/// Provides a new item to the collection with a weight of 1.
		/// </summary>
		/// <typeparam name="T">The type of items which are stored in the collection.
		/// </typeparam>
		/// <param name="collection">The collection where this item will be inserted in.
		/// </param>
		/// <param name="item">The collection item which can be retrieved with
		/// the specified weight.</param>
		/// <exception cref="ArgumentNullException">If the given collection does not exist.
		/// </exception>
		public static void Add<T>(this IWeightedCollection<T> collection, T item) {
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			collection.Add(item, 1);
		}

		/// <summary>
		/// Provides a new item to the collection with the specified weight.
		/// </summary>
		/// <typeparam name="T">The type of items which are stored in the collection.
		/// </typeparam>
		/// <param name="collection">The collection where this item will be inserted in.
		/// </param>
		/// <param name="item">The collection item which can be retrieved with
		/// the specified weight.</param>
		/// <param name="weight">The weight for which this item will be retrieved
		/// from the collection.</param>
		/// <exception cref="ArgumentNullException">If the given collection does not exist.
		/// </exception>
		/// <exception cref="ArgumentException">If the value of the weight is below 0.
		/// Weights may only have positive values.</exception>
		public static void Add<T>(this IWeightedCollection<T> collection, T item, int weight) {
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			if (weight < 0)
				throw new ArgumentException(
					string.Format(Settings.Culture,
								  Resources.EX_ExpPosValue,
								  nameof(weight)
								  ),
					nameof(weight)
					);

			collection.Add(item, (uint) weight);
		}

		/// <summary>
		/// Provides a random item from the collection where return chance is based
		/// on their weights.
		/// </summary>
		/// <typeparam name="T">The type of items which are stored in the collection.
		/// </typeparam>
		/// <param name="collection">The collection where this item will be retrieved from.
		/// </param>
		/// <returns>A random item from the collection.</returns>
		/// <exception cref="ArgumentNullException">If the given collection does not exist.
		/// </exception>
		public static T GetElement<T>(this IWeightedCollection<T> collection) {
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			return collection.GetElement(new Random());
		}

	}

}

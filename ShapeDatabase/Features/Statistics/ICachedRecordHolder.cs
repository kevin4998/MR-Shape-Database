using System;
using System.Collections.Generic;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// An extension of <see cref="IRecordHolder{T}"/> to allow for caching of values
	/// which can be shared between Records.
	/// </summary>
	/// <typeparam name="T">The type of object which will be converted to
	/// <see cref="Record"/>s. This specific object will be used when calculating
	/// statistics.</typeparam>
	public interface ICachedRecordHolder<T> : IRecordHolder<T> {

		/// <summary>
		/// A collection of stored values which can be used when calculating measures.
		/// </summary>
		ICache Cache { get; }


		/// <summary>
		/// Provides a new measurement to be taken for the objects in the database.
		/// </summary>
		/// <param name="measureName">The unique name of the measurement.</param>
		/// <param name="provider">The function to retrieve this measure.</param>
		/// <param name="overwrite">If a previous measure should be overwritten with
		/// the same name.</param>
		/// <returns>The current object for chaining.</returns>
		/// <exception cref="ArgumentNullException">If any of the given parameters
		/// is <see langword="null"/>.</exception>
		ICachedRecordHolder<T> AddMeasure(string measureName,
										  Func<T, ICache, object> provider,
										  bool overwrite = false);

		/// <summary>
		/// Provides a new measurement to be taken for the objects in the database.
		/// </summary>
		/// <param name="measureName">The unique name of the measurement.</param>
		/// <param name="provider">The function to retrieve this measure.</param>
		/// <param name="overwrite">If a previous measure should be overwritten with
		/// the same name.</param>
		/// <returns>The current object for chaining.</returns>
		/// <exception cref="ArgumentNullException">If any of the given parameters
		/// is <see langword="null"/>.</exception>
		new ICachedRecordHolder<T> AddMeasure(string measureName,
											  Func<T, object> provider,
											  bool overwrite = false);


		/// <summary>
		/// Takes a snapshot of the provided database and calculates statistics
		/// for each item present in it.
		/// </summary>
		/// <param name="library">The database containing values to get measurements
		/// for.</param>
		/// <returns>The current object for chaining.</returns>
		/// <exception cref="ArgumentNullException">If the given library
		/// is <see langword="null"/>.</exception>
		new ICachedRecordHolder<T> TakeSnapShot(IEnumerable<T> library);

		/// <summary>
		/// Deletes all previous snapshot values so a new one can be taken.
		/// This should always be performed before taking a new snapshot.
		/// </summary>
		/// <returns>The current object for chaining.</returns>
		new ICachedRecordHolder<T> Reset();

	}

	/// <summary>
	/// A class containing extension and helper methods for cached record holders.
	/// </summary>
	public static class CachedRecordHolderEx {

		/// <summary>
		/// Provides a new measurement to be taken for the objects in the database.
		/// </summary>
		/// <param name="holder">The recordholder that should get another measure.
		/// </param>
		/// <param name="measure">A tuple containing the name and the operations
		/// to provide a measurement of an object.</param>
		/// <param name="overwrite">If a previous measure should be overwritten with
		/// the same name.</param>
		/// <returns>The current object for chaining.</returns>
		/// <exception cref="ArgumentNullException">If the given holder is
		/// <see langword="null"/> or any of the provided measurements is
		/// <see langword="null"/>.</exception>
		public static ICachedRecordHolder<T> AddMeasure<T>(
				this ICachedRecordHolder<T> holder,
				(string name, Func<T, object> func) measure,
				bool overwrite = false) {
			if (holder == null)
				throw new ArgumentNullException(nameof(holder));

			return holder.AddMeasure(measure.name, measure.func, overwrite);
		}

		/// <summary>
		/// Provides new measurements to be taken for the objects in the database.
		/// </summary>
		/// <param name="holder">The recordholder that should get another measure.
		/// </param>
		/// <param name="measures">A collection of tuples containing the name
		/// and the operations to provide a measurement of an object.</param>
		/// <returns>The current object for chaining.</returns>
		/// <exception cref="ArgumentNullException">If the given holder is
		/// <see langword="null"/> or any of the provided measurements is
		/// <see langword="null"/>.</exception>
		public static ICachedRecordHolder<T> AddMeasure<T>(
				this ICachedRecordHolder<T> holder,
				params (string, Func<T, object>)[] measures) {
			if (holder == null)
				throw new ArgumentNullException(nameof(holder));

			return holder.AddMeasure(false, measures);
		}

		/// <summary>
		/// Provides new measurements to be taken for the objects in the database.
		/// </summary>
		/// <param name="holder">The recordholder that should get another measure.
		/// </param>
		/// <param name="overwrite">If a previous measure should be overwritten with
		/// the same name.</param>
		/// <param name="measures">A collection of tuples containing the name
		/// and the operations to provide a measurement of an object.</param>
		/// <returns>The current object for chaining.</returns>
		/// <exception cref="ArgumentNullException">If the given holder is
		/// <see langword="null"/> or any of the provided measurements is
		/// <see langword="null"/>.</exception>
		public static ICachedRecordHolder<T> AddMeasure<T>(
				this ICachedRecordHolder<T> holder,
				bool overwrite,
				params (string, Func<T, object>)[] measures) {
			if (holder == null)
				throw new ArgumentNullException(nameof(holder));

			foreach ((string name, Func<T, object> func) in measures)
				holder = holder.AddMeasure(name, func, overwrite);

			return holder;
		}



		/// <summary>
		/// Provides a new measurement to be taken for the objects in the database.
		/// </summary>
		/// <param name="holder">The recordholder that should get another measure.
		/// </param>
		/// <param name="measure">A tuple containing the name and the operations
		/// to provide a measurement of an object.</param>
		/// <param name="overwrite">If a previous measure should be overwritten with
		/// the same name.</param>
		/// <returns>The current object for chaining.</returns>
		/// <exception cref="ArgumentNullException">If the given holder is
		/// <see langword="null"/> or any of the provided measurements is
		/// <see langword="null"/>.</exception>
		public static ICachedRecordHolder<T> AddMeasure<T>(
				this ICachedRecordHolder<T> holder,
				(string name, Func<T, ICache, object> func) measure,
				bool overwrite = false) {
			if (holder == null)
				throw new ArgumentNullException(nameof(holder));

			return holder.AddMeasure(measure.name, measure.func, overwrite);
		}

		/// <summary>
		/// Provides new measurements to be taken for the objects in the database.
		/// </summary>
		/// <param name="holder">The recordholder that should get another measure.
		/// </param>
		/// <param name="measures">A collection of tuples containing the name
		/// and the operations to provide a measurement of an object.</param>
		/// <returns>The current object for chaining.</returns>
		/// <exception cref="ArgumentNullException">If the given holder is
		/// <see langword="null"/> or any of the provided measurements is
		/// <see langword="null"/>.</exception>
		public static ICachedRecordHolder<T> AddMeasure<T>(
				this ICachedRecordHolder<T> holder,
				params (string, Func<T, ICache, object>)[] measures) {
			if (holder == null)
				throw new ArgumentNullException(nameof(holder));

			return holder.AddMeasure(false, measures);
		}

		/// <summary>
		/// Provides new measurements to be taken for the objects in the database.
		/// </summary>
		/// <param name="holder">The recordholder that should get another measure.
		/// </param>
		/// <param name="overwrite">If a previous measure should be overwritten with
		/// the same name.</param>
		/// <param name="measures">A collection of tuples containing the name
		/// and the operations to provide a measurement of an object.</param>
		/// <returns>The current object for chaining.</returns>
		/// <exception cref="ArgumentNullException">If the given holder is
		/// <see langword="null"/> or any of the provided measurements is
		/// <see langword="null"/>.</exception>
		public static ICachedRecordHolder<T> AddMeasure<T>(
				this ICachedRecordHolder<T> holder,
				bool overwrite,
				params (string, Func<T, ICache, object>)[] measures) {
			if (holder == null)
				throw new ArgumentNullException(nameof(holder));

			foreach ((string name, Func<T, ICache, object> func) in measures)
				holder = holder.AddMeasure(name, func, overwrite);

			return holder;
		}

	}

}

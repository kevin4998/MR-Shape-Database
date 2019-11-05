using System;
using System.Collections;
using System.Collections.Generic;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// An interface which is able to derive a collection of measures
	/// from a specific database.
	/// </summary>
	public interface IRecordHolder : IEnumerable<Record> {

		/// <summary>
		/// Specifies if no snapshot has been taken yet by this <see cref="RecordHolder"/>.
		/// </summary>
		bool IsActive { get; }

		/// <summary>
		/// Specifies if the <see cref="IRecordHolder"/> is currently taking a snapshot.
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// The last moment in time when a snapshot was taken by this
		/// <see cref="IRecordHolder"/>.
		/// </summary>
		DateTime SnapshotTime { get; }

		/// <summary>
		/// A collection of names for each measure which will be taken.
		/// These names are unique and ordered in the way that they will be taken
		/// for each object.
		/// </summary>
		IEnumerable<string> MeasureNames { get; }

		/// <summary>
		/// The collection of <see cref="Record"/>s which was made during the last
		/// snapshot.
		/// </summary>
		ICollection<Record> Records { get; }

		/// <summary>
		/// Deletes all previous snapshot values so a new one can be taken.
		/// This should always be performed before taking a new snapshot.
		/// </summary>
		/// <returns>The current object for chaining.</returns>
		IRecordHolder Reset();
	}

	/// <summary>
	/// An interface which is able to derive a collection of measures
	/// from a specific database.
	/// </summary>
	/// <typeparam name="T">The type of object which will be converted to
	/// <see cref="Record"/>s. This specific object will be used when calculating
	/// statistics.</typeparam>
	public interface IRecordHolder<T> : IRecordHolder, IEnumerable<Record> {

		/// <summary>
		/// Provides a new measurement to be taken for the objects in the database.
		/// </summary>
		/// <param name="measureName">The unique name of the measurement.</param>
		/// <param name="provider">The function to retrieve this measure.</param>
		/// <param name="overwrite">If a previous measure should be overwritten with
		/// the same name.</param>
		/// <returns>The current object for chaining.</returns>
		IRecordHolder<T> AddMeasure(string measureName, Func<T, object> provider, bool overwrite = false);

		/// <summary>
		/// Takes a snapshot of the provided database and calculates statistics
		/// for each item present in it.
		/// </summary>
		/// <param name="library">The database containing values to get measurements
		/// for.</param>
		/// <returns>The current object for chaining.</returns>
		IRecordHolder<T> TakeSnapShot(IEnumerable<T> library);

		/// <summary>
		/// Deletes all previous snapshot values so a new one can be taken.
		/// This should always be performed before taking a new snapshot.
		/// </summary>
		/// <returns>The current object for chaining.</returns>
		new IRecordHolder<T> Reset();

	}

	/// <summary>
	/// A class containing extension and helper methods for record holders.
	/// </summary>
	public static class RecordHolderEx {

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
		public static IRecordHolder<T> AddMeasure<T>(this IRecordHolder<T> holder,
				(string name, Func<T, object> func) measure, bool overwrite = false) {
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
		public static IRecordHolder<T> AddMeasure<T>(this IRecordHolder<T> holder,
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
		public static IRecordHolder<T> AddMeasure<T>(this IRecordHolder<T> holder,
			bool overwrite, params (string, Func<T, object>)[] measures) {
			if (holder == null)
				throw new ArgumentNullException(nameof(holder));

			foreach ((string name, Func<T, object> func) in measures)
				holder = holder.AddMeasure(name, func, overwrite);

			return holder;
		}
	}
}
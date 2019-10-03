using System;
using System.Collections.Generic;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// An interface which is able to derive a collection of measures
	/// from a specific database.
	/// </summary>
	/// <typeparam name="T">The type of object which will be converted to
	/// <see cref="Record"/>s. This specific object will be used when calculating
	/// statistics.</typeparam>
	public interface IRecordHolder<T> : IEnumerable<Record> {

		/// <summary>
		/// Specifies if no snapshot has been taken yet by this <see cref="RecordHolder"/>.
		/// </summary>
		bool IsActive { get; }
		/// <summary>
		/// Specifies if the <see cref="RecordHolder"/> is currently taking a snapshot.
		/// </summary>
		bool IsEmpty { get; }
		/// <summary>
		/// A collection of names for each measure which will be taken.
		/// These names are unique and ordered in the way that they will be taken
		/// for each object.
		/// </summary>
		IEnumerable<string> MeasureNames { get; }
		/// <summary>
		/// A collection of measures which will be taken of all the objects in
		/// a provided database during the next snapshot.
		/// </summary>
		IEnumerable<(string, Func<T, object>)> Measures { get; }
		/// <summary>
		/// The collection of <see cref="Record"/>s which was made during the last
		/// snapshot.
		/// </summary>
		ICollection<Record> Records { get; }
		/// <summary>
		/// The last moment in time when a snapshot was taken by this
		/// <see cref="RecordHolder"/>.
		/// </summary>
		DateTime SnapshotTime { get; }

		/// <summary>
		/// Provides a new measurement to be taken for the objects in the database.
		/// </summary>
		/// <param name="measure">A tuple containing the name and the operations
		/// to provide a measurement of an object.</param>
		/// <param name="overwrite">If a previous measure should be overwritten with
		/// the same name.</param>
		/// <returns>The current object for chaining.</returns>
		IRecordHolder<T> AddMeasure((string, Func<T, object>) measure, bool overwrite = false);
		/// <summary>
		/// Provides new measurements to be taken for the objects in the database.
		/// </summary>
		/// <param name="overwrite">If a previous measure should be overwritten with
		/// the same name.</param>
		/// <param name="measures">A collection of tuples containing the name
		/// and the operations to provide a measurement of an object.</param>
		/// <returns>The current object for chaining.</returns>
		IRecordHolder<T> AddMeasure(bool overwrite, params (string, Func<T, object>)[] measures);
		/// <summary>
		/// Provides new measurements to be taken for the objects in the database.
		/// </summary>
		/// <param name="measures">A collection of tuples containing the name
		/// and the operations to provide a measurement of an object.</param>
		/// <returns>The current object for chaining.</returns>
		IRecordHolder<T> AddMeasure(params (string, Func<T, object>)[] measures);
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
		/// Deletes all previous snapshot values so a new one can be taken.
		/// This should always be performed before taking a new snapshot.
		/// </summary>
		/// <returns>The current object for chaining.</returns>
		IRecordHolder<T> Reset();
		/// <summary>
		/// Takes a snapshot of the provided database and calculates statistics
		/// for each item present in it.
		/// </summary>
		/// <param name="library">The database containing values to get measurements
		/// for.</param>
		/// <returns>The current object for chaining.</returns>
		IRecordHolder<T> TakeSnapShot(IEnumerable<T> library);

	}
}
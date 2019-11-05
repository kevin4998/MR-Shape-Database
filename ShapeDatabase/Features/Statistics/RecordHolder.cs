using System;
using System.Collections;
using System.Collections.Generic;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// A class containing a collection of <see cref="Record"/>s
	/// which is able to create snapshots of databases with its specific measures.
	/// </summary>
	/// <typeparam name="T">The object which will be used for making records.</typeparam>
	public class RecordHolder<T> : IRecordHolder<T> {

		#region --- Properties ---

		private readonly IDictionary<string, Func<T, object>> measures =
			new Dictionary<string, Func<T, object>>();
		protected Func<T, string> NameProvider { get; }

		/// <summary>
		/// Whether the recordholder is still empty.
		/// </summary>
		public virtual bool IsEmpty => SnapshotTime == DateTime.MinValue;

		/// <summary>
		/// Whether a snapshot is (currently) being taken.
		/// </summary>
		public virtual bool IsActive { get; protected set; } = false;

		/// <summary>
		/// The last time a snapshot was being taken.
		/// </summary>
		public virtual DateTime SnapshotTime { get; protected set; } = DateTime.MinValue;

		/// <summary>
		/// The collection of records which was made during the last snapshot.
		/// </summary>
		public virtual ICollection<Record> Records { get; private set; } = new List<Record>();
		
		/// <summary>
		/// A collection of measures which will be taken of all the objects in
		/// a provided database during the next snapshot.
		/// </summary>
		private IEnumerable<(string, Func<T, object>)> Measures {
			get {
				foreach (KeyValuePair<string, Func<T, object>> pair in measures)
					yield return (pair.Key, pair.Value);
			}
		}

		/// <summary>
		/// A collection of names for each measure which will be taken.
		/// These names are unique and ordered in the way that they will be taken
		/// for each object.
		/// </summary>
		public virtual IEnumerable<string> MeasureNames {
			get {
				// Using the KeyValuePairs instead of the Name Set to guarantee ordering.
				foreach (KeyValuePair<string, Func<T, object>> pair in measures)
					yield return pair.Key;
			}
		}

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instantiates a new <see cref="RecordHolder"/> with no measurements.
		/// These measurements can later be added with
		/// <see cref="AddMeasure(string, Func{T, object}, bool)"/>.
		/// </summary>
		/// <param name="nameProvider">The formula to get a unique name
		/// for the object.</param>
		public RecordHolder(Func<T, string> nameProvider) {
			this.NameProvider = nameProvider
				?? throw new ArgumentNullException(nameof(nameProvider));
		}

		/// <summary>
		/// Instantiates a new <see cref="RecordHolder"/> with the specified
		/// measurements.
		/// </summary>
		/// <param name="nameProvider">The formula to get a unique name
		/// for the object.</param>
		/// <param name="measures">A collection of measurements which should be
		/// taken for each object in the database.</param>
		/// <exception cref="ArgumentNullException">If a measurement is
		/// <see langword="null"/> or any of its properties is <see langword="null"/>.</exception>
		public RecordHolder(Func<T, string> nameProvider,
							params (string, Func<T, object>)[] measures)
			: this(nameProvider) {
			this.AddMeasure(measures);
		}

		#endregion

		#region --- Methods ---

		/// <summary>
		/// Provides a new measurement to be taken for the objects in the database.
		/// </summary>
		/// <param name="measureName">The unique name of the measurement.</param>
		/// <param name="provider">The function to retrieve this measure.</param>
		/// <param name="overwrite">If a previous measure should be overwritten with
		/// the same name.</param>
		/// <returns>The current object for chaining.</returns>
		public virtual IRecordHolder<T> AddMeasure(string measureName, Func<T, object> provider,
										bool overwrite = false) {
			if (measureName == null)
				throw new ArgumentNullException(nameof(measureName));
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

			if (overwrite || !measures.ContainsKey(measureName))
				measures[measureName] = provider;

			return this;
		}

		/// <summary>
		/// Deletes all previous snapshot values so a new one can be taken.
		/// This should always be performed before taking a new snapshot.
		/// </summary>
		/// <returns>The current object for chaining.</returns>
		public virtual IRecordHolder<T> Reset() {
			SnapshotTime = DateTime.MinValue;
			Records = new List<Record>();
			return this;
		}

		IRecordHolder IRecordHolder.Reset() => Reset();

		/// <summary>
		/// Takes a snapshot of the provided database and calculates statistics
		/// for each item present in it.
		/// </summary>
		/// <param name="library">The database containing values to get measurements
		/// for.</param>
		/// <returns>The current object for chaining.</returns>
		public virtual IRecordHolder<T> TakeSnapShot(IEnumerable<T> library) {
			if (!IsEmpty || IsActive)
				throw new SnapShotException();
			if (library == null)
				throw new ArgumentNullException(nameof(library));

			IsActive = true;
			SnapshotTime = DateTime.Now;
			IList<Record> localRecords = new List<Record>();
			foreach (T entry in library)
				localRecords.Add(EntrySnapShot(NameProvider(entry), entry));

			Records = localRecords;
			IsActive = false;
			return this;
		}

		/// <summary>
		/// Creates a single record for a single item from the database.
		/// This <see cref="Record"/> contains all the measurements which can be taken.
		/// </summary>
		/// <param name="entry">The database item to get values from.</param>
		/// <param name="entryName">The name of the entry to remember in the record.</param>
		/// <returns>The current object for chaining.</returns>
		protected virtual Record EntrySnapShot(string entryName, T entry) {
			Record record = new Record(SnapshotTime, entryName);
			foreach ((string name, Func<T, object> provider) in Measures)
				record.AddMeasure(name, provider(entry));
			return record;
		}

		/// <summary>
		/// Gets an enumerator for the records. 
		/// </summary>
		/// <returns>The enumerator</returns>
		public IEnumerator<Record> GetEnumerator() {
			return Records.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		#endregion

	}
}

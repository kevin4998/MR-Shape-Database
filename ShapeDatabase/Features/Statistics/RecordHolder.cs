using System;
using System.Collections;
using System.Collections.Generic;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// A class containing a collection of <see cref="Record"/>s
	/// which is able to create snapshots of databases with its specific measures.
	/// </summary>
	public class RecordHolder<T> : IRecordHolder<T> {

		#region --- Properties ---

		private readonly IDictionary<string, Func<T, object>> measures =
			new Dictionary<string, Func<T, object>>();
		private readonly Func<T, string> nameProvider;


		public bool IsEmpty => SnapshotTime == DateTime.MinValue;
		public bool IsActive { get; private set; } = false;

		public DateTime SnapshotTime { get; private set; } = DateTime.MinValue;
		public ICollection<Record> Records { get; private set; } = new List<Record>();
		public IEnumerable<(string, Func<T, object>)> Measures {
			get {
				foreach (KeyValuePair<string, Func<T, object>> pair in measures)
					yield return (pair.Key, pair.Value);
			}
		}
		public IEnumerable<string> MeasureNames {
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
		public RecordHolder(Func<T, string> nameProvider) {
			this.nameProvider = nameProvider
				?? throw new ArgumentNullException(nameof(nameProvider));
		}
		/// <summary>
		/// Instantiates a new <see cref="RecordHolder"/> with the specified
		/// measurements.
		/// </summary>
		/// <param name="measures">A collection of measurements which should be
		/// taken for each object in the database.</param>
		/// <exception cref="ArgumentNullException">If a measurement is
		/// <see langword="null"/> or any of its properties is <see langword="null"/>.
		/// </exception>
		public RecordHolder(Func<T, string> nameProvider,
							params (string, Func<T, object>)[] measures) 
			: this(nameProvider) {
			this.AddMeasure(measures);
		}

		#endregion

		#region --- Methods ---

		public IRecordHolder<T> AddMeasure(string measureName, Func<T, object> provider,
										bool overwrite = false) {
			if (measureName == null)
				throw new ArgumentNullException(nameof(measureName));
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

			if (overwrite || !measures.ContainsKey(measureName))
				measures[measureName] = provider;

			return this;
		}


		public IRecordHolder<T> Reset() {
			SnapshotTime = DateTime.MinValue;
			Records = new List<Record>();
			return this;
		}
		IRecordHolder IRecordHolder.Reset() => Reset();
		public IRecordHolder<T> TakeSnapShot(IEnumerable<T> library) {
			if (!IsEmpty || IsActive)
				throw new SnapShotException();
			if (library == null)
				throw new ArgumentNullException(nameof(library));

			IsActive = true;
			SnapshotTime = DateTime.Now;
			IList<Record> localRecords = new List<Record>();
			foreach (T entry in library)
				localRecords.Add(EntrySnapShot(nameProvider(entry), entry));

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
		private Record EntrySnapShot(string entryName, T entry) {
			Record record = new Record(SnapshotTime, entryName);
			foreach ((string name, Func<T, object> provider) in Measures)
				record.AddMeasure(name, provider(entry));
			return record;
		}



		public IEnumerator<Record> GetEnumerator() {
			return Records.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		#endregion

	}
}

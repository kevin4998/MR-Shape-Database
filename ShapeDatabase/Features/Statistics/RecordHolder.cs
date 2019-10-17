using System;
using System.Collections;
using System.Collections.Generic;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// A class containing a collection of <see cref="Record"/>s
	/// which is able to create snapshots of databases with its specific measures.
	/// </summary>
	public class RecordHolder : IRecordHolder<MeshEntry> {

		#region --- Properties ---

		private readonly IDictionary<string, Func<MeshEntry, object>> measures =
			new Dictionary<string, Func<MeshEntry, object>>();


		public bool IsEmpty => SnapshotTime == DateTime.MinValue;
		public bool IsActive { get; private set; } = false;

		public DateTime SnapshotTime { get; private set; } = DateTime.MinValue;
		public ICollection<Record> Records { get; private set; } = new List<Record>();
		public IEnumerable<(string, Func<MeshEntry, object>)> Measures {
			get {
				foreach (KeyValuePair<string, Func<MeshEntry, object>> pair in measures)
					yield return (pair.Key, pair.Value);
			}
		}
		public IEnumerable<string> MeasureNames {
			get {
				// Using the KeyValuePairs instead of the Name Set to guarantee ordering.
				foreach (KeyValuePair<string, Func<MeshEntry, object>> pair in measures)
					yield return pair.Key;
			}
		}

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instantiates a new <see cref="RecordHolder"/> with no measurements.
		/// These measurements can later be added with
		/// <see cref="AddMeasure(string, Func{MeshEntry, object}, bool)"/>.
		/// </summary>
		public RecordHolder() { }
		/// <summary>
		/// Instantiates a new <see cref="RecordHolder"/> with the specified
		/// measurements.
		/// </summary>
		/// <param name="measures">A collection of measurements which should be
		/// taken for each object in the database.</param>
		/// <exception cref="ArgumentNullException">If a measurement is
		/// <see langword="null"/> or any of its properties is <see langword="null"/>.
		/// </exception>
		public RecordHolder(params (string, Func<MeshEntry, object>)[] measures) {
			AddMeasure(measures);
		}

		#endregion

		#region --- Methods ---


		public IRecordHolder<MeshEntry> AddMeasure(string measureName, Func<MeshEntry, object> provider,
										bool overwrite = false) {
			if (measureName == null)
				throw new ArgumentNullException(nameof(measureName));
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

			if (overwrite || !measures.ContainsKey(measureName))
				measures[measureName] = provider;

			return this;
		}
		public IRecordHolder<MeshEntry> AddMeasure((string, Func<MeshEntry, object>) measure,
										bool overwrite = false) {
			return AddMeasure(measure.Item1, measure.Item2, overwrite);
		}
		public IRecordHolder<MeshEntry> AddMeasure(params (string, Func<MeshEntry, object>)[] measures) {
			return AddMeasure(false, measures);
		}
		public IRecordHolder<MeshEntry> AddMeasure(bool overwrite,
									   params (string, Func<MeshEntry, object>)[] measures) {
			IRecordHolder<MeshEntry> holder = this;

			if (measures != null)
				foreach ((string name, Func<MeshEntry, object> func) in measures)
					holder = holder.AddMeasure(name, func, overwrite);

			return holder;
		}


		public IRecordHolder<MeshEntry> Reset() {
			SnapshotTime = DateTime.MinValue;
			Records = new List<Record>();
			return this;
		}
		public IRecordHolder<MeshEntry> TakeSnapShot(IEnumerable<MeshEntry> library) {
			if (!IsEmpty || IsActive)
				throw new SnapShotException();
			if (library == null)
				throw new ArgumentNullException(nameof(library));

			IsActive = true;
			SnapshotTime = DateTime.Now;
			IList<Record> localRecords = new List<Record>();
			foreach (MeshEntry entry in library)
				localRecords.Add(EntrySnapShot(entry));

			Records = localRecords;
			IsActive = false;
			return this;
		}
		/// <summary>
		/// Creates a single record for a single item from the database.
		/// This <see cref="Record"/> contains all the measurements which can be taken.
		/// </summary>
		/// <param name="entry">The database item to get values from.</param>
		/// <returns>The current object for chaining.</returns>
		private Record EntrySnapShot(MeshEntry entry) {
			Record record = new Record(SnapshotTime, entry.Name);
			foreach ((string name, Func<MeshEntry, object> provider) in Measures)
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

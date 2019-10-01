using System;
using System.Collections;
using System.Collections.Generic;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// A class containing a collection of <see cref="Record"/>s
	/// which is able to create snapshots of databases with its specific measures.
	/// </summary>
	public class RecordHolder : IEnumerable<Record> {

		#region --- Properties ---

		#region -- Exception Messages --

		private const string EX_DUBBLE_SNAP = "The current record already contains a snapshot. " +
			"Please reset the RecordHolder first before taking another snapshot.";

		#endregion

		#region -- Instance Properties --

		private readonly IDictionary<string, Func<MeshEntry, object>> measures =
			new Dictionary<string, Func<MeshEntry, object>>();


		public bool IsEmpty => SnapshotTime == DateTime.MinValue;

		public bool IsActive { get; private set; } = false;


		public DateTime SnapshotTime { get; private set; } = DateTime.MinValue;

		public ICollection<Record> Records { get; private set; } = new List<Record>();

		public IEnumerable<(string, Func<MeshEntry, object>)> Measures {
			get {
				foreach(KeyValuePair<string, Func<MeshEntry, object>> pair in measures)
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

		#endregion

		#region --- Constructor Methods ---

		public RecordHolder() { }

		public RecordHolder(params (string, Func<MeshEntry, object>)[] measures) {
			AddMeasure(measures);
		}

		#endregion

		#region --- Methods ---

		public RecordHolder AddMeasure(string measureName, Func<MeshEntry, object> provider,
										bool overwrite = false) {
			if (string.IsNullOrEmpty(measureName))
				throw new ArgumentNullException(nameof(measureName));
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

			if (overwrite || !measures.ContainsKey(measureName))
				measures.Add(measureName, provider);
			
			return this;
		}

		public RecordHolder AddMeasure((string, Func<MeshEntry, object>) measure,
										bool overwrite = false) {
			return AddMeasure(measure.Item1, measure.Item2, overwrite);
		}

		public RecordHolder AddMeasure(params (string, Func<MeshEntry, object>)[] measures) {
			return AddMeasure(false, measures);
		}

		public RecordHolder AddMeasure(bool overwrite,
									   params (string, Func<MeshEntry, object>)[] measures) {
			RecordHolder holder = this;

			if (measures != null)
				foreach((string name, Func<MeshEntry, object> func) in measures)
					holder = holder.AddMeasure(name, func, overwrite);

			return holder;
		}



		public RecordHolder Reset() {
			SnapshotTime = DateTime.MinValue;
			Records = new List<Record>();
			return this;
		}

		public RecordHolder TakeSnapShot(MeshLibrary library) {
			if (!IsEmpty || IsActive)
				throw new InvalidOperationException(EX_DUBBLE_SNAP);
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

		private Record EntrySnapShot(MeshEntry entry) {
			Record record = new Record(SnapshotTime, entry.Name);
			foreach((string name, Func<MeshEntry, object> provider) in Measures)
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

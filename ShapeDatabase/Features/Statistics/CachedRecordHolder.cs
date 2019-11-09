using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// A class containing a collection of <see cref="Record"/>s
	/// which is able to create snapshots of databases with its specific measures.
	/// This recordholder has an internal cache which can be used to calculate
	/// different <see cref="Record"/>s.
	/// </summary>
	/// <typeparam name="T">The object which will be used for making records.</typeparam>
	public class CachedRecordHolder<T> : RecordHolder<T>, ICachedRecordHolder<T> {

		#region --- Properties ---

		/// <summary>
		/// The cached values of the recordholder.
		/// </summary>
		public ICache<T> Cache { get; } = new Cache<T>();

		private readonly IDictionary<string, Func<T, ICache<T>, object>> measures
			= new Dictionary<string, Func<T, ICache<T>, object>>();

		/// <summary>
		/// A collection of measures which will be taken of all the objects in
		/// a provided database during the next snapshot.
		/// </summary>
		private IEnumerable<(string, Func<T, ICache<T>, object>)> Measures {
			get {
				foreach (KeyValuePair<string, Func<T, ICache<T>, object>> pair in measures)
					yield return (pair.Key, pair.Value);
			}
		}

		/// <summary>
		/// The names of collected measures.
		/// </summary>
		public override IEnumerable<string> MeasureNames {
			get {
				foreach (KeyValuePair<string, Func<T, ICache<T>, object>> pair in measures)
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
		public CachedRecordHolder(Func<T, string> nameProvider) : base(nameProvider) { }

		/// <summary>
		/// Instantiates a new <see cref="RecordHolder"/> with the specified
		/// measurements.
		/// </summary>
		/// <param name="nameProvider">The formula to get a unique name
		/// for the object.</param>
		/// <param name="measures">A collection of measurements which should be
		/// taken for each object in the database.</param>
		/// <exception cref="ArgumentNullException">If a measurement is
		/// <see langword="null"/> or any of its properties is <see langword="null"/>.
		/// </exception>
		public CachedRecordHolder(Func<T, string> nameProvider,
								  params (string, Func<T, object>)[] measures)
			: base(nameProvider, measures) { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Adds a measure to the cached recordholder.
		/// </summary>
		/// <param name="measureName">Name of the measure.</param>
		/// <param name="provider">Function to calculate the measure.</param>
		/// <param name="overwrite">Whether the provider should be overwritten in case it is already present.</param>
		/// <returns>Cached recordholder containing the measure and provider.</returns>
		public ICachedRecordHolder<T> AddMeasure(
				string measureName,
				Func<T, ICache<T>, object> provider,
				bool overwrite = false) {

			if (measureName == null)
				throw new ArgumentNullException(nameof(measureName));
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

			if (overwrite || !measures.ContainsKey(measureName))
				measures[measureName] = provider;

			return this;
		}

		public override IRecordHolder<T> AddMeasure(
										string measureName,
										Func<T, object> provider,
										bool overwrite = false) {
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));
			return AddMeasure(measureName, (x, _) => provider(x), overwrite);
		}

		ICachedRecordHolder<T> ICachedRecordHolder<T>.AddMeasure(
				string measureName,
				Func<T, object> provider,
				bool overwrite) {
			AddMeasure(measureName, provider, overwrite);
			return this;
		}

		ICachedRecordHolder<T> ICachedRecordHolder<T>.Reset() {
			base.Reset();
			return this;
		}

		ICachedRecordHolder<T> ICachedRecordHolder<T>.TakeSnapShot(ICollection<T> library) {
			TakeSnapShot(library);
			return this;
		}

		/// <summary>
		/// Creates a single record for a single item from the database.
		/// This <see cref="Record"/> contains all the measurements which can be taken.
		/// </summary>
		/// <param name="entry">The database item to get values from.</param>
		/// <param name="entryName">The name of the entry to remember in the record.</param>
		/// <returns>The current object for chaining.</returns>
		protected override Record EntrySnapShot(string entryName, T entry) {
			Cache.Clear();
			Record record = new Record(SnapshotTime, entryName);
			foreach ((string name, Func<T, ICache<T>, object> provider) in Measures)
				record.AddMeasure(name, provider(entry, Cache));
			return record;
		}

		#endregion

	}

}

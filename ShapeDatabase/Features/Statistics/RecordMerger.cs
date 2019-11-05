using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Properties;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// A class with the purpose of merging multiple Recordholders together
	/// base on a property in the records.
	/// </summary>
	public class RecordMerger {

		#region --- Properties ---

		/// <summary>
		/// A function to give a class to the specified record.
		/// All records with the same class will be combined into one record.
		/// </summary>
		/// <param name="record">The record whose class to retrieve.</param>
		/// <returns>A string to identify the new class for this record.</returns>
		public delegate string MergeClass(Record record);
		/// <summary>
		/// A function to combine a collection of values into one specific property.
		/// </summary>
		/// <param name="record">The collection of values which represent all the
		/// individual characteristics or the Records.</param>
		/// <returns>An aggregated value of the specified records.</returns>
		public delegate object MergeMeasures(ICollection<object> record);

		private MergeClass mergeClass;
		private IDictionary<string, MergeRecord> records
			= new Dictionary<string, MergeRecord>();
		private IDictionary<string, MergeMeasures> classMeasures
			= new Dictionary<string, MergeMeasures>();
		private IDictionary<Type,	MergeMeasures> typeMeasures
			= new Dictionary<Type, MergeMeasures>();

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new merger which can combine records.
		/// </summary>
		public RecordMerger(MergeClass mergeClass) {
			this.mergeClass = mergeClass ?? throw new ArgumentNullException(nameof(mergeClass));
		}

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Provides a new way to combine data for the specified class values.
		/// </summary>
		/// <param name="className">The name of the class where this value is applicable.
		/// </param>
		/// <param name="func">The function to combine all the values into one
		/// property.</param>
		/// <returns>The current <see cref="RecordMerger"/> for chaining.</returns>
		public RecordMerger AddMeasure(string className, MergeMeasures func) {
			classMeasures.Add(className, func);
			return this;
		}
		/// <summary>
		/// Provides a new way to combine data for the specified class values.
		/// </summary>
		/// <param name="type">The type of objects for which this formula is
		/// applicable.</param>
		/// <param name="func">The function to combine all the values into one
		/// property.</param>
		/// <returns>The current <see cref="RecordMerger"/> for chaining.</returns>
		public RecordMerger AddMeasure(Type type, MergeMeasures func) {
			typeMeasures.Add(type, func);
			return this;
		}
		/// <summary>
		/// Provides a new way to combine data for the specified class values.
		/// </summary>
		/// <typeparam name="T">The type of objects for which this formula is
		/// applicable.</typeparam>
		/// <param name="func">The function to combine all the values into one
		/// property.</param>
		/// <returns>The current <see cref="RecordMerger"/> for chaining.</returns>
		public RecordMerger AddMeasure<T>(MergeMeasures func) =>
			AddMeasure(typeof(T), func);

		/// <summary>
		/// Combines all the records in the specified Record Holder with
		/// the current combination formulas in this object.
		/// </summary>
		/// <param name="holder">The collection of records which hsould be merged.</param>
		/// <returns>A new <see cref="IRecordHolder"/> which contains the aggregated
		/// data of all the previous records.</returns>
		public IRecordHolder Merge(IRecordHolder holder) {
			// Combines all the records in a merge record.
			foreach(Record record in holder) {
				string group = mergeClass(record);
				MergeRecord merge = GetOrCreate(group);
				foreach((string measureName, object measureValue) in record.Measures)
					merge.AddMeasure(measureName, measureValue);
			}

			DirectRecordHolder recordHolder = new DirectRecordHolder();
			// Aggregates all the data in a merge record into a single value.
			foreach(string measureName in holder.MeasureNames)
				recordHolder.AddMeasureName(measureName);
			Parallel.ForEach(records, recordPair => {
				string name = recordPair.Key;
				MergeRecord merge = recordPair.Value;

				foreach(KeyValuePair<string, MergeMeasures> cPair in classMeasures)
					merge.AddMergeFunction(cPair.Key, cPair.Value);
				foreach(KeyValuePair<Type, MergeMeasures> tPair in typeMeasures)
					merge.AddMergeFunction(tPair.Key, tPair.Value);

				merge.Merge();
				recordHolder.AddRecord(merge);
			});

			return recordHolder;
		}

		private MergeRecord GetOrCreate(string name) {
			if (!records.TryGetValue(name, out MergeRecord record)) {
				record = new MergeRecord(name);
				records.Add(name, record);
			}
			return record;
		}

		#endregion

	}

	/// <summary>
	/// A specific Record which allows mergin of other records.
	/// </summary>
	class MergeRecord : Record, IEquatable<MergeRecord> {

		#region --- Properties ---

		/// <summary>
		/// Functions which can combine a specific type.
		/// </summary>
		private IList<(Type, RecordMerger.MergeMeasures)> functionList
			= new List<(Type, RecordMerger.MergeMeasures)>();
		/// <summary>
		/// Functions which can combine all values for a specific measure.
		/// </summary>
		private IDictionary<string, RecordMerger.MergeMeasures> mergeFunction
			= new Dictionary<string, RecordMerger.MergeMeasures>();
		/// <summary>
		/// Collection of all measures by different objects to merge.
		/// </summary>
		private IDictionary<string, IList<object>> mergeMeasures
			= new Dictionary<string, IList<object>>();
		/// <summary>
		/// If this record is merged yet and ready to use.
		/// </summary>
		private bool merged = false;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new record as a combination of other records.
		/// </summary>
		/// <param name="name">The aggregation name for the records.</param>
		public MergeRecord(string name) : this(DateTime.Now, name) { }

		/// <summary>
		/// Initialises a new record as a combination of other records.
		/// </summary>
		/// <param name="time">The time this aggregation was created.</param>
		/// <param name="name">The aggregation name for the records.</param>
		public MergeRecord(DateTime time, string name) : base(time, name) { }

		#endregion

		#region --- Instance Methods ---

		#region -- Merge Methods --

		/// <summary>
		/// Specifies a new function which can combine different values for the
		/// given property.
		/// </summary>
		/// <param name="name">The name of the measurement for which this function
		/// works.</param>
		/// <param name="func">The function to combine the different values together.
		/// </param>
		/// <returns>The current <see cref="Record"/> for chaining purposes.</returns>
		public virtual Record AddMergeFunction(string name,
											   RecordMerger.MergeMeasures func) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));
			if (func == null)
				throw new ArgumentNullException(nameof(func));

			mergeFunction.Add(name, func);
			return this;
		}

		public virtual Record AddMergeFunction<T>(RecordMerger.MergeMeasures func) =>
			AddMergeFunction(typeof(T), func);
		public virtual Record AddMergeFunction(Type type,
											   RecordMerger.MergeMeasures func) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (func == null)
				throw new ArgumentNullException(nameof(func));

			if (ContainsFunction(type))
				RemoveFunction(type);
			functionList.Add((type, func));
			return this;
		}

		/// <summary>
		/// Combines all the internally saved variables and create one Record
		/// with the aggregated values.
		/// </summary>
		/// <returns>The current <see cref="Record"/> for chaining purposes.</returns>
		public Record Merge() {
			foreach (KeyValuePair<string, IList<object>> pairs in mergeMeasures) {
				string name = pairs.Key;
				IList<object> measures = pairs.Value;
				// We have a function to combine the measures.
				if (mergeFunction.TryGetValue(name,
											  out RecordMerger.MergeMeasures merge)) {
					object value = merge(measures);
					base.AddMeasure(name, value);
				// We have a function for the object type.
				} else if(measures.Count != 0
					&& TryGetFunction(measures[0].GetType(), out merge)) {
					object value = merge(measures);
					base.AddMeasure(name, value);
					// We do not have a function but it is a single value.
				} else if (measures.Count == 1) {
					base.AddMeasure(name, measures[0]);
					// There is no function and there are multiple objects.
				} else {
					throw new InvalidOperationException(
						string.Format(
							Settings.Culture,
							Resources.EX_Merge_MissingFunc,
							name
						)
					);
				}
			}
			return this;
		}


		private bool ContainsFunction(Type type) {
			foreach((Type fType, RecordMerger.MergeMeasures _) in functionList)
				if (fType == type)
					return true;
			return false;
		}

		private void RemoveFunction(Type type) {
			(Type, RecordMerger.MergeMeasures) goal = default;
			foreach ((Type fType, RecordMerger.MergeMeasures func) in functionList)
				if (fType == type
					|| fType.IsAssignableFrom(type)
					|| type.IsAssignableFrom(fType)) {
					goal = (fType, func);
					break;
				}

			if (goal != default)
				functionList.Remove(goal);
		}

		private bool TryGetFunction(Type type, out RecordMerger.MergeMeasures function) {
			foreach ((Type fType, RecordMerger.MergeMeasures func) in functionList)
				if (fType == type || fType.IsAssignableFrom(type)) { 
					function = func;
					return true;
				}
			function = null;
			return false;
		}

		#endregion

		#region -- Record Methods --

		public override Record AddMeasure(string name, object value) {
			if (mergeMeasures.TryGetValue(name, out IList<object> values))
				values.Add(value);
			else
				mergeMeasures.Add(name, new List<object>() { value });
			return this;
		}

		public override bool TryGetValue(string name, out object value) {
			if (!merged) throw new InvalidOperationException(Resources.EX_Merge_State);
			return base.TryGetValue(name, out value);
		}

		#endregion

		#region -- Object Methods --

		public override bool Equals(Record other) {
			return other is MergeRecord && Equals((MergeRecord) other);
		}

		public bool Equals(MergeRecord other) {
			return base.Equals(other) && merged == other.merged;
		}

		#endregion

		#endregion

	}

}

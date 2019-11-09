using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Properties;
using ShapeDatabase.Shapes;
using ShapeDatabase.Util;
using ShapeDatabase.Util.Collections;

namespace ShapeDatabase.Query {

	/// <summary>
	/// A collection of a comparison of a single item with the entire database.
	/// This object shows the distances between the given item and all other
	/// database items.
	/// </summary>
	[DebuggerDisplay("{QueryName}: {Count} comparisons")]
	public class QueryResult : System.IComparable<QueryResult> {

		#region --- Properties ---

		private readonly IList<QueryItem> results;

		/// <summary>
		/// The name of the item which is used as a reference during comparison.
		/// This is the provided query item's name.
		/// </summary>
		public string QueryName { get; }

		/// <summary>
		/// A collection containing all the compared items from the database in order
		/// of the most accurate one first.
		/// </summary>
		public IEnumerable<QueryItem> Results => results;

		/// <summary>
		/// The number of elements which have been compared to the reference one.
		/// </summary>
		public int Count => results.Count;

		/// <summary>
		/// Shows if no items have been compared yet to the reference item.
		/// </summary>
		public bool IsEmpty => Count == 0;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new result for the given mesh.
		/// </summary>
		/// <param name="name">The name of the object used for comparison.</param>
		/// <param name="mesh">The mesh to extract the feature vectors from.</param>
		public QueryResult(string name) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			QueryName = name;
			IComparer<QueryItem> comparer = new QueryItemComparer();
			results = new ConcurrentSortedList<QueryItem>(comparer);
		}

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Resets the current collection of results.
		/// </summary>
		public void Clear() {
			results.Clear();
		}

		/// <summary>
		/// Adds a new compared item to the collection.
		/// </summary>
		/// <param name="item">The querried database item with their distance.</param>
		public void AddItem(QueryItem item) {
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			IList list = (IList) results;
			if (list.IsSynchronized)
				results.Add(item);
			else
				lock (list.SyncRoot) {
					results.Add(item);
				}
		}

		/// <summary>
		/// A new compared item to add to the collection.
		/// </summary>
		/// <param name="name">The name of the item from the database.</param>
		/// <param name="distance">The distance between the reference item and database
		/// item. The larger the distance the further it is from the reference item.
		/// </param>
		public void AddItem(string name, double distance) => AddItem(new QueryItem(name, distance));

		/// <summary>
		/// Gives the best results from the query object as an array.
		/// The size of the array will be the given size or the maximum
		/// amount of queried objects in this class.
		/// </summary>
		/// <param name="resultCount">The number of elements to retrieve.</param>
		/// <returns>An array containing the best querried items from the solution.</returns>
		public QueryItem[] GetBestResults(int resultCount) {
			if (resultCount < 0)
				throw new ArgumentException(Resources.EX_ExpPosValue, nameof(resultCount));

			IList list = (IList) results;
			if (list.IsSynchronized)
				return SafeBestResults(resultCount);
			else
				lock (list.SyncRoot) {
					return SafeBestResults(resultCount);
				}
		}

		private QueryItem[] SafeBestResults(int resultCount) {
			int count = Math.Min(resultCount, Count);
			QueryItem[] result = new QueryItem[count];
			for (int i = 0; i < count; i++)
				result[i] = results[i];
			return result;
		}

		public override string ToString() {
			return string.Format(
				Settings.Culture,
				"{0} : {1}",
				QueryName,
				string.Join(", ", Results)
			);
		}

		/// <summary>
		/// Compares the name of one QueryResult to the name of another QueryResult (for alphabetic sorting).
		/// </summary>
		/// <param name="other">The other QueryResult.</param>
		/// <returns>Returns an int < 0 if lower, = 0 if equal, and > 0 if higher (alphabetically).</returns>
		public int CompareTo(QueryResult other) {
			if (other == null)
				throw new ArgumentNullException(nameof(other));
			return QueryName.CompareTo(other.QueryName);
		}

		#endregion
	
	}

}

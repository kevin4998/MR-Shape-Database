using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Properties;
using ShapeDatabase.Shapes;
using ShapeDatabase.Util.Collections;

namespace ShapeDatabase.Query {

	/// <summary>
	/// A collection of a comparison of a single item with the entire database.
	/// This object shows the distances between the given item and all other
	/// database items.
	/// </summary>
	public class QueryResult {

		#region --- Properties ---

		private readonly IList<QueryItem> results;

		/// <summary>
		/// The name of the item which is used as a reference during comparison.
		/// This is the provided query item's name.
		/// </summary>
		public string QueryName { get; }
		/// <summary>
		/// The mesh of the item which is used as a reference during comparison.
		/// This is the mesh used to extract the vectors from.
		/// </summary>
		public IMesh QueryMesh { get; }

		/// <summary>
		/// A collection containing all the compared items from the database in order
		/// of the most accurate one first.
		/// </summary>
		public IEnumerable<QueryItem> Results => results;
		/// <summary>
		/// The number of elements which have been compared to the reference one.
		/// </summary>
		public int Count => results.Count;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new result for the given mesh.
		/// </summary>
		/// <param name="name">The name of the object used for comparison.</param>
		/// <param name="mesh">The mesh to extract the feature vectors from.</param>
		public QueryResult(string name, IMesh mesh) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			QueryName = name;
			QueryMesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
			results = new ConcurrentSortedList<QueryItem>();
		}

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Resets the current collection of results to compare again with the
		/// database.
		/// </summary>
		public void Clear() {
			results.Clear();
		}

		/// <summary>
		/// A new compared item to add to the collection.
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
		/// <param name="distance">The distance between the reference and database
		/// item. The larger the distance the further it is from the reference item.
		/// </param>
		public void AddItem(string name, double distance) => AddItem(new QueryItem(name, distance));

		/// <summary>
		/// Gives the best results from the query object as an array.
		/// The size of the array will be the given size or the maximum
		/// amount of queried objects in this class.
		/// </summary>
		/// <param name="resultCount">The number of elements to retrieve.</param>
		/// <returns>An array containing the best querried items from the
		/// solution.</returns>
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

		#endregion

	}

}

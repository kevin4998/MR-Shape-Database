using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		private readonly IList<QueryItem> results = new SortedList<QueryItem>();

		public string QueryName { get; }
		public IMesh QueryMesh { get; }

		public IEnumerable<QueryItem> Results => results;

		#endregion

		#region --- Constructor Methods ---

		public QueryResult(string name, IMesh mesh) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			QueryName = name;
			QueryMesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
			results = new SortedList<QueryItem>();
		}

		#endregion

		#region --- Instance Methods ---

		public void Clear() {
			results.Clear();
		}

		public void AddItem(QueryItem item) {
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			results.Add(item);
		}

		public void AddItem(string name, double distance) => AddItem(new QueryItem(name, distance));

		#endregion

	}

}

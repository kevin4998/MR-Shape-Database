using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Util;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// A collection of all the loaded meshes/shapes in the application.
	/// </summary>
	public class MeshLibrary : IMeshLibrary {

		#region --- Properties ---

		private readonly IDictionary<string, MeshEntry> library
			= new Dictionary<string, MeshEntry>();

		public MeshEntry this[string name] => library[name];
		public ICollection<MeshEntry> Meshes => library.Values;
		public ICollection<string> Names => library.Keys;

		/// <summary>
		/// The amount of loaded Meshes.
		/// </summary>
		public int Count => library.Count;
		/// <summary>
		/// Specifies if it is possible to write new data to this library.
		/// </summary>
		public bool IsReadOnly => false;

		#endregion

		#region --- Constructor Methods --- 

		/// <summary>
		/// Creates a new Library to hold the different loaded meshes.
		/// </summary>
		public MeshLibrary() { }

		#endregion

		#region --- Instance Methods ---

		#region -- Library Methods --

		public void Add(MeshEntry entry, bool replace = false) {
			if (replace || !library.ContainsKey(entry.Name))
				library[entry.Name] = entry;
		}

		public bool TryGetValue(string name, out MeshEntry entry) {
			return library.TryGetValue(name, out entry);
		}

		#endregion

		#region -- Interface Methods --

		void ICollection<MeshEntry>.Add(MeshEntry entry) {
			Add(entry, false);
		}

		public void Clear() {
			library.Clear();
		}

		public bool Contains(string name) {
			return library.ContainsKey(name);
		}

		public bool Contains(MeshEntry entry) {
			return Contains(entry.Name);
		}

		public bool Remove(MeshEntry entry) {
			if (TryGetValue(entry.Name, out MeshEntry result)
				&& entry.Mesh.Equals(result.Mesh)) {
				library.Remove(entry.Name);
				return true;
			}
			return false;
		}

		public void CopyTo(MeshEntry[] array, int arrayIndex) {
			Meshes.CopyTo(array, arrayIndex);
		}

		public IEnumerator<MeshEntry> GetEnumerator() {
			return Meshes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		#endregion

		#endregion

	}

}

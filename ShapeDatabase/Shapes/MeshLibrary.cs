using System;
using System.Collections;
using System.Collections.Generic;
using ShapeDatabase.Features.Descriptors;

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

		public bool Contains(MeshEntry entry) {
			return library.ContainsKey(entry.Name);
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

	/// <summary>
	/// A single loaded Mesh with extra information about its shape.
	/// </summary>
	public struct MeshEntry : IMeshEntry {

		#region --- Properties ---

		/// <summary>
		/// The class given to all shapes without a specified class.
		/// </summary>
		public const string DefaultClass = "Unspecified";

		public string Name { get; }
		public string Class { get; }
		public IMesh Mesh { get; }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Creates a new entry to be used in the <see cref="MeshLibrary"/>
		/// </summary>
		/// <param name="name">The unique name for this shape.</param>
		/// <param name="mesh">The actual 3 dimensional shape.</param>
		public MeshEntry(string name, IMesh mesh)
			: this(name, null, mesh) { }
		/// <summary>
		/// Creates a new entry to be used in the <see cref="MeshLibrary"/>
		/// </summary>
		/// <param name="name">The unique name for this shape.</param>
		/// <param name="clazz">The type of shape which this one is specified as.</param>
		/// <param name="mesh">The actual 3 dimensional shape.</param>
		public MeshEntry(string name, string clazz, IMesh mesh) {
			Name = string.IsNullOrEmpty(name)
					? throw new ArgumentNullException(nameof(name))
					: name;
			Class = string.IsNullOrEmpty(clazz) ? DefaultClass : clazz;
			Mesh = mesh;
		}

		#endregion

		#region --- Instance Methods ---

		public override bool Equals(object obj) {
			return obj is IMeshEntry entry && Equals(entry);
		}

		public bool Equals(IMeshEntry other) {
			return this.Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);
		}

		public override int GetHashCode() {
			return this.Name.GetHashCode();
		}


		public static bool operator ==(MeshEntry left, MeshEntry right) {
			return left.Equals(right);
		}

		public static bool operator !=(MeshEntry left, MeshEntry right) {
			return !(left == right);
		}

		#endregion

	}

}

using ShapeDatabase.Features.Descriptors;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// A collection of all the loaded meshes/shapes in the application.
	/// </summary>
	public class MeshLibrary : ICollection<MeshEntry> {

		#region --- Properties ---

		private readonly IDictionary<string, MeshEntry> library
			= new Dictionary<string, MeshEntry>();

		/// <summary>
		/// All the different loaded meshes.
		/// </summary>
		public ICollection<MeshEntry> Meshes => library.Values;
		/// <summary>
		/// All the different unique names used for the meshes.
		/// </summary>
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

		/// <summary>
		/// Adds a new mesh into the collection
		/// if it is not present already.
		/// </summary>
		/// <param name="entry">A mesh with its information.</param>
		/// <param name="replace">If an old value can be overriden.</param>
		public void Add(MeshEntry entry, bool replace = false) {
			if (replace || !library.ContainsKey(entry.Name))
				library[entry.Name] = entry;
		}

		/// <summary>
		/// Attempts to get an entry with the given name from the library of meshes.
		/// </summary>
		/// <param name="name">The name of the shape to retrieve.</param>
		/// <param name="entry">The shape which was retrieved from the database.</param>
		/// <returns><see langword="true"/> if the shape could be retrieved.</returns>
		/// <exception cref="ArgumentNullException">If the name is <see langword="null"/>
		/// or <see cref="string.Empty"/>.</exception>
		public bool TryGetValue(string name, out MeshEntry entry) {
			return library.TryGetValue(name, out entry);
		}

		/// <summary>
		/// Gives an <see cref="MeshEntry"/> with the specified name in the library.
		/// </summary>
		/// <param name="name">The name of the shape to retrieve.</param>
		/// <returns>A mesh entry with the specified name.</returns>
		/// <exception cref="ArgumentNullException">If the given name is
		/// <see langword="null"/>.</exception>
		/// <exception cref="KeyNotFoundException">If there is no entry with the
		/// provided name.</exception>
		public MeshEntry this[string name] {
			get {
				return library[name];
			}
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
	public struct MeshEntry {

		/// <summary>
		/// The class given to all shapes without a specified class.
		/// </summary>
		public const string DefaultClass = "Unspecified";

		/// <summary>
		/// The unique name for this mesh,
		/// this will most likely be the file name.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// The class to which this item belongs,
		/// this is determined based on its shape.
		/// </summary>
		public string Class { get; }
		/// <summary>
		/// The mesh containing the shape of this object.
		/// </summary>
		public IMesh Mesh { get; }
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

	}

}

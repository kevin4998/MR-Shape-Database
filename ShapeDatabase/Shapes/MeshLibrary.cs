using System;
using System.Collections;
using System.Collections.Generic;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// A collection of all the loaded meshes/shapes in the application.
	/// </summary>
	public class MeshLibrary : IEnumerable<MeshEntry> {

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
		/// Creates a new Library to hold the different loaded meshes.
		/// </summary>
		public MeshLibrary() { }


		/// <summary>
		/// Adds a new mesh into the collection
		/// if it is not present already.
		/// </summary>
		/// <param name="entry">A mesh with its information.</param>
		/// <param name="replace">If an old value can be overriden.</param>
		public void Add(MeshEntry entry, bool replace = false) {
			if (replace || !library.ContainsKey(entry.Name))
				library.Add(entry.Name, entry);
		}


		public MeshEntry this[string name] {
			get {
				return library[name];
			}
		}

		public IEnumerator<MeshEntry> GetEnumerator() {
			return Meshes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
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
		/// A collection of different features to identify this shape.
		/// </summary>
		public string[] Features { get; }
		/// <summary>
		/// The mesh containing the shape of this object.
		/// </summary>
		public UnstructuredMesh Mesh { get; }

		/// <summary>
		/// Creates a new entry to be used in the <see cref="MeshLibrary"/>
		/// </summary>
		/// <param name="name">The unique name for this shape.</param>
		/// <param name="mesh">The actual 3 dimensional shape.</param>
		public MeshEntry(string name, UnstructuredMesh mesh) 
			: this(name, null, mesh) { }
		/// <summary>
		/// Creates a new entry to be used in the <see cref="MeshLibrary"/>
		/// </summary>
		/// <param name="name">The unique name for this shape.</param>
		/// <param name="clazz">The type of shape which this one is specified as.</param>
		/// <param name="mesh">The actual 3 dimensional shape.</param>
		public MeshEntry(string name, string clazz, UnstructuredMesh mesh)
			: this(name, clazz, mesh, null) { }
		/// <summary>
		/// Creates a new entry to be used in the <see cref="MeshLibrary"/>
		/// </summary>
		/// <param name="name">The unique name for this shape.</param>
		/// <param name="clazz">The type of shape which this one is specified as.</param>
		/// <param name="mesh">The actual 3 dimensional shape.</param>
		/// <param name="features">A collection of tags to identify this shape.</param>
		public MeshEntry(string name, string clazz, UnstructuredMesh mesh, params string[] features) {
			Name = string.IsNullOrEmpty(name)
					? throw new ArgumentNullException(nameof(name))
					: name;
			Class = string.IsNullOrEmpty(clazz) ? DefaultClass : clazz;
			Mesh = mesh;
			Features = features ?? new string[0];
		}

	}

}

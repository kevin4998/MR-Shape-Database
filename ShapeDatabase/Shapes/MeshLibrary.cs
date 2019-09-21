using System;
using System.Collections.Generic;

namespace ShapeDatabase.Shapes {
	public class MeshLibrary {

		private readonly IDictionary<string, MeshEntry> library
			= new Dictionary<string, MeshEntry>();

		public ICollection<MeshEntry> Meshes => library.Values;
		public ICollection<string> Names => library.Keys;
		public int Count => library.Count;


		public MeshLibrary() { }

		
		public void Add(MeshEntry entry) {
			if (!library.ContainsKey(entry.Name))
				library.Add(entry.Name, entry);
		}

	}

	public struct MeshEntry {

		public const string DefaultClass = "Unspecified";

		public string Name { get; }
		public string Class { get; }
		public string[] Features { get; }
		public UnstructuredMesh Mesh { get; }

		public MeshEntry(string name, UnstructuredMesh mesh) 
			: this(name, null, mesh) { }
		public MeshEntry(string name, string clazz, UnstructuredMesh mesh)
			: this(name, clazz, mesh, null) { }
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Util;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// A single loaded Mesh with extra information about its shape.
	/// </summary>
	[DebuggerDisplay("{Class}/{Name}")]
	public struct MeshEntry : IMeshEntry {

		#region --- Properties ---

		/// <summary>
		/// The class given to all shapes without a specified class.
		/// </summary>
		public const string DefaultClass = "Unspecified";

		public string Name { get; }
		public string Class { get; }
		public IMesh Mesh { get; }

		/// <summary>
		/// If the current meshentry has been initalised a null or default value.
		/// </summary>
		public bool IsNull => Name == null || Class == null;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Creates a new entry to be used in the <see cref="MeshLibrary"/>
		/// </summary>
		/// <param name="file">The file where this entry is stored in.</param>
		public MeshEntry(FileInfo file) 
			:this(file.NameWithoutExtension(), file.Directory.Name, new LazyMesh(file)) { }
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

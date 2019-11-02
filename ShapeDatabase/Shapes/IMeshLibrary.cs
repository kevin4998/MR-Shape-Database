using System.Collections.Generic;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// A collection of shapes which can be accessed by the application.
	/// </summary>
	public interface IMeshLibrary : ICollection<MeshEntry> {

		/// <summary>
		/// Gives an <see cref="MeshEntry"/> with the specified name in the library.
		/// </summary>
		/// <param name="name">The name of the shape to retrieve.</param>
		/// <returns>A mesh entry with the specified name.</returns>
		/// <exception cref="ArgumentNullException">If the given name is
		/// <see langword="null"/>.</exception>
		/// <exception cref="KeyNotFoundException">If there is no entry with the
		/// provided name.</exception>
		MeshEntry this[string name] { get; }

		/// <summary>
		/// All the different meshes which are stored in the library.
		/// </summary>
		ICollection<MeshEntry> Meshes { get; }
		/// <summary>
		/// All the different unique names used for the meshes.
		/// </summary>
		ICollection<string> Names { get; }

		/// <summary>
		/// Adds a new mesh into the collection
		/// if it is not present already.
		/// </summary>
		/// <param name="entry">A mesh with its information.</param>
		/// <param name="replace">If an old value can be overriden.</param>
		void Add(MeshEntry entry, bool replace = false);
		/// <summary>
		/// Attempts to get an entry with the given name from the library of meshes.
		/// </summary>
		/// <param name="name">The name of the shape to retrieve.</param>
		/// <param name="entry">The shape which was retrieved from the database.</param>
		/// <returns><see langword="true"/> if the shape could be retrieved.</returns>
		/// <exception cref="ArgumentNullException">If the name is <see langword="null"/>
		/// or <see cref="string.Empty"/>.</exception>
		bool TryGetValue(string name, out MeshEntry entry);

	}
}
using System;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// A single mesh together with extra information about the shape.
	/// </summary>
	public interface IMeshEntry : IEquatable<IMeshEntry> {

		/// <summary>
		/// The unique name for this mesh,
		/// this will most likely be the file name.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// The class to which this item belongs,
		/// this is determined based on its shape.
		/// </summary>
		string Class { get; }
		/// <summary>
		/// The mesh containing the shape of this object.
		/// </summary>
		IMesh Mesh { get; }

	}

}
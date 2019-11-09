using OpenTK;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// The definition of a box surrounding a shapes which includes all the vertices
	/// inside this box. 
	/// </summary>
	public interface IBoundingBox {

		/// <summary>
		/// The smallest X position of the box in space.
		/// </summary>
		float MinX { get; }
		/// <summary>
		/// The smallest Y position of the box in space.
		/// </summary>
		float MinY { get; }
		/// <summary>
		/// The smallest Z position of the box in space.
		/// </summary>
		float MinZ { get; }

		/// <summary>
		/// The largest X position of the box in space.
		/// </summary>
		float MaxX { get; }
		/// <summary>
		/// The largest Y position of the box in space.
		/// </summary>
		float MaxY { get; }
		/// <summary>
		/// The largest Z position of the box in space.
		/// </summary>
		float MaxZ { get; }

		/// <summary>
		/// The different along the X direction of the box in space.
		/// </summary>
		float Width { get; }    // X difference
		/// <summary>
		/// The different along the Y direction of the box in space.
		/// </summary>
		float Height { get; }   // Y difference
		/// <summary>
		/// The different along the Z direction of the box in space.
		/// </summary>
		float Depth { get; }    // Z difference

		/// <summary>
		/// The total volume of the box defined by its
		/// <see cref="Width"/>, <see cref="Height"/> and <see cref="Depth"/>.
		/// </summary>
		float Volume { get; }

		/// <summary>
		/// A vector containing all the minimum coordinate values.
		/// </summary>
		Vector3 Min { get; }
		/// <summary>
		/// A vector containing all the maximum coordinate values.
		/// </summary>
		Vector3 Max { get; }
		/// <summary>
		/// A vector containing all ranges (width, height and depth) of the box..
		/// </summary>
		Vector3 Size { get; }
		/// <summary>
		/// A position in the middle of the box.
		/// </summary>
		Vector3 Center { get; }

	}
}

using OpenTK;

namespace ShapeDatabase.Shapes {

	public interface IBoundingBox {

		float MinX { get; }
		float MinY { get; }
		float MinZ { get; }

		float MaxX { get; }
		float MaxY { get; }
		float MaxZ { get; }

		float Width { get; }	// X difference
		float Height { get; }	// Y difference
		float Depth { get; }	// Z difference

		float Volume { get; }

		Vector3 Min { get; }
		Vector3 Max { get; }
		Vector3 Size { get; }
		Vector3 Center { get; }

	}
}

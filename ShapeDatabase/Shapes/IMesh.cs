using System.Collections.Generic;
using OpenTK;

namespace ShapeDatabase.Shapes {

	public interface IMesh {

		bool IsNormalised { get; }

		uint VertexCount { get; }
		uint FaceCount { get; }
		uint EdgeCount { get; }

		IEnumerable<Vector3> Vertices { get; }
		IEnumerable<Vector3> Faces { get; }
		IEnumerable<Vector3> Edges { get; }

		IBoundingBox GetBoundingBox();

		Vector3 GetVertex(uint pos);

	}

}

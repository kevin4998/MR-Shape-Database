using System.Collections.Generic;
using OpenTK;

namespace ShapeDatabase.Shapes {

	public interface IMesh {

		bool IsNormalised { get; }

		uint VertexCount { get; }
		uint FaceCount { get; }
		uint EdgeCount { get; }
		uint NormalCount { get; }

		IEnumerable<Vector3> Vertices { get; }
		IEnumerable<Vector3> Faces { get; }
		IEnumerable<Vector3> Edges { get; }
		IEnumerable<Vector3> Normals { get; }

		IBoundingBox GetBoundingBox();

		Vector3 GetVertex(uint pos);
		Vector3 GetFace(uint pos);
		Vector3 GetNormal(uint pos);

		double GetTriArea(int tID);

	}

}

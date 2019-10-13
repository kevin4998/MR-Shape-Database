using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
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

	}

	public static class MeshEx {

		public static Vector3[] GetVerticesFromFace(this IMesh mesh, uint pos) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (pos >= mesh.FaceCount)
				throw new ArgumentOutOfRangeException(nameof(pos));

			return GetVerticesFromFace(mesh, mesh.GetFace(pos));
		}

		public static Vector3[] GetVerticesFromFace(this IMesh mesh, Vector3 face) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			return new Vector3[3] {
				mesh.GetVertex((uint) face.X),
				mesh.GetVertex((uint) face.Y),
				mesh.GetVertex((uint) face.Z)
			};
		}

		// http://james-ramsden.com/area-of-a-triangle-in-3d-c-code/
		public static double GetTriArea(this IMesh mesh, uint pos) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (pos >= mesh.FaceCount)
				throw new ArgumentOutOfRangeException(nameof(pos));

			return mesh.GetTriArea(mesh.GetVerticesFromFace(pos));
		}

		public static double GetTriArea(this IMesh mesh, Vector3 face) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			Vector3[] points = mesh.GetVerticesFromFace(face);
			return mesh.GetTriArea(points);
		}

		public static double GetTriArea(this IMesh mesh, Vector3[] points) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (points == null || points.Length != 3)
				throw new ArgumentNullException(nameof(points));


			double a = Vector3.Distance(points[0], points[1]);
			double b = Vector3.Distance(points[1], points[2]);
			double c = Vector3.Distance(points[2], points[0]);
			double sum = (a + b + c) / 2;
			return Math.Sqrt(sum * (sum - a) * (sum - b) * (sum - c));
		}

	}

}

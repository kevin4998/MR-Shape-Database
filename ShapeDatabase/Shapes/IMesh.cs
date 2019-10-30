using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using OpenTK;
using ShapeDatabase.Properties;
using ShapeDatabase.Util;

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

		Vector3 GetRandomVertex(Random rand);
	}

	public static class MeshEx {

		public static Vector3 GetVertex(this IMesh mesh, int pos) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (pos < 0)
				throw new ArgumentException(
					string.Format(
						Settings.Culture,
						Resources.EX_ExpPosValue,
						pos
					)
				);
			return mesh.GetVertex((uint) pos);
		}

		public static Vector3 GetFace(this IMesh mesh, int pos) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (pos < 0)
				throw new ArgumentException(
					string.Format(
						Settings.Culture,
						Resources.EX_ExpPosValue,
						pos
					)
				);
			return mesh.GetFace((uint) pos);
		}

		public static Vector3 GetNormal(this IMesh mesh, int pos) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (pos < 0)
				throw new ArgumentException(
					string.Format(
						Settings.Culture,
						Resources.EX_ExpPosValue,
						pos
					)
				);
			return mesh.GetNormal((uint) pos);
		}


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

			return Functions.GetTriArea(points);
		}
	}

}

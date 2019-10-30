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

		public static double GetTriArea(this IMesh mesh, Vector3[] points)
		{
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (points == null || points.Length != 3)
				throw new ArgumentNullException(nameof(points));

			return Functions.GetTriArea(points);
		}

		public static double GetTriArea(this IMesh mesh, Vector3 face) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			Vector3[] points = mesh.GetVerticesFromFace(face);
			return mesh.GetTriArea(points);
		}

		public static Vector3 GetRandomVertex(this IMesh mesh, Random rand, uint[] weightedvertexarray)
		{
			if (mesh == null || rand == null || weightedvertexarray == null)
			{
				throw new ArgumentNullException();
			}

			int index = rand.Next(0, Settings.WeightedVertexArraySize);
			Vector3 face = mesh.GetFace(weightedvertexarray[index]);

			index = rand.Next(0, 3);
			Vector3 vertex = mesh.GetVertex((uint)face[index]);

			return vertex;
		}

		public static uint[] SetWeightedVertexArray(this IMesh mesh)
		{
			if (mesh == null)
			{
				throw new ArgumentNullException();
			}

			double surfaceArea = 0;
			for (int i = 0; i < mesh.FaceCount; i++)
				surfaceArea += GetTriArea(mesh, mesh.GetFace((uint)i));

			uint[] WeightedVertexArray = new uint[Settings.WeightedVertexArraySize];

			double currentTotal = 0;
			for (uint i = 0; i < mesh.FaceCount; i++)
			{
				double endTotal = currentTotal + GetTriArea(mesh, mesh.GetFace(i)) / surfaceArea * (Settings.WeightedVertexArraySize - 1);

				for (int j = (int)Math.Ceiling(currentTotal); j < endTotal; j++)
					WeightedVertexArray[j] = i;

				currentTotal = endTotal;
			}

			return WeightedVertexArray;
		}
	}

}

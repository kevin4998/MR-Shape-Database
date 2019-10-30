using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using static ShapeDatabase.Util.Functions;
using g3;

namespace ShapeDatabase.Shapes {

	public class GeometryMesh : IMesh {

		#region --- Properties ---

		public g3.DMesh3 Base { get; }
		public bool IsNormalised { get; }

		public uint VertexCount => (uint) Base.VertexCount;
		public uint FaceCount => (uint) Base.TriangleCount;
		public uint EdgeCount => 0;
		public uint NormalCount => (uint) Base.VertexCount;

		private readonly Lazy<IBoundingBox> lazy;
		private readonly uint[] weightedvertexarray;

		public IEnumerable<Vector3> Vertices {
			get {
				for(int i = 0; i < VertexCount; i++)
					yield return VectorConvert(Base.GetVertex(i));
			}
		}
		public IEnumerable<Vector3> Faces {
			get {
				for (int i = 0; i < FaceCount; i++)
					yield return VectorConvert(Base.GetTriangle(i));
			}
		}
		public IEnumerable<Vector3> Edges => Enumerable.Empty<Vector3>();
		public IEnumerable<Vector3> Normals {
			get {
				if (Base.HasVertexNormals)
					for (int i = 0; i < NormalCount; i++)
						yield return VectorConvert(Base.GetVertexNormal(i));
				yield break;
			}
		}

		#endregion

		#region --- Constructor Methods ---

		public GeometryMesh(g3.DMesh3 mesh, bool normalised = false) {
			Base = mesh ?? throw new ArgumentNullException(nameof(mesh));
			IsNormalised = normalised;

			lazy = new Lazy<IBoundingBox>(InitializeBoundingBox);
			weightedvertexarray = SetWeightedVertexArray();
		}

		#endregion

		#region --- Methods ---

		#region -- Instance Methods --

		private IBoundingBox InitializeBoundingBox()
		{
			return AABB.FromMesh(this);
		}

		public IBoundingBox GetBoundingBox() {
			return lazy.Value;
		}

		public Vector3 GetVertex(uint pos) {
			return VectorConvert(Base.GetVertex(Convert.ToInt32(pos)));
		}

		public Vector3 GetFace(uint pos) {
			return VectorConvert(Base.GetTriangle(Convert.ToInt32(pos)));
		}

		public Vector3 GetNormal(uint pos) {
			return VectorConvert(Base.GetVertexNormal(Convert.ToInt32(pos)));
		}

		public double GetTriArea(uint tID)
		{
			return Base.GetTriArea(Convert.ToInt32(tID));
		}

		public Vector3 GetRandomVertex(Random rand)
		{
			if (rand == null)
			{
				throw new ArgumentNullException();
			}
			int index = rand.Next(0, 1000000);
			Vector3 face = GetFace(weightedvertexarray[index]);
			index = rand.Next(0, 3);
			Vector3 vertex = GetVertex((uint)face[index]);
			return vertex;
		}

		private uint[] SetWeightedVertexArray()
		{
			double surfaceArea = 0;
			for (int i = 0; i < FaceCount; i++)
			{
				surfaceArea += MeshEx.GetTriArea(this, GetFace((uint)i));
			}

			uint[] WeightedVertexArray = new uint[1000000];

			double currentTotal = 0;
			for (uint i = 0; i < FaceCount; i++)
			{
				double test = MeshEx.GetTriArea(this, GetFace((uint)i));
				double endTotal = currentTotal + MeshEx.GetTriArea(this, GetFace((uint)i)) / surfaceArea * 999999;
				
				for (int j = (int)Math.Ceiling(currentTotal); j < endTotal; j++)
				{
					WeightedVertexArray[j] = i;
				}
				currentTotal = endTotal;
			}

			return WeightedVertexArray;
		}

		public static GeometryMesh Create(g3.DMesh3 mesh) {
			return mesh == null ? null : new GeometryMesh(mesh);
		}

		#endregion

		#region -- Operators --

		public static implicit operator GeometryMesh(g3.DMesh3 mesh) {
			return ToGeometryMesh(mesh);
		}

		public static implicit operator g3.DMesh3(GeometryMesh mesh) {
			return ToDMesh3(mesh);
		}


		public static GeometryMesh ToGeometryMesh(DMesh3 mesh) {
			return mesh == null ? null : new GeometryMesh(mesh);
		}

		public static DMesh3 ToDMesh3(GeometryMesh mesh) {
			return mesh?.Base as g3.DMesh3;
		}

		#endregion

		#endregion

	}

}

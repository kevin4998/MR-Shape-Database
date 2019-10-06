using System;
using System.Collections.Generic;
using OpenTK;
using ShapeDatabase.Util;
using static ShapeDatabase.Util.Enumerators;
using static ShapeDatabase.Util.Functions;

namespace ShapeDatabase.Shapes {

	public class GeometryMesh : IMesh {

		#region --- Properties ---

		public g3.IMesh Base { get; }
		public bool IsNormalised => throw new NotImplementedException();

		public uint VertexCount => (uint) Base.VertexCount;
		public uint FaceCount => (uint) Base.TriangleCount;
		public uint EdgeCount => throw new NotImplementedException();
		public uint NormalCount => (uint) Base.VertexCount;

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
		public IEnumerable<Vector3> Edges => throw new NotImplementedException();
		public IEnumerable<Vector3> Normals {
			get {
				if (Base.HasVertexNormals)
					for (int i = 0; i < NormalCount; i++)
						yield return VectorConvert(Base.GetVertexNormal(i));
				else
					throw new NotImplementedException();
			}
		}

		#endregion

		#region --- Constructor Methods ---

		public GeometryMesh(g3.IMesh mesh) {
			Base = mesh ?? throw new ArgumentNullException(nameof(mesh));
		}

		#endregion

		#region --- Methods ---

		#region -- Instance Methods --

		public IBoundingBox GetBoundingBox() {
			throw new NotImplementedException();
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

		public static GeometryMesh Create(g3.IMesh mesh) {
			return mesh == null ? null : new GeometryMesh(mesh);
		}

		#endregion

		#region -- Operators --

		public static implicit operator GeometryMesh(g3.DMesh3 mesh) {
			return new GeometryMesh(mesh);
		}

		public static implicit operator g3.DMesh3(GeometryMesh mesh) {
			return mesh.Base as g3.DMesh3;
		}

		#endregion

		#endregion

	}

}

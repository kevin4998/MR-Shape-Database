using System;
using System.Collections.Generic;
using g3;
using OpenTK;
using ShapeDatabase.Properties;
using ShapeDatabase.Util.Collections;
using static ShapeDatabase.Util.Functions;

namespace ShapeDatabase.Shapes {

	public class GeometryMesh : AbstractMesh {

		#region --- Properties ---

		public g3.DMesh3 Base { get; }
		private bool normalised;

		public override bool IsNormalised {
			get => normalised;
			set => throw new InvalidOperationException();
		}

		public override uint VertexCount => (uint) Base.VertexCount;
		public override uint FaceCount => (uint) Base.TriangleCount;
		public override uint EdgeCount => 0;
		public override uint NormalCount => (uint) Base.VertexCount;

		public override IEnumerable<Vector3> Vertices {
			get {
				for (int i = 0; i < VertexCount; i++)
					yield return VectorConvert(Base.GetVertex(i));
			}
			set => throw new InvalidOperationException();
		}
		public override IEnumerable<Vector3> Faces {
			get {
				for (int i = 0; i < FaceCount; i++)
					yield return VectorConvert(Base.GetTriangle(i));
			}
			set => throw new InvalidOperationException();
		}
		public override IEnumerable<Vector3> Edges {
			get => System.Linq.Enumerable.Empty<Vector3>();
			set => throw new InvalidOperationException();
		}
		public override IEnumerable<Vector3> Normals {
			get {
				if (Base.HasVertexNormals)
					for (int i = 0; i < NormalCount; i++)
						yield return VectorConvert(Base.GetVertexNormal(i));
				yield break;
			}
			set => throw new InvalidOperationException();
		}

		#endregion

		#region --- Constructor Methods ---

		public GeometryMesh(g3.DMesh3 mesh, bool normalised = false) 
			: base() {
			Base = mesh ?? throw new ArgumentNullException(nameof(mesh));
			this.normalised = normalised;
		}

		#endregion

		#region --- Methods ---

		#region -- Instance Methods --

		public override Vector3 GetVertex(uint pos) {
			return VectorConvert(Base.GetVertex(Convert.ToInt32(pos)));
		}

		public override Vector3 GetFace(uint pos) {
			return VectorConvert(Base.GetTriangle(Convert.ToInt32(pos)));
		}

		public override Vector3 GetNormal(uint pos) {
			return VectorConvert(Base.GetVertexNormal(Convert.ToInt32(pos)));
		}

		public double GetTriArea(uint tID) {
			return Base.GetTriArea(Convert.ToInt32(tID));
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

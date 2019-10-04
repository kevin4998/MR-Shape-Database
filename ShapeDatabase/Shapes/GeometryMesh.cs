using System;
using System.Collections.Generic;
using OpenTK;
using ShapeDatabase.Util;
using static ShapeDatabase.Util.Enumerators;
using static ShapeDatabase.Util.Functions;

namespace ShapeDatabase.Shapes {

	public class GeometryMesh : IMesh {

		#region --- Properties ---

		private g3.IMesh Base { get; }
		public bool IsNormalised => throw new NotImplementedException();

		public uint VertexCount => (uint) Base.VertexCount;
		public uint FaceCount => (uint) Base.TriangleCount;
		public uint EdgeCount => throw new NotImplementedException();

		public IEnumerable<Vector3> Vertices =>
			FromEnumerator(
				CombineConvert(
					Base.VertexIndices().GetEnumerator(),
					Functions.VectorCreate
				)
			);
		public IEnumerable<Vector3> Faces =>
			FromEnumerator(
				CombineConvert(
					Base.TriangleIndices().GetEnumerator(),
					VectorCreate
				)
			);
		public IEnumerable<Vector3> Edges => throw new NotImplementedException();

		#endregion

		#region --- Constructor Methods ---

		public GeometryMesh(g3.IMesh mesh) {
			Base = mesh ?? throw new ArgumentNullException(nameof(mesh));
		}

		#endregion

		#region --- Methods ---

		public IBoundingBox GetBoundingBox() {
			throw new NotImplementedException();
		}
		public Vector3 GetVertex(uint pos) {
			return VectorConvert(Base.GetVertex(Convert.ToInt32(pos)));
		}

		public static GeometryMesh Create(g3.IMesh mesh) {
			return mesh == null ? null : new GeometryMesh(mesh);
		}

		#endregion

	}

}

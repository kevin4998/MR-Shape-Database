using System;
using System.Collections.Generic;
using OpenTK;
using ShapeDatabase.Util.Collections;

namespace ShapeDatabase.Shapes {

	public abstract class AbstractMesh : IMesh {

		#region --- Properties ---

		private readonly Lazy<IBoundingBox> lazyBB;
		private readonly Lazy<IWeightedCollection<Vector3>> lazyVertices;

		public abstract bool IsNormalised { get; set; }
		public abstract uint VertexCount { get; }
		public abstract uint FaceCount { get; }
		public abstract uint EdgeCount { get; }
		public abstract uint NormalCount { get; }
		public abstract IEnumerable<Vector3> Vertices { get; set; }
		public abstract IEnumerable<Vector3> Faces { get; set; }
		public abstract IEnumerable<Vector3> Edges { get; set; }
		public abstract IEnumerable<Vector3> Normals { get; set; }

		#endregion

		#region --- Constructor Methods ---

		public AbstractMesh() {
			lazyBB = new Lazy<IBoundingBox>(InitializeBoundingBox);
			lazyVertices = new Lazy<IWeightedCollection<Vector3>>(InitialiseWeights);
		}

		#endregion

		#region --- Instance Methods ---

		private IBoundingBox InitializeBoundingBox() => AABB.FromMesh(this);
		public IBoundingBox GetBoundingBox() => lazyBB.Value;

		private IWeightedCollection<Vector3> InitialiseWeights() {
			IWeightedCollection<Vector3> col =
						new WeightedCollection<Vector3>(Vertices);

			foreach (Vector3 face in Faces) {
				Vector3[] vertices = this.GetVerticesFromFace(face);
				double area = this.GetTriArea(vertices);

				foreach (Vector3 vertice in vertices)
					col.AddWeight(vertice, area);
			}

			return col;
		}
		public IWeightedCollection<Vector3> GetWeights() => lazyVertices.Value;

		public abstract Vector3 GetVertex(uint pos);
		public abstract Vector3 GetFace(uint pos);
		public abstract Vector3 GetNormal(uint pos);

		#endregion

	}
}

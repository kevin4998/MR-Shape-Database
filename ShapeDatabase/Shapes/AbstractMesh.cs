using System;
using System.Collections.Generic;
using OpenTK;
using ShapeDatabase.Util.Collections;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// A base implementation for meshes to handle lazy calculation
	/// of common variables.
	/// </summary>
	public abstract class AbstractMesh : IMesh {

		#region --- Properties ---

		private readonly Lazy<IBoundingBox> lazyBB;
		private readonly Lazy<IWeightedCollection<uint>> lazyVertices;

		public abstract bool IsNormalised { get; set; }
		public abstract uint VertexCount { get; }
		public abstract uint FaceCount { get; }
		public abstract uint EdgeCount { get; }
		public abstract uint NormalCount { get; }
		public abstract IEnumerable<Vector3> Vertices { get; set; }
		public abstract IEnumerable<Vector3> Faces { get; set; }
		public abstract IEnumerable<Vector2> Edges { get; set; }
		public abstract IEnumerable<Vector3> Normals { get; set; }

		#endregion

		#region --- Constructor Methods ---

		public AbstractMesh() {
			lazyBB = new Lazy<IBoundingBox>(InitializeBoundingBox);
			lazyVertices = new Lazy<IWeightedCollection<uint>>(InitialiseWeights, true);
		}

		#endregion

		#region --- Instance Methods ---

		private IBoundingBox InitializeBoundingBox() => AABB.FromMesh(this);
		public IBoundingBox GetBoundingBox() => lazyBB.Value;

		private IWeightedCollection<uint> InitialiseWeights() {
			IWeightedCollection<uint> col = new ArrayWC<uint>();

			for (int faceID = ((int) FaceCount) - 1; faceID >= 0; faceID--) {
				double area = this.GetTriArea((uint) faceID);
				col.AddWeight((uint) faceID, area);
			}

			return col;
		}
		public IWeightedCollection<uint> GetWeights() => lazyVertices.Value;

		public abstract Vector3 GetVertex(uint pos);
		public abstract Vector3 GetFace(uint pos);
		public abstract Vector3 GetNormal(uint pos);

		#endregion

	}
}

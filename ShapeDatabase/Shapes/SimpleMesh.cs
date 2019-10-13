using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// A direct implementation of <see cref="IMesh"/> which keeps all
	/// the vectors stored as arrays which get directly accessed and modified.
	/// </summary>
	public class SimpleMesh : IMesh {

		#region --- Properties ---

		private Vector3[] vertices;
		private Vector3[] faces;
		private Vector3[] edges;
		private Vector3[] normals;

		public bool IsNormalised { get; set; }

		public uint VertexCount => (uint) vertices.Length;
		public uint FaceCount	=> (uint) faces.Length;
		public uint EdgeCount	=> (uint) edges.Length;
		public uint NormalCount => (uint) normals.Length;


		public IEnumerable<Vector3> Vertices {
			get { return vertices; }
			set { vertices = AsArray(value, true); }
		}
		public IEnumerable<Vector3> Faces {
			get { return faces; }
			set { faces = AsArray(value, true); }
		}
		public IEnumerable<Vector3> Edges {
			get { return edges; }
			set { edges = AsArray(value, true); }
		}
		public IEnumerable<Vector3> Normals {
			get { return normals; }
			set { normals = AsArray(value, true); }
		}

		#endregion

		#region --- Constructor Methods ---

		public SimpleMesh(IMesh mesh)
			: this(mesh.Vertices, mesh.Faces,
				   mesh.Edges, mesh.Normals,
				   mesh.IsNormalised) { }

		public SimpleMesh(IEnumerable<Vector3> vertices,
						  IEnumerable<Vector3> faces,
						  IEnumerable<Vector3> edges = null,
						  IEnumerable<Vector3> normals = null,
						  bool normalised = false) {
			this.vertices = AsArray(vertices, true);
			this.faces	  = AsArray(faces,	  true);
			this.edges	  = AsArray(edges,	  false);
			this.normals  = AsArray(normals,  false);
			IsNormalised = normalised;
		}

		#endregion

		#region --- Methods ---

		#region -- Instance Methods --

		public IBoundingBox GetBoundingBox() {
			return AABB.FromMesh(this);
		}

		public Vector3 GetFace(uint pos) {
			return faces[pos];
		}
		public Vector3 GetNormal(uint pos) {
			return normals[pos];
		}
		public Vector3 GetVertex(uint pos) {
			return vertices[pos];
		}


		public void SetFace(uint pos, Vector3 value) {
			faces[pos] = value;
		}
		public void SetNormal(uint pos, Vector3 value) {
			normals[pos] = value;
		}
		public void SetVertex(uint pos, Vector3 value) {
			vertices[pos] = value;
		}

		#endregion

		#region -- Static Methods --

		private static Vector3[] AsArray(IEnumerable<Vector3> vectors,
										 bool error = true) {
			if (vectors == null) {
				if (error)
					throw new ArgumentNullException(nameof(vectors));
				else
					return new Vector3[0];
			}

			if (vectors is Vector3[] array)
				return array;

			return vectors.ToArray();
		}

		public static SimpleMesh CreateFrom(IMesh mesh) {
			return (mesh is SimpleMesh simple) ? simple : new SimpleMesh(mesh); 
		}

		#endregion

		#endregion

	}
}

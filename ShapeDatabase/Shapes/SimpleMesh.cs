using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// A direct implementation of <see cref="IMesh"/> which keeps all
	/// the vectors stored as arrays which get directly accessed and modified.
	/// </summary>
	public class SimpleMesh : AbstractMesh {

		#region --- Properties ---

		private Vector3[] vertices;
		private Vector3[] faces;
		private Vector2[] edges;
		private Vector3[] normals;

		public override bool IsNormalised { get; set; }

		public override uint VertexCount => (uint) vertices.Length;
		public override uint FaceCount => (uint) faces.Length;
		public override uint EdgeCount => (uint) edges.Length;
		public override uint NormalCount => (uint) normals.Length;


		public override IEnumerable<Vector3> Vertices {
			get { return vertices; }
			set { vertices = AsArray(value, true); }
		}
		public override IEnumerable<Vector3> Faces {
			get { return faces; }
			set { faces = AsArray(value, true); }
		}
		public override IEnumerable<Vector2> Edges {
			get { return edges; }
			set { edges = AsArray(value, true); }
		}
		public override IEnumerable<Vector3> Normals {
			get { return normals; }
			set { normals = AsArray(value, true); }
		}

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new mesh which can be edited using the specified interface.
		/// </summary>
		/// <param name="mesh">An implementation of a mesh which values need
		/// to be modified.</param>
		public SimpleMesh(IMesh mesh)
			: this(mesh?.Vertices, mesh.Faces,
				   mesh?.Edges, mesh.Normals,
				   (mesh == null) ? false : mesh.IsNormalised) { }

		/// <summary>
		/// Initialises a new mesh which can be edited using the specified default values.
		/// </summary>
		/// <param name="vertices">The vertices which make up the 3D shape.</param>
		/// <param name="faces">The faces which make up the 3D shapes.</param>
		/// <param name="edges">An optional collection of edges.</param>
		/// <param name="normals">An optional collection of normals.</param>
		/// <param name="normalised">If the current mesh is normalised.</param>
		public SimpleMesh(IEnumerable<Vector3> vertices,
						  IEnumerable<Vector3> faces,
						  IEnumerable<Vector2> edges = null,
						  IEnumerable<Vector3> normals = null,
						  bool normalised = false) : base() {
			this.vertices = AsArray(vertices, true);
			this.faces = AsArray(faces, true);
			this.edges = AsArray(edges, false);
			this.normals = AsArray(normals, false);
			IsNormalised = normalised;
		}

		#endregion

		#region --- Methods ---

		#region -- Instance Methods --

		public override Vector3 GetFace(uint pos) => faces[pos];
		public override Vector3 GetNormal(uint pos) => normals[pos];
		public override Vector3 GetVertex(uint pos) => vertices[pos];


		public void SetFace(uint pos, Vector3 value) => faces[pos] = value;
		public void SetNormal(uint pos, Vector3 value) => normals[pos] = value;
		public void SetVertex(uint pos, Vector3 value) => vertices[pos] = value;

		#endregion

		#region -- Static Methods --

		private static T[] AsArray<T>(IEnumerable<T> vectors,
										 bool error = true) {
			if (vectors == null) {
				if (error)
					throw new ArgumentNullException(nameof(vectors));
				else
					return Array.Empty<T>();
			}

			if (vectors is T[] array)
				return array;

			return vectors.ToArray();
		}

		/// <summary>
		/// Converts the specified interface object into a <see cref="SimpleMesh"/>
		/// safely.
		/// </summary>
		/// <param name="mesh">The mesh which needs to be converted.</param>
		/// <returns>A <see cref="SimpleMesh"/> which either is the same object
		/// if it could be cast or otherwise a mesh made from the data of the interface.
		/// </returns>
		public static SimpleMesh CreateFrom(IMesh mesh) {
			return (mesh is SimpleMesh simple) ? simple : new SimpleMesh(mesh);
		}

		#endregion

		#endregion

	}
}

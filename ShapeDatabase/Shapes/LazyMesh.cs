using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using ShapeDatabase.IO;
using ShapeDatabase.Util.Collections;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// An implementation of <see cref="IMesh"/> which lazy loads in the mesh when
	/// needed. If too much memory is used then the mesh can be collected by the GC
	/// for better memory usage.
	/// </summary>
	public class LazyMesh : IMesh {

		#region --- Properties ---

		private FileInfo file;
		private readonly Func<IMesh> meshProvider;
		private WeakReference<IMesh> lazyMesh;

		private IMesh Mesh {
			get {
				if (lazyMesh.TryGetTarget(out IMesh mesh))
					return mesh;
				mesh = meshProvider();
				lazyMesh.SetTarget(mesh);
				return mesh;
			}
		}

		public bool IsNormalised => this.Mesh.IsNormalised;

		public uint VertexCount => this.Mesh.VertexCount;
		public uint FaceCount => this.Mesh.FaceCount;
		public uint EdgeCount => this.Mesh.EdgeCount;
		public uint NormalCount => this.Mesh.NormalCount;

		public IEnumerable<Vector3> Vertices => this.Mesh.Vertices;
		public IEnumerable<Vector3> Faces => this.Mesh.Faces;
		public IEnumerable<Vector3> Edges => this.Mesh.Edges;
		public IEnumerable<Vector3> Normals => this.Mesh.Normals;

		#endregion

		#region --- Constructor Methods ---

		public LazyMesh(string filePath) : this(new FileInfo(filePath)) { }

		public LazyMesh(FileInfo info) {
			if (info == null || !info.Exists)
				throw new ArgumentNullException(nameof(info));

			this.file = info;
			this.meshProvider = () => Settings.FileManager.Read<IMesh>(file.FullName);
			this.lazyMesh = new WeakReference<IMesh>(meshProvider());
		}

		#endregion

		#region --- Instance Methods ---

		public IBoundingBox GetBoundingBox() => this.Mesh.GetBoundingBox();

		public Vector3 GetFace(uint pos) => this.Mesh.GetFace(pos);
		public Vector3 GetNormal(uint pos) => this.Mesh.GetNormal(pos);
		public Vector3 GetVertex(uint pos) => this.Mesh.GetVertex(pos);

		public Vector3 GetRandomVertex(Random rand) => this.Mesh.GetRandomVertex(rand);

		public IWeightedCollection<uint> GetWeights() => this.Mesh.GetWeights();

		#endregion

	}
}

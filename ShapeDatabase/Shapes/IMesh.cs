using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using OpenTK;
using ShapeDatabase.Properties;
using ShapeDatabase.Util;
using ShapeDatabase.Util.Collections;

namespace ShapeDatabase.Shapes {

	/// <summary>
	/// A collection of vertices, edges and faces to define a polyhedral 3D shape.
	/// </summary>
	public interface IMesh {

		/// <summary>
		/// If the current mesh is normalised in the [-1,1] bounds and
		/// with refinement as defined by the application.
		/// </summary>
		bool IsNormalised { get; }

		/// <summary>
		/// The number of vertices which this shapes holds.
		/// </summary>
		uint VertexCount { get; }
		/// <summary>
		/// The number of faces which this shapes holds.
		/// </summary>
		uint FaceCount { get; }
		/// <summary>
		/// The number of edges which this shapes holds.
		/// </summary>
		uint EdgeCount { get; }
		/// <summary>
		/// The number of normals which this shapes holds.
		/// </summary>
		uint NormalCount { get; }

		/// <summary>
		/// A collection of points in 3D space which can be connected in a variety of ways.
		/// </summary>
		IEnumerable<Vector3> Vertices { get; }
		/// <summary>
		/// A collection of surface faces which can be shown the user.
		/// The faces specify 3 points which can be converted to vertices.
		/// </summary>
		IEnumerable<Vector3> Faces { get; }
		/// <summary>
		/// A collection of connections between 2 vertices.
		/// </summary>
		IEnumerable<Vector2> Edges { get; }
		/// <summary>
		/// A collection of vectors in space on each face to define the viewing direction
		/// of the face. As each 3D plane can have 2 normals.
		/// </summary>
		IEnumerable<Vector3> Normals { get; }

		/// <summary>
		/// The box which perfectly surrounds all the vertices in this shape.
		/// The vertices may collide with the bounding box but may not go through it.
		/// </summary>
		/// <returns></returns>
		IBoundingBox GetBoundingBox();
		/// <summary>
		/// A weighted collection of all the triangles which allow for more accurate
		/// vertex chosing which is equally distributed along the shape.
		/// </summary>
		/// <returns>A collection containing the face positions weighted
		/// to allow equal distribution along the shape.</returns>
		IWeightedCollection<uint> GetWeights();

		/// <summary>
		/// Provides the vertex at the specified position within this mesh.
		/// </summary>
		/// <param name="pos">The location of the vertex.</param>
		/// <returns>A vertex in 3D space at this position within the collection.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If the specified position
		/// is below 0 or higher than the amount of vertices within the collection.
		/// </exception>
		Vector3 GetVertex(uint pos);
		/// <summary>
		/// Provides the face at the specified position within this mesh.
		/// </summary>
		/// <param name="pos">The location of the face.</param>
		/// <returns>A face in 3D space at this position within the collection.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If the specified position
		/// is below 0 or higher than the amount of faces within the collection.
		/// </exception>
		Vector3 GetFace(uint pos);
		/// <summary>
		/// Provides the normal at the specified position within this mesh.
		/// </summary>
		/// <param name="pos">The location of the normal.</param>
		/// <returns>A normal in 3D space at this position within the collection.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If the specified position
		/// is below 0 or higher than the amount of normals within the collection.
		/// </exception>
		Vector3 GetNormal(uint pos);
	}

	/// <summary>
	/// A class containing extension methods for easier mesh usage.
	/// </summary>
	public static class MeshEx {

		/// <summary>
		/// Provides the vertex at the specified position within this mesh.
		/// </summary>
		/// <param name="mesh">The mesh to retrieve the vertex from.</param>
		/// <param name="pos">The location of the vertex.</param>
		/// <returns>A vertex in 3D space at this position within the collection.</returns>
		/// <exception cref="ArgumentNullException">If the specified mesh does not exist.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">If the specified position
		/// is below 0 or higher than the amount of vertices within the collection.
		/// </exception>
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

		/// <summary>
		/// Provides the face at the specified position within this mesh.
		/// </summary>
		/// <param name="mesh">The mesh to retrieve the face from.</param>
		/// <param name="pos">The location of the face.</param>
		/// <returns>A face in 3D space at this position within the collection.</returns>
		/// <exception cref="ArgumentNullException">If the specified mesh does not exist.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">If the specified position
		/// is below 0 or higher than the amount of faces within the collection.
		/// </exception>
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

		/// <summary>
		/// Provides the normal at the specified position within this mesh.
		/// </summary>
		/// <param name="mesh">The mesh to retrieve the normal from.</param>
		/// <param name="pos">The location of the normal.</param>
		/// <returns>A normal in 3D space at this position within the collection.</returns>
		/// <exception cref="ArgumentNullException">If the specified mesh does not exist.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">If the specified position
		/// is below 0 or higher than the amount of normals within the collection.
		/// </exception>
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


		/// <summary>
		/// Converts the given face position into a collection of 3 vertices
		/// defining this face.
		/// </summary>
		/// <param name="mesh">The mesh to retrieve the vertices from.</param>
		/// <param name="pos">The location of the face.</param>
		/// <returns>An array containing 3 <see cref="Vector3"/>s who make up
		/// this triangle.</returns>
		/// <exception cref="ArgumentNullException">If the specified mesh does not exist.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">If the specified position
		/// is below 0 or higher than the amount of faces within the collection.
		/// </exception>
		public static Vector3[] GetVerticesFromFace(this IMesh mesh, uint pos) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (pos >= mesh.FaceCount)
				throw new ArgumentOutOfRangeException(nameof(pos));

			return GetVerticesFromFace(mesh, mesh.GetFace(pos));
		}

		/// <summary>
		/// Converts the given face into a collection of 3 vertices
		/// defining this face.
		/// </summary>
		/// <param name="mesh">The mesh to retrieve the vertices from.</param>
		/// <param name="face">The face with the position of the vertices.</param>
		/// <returns>An array containing 3 <see cref="Vector3"/>s who make up
		/// this triangle.</returns>
		/// <exception cref="ArgumentNullException">If the specified mesh does not exist.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">If the specified position
		/// is below 0 or higher than the amount of faces within the collection.
		/// </exception>
		public static Vector3[] GetVerticesFromFace(this IMesh mesh, Vector3 face) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			return new Vector3[3] {
				mesh.GetVertex((uint) face.X),
				mesh.GetVertex((uint) face.Y),
				mesh.GetVertex((uint) face.Z)
			};
		}


		/// <summary>
		/// Calculates the area of a triangle defined by this position.
		/// </summary>
		/// <param name="mesh">The mesh to retrieve the vertices from.</param>
		/// <param name="pos">The location of the face.</param>
		/// <returns>A single value defining the size of the surface area of
		/// a triangle.</returns>
		/// <exception cref="ArgumentNullException">If the specified mesh does not exist.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">If the specified position
		/// is below 0 or higher than the amount of faces within the collection.
		/// </exception>
		public static double GetTriArea(this IMesh mesh, uint pos) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (pos >= mesh.FaceCount)
				throw new ArgumentOutOfRangeException(nameof(pos));

			return mesh.GetTriArea(mesh.GetVerticesFromFace(pos));
		}

		/// <summary>
		/// Calculates the area of a triangle defined by these points.
		/// </summary>
		/// <param name="mesh">The mesh to retrieve the vertices from.</param>
		/// <param name="points">The 3 points which construct a triangle.</param>
		/// <returns>A single value defining the size of the surface area of
		/// a triangle.</returns>
		/// <exception cref="ArgumentNullException">If the specified mesh does not exist.
		/// Or the array did not contain exactly 3 points.</exception>
		public static double GetTriArea(this IMesh mesh, Vector3[] points)
		{
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (points == null || points.Length != 3)
				throw new ArgumentNullException(nameof(points));

			return Functions.GetTriArea(points);
		}

		/// <summary>
		/// Calculates the area of a triangle defined by this face.
		/// </summary>
		/// <param name="mesh">The mesh to retrieve the vertices from.</param>
		/// <param name="face">The face with the position of the vertices.</param>
		/// <returns>A single value defining the size of the surface area of
		/// a triangle.</returns>
		/// <exception cref="ArgumentNullException">If the specified mesh does not exist.
		/// </exception>
		public static double GetTriArea(this IMesh mesh, Vector3 face) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));

			Vector3[] points = mesh.GetVerticesFromFace(face);
			return mesh.GetTriArea(points);
		}


		/// <summary>
		/// Retrieves the specified number of random vertices from the mesh
		/// using the weighted collection of equal distribution along the shape
		/// ignoring high levels of detail.
		/// </summary>
		/// <param name="mesh">The mesh to retrieve the vertices from.</param>
		/// <param name="count">The amount of unique vectors to retrieve.</param>
		/// <param name="rand">The randomizer for deterministic behaviour.</param>
		/// <returns>An array containing the specified amount of randomly chosen
		/// vertices.</returns>
		/// <exception cref="ArgumentNullException">If the specified mesh or randomizer
		/// does not exist.</exception>
		public static Vector3[] GetRandomVertices(this IMesh mesh, int count, Random rand) {
			if (mesh == null)
				throw new ArgumentNullException(nameof(mesh));
			if (rand == null)
				throw new ArgumentNullException(nameof(rand));
			if (count < 0)
				throw new ArgumentException(
					string.Format(
						Settings.Culture,
						Resources.EX_ExpPosValue,
						count
					),
					nameof(count)
				);

			Vector3[] vertices = new Vector3[count];
			uint[] vertexIndices = new uint[count--];
			while (count >= 0)
			{
				int vertexIndex = rand.Next(0, 3);
				uint randomVertex = (uint)mesh.GetFace(mesh.GetWeights().GetElement(rand))[vertexIndex];
				while(vertexIndices.Contains(randomVertex))
				{
					randomVertex = (uint)mesh.GetFace(mesh.GetWeights().GetElement(rand))[vertexIndex];
				}
				vertices[count] = mesh.GetVertex((uint)mesh.GetFace(mesh.GetWeights().GetElement(rand))[vertexIndex]);
				vertexIndices[count--] = randomVertex;
			}

			return vertices;
		}

		/// <summary>
		/// Retrieves a single random vertex from the mesh using the
		/// weighted collection of equal distribution along the shape
		/// ignoring high levels of detail.
		/// </summary>
		/// <param name="mesh">The mesh to retrieve the vertices from.</param>
		/// <param name="rand">The randomizer for deterministic behaviour.</param>
		/// <returns>A single verted randomly chosen from the mesh.</returns>
		/// <exception cref="ArgumentNullException">If the specified mesh or randomizer
		/// does not exist.</exception>
		public static Vector3 GetRandomVertex(this IMesh mesh, Random rand)
			=> GetRandomVertices(mesh, 1, rand)[0];
	
	}

}

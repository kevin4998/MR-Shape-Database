using System;
using g3;
using OpenTK;

namespace ShapeDatabase.Util {

	public static class Functions {

		public static Vector3 VectorCreate(uint x, uint y, uint z) {
			return new Vector3(x, y, z);
		}

		public static Vector3 VectorCreate(int x, int y, int z) {
			return new Vector3(x, y, z);
		}

		public static Vector3 VectorCreate(float x, float y, float z) {
			return new Vector3(x, y, z);
		}

		public static Vector3 VectorCreate(double x, double y, double z) {
			return new Vector3(
				Convert.ToSingle(x),
				Convert.ToSingle(y),
				Convert.ToSingle(z)
			);
		}


		public static Vector3 VectorConvert(Vector3f vector) {
			return new Vector3(vector.x, vector.y, vector.z);
		}

		public static Vector3 VectorConvert(OpenTK.Vector3d vector) {
			return (Vector3) vector;
		}

		public static Vector3 VectorConvert(g3.Vector3d vector) {
			return new Vector3(
				Convert.ToSingle(vector.x),
				Convert.ToSingle(vector.y),
				Convert.ToSingle(vector.z)
			);
		}

		public static Vector3 VectorConvert(Index3i vector) {
			return new Vector3(vector.a, vector.b, vector.c);
		}

		/// <summary>
		/// Returns the area of a triangle
		/// </summary>
		/// <param name="points">Array with the three vertices</param>
		/// <returns>The area</returns>
		public static double GetTriArea(Vector3[] points)
		{
			double area = 0.5 * Vector3.Cross(points[1] - points[0], points[2] - points[0]).Length;
			return double.IsNaN(area) ? 0 : area;
		}

		/// <summary>
		/// Returns the volume of a tetrahedron
		/// </summary>
		/// <param name="points">Array with the four vertices of the tetrahedron</param>
		/// <returns>The volume</returns>
		public static double GetTetVolume(Vector3[] points)
		{
			double volume = Math.Abs(Vector3.Dot(points[0] - points[3], Vector3.Cross(points[1] - points[3], points[2] - points[3]))) / 6;
			return double.IsNaN(volume) ? 0 : volume;
		}
	}
}

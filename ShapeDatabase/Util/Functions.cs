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
		/// Returns the area of three vertices
		/// </summary>
		/// <param name="points">Array with the three vertices</param>
		/// <returns>The area</returns>
		public static double GetTriArea(Vector3[] points)
		{
			double a = Vector3.Distance(points[0], points[1]);
			double b = Vector3.Distance(points[1], points[2]);
			double c = Vector3.Distance(points[2], points[0]);
			double sum = (a + b + c) / 2;
			double area = Math.Sqrt(sum * (sum - a) * (sum - b) * (sum - c));
			return double.IsNaN(area) ? 0 : area;
		}
	}
}

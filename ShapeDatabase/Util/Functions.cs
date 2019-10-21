using System;
using System.Linq;
using g3;
using OpenTK;
using ShapeDatabase.Properties;

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
		public static double GetTriArea(Vector3[] points) {
			VerifyVectorArray(points, 3);

			double area = 0.5 * Vector3.Cross(points[1] - points[0], points[2] - points[0]).Length;
			return double.IsNaN(area) ? 0 : area;
		}

		/// <summary>
		/// Returns the volume of a tetrahedron
		/// </summary>
		/// <param name="points">Array with the four vertices of the tetrahedron</param>
		/// <returns>The volume</returns>
		public static double GetTetVolume(Vector3[] points) {
			VerifyVectorArray(points, 4);

			double volume = Math.Abs(Vector3.Dot(points[0] - points[3], Vector3.Cross(points[1] - points[3], points[2] - points[3]))) / 6;
			return double.IsNaN(volume) ? 0 : volume;
		}

		/// <summary>
		/// Returns the angle between three vertices
		/// </summary>
		/// <param name="points">Array with the three vertices</param>
		/// <returns>The angle (0-180 degrees)</returns>
		public static double GetAngleVertices(Vector3[] points) {
			VerifyVectorArray(points, 3);

			double angle = Math.Acos(Vector3.Dot(Vector3.Normalize(points[1] - points[0]), Vector3.Normalize(points[2] - points[0])));
			return double.IsNaN(angle) ? 0 : angle * 180/Math.PI;
		}

		/// <summary>
		/// Calculates the PTD, given two arrays with values and their weights.
		/// </summary>
		/// <param name="X_values">The values of the first array</param>
		/// <param name="X_weights">The weights of the first array</param>
		/// <param name="Y_values">The values of the second array</param>
		/// <param name="Y_weights">The values of the second array</param>
		/// <returns>The PTD</returns>
		public static double CalculatePTD(double[] X_values, double[] X_weights, double[] Y_values, double[] Y_weights)
		{
			if (X_values == null || X_weights == null || Y_values == null || Y_weights == null)
				throw new ArgumentNullException();
			if ((X_values.Length != X_weights.Length) || (Y_values.Length != Y_weights.Length) || (X_values.Length != Y_values.Length))
				throw new ArgumentNullException();
			
			double W = X_weights.Sum();
			double U = Y_weights.Sum();

			//Normalise both arrays, and the weights of the second array
			for (int i = 0; i < X_weights.Length; i++)
			{
				X_values[i] /= W;
				Y_values[i] /= U;
				Y_weights[i] *= (W / U);
			}

			//Count the flow of each bin
			double[] Distances = new double[X_values.Length];
			Distances[0] = X_values[0] * X_weights[0] - Y_values[0] * Y_weights[0];
			for (int i = 1; i < Distances.Length; i++)
			{
				Distances[i] = X_values[i] * X_weights[i] + Distances[i - 1] - Y_values[i] * Y_weights[i];
			}

			//Return the PTD
			return Math.Abs(Distances.Sum() / W);
		}

		/// <summary>
		/// Helper funtions to test if the given array follows the vector assumptions.
		/// </summary>
		/// <param name="points">The array that is being tested.</param>
		/// <param name="size">The size it should have.</param>
		private static void VerifyVectorArray(Vector3[] points, int size) {
			if (points == null)
				throw new ArgumentNullException(nameof(points));
			if (points.Length != size)
				throw new ArgumentException(
					Resources.EX_Invalid_Vector_Size,
					points.Length.ToString(Settings.Culture));
		}
	}
}

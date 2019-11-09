using System;
using System.Linq;
using g3;
using OpenTK;
using ShapeDatabase.Properties;

namespace ShapeDatabase.Util {

	/// <summary>
	/// A class to provide a collection of methods which can be used
	/// in functional programming with respect to vectors.
	/// </summary>
	public static class Functions {

		/// <summary>
		/// Converts the provided float vector from g3 into the OpenTK variant.
		/// </summary>
		/// <param name="vector">The vector to convert from its current float variant.
		/// </param>
		/// <returns>An OpenTK vector to use in all calculations.</returns>
		public static Vector3 VectorConvert(g3.Vector3f vector) {
			return new Vector3(vector.x, vector.y, vector.z);
		}

		/// <summary>
		/// Converts the provided double vector from g3 into the OpenTK variant.
		/// </summary>
		/// <param name="vector">The vector to convert from its current double variant.
		/// </param>
		/// <returns>An OpenTK vector to use in all calculations.</returns>
		public static Vector3 VectorConvert(g3.Vector3d vector) {
			return new Vector3(
				Convert.ToSingle(vector.x),
				Convert.ToSingle(vector.y),
				Convert.ToSingle(vector.z)
			);
		}

		/// <summary>
		/// Converts the provided integer vector from g3 into the OpenTK variant.
		/// </summary>
		/// <param name="vector">The vector to convert from its current integer variant.
		/// </param>
		/// <returns>An OpenTK vector to use in all calculations.</returns>
		public static Vector3 VectorConvert(g3.Index3i vector) {
			return new Vector3(vector.a, vector.b, vector.c);
		}


		/// <summary>
		/// Calculates the normal created by the given provided points.
		/// The order of the given points is important to determine the right normal.
		/// </summary>
		/// <param name="points">Array with the three vertices</param>
		/// <returns>A vector containing the normal.</returns>
		public static Vector3 GetNormal(Vector3[] points, bool normalised = true) {
			VerifyVectorArray(points, 3);

			Vector3 v1 = points[0];
			Vector3 v2 = points[1];
			Vector3 v3 = points[2];

			Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1);
			if (normalised)
				normal.Normalize();
			return normal;
		}

		/// <summary>
		/// Calculates the middle point of the provided triangle.
		/// </summary>
		/// <param name="points">Array with the three vertices</param>
		/// <returns>A vector containing the center point.</returns>
		public static Vector3 GetCenter(Vector3[] points) {
			VerifyVectorArray(points, 3);
			return (points[0] + points[1] + points[2]) / 3;
		}


		/// <summary>
		/// Returns the area of a triangle
		/// 
		/// <seealso cref="http://james-ramsden.com/area-of-a-triangle-in-3d-c-code/"/>
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
			return double.IsNaN(angle) ? 0 : angle * 180 / Math.PI;
		}

		/// <summary>
		/// Calculates the EMD, given two arrays with values.
		/// </summary>
		/// <param name="xValues">The values of the first array</param>
		/// <param name="yValues">The values of the second array</param>
		/// <returns>The EMD</returns>
		public static double CalculateEMD(double[] xValues, double[] yValues) {
			#region Parameter Checks
			if (xValues == null)
				throw new ArgumentNullException(nameof(xValues));
			if (yValues == null)
				throw new ArgumentNullException(nameof(yValues));
			if (xValues.Length != yValues.Length)
				throw new ArgumentException(
					string.Format(Settings.Culture,
						Resources.EX_UnEqual_Sizes,
						xValues.Length, yValues.Length
					)
				);
			#endregion

			//Count the flow of each bin
			double[] Distances = new double[xValues.Length];
			Distances[0] = 0;
			for (int i = 0; i < Distances.Length - 1; i++) {
				Distances[i + 1] = xValues[i] + Distances[i] - yValues[i];
			}

			//Return the EMD
			double totalDistance = Distances.Select(x => Math.Abs(x)).Sum();
			double normalisedTotalDistanece = totalDistance / (xValues.Length - 1);
			return (normalisedTotalDistanece);
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
					string.Format(
						Settings.Culture,
						Resources.EX_Invalid_Vector_Size,
						size,
						points.Length
					));
		}
	}
}

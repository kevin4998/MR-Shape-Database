using System;
using OpenTK;
using ShapeDatabase.Properties;

namespace ShapeDatabase.Util
{

	/// <summary>
	/// A class to provide extra functionalty on Arrays and numbers.
	/// </summary>
	public static class NumberUtil {

		/// <summary>
		/// Converts all the values in this array to a range between -1 and 1.
		/// This will make sure that the smallest value is always -1 and the biggest always 1.
		/// </summary>
		/// <param name="array">The array which will be used as a base for conversion.</param>
		/// <returns>A new array with the scaled values.</returns>
		public static Vector3[] Normalise(this Vector3[] array) {
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			float min = float.MaxValue;
			float max = float.MinValue;

			foreach (Vector3 vector in array)
			{
				for (int i = 0; i < 3; i++)
				{
					float value = vector[i];
					min = min > value ? value : min;
					max = max < value ? value : max;
				}
			}

			return Normalise(array, min, max);
		}

		/// <summary>
		/// Converts all the values in this array to a range between -1 and 1.
		/// This will make sure that the smallest value is always -1 and the biggest always 1.
		/// </summary>
		/// <param name="array">The array which will be used as a base for conversion.</param>
		/// <param name="min">The minimum value within the array.</param>
		/// <param name="max">The maximum value within the array.</param>
		/// <returns>A new array with the scaled values.</returns>
		public static Vector3[] Normalise(this Vector3[] array, float min, float max) {
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			Vector3[] result = new Vector3[array.Length];

			// Scale to [-1,1]
			float dif = 2 / (max - min);

			for (int i = array.Length - 1; i >= 0; i--)
			{
				Vector3 vector = array[i];
				Vector3 next = new Vector3();
				for (int j = 0; j < 3; j++)
				{
					next[j] = (vector[j] - min) * dif - 1.0f;
				}
				result[i] = next;
			}

			// Relocate to the center.
			float minx = float.MaxValue, miny = float.MaxValue, minz = float.MaxValue;
			float maxx = float.MinValue, maxy = float.MinValue, maxz = float.MinValue;

			foreach (Vector3 point in result) {
				minx = minx < point.X ? minx : point.X;
				maxx = maxx > point.X ? maxx : point.X;
				miny = miny < point.Y ? miny : point.Y;
				maxy = maxy > point.Y ? maxy : point.Y;
				minz = minz < point.Z ? minz : point.Z;
				maxz = maxz > point.Z ? maxz : point.Z;
			}
			float difx = (minx + maxx) / 2;
			float dify = (miny + maxy) / 2;
			float difz = (minz + maxz) / 2;
			Vector3 difv = new Vector3(difx, dify, difz);
			for (int i = array.Length - 1; i >= 0; i--)
				result[i] -= difv;

			return result;
		}

		/// <summary>
		/// Compares two floating point numbers for equality using a certain precision.
		/// 
		/// https://stackoverflow.com/a/3875619
		/// </summary>
		/// <param name="a">One value used for comparison.</param>
		/// <param name="b">Another value to compare with.</param>
		/// <param name="precision">The maximum difference between the two values.</param>
		/// <returns><see langword="true"/> if both values are equal
		/// with the specified precision.</returns>
		public static bool NearlyEqual(float a, float b, float precision) {
			const float Epsilon = float.Epsilon;
			float absA = Math.Abs(a);
			float absB = Math.Abs(b);
			float diff = Math.Abs(a - b);

			if (a.Equals(b))
				// Shortcut, handles infinities
				return true;
			else if (a == 0 || b == 0 || absA + absB < Epsilon)
				// a or b is zero or both are extremely close to it
				// relative error is less meaningful here
				return diff < (precision * Epsilon);
			else
				// Use relative error.
				return diff / (absA + absB) < precision;
		}

		/// <summary>
		/// A generic hash method based on java's hash implementation.
		/// <see cref="http://hg.openjdk.java.net/jdk8u/jdk8u/jdk/file/be44bff34df4/src/share/classes/java/util/Arrays.java"/>
		/// </summary>
		/// <param name="values">The different items to combine the hashcode off.</param>
		/// <returns>An integer uniquely representing the different values.</returns>
		public static int Hash(params object[] values) {
			if (values == null || values.Length == 0)
				return 0;

			int result  =1;
			foreach (object value in values)
				result = 31 * result + (value == null ? 0 : value.GetHashCode());
			return result;
		}

		public static Vector3[] Vectorize(this double[][] matrix) {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

			Vector3[] vectors = new Vector3[matrix.GetLength(0)];

			for (int i = vectors.Length - 1; i >= 0; i--)
				vectors[i] = AsVector(matrix[i]);

			return vectors;
				
		}

		public static float[][] InvertVectorize(Vector3[] vector) {
			if (vector == null)
				throw new ArgumentNullException(nameof(vector));

			float[][] matrix = new float[vector.Length][];
			for (int i = vector.Length - 1; i >= 0; i--)
				matrix[i] = vector[i].AsArray();
			return matrix;
		}

		public static double[][] InvertVectorizeD(Vector3[] vector) {
			if (vector == null)
				throw new ArgumentNullException(nameof(vector));

			double[][] matrix = new double[vector.Length][];
			for (int i = vector.Length - 1; i >= 0; i--)
				matrix[i] = vector[i].AsArrayD();
			return matrix;
		}

		public static float[] AsArray(this Vector3 vector) {
			if (vector == null)
				throw new ArgumentNullException(nameof(vector));
			return new float[] { vector.X, vector.Y, vector.Z };
		}

		public static double[] AsArrayD(this Vector3 vector) {
			if (vector == null)
				throw new ArgumentNullException(nameof(vector));
			return new double[] { vector.X, vector.Y, vector.Z };
		}

		public static Vector3 AsVector(this double[] array) {
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			float[] floats = new float[array.Length];
			for(int i = array.Length - 1; i >= 0; i--)
				floats[i] = Convert.ToSingle(array[i]);
			return AsVector(floats);
		}

		public static Vector3 AsVector(this float[] array) {
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (array.Length != 3)
				throw new ArgumentException(
					Resources.EX_Invalid_Vector_Size,
					array.Length.ToString(Settings.Culture)
				);

			return new Vector3(array[0], array[1], array[2]);
		}

		public static T Min<T>(params T[] values) where T : IComparable {
			if (values == null || values.Length == 0)
				throw new ArgumentNullException(nameof(values));

			T min = default;
			foreach(T value in values)
				min = min.CompareTo(value) <= 0 ? min : value;
			return min;
		}

		public static T Max<T>(params T[] values) where T : IComparable {
			if (values == null || values.Length == 0)
				throw new ArgumentNullException(nameof(values));

			T max = default;
			foreach (T value in values)
				max = max.CompareTo(value) >= 0 ? max : value;
			return max;
		}

		public static TGoal[] Cast<TFrom,TGoal>(this TFrom[] values,
								Func<TFrom, TGoal> parser) {
			if (values == null)
				throw new ArgumentNullException(nameof(values));
			if (parser == null)
				throw new ArgumentNullException(nameof(parser));

			TGoal[] result = new TGoal[values.Length];
			for(int i = values.Length - 1; i >= 0; i--)
				result[i] = parser(values[i]);
			return result;
		}

	}
}
using System;
using OpenTK;

namespace ShapeDatabase.Util
{

	/// <summary>
	/// A class to provide extra functionalty on Arrays.
	/// </summary>
	public static class ArrayUtil
	{

		/// <summary>
		/// Converts all the values in this array to a range between -1 and 1.
		/// This will make sure that the smallest value is always -1 and the biggest always 1.
		/// </summary>
		/// <param name="array">The array which will be used as a base for conversion.</param>
		/// <returns>A new array with the scaled values.</returns>
		public static Vector3[] Normalise(this Vector3[] array)
		{
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
		public static Vector3[] Normalise(this Vector3[] array, float min, float max)
		{
			Vector3[] result = new Vector3[array.Length];
			float dif = 2 / (max - min);

			// Scale to [-1,1]

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

	}
}
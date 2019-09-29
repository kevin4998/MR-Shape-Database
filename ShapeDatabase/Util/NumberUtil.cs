﻿using System;
using OpenTK;

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
		/// Compares to floating point numbers for equality using a certain precision.
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

	}
}
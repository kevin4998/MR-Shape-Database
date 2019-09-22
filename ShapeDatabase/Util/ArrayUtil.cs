namespace ShapeDatabase.Util {

	/// <summary>
	/// A class to provide extra functionalty on Arrays.
	/// </summary>
	public static class ArrayUtil {

		/// <summary>
		/// Converts all the values in this array to a range between -1 and 1.
		/// This will make sure that the smallest value is always -1 and the biggest always 1.
		/// </summary>
		/// <param name="array">The array which will be used as a base for conversion.</param>
		/// <returns>A new array with the scaled values.</returns>
		public static float[] Normalise(this float[] array) {
			float min = float.MaxValue;
			float max = float.MinValue;

			foreach(float value in array) { 
				min = min > value ? value : min;
				max = max < value ? value : max;
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
		public static float[] Normalise(this float[] array, float min, float max) {
			float[] result = new float[array.Length];
			float dif = 2 / (max - min);

			for (int i = array.Length - 1; i >= 0; i--)
				// Double cast for extra precision.
				result[i] = (array[i] - min) * dif - 1.0f;

			return result;
		}

	}
}

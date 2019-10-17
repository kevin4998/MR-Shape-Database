using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShapeDatabase.Properties;

namespace ShapeDatabase.Util {

	/// <summary>
	/// A class to provide extra functionaly which concerns with random
	/// operations.
	/// </summary>
	public static class RandomUtil {

		private static readonly Random globalRandom = new Random();
		private static readonly ThreadLocal<Random> localRandom =
			new ThreadLocal<Random>(RandomGenerator);

		/// <summary>
		/// Initialises a new Random object which will be unique
		/// even in a multi-threaded environment.
		/// </summary>
		/// <returns>A <see cref="Random"/> which is unique per thread.</returns>
		private static Random RandomGenerator() {
			int seed;
			lock (globalRandom) {
				seed = globalRandom.Next();
			}
			return new Random(seed);
		}

		/// <summary>
		/// Provides a instance of the <see cref="Random"/> class which can safely
		/// be used in a multi-threaded environment.
		/// </summary>
		public static Random ThreadSafeRandom {
			get {
				return localRandom.Value;
			}
		}


		/// <summary>
		/// Returns a non-negative random unsigned integer.
		/// </summary>
		/// <param name="random">The randomizer which will perform the calculations.
		/// </param>
		/// <returns> A 32-bit unsigned integer that is greater than or equal to 0
		/// and less than System.UInt32.MaxValue</returns>
		public static uint NextUint(this Random random) {
			if (random == null)
				throw new ArgumentNullException(nameof(random));

			/* Bitwise approach - 4 random calls in NextBytes() */
			// byte[] bytes = new byte[sizeof(uint)];
			// random.NextBytes(bytes);
			// return BitConverter.ToUInt32(bytes, 0);

			/* Integer approach - 2 random calls with Next() */
			return ((uint) random.Next(1 << 30)) << 2 | (uint) random.Next(1 << 2);
		}

		/// <summary>
		/// Returns a non-negative random unsigned integer
		/// that is less than the specified maximum.
		/// </summary>
		/// <param name="random">The randomizer which will perform the calculations.
		/// </param>
		/// <param name="maxValue">The exclusive upper bound of the random number
		/// to be generated. maxValue must be greater than or equal to 0.</param>
		/// <returns>A 32-bit unsigned integer that is greater than or equal to 0,
		/// and less than maxValue; that is, the range of return values ordinarily
		/// includes 0 but not maxValue. However, if maxValue equals 0,
		/// maxValue is returned.</returns>
		public static uint NextUint(this Random random, uint maxValue) {
			if (random == null)
				throw new ArgumentNullException(nameof(random));

			return (uint) (random.NextDouble() * maxValue);
		}

		/// <summary>
		/// Returns a random unsigned integer that is within a specified range.
		/// </summary>
		/// <param name="random">The randomizer which will perform the calculations.
		/// </param>
		/// <param name="minValue">The inclusive lower bound of the random number
		/// returned.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number
		/// returned. maxValue must be greater than or equal to minValue.</param>
		/// <returns>A 32-bit unsigned integer greater than or equal to minValue
		/// and less than maxValue; that is, the range of return values includes
		/// minValue but not maxValue. If minValue equals maxValue, minValue is
		/// returned.</returns>
		public static uint NextUint(this Random random, uint minValue, uint maxValue) {
			if (random == null)
				throw new ArgumentNullException(nameof(random));
			if (maxValue < minValue)
				throw new ArgumentOutOfRangeException(nameof(maxValue),
													  Resources.EX_MinMax_Switch);

			uint range = maxValue - minValue;
			if (range < int.MaxValue) {
				return ((uint) random.Next((int) range)) + minValue;
			} else { 
				return (uint) (random.NextDouble() * range) + minValue;
			}
		}

	}

}

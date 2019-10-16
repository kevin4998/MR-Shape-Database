using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Util
{
	/// <summary>
	/// Class for creating a thread safe random number generator
	/// </summary>
	public class ThreadSafeRandom
	{
		private static readonly Random _global = new Random();
		[ThreadStatic] private static Random _local;

		/// <summary>
		/// Generates a random integer between the lower (inclusive) and upper (exclusive) bound.
		/// </summary>
		/// <param name="low">Lower bound</param>
		/// <param name="high">Upper bound</param>
		/// <returns></returns>
		public int Next(int low, int high)
		{
			if (_local == null)
			{
				lock (_global)
				{
					if (_local == null)
					{
						int seed = _global.Next();
						_local = new Random(seed);
					}
				}
			}

			return _local.Next(low, high);
		}
	}
}

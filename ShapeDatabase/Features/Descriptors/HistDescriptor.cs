using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Properties;
using ShapeDatabase.Util;

namespace ShapeDatabase.Features.Descriptors {

	/// <summary>
	/// A class for complex descriptors which hold multiple values.
	/// These values are stored by making use if histograms where the values
	/// are grouped in buckets or bins.
	/// </summary>
	[DebuggerDisplay("{Name}: [{BinCount}]")]
	public class HistDescriptor : BaseDescriptor<HistDescriptor> {

		#region --- Properties ---

		/// <summary>
		/// The character which is used to seperate values of
		/// a single histogram descriptor.
		/// </summary>
		public static char HistSeperator => ';';

		public static string HistSeperatorString => HistSeperator.ToString(Settings.Culture);

		/// <summary>
		/// The width of one bin
		/// </summary>
		private double BinSize { get; }
		/// <summary>
		/// The number of values in each of the bins.
		/// </summary>
		private float[] BinValues { get; }

		public int BinCount => BinValues.Length;

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Constructor of the histogram descriptor
		/// </summary>
		/// <param name="name">Name of the descriptor</param>
		/// <param name="binsize">Bin width of the descriptor</param>
		/// <param name="binvalues">Bin values of the descriptor</param>
		public HistDescriptor(string name, double binsize, float[] binvalues)
			: base(name) {
			BinSize = binsize;

			float inverseLargest = 1 / binvalues.Sum();
			float[] normalisedBinValues = Array.ConvertAll(binvalues, value => value * inverseLargest);

			Debug.Assert(VerifyHistogram(normalisedBinValues));

			BinValues = normalisedBinValues;
		}

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Calculates the EMD distance with another histogram descriptor.
		/// </summary>
		/// <param name="desc">The other histogram descriptor</param>
		/// <returns>The distance (0 = Equal Descriptors, 1 = Completely Different)</returns>
		public override double Compare(HistDescriptor desc) {
			if (desc == null)
				throw new ArgumentNullException(nameof(desc));

			return Functions.CalculateEMD(
				BinValues.Cast<float, double>(x => x),
				desc.BinValues.Cast<float, double>(x => x));
		}

		/// <summary>
		/// Serializes the histogram descriptor.
		/// </summary>
		/// <returns>Serialized histogram descriptor</returns>
		public override string Serialize() {
			IFormatProvider format = Settings.Culture;

			string[] histValues = new string[BinValues.Length + 1];
			histValues[0] = BinSize.ToString(format);

			for (int i = 1; i < BinValues.Length + 1; i++)
				histValues[i] = BinValues[i - 1].ToString(format);

			return string.Join(HistSeperatorString, histValues);
		}

		#endregion

		#region -- Static Methods --

		public static bool TryParse(string name, string serialised,
									out HistDescriptor desc) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrEmpty(serialised))
				throw new ArgumentNullException(nameof(serialised));

			desc = null;
			string[] splits = serialised.Split(HistSeperator);

			string sizeString = splits[0];
			if (!double.TryParse(sizeString, out double binSize))
				return false;

			string[] stringBins = splits.Skip(1).ToArray();
			//if (stringBins.Length != BinCount)
			//	return false;
			if (!NumberUtil.TryParse(stringBins, float.TryParse, out float[] bins))
				return false;

			desc = new HistDescriptor(name, binSize, bins);
			return true;
		}

		public static HistDescriptor FromIntHistogram(string name, double binSize,
													int[] histogram) {
			return new HistDescriptor(name, binSize,
								 Array.ConvertAll(histogram, x => (float) x));
		}

#if DEBUG

		private static bool VerifyHistogram(float[] histogram) {
			if (histogram == null)
				throw new ArgumentNullException(nameof(histogram));
			if (histogram.Length == 0)
				throw new ArgumentException(Resources.EX_Empty_Array, nameof(histogram));
			// Verify that the next value is always between 0 and 1
			for (int i = 1; i < histogram.Length; i++)
				if (histogram[i] < 0 || histogram[i] > 1)
					return false;

			// Notify that the whole histogram is ok.
			return true;
		}

#endif

		#endregion

	}

}

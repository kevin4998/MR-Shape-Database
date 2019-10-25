using Accord.Diagnostics;
using ShapeDatabase.Properties;
using ShapeDatabase.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Descriptors {

	/// <summary>
	/// A class for complex descriptors which hold multiple values.
	/// These values are stored by making use if histograms where the values
	/// are grouped in buckets or bins.
	/// </summary>
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
		public float[] BinValues { get; }

		public int BinCount => BinValues.Length;

		/// <summary>
		/// Weight of the histogram descriptor
		/// </summary>
		public double Weight { get; } = 1;

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
			Debug.Assert(VerifyHistogram(binvalues));
			BinSize = binsize;
			BinValues = binvalues;
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

			double[] weights = Enumerable.Repeat(1d, BinValues.Length).ToArray();
			return Functions.CalculatePTD(
				BinValues.Cast<float, double>(x => x),
				weights,
				desc.BinValues.Cast<float, double>(x => x),
				weights);
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

			desc = FromNormalised(name, binSize, bins);
			return true;
		}


		public static HistDescriptor FromHistogram(string name, double binSize,
													int[] histogram) {
			return FromHistogram(name, binSize,
								 Array.ConvertAll(histogram, x => (float) x));
		}

		public static HistDescriptor FromHistogram(string name, double binSize,
													float[] histogram) {
			if (histogram == null)
				throw new ArgumentNullException(nameof(histogram));
			if (histogram.Length == 0)
				throw new ArgumentException(Resources.EX_Empty_Array, nameof(histogram));

			float[] accumulated = new float[histogram.Length];
			accumulated[0] = histogram[0];
			for (int i = 1; i < histogram.Length; i++)
				accumulated[i] = accumulated[i - 1] + histogram[i];

			return FromAccumulative(name, binSize, accumulated);
		}

		public static HistDescriptor FromAccumulative(string name, double binSize,
													float[] accHist) {
			if (accHist == null)
				throw new ArgumentNullException(nameof(accHist));
			if (accHist.Length == 0)
				throw new ArgumentException(Resources.EX_Empty_Array, nameof(accHist));

			float inverseLargest = 1 / accHist[accHist.Length - 1];
			float[] normalised = Array.ConvertAll(accHist, value => value * inverseLargest);
			return FromNormalised(name, binSize, normalised);
		}

		public static HistDescriptor FromNormalised(string name, double binSize,
													float[] normHist) {
			return new HistDescriptor(name, binSize, normHist);
		}

#if DEBUG

		private static bool VerifyHistogram(float[] histogram) {
			if (histogram == null)
				throw new ArgumentNullException(nameof(histogram));
			if (histogram.Length == 0)
				throw new ArgumentException(Resources.EX_Empty_Array, nameof(histogram));
			float last = histogram[0];
			// Verify that the next value is always bigger than the previous.
			for (int i = 1; i < histogram.Length; i++)
				if (last > histogram[i])
					return false;
				else
					last = histogram[i];
			// Verify that the last value always contains all scores.
			if (last != 1f)
				return false;
			// Notify that the whole histogram is ok.
			return true;
		}

#endif

		#endregion

	}

}

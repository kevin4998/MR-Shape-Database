using ShapeDatabase.Features.Descriptors;
using ShapeDatabase.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features {

	/// <summary>
	/// A class to represent a collection of descriptor values for a shape.
	/// </summary>
	public class FeatureVector : Util.IComparable<FeatureVector> {

		#region --- Properties ---

		// A sorted array containing all the descriptors.
		private readonly IDescriptor[] descriptors;

		/// <summary>
		/// A collection of descriptors which have been saved in this
		/// <see cref="FeatureVector"/>.
		/// </summary>
		public IEnumerable<IDescriptor> Descriptors => descriptors;

		/// <summary>
		/// A collection of the names of all the descriptors which have been
		/// saved in the current <see cref="FeatureVector"/>.
		/// </summary>
		public IEnumerable<string> DescriptorNames {
			get {
				foreach (IDescriptor descriptor in Descriptors)
					yield return descriptor.Name;
			}
		}

		/// <summary>
		/// The number of descriptors which are present in this vector.
		/// </summary>
		public int DescriptorCount => Descriptors.Count();

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Constructor for the featurevector, given all of its descriptors
		/// </summary>
		/// <param name="descriptors">A collection of specifications for a specific shape.
		/// </param>
		/// <exception cref="ArgumentNullException">if the given descriptors are
		/// <see langword="null"/>.</exception>
		public FeatureVector(IDescriptor[] descriptors) {
			if (descriptors == null)
				throw new ArgumentNullException(nameof(descriptors));

			Array.Sort(descriptors, DescriptorComparer.Instance);
			this.descriptors = descriptors;
		}

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Compare with another using the Proportional Transportation Distance.
		/// </summary>
		/// <param name="vector">The other vector</param>
		/// <returns>The PTD</returns>
		public double Compare(FeatureVector vector) 
		{
			return Compare(this, vector);
		}

		/// <summary>
		/// A manner to compare two features and see how similar they are.
		/// This method takes the assumption that the enumerator from the
		/// <see cref="FeatureVector"/>s return objects in an ordered manner.
		/// </summary>
		/// <param name="primary">The reference item for comparison.
		/// All descriptors will be compared with the other vector.</param>
		/// <param name="secondary">The comparison item, only descriptors which
		/// it shares with the reference vector will be compared.</param>
		/// <returns>A weighted average between [0,1] which shows the similarity.
		/// A value of 1 means that they are the exact same and 0 means that they
		/// are nothing alike.</returns>
		public static double Compare(FeatureVector primary, FeatureVector secondary) {
			if (primary == null)
				throw new ArgumentNullException(nameof(primary));
			if (secondary == null)
				throw new ArgumentNullException(nameof(secondary));

			DescriptorComparer comparer = DescriptorComparer.Instance;
			IEnumerator<IDescriptor> pdescs = primary.Descriptors.GetEnumerator();
			IEnumerator<IDescriptor> sdescs = secondary.Descriptors.GetEnumerator();
			 
			double sumCubedElem = 0;
			double sumHist = 0;
			int countHist = 0;
			bool hasNext = true;
			hasNext &= pdescs.MoveNext();
			hasNext &= sdescs.MoveNext();

			while (hasNext)
			{
				IDescriptor pdesc = pdescs.Current;
				IDescriptor sdesc = sdescs.Current;

				int dif = comparer.Compare(pdesc, sdesc);
				if (dif == 0)
				{
					if(pdesc is ElemDescriptor)
					{
						sumCubedElem += Math.Pow(pdesc.Compare(sdesc), 2);
					}
					else
					{
						sumHist += pdesc.Compare(sdesc);
						countHist = 0;
					}

					hasNext &= pdescs.MoveNext();
					hasNext &= sdescs.MoveNext();
					continue;

					// The primary one is beind so increment that side.
				}
				else if (dif < 0)
				{
					hasNext &= pdescs.MoveNext();

					// The secondary one is beind so increment that side.
				}
				else if (dif > 0)
				{
					hasNext &= sdescs.MoveNext();
				}
			}

			double L2 = Math.Sqrt(sumCubedElem);
			double distance = (sumHist + L2) / (countHist + 1);

			return distance;
		}

		#endregion

	}

	public static class FeatureVectorEx {

		public static IEnumerable<T> GetDescriptors<T>(this FeatureVector vector) 
			where T : IDescriptor{
			if (vector == null)
				throw new ArgumentNullException(nameof(vector));

			foreach (IDescriptor descriptor in vector.Descriptors)
				if (descriptor is T)
					yield return (T) descriptor;
		}

	}

}

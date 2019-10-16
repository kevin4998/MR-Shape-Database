using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.IO;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Features.Descriptors
{
	/// <summary>
	/// Class for calculating and/or importing featurevectors.
	/// </summary>
	public sealed class FeatureManager {

		#region --- Properties ---

		/// <summary>
		/// Delegate for calculating a descriptor.
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor is calculated</param>
		/// <returns>The calculated descriptor</returns>
		public delegate IDescriptor DescriptorCalculator(IMesh mesh);

		/// <summary>
		/// IDictionary with mesh names as key, and their featurevector as value.
		/// </summary>
		public IDictionary<string, FeatureVector> FeatureVectors { get; }
		/// <summary>
		/// IList of all descriptor calculater delegates.
		/// </summary>
		public IList<DescriptorCalculator> DescriptorCalculators { get; }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Constructor method of the featuremanager
		/// </summary>
		/// <param name="descriptorcalculators">The descriptor calculators that can be used to calculate the featurevectors</param>
		public FeatureManager(params DescriptorCalculator[] descriptorcalculators) 
			: this(null, descriptorcalculators) { }

		/// <summary>
		/// Constructor method of the featuremanager
		/// </summary>
		/// <param name="featurevectors">The featuresvectors, e.g. imported by the FMReader.</param>
		/// <param name="descriptorcalculators">The descriptor calculators that can be used to calculate the featurevectors</param>
		public FeatureManager(IDictionary<string, FeatureVector> featurevectors,
							  params DescriptorCalculator[] descriptorcalculators) {

			FeatureVectors = featurevectors ?? new Dictionary<string, FeatureVector>();
			DescriptorCalculators = new List<DescriptorCalculator>();

			if (DescriptorCalculators != null)
				foreach (DescriptorCalculator calculator in descriptorcalculators)
					if (calculator != null)
						DescriptorCalculators.Add(calculator);
		}

		#endregion

		#region --- Methods ---

		/// <summary>
		/// Add a descriptor calculator to the featuremanager
		/// </summary>
		/// <param name="calculator">The descriptor calculator delegate to add</param>
		/// <exception cref="ArgumentNullException">If the given calculator is
		/// <see langword="null"/>.</exception>
		public void AddCalculator(DescriptorCalculator calculator) {
			if (calculator == null) throw new ArgumentNullException(nameof(calculator));
			DescriptorCalculators.Add(calculator);
		}


		/// <summary>
		/// Calculate the featurevectores of all meshes in a library using all descriptor calculators of the featuremanager
		/// </summary>
		/// <param name="library">The library of which the featurevectors should be calculated</param>
		public void CalculateVectors(params MeshEntry[] library) {

			if (library != null)
				foreach(MeshEntry entry in library)
					CalculateVector(entry);
		}

		/// <summary>
		/// Calculate the featurevector of one mesh using all descriptor calculators of the featuremanager
		/// </summary>
		/// <param name="entry">The mesh of which the featurevector should be calculated</param>
		/// <returns></returns>
		public FeatureVector CalculateVector(MeshEntry entry, bool replace = false) {

			// Check if we already ran the computations for this entry.
			if (FeatureVectors.TryGetValue(entry.Name, out FeatureVector found)) {
				// If we may not change the vector then just give the old one.
				if (!replace)
					return found;
				// If we may change the vector then update the content and save that
				// in the database.
				FeatureVector expanded = ExpandVector(entry, found);
				FeatureVectors.Remove(entry.Name);
				FeatureVectors.Add(entry.Name, expanded);
				return expanded;
			} else {
				// If we do not own it then create a new one to save and return.
				FeatureVector created = CreateVector(entry);
				FeatureVectors.Add(entry.Name, created);
				return created;
			}

		}

		/// <summary>
		/// Expands an already existing vector with new information if
		/// new descriptors has been added since the last creation.
		/// </summary>
		/// <param name="entry">The element which will be used to calculate
		/// new features.</param>
		/// <param name="baseVector">The previous <see cref="FeatureVector"/>
		/// from this entry which should be enlarged with more information.</param>
		/// <returns>The new <see cref="FeatureVector"/> with all the descriptors.
		/// </returns>
		private FeatureVector ExpandVector(MeshEntry entry, FeatureVector baseVector) {


			FeatureVector newVector = CreateVector(entry);
			IEnumerable<IDescriptor> updates = ExceptWith(newVector.Descriptors,
														  baseVector.Descriptors);
			IList<IDescriptor> descriptors = baseVector.Descriptors.ToList();

			foreach (IDescriptor desc in updates)
				descriptors.Add(desc);

			return new FeatureVector(descriptors);
		}

		/// <summary>
		/// Creates a new vector for the current entry to be added in the database.
		/// </summary>
		/// <param name="entry">The element whose <see cref="FeatureVector"/>
		/// should be created.</param>
		/// <returns>A <see cref="FeatureVector"/> containing the information
		/// for the provided mesh.</returns>
		private FeatureVector CreateVector(MeshEntry entry) {
			IList<IDescriptor> descriptors = new List<IDescriptor>();

			foreach (DescriptorCalculator calculator in DescriptorCalculators)
				descriptors.Add(calculator(entry.Mesh));

			return new FeatureVector(descriptors);
		}

		/// <summary>
		/// Gives the difference between two <see cref="IDescriptor"/> collections.
		/// Hereby all the descriptors which were not present in the right set but
		/// were in the left set will be returned.
		/// </summary>
		/// <param name="left">The collection of descriptors which might be returned.
		/// </param>
		/// <param name="right">The collection of descriptors which should not be
		/// returned/used.</param>
		/// <returns>A collection which contains only descriptors from the left set
		/// that were not present in the right one.</returns>
		private static IEnumerable<IDescriptor> ExceptWith(
			IEnumerable<IDescriptor> left,  IEnumerable<IDescriptor> right) {

			return left.Except(right, DescriptorComparer.Instance);
		}


		/// <summary>
		/// Compares the given mesh with all the saved meshes in this
		/// <see cref="FeatureManager"/> and returns all the meshes ordered by the
		/// similarity.
		/// </summary>
		/// <param name="mesh">The name of the mesh that should be compared with all other meshes in the database.</param>
		/// <returns>A <see cref="IList{T}"/> containing all the meshes in this manager
		/// and ordered by their similarity. The <see cref="IList{T}"/> has a tuple
		/// containing the name of the mesh as well as an indicator of similarity
		/// represented as double. The results are ordered (best match first).</returns>
		public IList<(string, double)> CalculateResults(string mesh)
		{
			(string, double)[] results = new (string, double)[FeatureVectors.Count];
			FeatureVector vector = FeatureVectors[mesh];

			Parallel.For(0, FeatureVectors.Count, i =>
			{
				double result = FeatureVectors.ElementAt(i).Value.Compare(vector);
				results[i] = (FeatureVectors.ElementAt(i).Key, result);
			});

			return results.OrderBy(x => x.Item2).Skip(1).ToList();
		}

		#endregion

	}
}

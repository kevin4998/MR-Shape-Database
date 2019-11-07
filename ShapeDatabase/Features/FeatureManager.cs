﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Diagnostics;
using ShapeDatabase.IO;
using ShapeDatabase.Query;
using ShapeDatabase.Shapes;
using ShapeDatabase.Util;

namespace ShapeDatabase.Features.Descriptors {
	/// <summary>
	/// Class for calculating and/or importing featurevectors.
	/// </summary>
	public sealed class FeatureManager {

		#region --- Properties ---

		/// <summary>
		/// A dictionary with mesh names as key, and their featurevector as value.
		/// </summary>
		private IDictionary<string, FeatureVector> features;

		/// <summary>
		/// Delegate for calculating a descriptor.
		/// </summary>
		/// <param name="mesh">The mesh of which the descriptor is calculated</param>
		/// <returns>The calculated descriptor</returns>
		public delegate IDescriptor DescriptorCalculator(IMesh mesh);

		/// <summary>
		/// A dictionary with mesh names as key, and their featurevector as value.
		/// </summary>
		public IDictionary<string, FeatureVector> VectorDictionary => features;
		/// <summary>
		/// A collection of all the saved feature vectors in this manager.
		/// </summary>
		public IEnumerable<FeatureVector> FeatureVectors => features.Values;
		/// <summary>
		/// The total amount of features stored in this manager.
		/// </summary>
		public int FeatureCount => features.Count;
		/// <summary>
		/// IList of all descriptor calculater delegates.
		/// </summary>
		public IList<DescriptorCalculator> DescriptorCalculators { get; }
		/// <summary>
		/// A collection of all the names generated by the different descriptors.
		/// The descriptors here are only valid if at least 1 featurevector has been
		/// generated.
		/// </summary>
		public IEnumerable<string> DescriptorNames {
			get {
				// Exception case if there are no values yet.
				if (features.Count == 0)
					return Enumerable.Empty<string>();
				// We expect all the vectors to have the same amount of descriptors,
				// so taking any should suffice to find the names.
				FeatureVector exampleVector = features.First().Value;
				return exampleVector.DescriptorNames;
			}
		}

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
			if (featurevectors == null)
				throw new ArgumentNullException(nameof(featurevectors));
			Debug.Assert(descriptorcalculators.All(x => x != null));

			features = new Dictionary<string, FeatureVector>(featurevectors);
			DescriptorCalculators = new List<DescriptorCalculator>(descriptorcalculators);
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
			if (calculator == null)
				throw new ArgumentNullException(nameof(calculator));
			DescriptorCalculators.Add(calculator);
		}


		/// <summary>
		/// Calculate the featurevectores of all meshes in a library using all descriptor calculators of the featuremanager
		/// </summary>
		/// <param name="library">The library of which the featurevectors should be calculated</param>
		public void CalculateVectors(params MeshEntry[] library) {
			CalculateVectors((ICollection<MeshEntry>) library);
		}

		/// <summary>
		/// Calculate the featurevectores of all meshes in a library using all descriptor calculators of the featuremanager
		/// </summary>
		/// <param name="library">The library of which the featurevectors should be calculated</param>
		public void CalculateVectors(ICollection<MeshEntry> library, bool async = true) {
			if (library == null)
				throw new ArgumentNullException(nameof(library));

			using (ProgressBar progress = new ProgressBar(library.Count)) { 
				foreach (MeshEntry entry in library) { 
					CalculateVector(entry);
					progress.CompleteTask();
				}
			}

			NormaliseVectors();
		}

		/// <summary>
		/// Calculate the featurevector of one mesh using all descriptor calculators of the featuremanager
		/// </summary>
		/// <param name="entry">The mesh of which the featurevector should be calculated</param>
		/// <returns></returns>
		private FeatureVector CalculateVector(MeshEntry entry, bool replace = false) {

			// Check if we already ran the computations for this entry.
			if (features.TryGetValue(entry.Name, out FeatureVector found)) {
				// If we may not change the vector then just give the old one.
				if (!replace)
					return found;
				// If we may change the vector then update the content and save that
				// in the database.
				FeatureVector expanded = ExpandVector(entry, found);
				features.Remove(entry.Name);
				features.Add(entry.Name, expanded);
				return expanded;
			} else {
				// If we do not own it then create a new one to save and return.
				FeatureVector created = CreateVector(entry);
				features.Add(entry.Name, created);
				return created;
			}
		}


		/// <summary>
		/// Updates the min and max values and normalises the database meshes.
		/// </summary>
		public void NormaliseVectors() {
			FeatureNormaliser.Instance.UpdateMinMaxDictionary(features);
			FeatureNormaliser.Instance.NormaliseVectors(ref features);
		}

		public FeatureVector NormaliseVector(FeatureVector vector) {
			return FeatureNormaliser.Instance.NormaliseVector(vector);
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

			return new FeatureVector(descriptors.ToArray());
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

			return new FeatureVector(descriptors.ToArray());
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
			IEnumerable<IDescriptor> left, IEnumerable<IDescriptor> right) {

			return left.Except(right, DescriptorComparer.Instance);
		}



		/// <summary>
		/// Compares the given mesh with all the saved meshes in this
		/// <see cref="FeatureManager"/> and returns all the meshes ordered by the
		/// similarity.
		/// </summary>
		/// <param name="size">The size of the return value for the query specified
		/// by an enum constant.</param>
		/// <param name="mesh">The mesh that should be compared with all other meshes in the database.</param>
		/// <returns>A <see cref="QueryResult"/> which contains the K-best similair
		/// matches from the entire database for this entry.</returns>
		public QueryResult CalculateResults(QuerySize size, MeshEntry mesh) =>
			CalculateResults(size, new MeshEntry[] { mesh })[0];

		/// <summary>
		/// Compares the given meshes with all the saved meshes in this
		/// <see cref="FeatureManager"/> and returns all the meshes ordered by the
		/// similarity.
		/// </summary>
		/// <param name="count">The number of elements to return in the result.
		/// -1 identifies that all the results should be returned.</param>
		/// <param name="meshes">The mesh that should be compared with all other meshes in the database.</param>
		/// <returns>A <see cref="QueryResult"/> array which contains the K-best similair
		/// matches from the entire database for these entries.</returns>
		public QueryResult[] CalculateResults(QuerySize size, params MeshEntry[] meshes) =>
			CalculateResults(size, (IEnumerable<MeshEntry>) meshes);

		/// <summary>
		/// Compares the given meshes with all the saved meshes in this
		/// <see cref="FeatureManager"/> and returns all the meshes ordered by the
		/// similarity.
		/// </summary>
		/// <param name="meshes">The mesh that should be compared with all other meshes
		/// in the database.</param>
		/// <returns>A <see cref="QueryResult"/> array which contains the K-best similair
		/// matches from the entire database for these entries.</returns>
		public QueryResult[] CalculateResults(QuerySize size, IEnumerable<MeshEntry> meshes) {
			if (meshes == null)
				throw new ArgumentNullException(nameof(meshes));

			ANN HNSW = new ANN(features);

			ConcurrentBag<QueryResult> results = new ConcurrentBag<QueryResult>();
			Parallel.ForEach(meshes, entry => {
				FeatureVector queryVector = CreateVector(entry);
				queryVector = NormaliseVector(queryVector);
				int count = QuerySizeToInt(size, entry.Class);
				results.Add(HNSW.RunANNQuery(entry.Name,
											 queryVector,
											 count));
			});

			QueryResult[] array = results.ToArray();
			Array.Sort(array);
			return array;
		}


		/// <summary>
		/// Compares all the featurevectors in the database with each other.
		/// </summary>
		/// <param name="size">The size of best number of elements.</param>
		/// <returns>A <see cref="QueryResult"/> array which contains the K-best similair
		/// matches from the entire database for these entries.</returns>
		public QueryResult[] InternalCompare(QuerySize size) {

			ANN HNSW = new ANN(features);

			ConcurrentBag<QueryResult> results = new ConcurrentBag<QueryResult>();
			Parallel.ForEach(VectorDictionary, pair => {
				string name = pair.Key;
				FeatureVector vector = pair.Value;
				string clazz = Settings.FileManager.ClassByShapeName(name);
				int count = QuerySizeToInt(size, clazz);

				results.Add(HNSW.RunANNQuery(name, vector, count));
			});

			return results.ToArray();
		}

		private int QuerySizeToInt(QuerySize size, string clazz) {
			int count = 0;
			// Calculate the size based on the given enum.
			switch (size) {
			case QuerySize.KBest:
				count = Math.Min(Settings.KBestResults, FeatureCount);
				break;
			case QuerySize.Class:
				count = Settings.FileManager.ShapesInClass(clazz);
				break;
			case QuerySize.All:
				count = FeatureCount;
				break;
			}
			// Edge case: The value is still 0 for whatever reason.
			if (count == 0)
				count = Math.Min(Settings.KBestResults, FeatureCount);

			return count;
		}

		#endregion

	}
}

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Query;
using ShapeDatabase.Util;

namespace ShapeDatabase.Features {

	/// <summary>
	/// A single class containing all the weights for the provided Descriptors.
	/// </summary>
	public class WeightManager {

		#region --- Properties ---

		#region -- Static Properties --

		/// <summary>
		/// Provides a common instance throughout the whole application
		/// of all the descriptors with their weights.
		/// </summary>
		public static WeightManager Instance => lazy.Value;

		private static readonly Lazy<WeightManager> lazy
			= new Lazy<WeightManager>(() => new WeightManager());

		#endregion

		#region -- Instance Properties --

		protected virtual IDictionary<string, double> WeightDic { get; }
			= new ConcurrentDictionary<string, double>();
		protected virtual LockManager Locks { get; } = new LockManager();


		/// <summary>
		/// An interface to access or change weight values
		/// of descriptors.
		/// </summary>
		/// <param name="name">The name of the descriptor whose value to change.</param>
		/// <returns>A double representing the weight of the value in a feature vector.
		/// </returns>
		public double this[string name] {
			get => GetWeight(name);
			set => SetWeight(name, value);
		}

		/// <summary>
		/// A collection of all the weights with the name of their descriptor.
		/// </summary>
		public IEnumerable<(string, double)> Weights {
			get {
				foreach (KeyValuePair<string, double> pair in WeightDic)
					yield return (pair.Key, pair.Value);
			}
		}

		#endregion

		#endregion

		#region --- Constructor Methods ---

		private WeightManager() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Provides the weight of the given descriptor or 1 if it is not in
		/// the manager.
		/// </summary>
		/// <param name="descriptorName">The name of the descriptor to retrieve
		/// the value from.</param>
		/// <returns>A double which is the weight of this one descriptor in comparison
		/// to all the other values.</returns>
		public double GetWeight(string descriptorName) {
			if (!WeightDic.TryGetValue(descriptorName, out double value))
				lock (Locks[descriptorName]) {
					if (!WeightDic.TryGetValue(descriptorName, out value)) {
						WeightDic[descriptorName] = value = 1;
					}
				}
			return value;
		}

		/// <summary>
		/// Specifiest he weight of the given descriptor in the manager.
		/// </summary>
		/// <param name="descriptorName">The name of the descriptor to specify
		/// the value of.</param>
		/// <param name="weight">A double which is the weight of this one descriptor
		/// in comparison to all the other values.</param>
		public void SetWeight(string descriptorName, double weight) {
			lock (Locks[descriptorName]) {
				WeightDic[descriptorName] = weight;
			}
		}

		#endregion

	}

}

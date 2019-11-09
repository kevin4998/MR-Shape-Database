using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features {

	/// <summary>
	/// Class for storing a mesh name and corresponding featurevector.
	/// </summary>
	public struct NamedFeatureVector : IEquatable<NamedFeatureVector> {

		/// <summary>
		/// Name of the featurevector.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The corresponding featurevector.
		/// </summary>
		public FeatureVector FeatureVector { get; }

		/// <summary>
		/// Initialization of the namedfeaturevector.
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="featurevector">The featurevector</param>
		public NamedFeatureVector(string name, FeatureVector featurevector) {
			Name = name;
			FeatureVector = featurevector;
		}

		public override bool Equals(object obj) {
			return obj != null
				&& obj is NamedFeatureVector
				&& Equals((NamedFeatureVector) obj);
		}

		public override int GetHashCode() {
			return Name.GetHashCode();
		}

		/// <summary>
		/// States whether this named featurevector is equal to another named featurevector.
		/// </summary>
		/// <param name="other">The other named featurevector.</param>
		/// <returns>Boolstating whether they are equal.</returns>
		public bool Equals(NamedFeatureVector other) {
			return other.Name == Name;
		}

		/// <summary>
		/// States whether two featurevectors are equal.
		/// </summary>
		/// <param name="left">The first featurevector.</param>
		/// <param name="right">The second featurevector.</param>
		/// <returns>Bool stating whether they are equal.</returns>
		public static bool operator ==(NamedFeatureVector left, NamedFeatureVector right) {
			return left.Equals(right);
		}

		/// <summary>
		/// States whether two featurevectors are not equal.
		/// </summary>
		/// <param name="left">The first featurevector.</param>
		/// <param name="right">The second featurevector.</param>
		/// <returns>Bool stating whether they are not equal.</returns>
		public static bool operator !=(NamedFeatureVector left, NamedFeatureVector right) {
			return !(left == right);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeDatabase.Features
{
	/// <summary>
	/// Class for storing a mesh name and corresponding featurevector
	/// </summary>
	public struct NamedFeatureVector : IEquatable<NamedFeatureVector>
	{
		/// <summary>
		/// Name of the featurevector
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The corresponding featurevector
		/// </summary>
		public FeatureVector FeatureVector { get; }

		/// <summary>
		/// Initialization of the namedfeaturevector
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="featurevector">The featurevector</param>
		public NamedFeatureVector(string name, FeatureVector featurevector)
		{
			Name = name;
			FeatureVector = featurevector;
		}

		public override bool Equals(object obj)
		{
			return obj != null
				&& obj is NamedFeatureVector
				&& Equals((NamedFeatureVector)obj);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public bool Equals(NamedFeatureVector other)
		{
			return other.Name == Name;
		}

		public static bool operator ==(NamedFeatureVector left, NamedFeatureVector right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(NamedFeatureVector left, NamedFeatureVector right)
		{
			return !(left == right);
		}
	}
}

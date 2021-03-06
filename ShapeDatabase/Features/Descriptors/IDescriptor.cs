﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Util;

namespace ShapeDatabase.Features.Descriptors {

	/// <summary>
	/// An interface to specify descriptions or features of shapes.
	/// </summary>
	public interface IDescriptor : ISerialisable, Util.IComparable<IDescriptor> {

		/// <summary>
		/// The unique identifier of this descriptor.
		/// This specifies which feature is being used.
		/// </summary>
		string Name { get; }

	}

	/// <summary>
	/// An interface to specify descriptors or features of shapes.
	/// </summary>
	/// <typeparam name="T">Type of descriptor</typeparam>
	public interface IDescriptor<T> : IDescriptor where T : IDescriptor<T> {

		/// <summary>
		/// Method for comparison with another object of the same type.
		/// </summary>
		/// <param name="desc">The object to check for similarity.</param>
		/// <returns>A double which represent the similarity between the objects.
		/// 1 means that two values are the same and 0 means that they are nothing alike.
		/// The returned value will always be within the range of [0,1].</returns>
		double Compare(T desc);
	}

	/// <summary>
	/// A class which can be used to compare existing <see cref="IDescriptor"/>s by using their <see cref="IDescriptor.Name"/>.
	/// </summary>
	public class DescriptorComparer : IEqualityComparer<IDescriptor>,
									  IComparer<IDescriptor> {

		#region --- Properties ---

		private static readonly Lazy<DescriptorComparer> lazy =
			new Lazy<DescriptorComparer>(() => new DescriptorComparer());
		/// <summary>
		/// The only object of this kind for comparison.
		/// </summary>
		public static DescriptorComparer Instance => lazy.Value;

		private readonly StringComparison comparison =
			StringComparison.InvariantCultureIgnoreCase;
		private readonly StringComparer comparer =
			StringComparer.InvariantCultureIgnoreCase;

		#endregion

		#region --- Constructor Methods ---

		private DescriptorComparer() { }

		#endregion

		#region --- Instance Methods ---

		public bool Equals(IDescriptor x, IDescriptor y) {
			return x == null
				? y == null
				: y != null
					&& x.Name.Equals(y.Name, comparison);
		}

		public int GetHashCode(IDescriptor obj) {
			return (obj == null) ? 0 : obj.GetHashCode();
		}

		public int Compare(IDescriptor x, IDescriptor y) {
			if (x == null)
				throw new ArgumentNullException(nameof(x));
			if (y == null)
				throw new ArgumentNullException(nameof(y));

			return comparer.Compare(x.Name, y.Name);
		}

		#endregion

	}

}

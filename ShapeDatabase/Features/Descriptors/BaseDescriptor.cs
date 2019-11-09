using System;

namespace ShapeDatabase.Features.Descriptors {

	/// <summary>
	/// A simpel implementation to unify all the methods which are similar for
	/// descriptors such as the name.
	/// </summary>
	public abstract class BaseDescriptor<T> : IDescriptor<T>
		where T : class, IDescriptor<T> {

		#region --- Properties ---

		/// <summary>
		/// The name of the descriptor.
		/// </summary>
		public string Name { get; }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new descriptor with common values such as the name.
		/// </summary>
		/// <param name="name">The unique name for each descriptor.</param>
		/// <exception cref="ArgumentNullException">If the name is <see langword="null"/>.
		/// </exception>
		public BaseDescriptor(string name) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			Name = name;
		}

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Compares the descriptor with another descriptor.
		/// </summary>
		/// <param name="desc">The other descriptor.</param>
		/// <returns>Value between 0(no resemblance) to 1(equal).</returns>
		public double Compare(IDescriptor desc) => Compare(desc as T);
		public abstract double Compare(T desc);

		/// <summary>
		/// Serializes the descriptor.
		/// </summary>
		/// <returns>The serialization string.</returns>
		public abstract string Serialize();

		#endregion

	}
}

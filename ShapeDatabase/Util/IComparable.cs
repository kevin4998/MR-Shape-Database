namespace ShapeDatabase.Util {

	/// <summary>
	/// An interface to allow objects to be compared with each other and
	/// return a decimal or percentual representation of the similarity.
	/// </summary>
	/// <typeparam name="T">The type of object which can compare with each other.
	/// </typeparam>
	public interface IComparable<T> where T : IComparable<T> {

		/// <summary>
		/// Method for comparison with another object of the same type.
		/// This will evaluate the similarity between two objects,
		/// where a value of 1 represents 100% similarity, or it being the same one,
		/// and a similarity of 0 meaning that they are nothing alike.
		/// </summary>
		/// <param name="alternative">The object to compare against for similarity.
		/// </param>
		/// <returns>A double which represent the similarity between the objects.
		/// 1 means that two values are the same and 0 means that they are nothing alike.
		/// The returned value will always be within the range of [0,1].</returns>
		double Compare(T alternative);

	}
}

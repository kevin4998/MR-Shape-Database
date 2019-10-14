
namespace ShapeDatabase.Util {

	/// <summary>
	/// An interface identify objects which can be serialised to a string.
	/// All classes which implement this interface must also implement
	/// a static method called Deserialize which given a string can construct
	/// it back to that specific object.
	/// </summary>
	public interface ISerialisable {

		/// <summary>
		/// Converts all the properties to a string representation which
		/// can later be deserialised again into the same object.
		/// </summary>
		/// <returns>A string which contains all the information from this object
		/// to restore it to its original shape again.</returns>
		string Serialize();

	}
}

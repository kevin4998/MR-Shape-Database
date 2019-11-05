namespace ShapeDatabase.UI.Console.Verbs {

	/// <summary>
	/// The modus to specify what should be done with the input shapes.
	/// </summary>
	public enum QueryInputMode {
		
		/// <summary>
		/// Refine the shapes which is specified in the query directory.
		/// </summary>
		Refine,
		/// <summary>
		/// Directly include the shapes in the specified query directory,
		/// assuming that they are refined and normalised.
		/// </summary>
		Direct,
		/// <summary>
		/// Do not use shapes from the query directory but ones already known
		/// by the application.
		/// </summary>
		Internal

	}

	/// <summary>
	/// The modus to specify how large a single query operation should be.
	/// </summary>
	public enum QuerySizeMode {

		/// <summary>
		/// The query operation size should be a number called K.
		/// </summary>
		KBest,
		/// <summary>
		/// The query operation size should be the size of the class.
		/// </summary>
		Class,
		/// <summary>
		/// The query operation size should retrieve all the shapes in the database.
		/// </summary>
		All

	}

}

namespace ShapeDatabase.UI.Console.Verbs {

	/// <summary>
	/// The modus to specify what should be done with the result statistics.
	/// </summary>
	public enum EvaluationMode {

		/// <summary>
		/// Provide the individual shape results.
		/// </summary>
		Individual,
		/// <summary>
		/// Combine the results based on a shape their class.
		/// </summary>
		Aggregated

	}
}

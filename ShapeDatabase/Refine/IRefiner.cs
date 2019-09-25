using System;
using System.IO;
using ShapeDatabase.Shapes;

namespace ShapeDatabase.Refine {

	/// <summary>
	/// Interface for mesh refinement.
	/// </summary>
	public interface IRefiner<T> {

		/// <summary>
		/// Checks whether mesh needs refinment (true) or not (false).
		/// </summary>
		bool RequireRefinement(UnstructuredMesh mesh);

		/// <summary>
		/// Refines the mesh. Overwrites the .off file.
		/// </summary>
		void RefineMesh(FileInfo file);

	}
}

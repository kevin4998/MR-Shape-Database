using System;
using System.Collections.Generic;
using System.IO;
using ShapeDatabase.Shapes;
using ShapeDatabase.Util;

namespace ShapeDatabase.Refine {

	/// <summary>
	/// The class responsible with refining all the meshes
	/// </summary>
	public class RefineManager {

		#region --- Properties ---

		private readonly IList<IRefiner<IMesh>> refiners = new List<IRefiner<IMesh>>();

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Creates an object which will handle shape refinement.
		/// </summary>
		public RefineManager() {
			PopulateRefiners(refiners);
		}

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Provides another refiner which can normalise meshes for easier feature extraction.
		/// This will have effect on the next provided directories.
		/// It will not try to recover the extra files from previous directories.
		/// </summary>
		/// <param name="refiners">The refiner which can normalise a shape in any way.</param>
		public void AddRefiners(params IRefiner<IMesh>[] refiners) {
			foreach (IRefiner<IMesh> refine in refiners)
				AddRefiner(refine);
		}

		/// <summary>
		/// Provides another refiner which can normalise meshes for easier feature extraction.
		/// This will have effect on the next provided directories.
		/// It will not try to recover the extra files from previous directories.
		/// </summary>
		/// <param name="refiner">The refiner which can normalise a shape in any way.</param>
		public void AddRefiner(IRefiner<IMesh> refiner) {
			if (refiner != null && !refiners.Contains(refiner))
				refiners.Add(refiner);
		}


		/// <summary>
		/// Removes the specified refiners from the refine manager.
		/// </summary>
		/// <param name="refiners">The refiners that should no longer be present.</param>
		/// <returns><see langword="true"/> if all the refiners were removed from the
		/// collection. If there is a single refiner which could not be removed, because
		/// it was either not present or because the collection is read-only,
		/// then this will return <see langword="false"/>.</returns>
		public bool RemoveRefiners(params IRefiner<IMesh>[] refiners) {
			bool result = true;
			foreach (IRefiner<IMesh> refine in refiners)
				if (refine != null)
					result &= RemoveRefiner(refine);
			return result;
		}

		/// <summary>
		/// Removes the specified refiner from the refine manager.
		/// </summary>
		/// <param name="refiner">The refiner that should no longer be present.</param>
		/// <returns><see langword="true"/> if the refiner was successfully removed
		/// from the current refiners.</returns>
		public bool RemoveRefiner(IRefiner<IMesh> refiner) {
			return refiners.Remove(refiner);
		}


		/// <summary>
		/// Refines the given mesh from the specified file if it does not follow
		/// the refiners specifications. This will perform a single refinement iteration.
		/// If it is not refined after one call then multiple calls to this method
		/// should be made.
		/// </para>
		/// </summary>
		/// <param name="info">The file where the mesh was loaded from.</param>
		/// <returns><see langword="true"/> if this file was already refined
		/// from the start and needs no further improvements.</returns>
		/// <exception cref="ArgumentNullException">If the given file info
		/// does not exist or is <see langword="null"/>.</exception>
		public bool RefineFile(FileInfo info) {
			if (info == null || !info.Exists)
				throw new ArgumentNullException(nameof(info));
			if (Settings.FileManager.TryRead(info.FullName, out IMesh mesh))
				return RefineFile(info, mesh);
			return false;
		}

		/// <summary>
		/// Refines the given mesh from the specified file if it does not follow
		/// the refiners specifications. This will perform a single refinement iteration.
		/// If it is not refined after one call then multiple calls to this method
		/// should be made.
		/// </summary>
		/// <param name="info">The file where the mesh was loaded from.</param>
		/// <param name="mesh">The mesh which needs to be refined or checked.</param>
		/// <returns><see langword="true"/> if this file was already refined
		/// from the start and needs no further improvements.</returns>
		/// <exception cref="ArgumentNullException">If the given file info
		/// does not exist or is <see langword="null"/>.</exception>
		public bool RefineFile(FileInfo info, IMesh mesh) {
			if (info == null || !info.Exists)
				throw new ArgumentNullException(nameof(info));

			bool isRefined = true;  // Holds if no refinement has been made.
			foreach (IRefiner<IMesh> refiner in refiners) {
				if (refiner.RequireRefinement(mesh)) {
					refiner.RefineMesh(mesh, info);
					isRefined = false;
					break;
				}
			}
			return isRefined;
		}

		#endregion

		#region --- Static Methods ---

		/// <summary>
		/// Initialises all the refiners in the current repository.
		/// </summary>
		/// <param name="refiners">The list which will be filled with new
		/// refiners from the current repository.</param>
		private static void PopulateRefiners(IList<IRefiner<IMesh>> refiners) {
			refiners.Add(ExtendRefiner.Instance);
			refiners.Add(SimplifyRefiner.Instance);
			refiners.Add(NormalisationRefiner.Instance);
		}

		#endregion

	}

}

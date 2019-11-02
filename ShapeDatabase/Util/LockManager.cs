using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ShapeDatabase.Util {

	/// <summary>
	/// A class for managing multiple locks on arrays or dictionary objects
	/// in a safe way.
	/// </summary>
	public class LockManager {

		#region --- Properties ---

		/// <summary>
		/// A dictionary containing all the named locks to access named variables.
		/// </summary>
		protected IDictionary<string, object> Locks { get; }
			= new ConcurrentDictionary<string, object>();

		/// <summary>
		/// Safely retrieves a lock which can be used to access values from
		/// the given property.
		/// </summary>
		/// <param name="lockName">The name of the property on which you will perform
		/// operations.</param>
		/// <returns>The lock which can be used to safely modify the given property.
		/// </returns>
		public object this[string lockName] {
			get => GetLock(lockName);
			set {
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				lock (value) {
					FreeLock(lockName);
				}
			}
		}

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Instaniates a new empty lock manager.
		/// Everytime a call is made to rtrieve a lock, this lock will be added
		/// to the manager so that future access calls make use of the same lock.
		/// </summary>
		public LockManager() { }

		#endregion

		#region --- Instance Methods ---

		/// <summary>
		/// Safely retrieves a lock which can be used to access values from
		/// the given property.
		/// </summary>
		/// <param name="lockName">The name of the property on which you will perform
		/// operations.</param>
		/// <returns>The lock which can be used to safely modify the given property.
		/// </returns>
		public object GetLock(string lockName) {
			if (lockName == null)
				throw new ArgumentNullException(nameof(lockName));

			if (!Locks.TryGetValue(lockName, out object _lock))
				lock (Locks) {
					if (!Locks.TryGetValue(lockName, out _lock)) {
						_lock = new object();
						Locks.Add(lockName, _lock);
					}
				}
			return _lock;
		}

		/// <summary>
		/// Frees up locks from properties which are not stored in the current manager.
		/// This operation should be performed inside the lock of the same object.
		/// </summary>
		/// <param name="lockName">The name of the property which didn't exist.</param>
		public void FreeLock(string lockName) {
			if (lockName == null)
				throw new ArgumentNullException(nameof(lockName));

			lock (Locks) {
				Locks.Remove(lockName);
			}
		}

		#endregion

	}
}

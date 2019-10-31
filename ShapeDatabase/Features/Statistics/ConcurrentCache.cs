using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// An implementation of <see cref="ICache"/> inside a concurrent environment.
	/// All operations on this object are safe in a multi-threaded environment.
	/// </summary>
	public class ConcurrentCache<T> : Cache<T> {

		#region --- Properties ---

		private long version = 0;

		protected override IDictionary<string, Func<T, ICache<T>, object>> CacheProvider { get; }
			= new ConcurrentDictionary<string, Func<T, ICache<T>, object>>();
		protected override IDictionary<string, object> CacheValues { get; }
			= new ConcurrentDictionary<string, object>();
		/// <summary>
		/// A <see cref="IDictionary{TKey, TValue}"/> containing the locks for each
		/// property to ensure that it works in a multi-threaded environment.
		/// </summary>
		protected virtual IDictionary<string, object> CacheLocks { get; }
			= new ConcurrentDictionary<string, object>();

		public override int Count => CacheLocks.Count;
		public virtual long Version => version;


		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new cache with no stored values or provider methods.
		/// </summary>
		public ConcurrentCache() { }
		/// <summary>
		/// Initialises a new cache with the given saved methods.
		/// </summary>
		/// <param name="values">The collection of values to add to the cache.</param>
		public ConcurrentCache(params (string, object)[] values)
			: base(values) { }
		/// <summary>
		/// Initialises a new cache with the given functions to calculate values.
		/// </summary>
		/// <param name="providers">The collection of methods to add to the cache.</param>
		public ConcurrentCache(params (string, Func<object>)[] providers)
			: base(providers) { }
		/// <summary>
		/// Initialises a new cache with the given functions to calculate values.
		/// The provided functions may use the cache for help.
		/// </summary>
		/// <param name="providers">The collection of methods to add to the cache.</param>
		public ConcurrentCache(params (string, Func<T, ICache<T>, object>)[] providers)
			: base(providers) { }

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
		protected object GetLock(string lockName) {
			if (lockName == null)
				throw new ArgumentNullException(nameof(lockName));

			if (!CacheLocks.TryGetValue(lockName, out object _lock))
				lock (CacheLocks) {
					if (!CacheLocks.TryGetValue(lockName, out _lock)) { 
						_lock = new object();
						CacheLocks.Add(lockName, _lock);
					}
				}
			return _lock;
		}

		/// <summary>
		/// Frees up locks from properties which are not stored in the current cache.
		/// This operation should be performed inside the lock of the same object.
		/// </summary>
		/// <param name="lockName">The name of the property which didn't exist.</param>
		protected void FreeLock(string lockName) {
			if (lockName == null)
				throw new ArgumentNullException(nameof(lockName));

			lock (CacheLocks) {
				CacheLocks.Remove(lockName);
			}
		}


		public override ICache<T> AddLazyValue(string name, Func<T, ICache<T>, object> provider) {
			lock (GetLock(name)) { 
				base.AddLazyValue(name, provider);
				Interlocked.Increment(ref version);
				return this;
			}
		}

		public override ICache<T> AddValue(string name, object value) {
			lock (GetLock(name)) {
				base.AddValue(name, value);
				Interlocked.Increment(ref version);
				return this;
			}
		}

		public override bool TryGetValue(string name, T helper, out object value) {
			lock (GetLock(name)) {
				bool result = base.TryGetValue(name, helper, out value);
				if (!result)
					FreeLock(name);
				return result;
			}
		}


		public override void Clear() {
			lock (CacheLocks) { 
				base.Clear();
			}
		}

		#endregion

	}
}

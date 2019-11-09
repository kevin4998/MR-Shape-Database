using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShapeDatabase.Util;

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
		/// An object to handle multiple locks on a single object, this is done
		/// by dividing it into multiple individual parts, such as the name of
		/// a cache value, ensuring that only 1 object modified this position in
		/// the dictionary at the same time.
		/// </summary>
		protected virtual LockManager CacheLocks { get; }
			= new LockManager();

		public override int Count => CacheLocks.Count;

		/// <summary>
		/// Current version of the cache.
		/// </summary>
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

		public override ICache<T> AddLazyValue(string name, Func<T, ICache<T>, object> provider) {
			lock (CacheLocks[name]) {
				base.AddLazyValue(name, provider);
				Interlocked.Increment(ref version);
				return this;
			}
		}

		public override ICache<T> AddValue(string name, object value) {
			lock (CacheLocks[name]) {
				base.AddValue(name, value);
				Interlocked.Increment(ref version);
				return this;
			}
		}

		public override bool TryGetValue(string name, T helper, out object value) {
			lock (CacheLocks[name]) {
				bool result = base.TryGetValue(name, helper, out value);
				if (!result)
					CacheLocks.FreeLock(name);
				return result;
			}
		}

		public override void Clear() {
			lock (CacheLocks) {
				base.Clear();
				Interlocked.Increment(ref version);
			}
		}

		#endregion

	}
}

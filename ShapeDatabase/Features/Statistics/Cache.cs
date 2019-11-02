using System;
using System.Collections.Generic;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// An implementation of <see cref="ICache"/> which makes use of 2 different
	/// dictionaries which contain all the values and all the formulas.
	/// The current implementation is single threaded.
	/// </summary>
	public class Cache<T> : ICache<T> {

		#region --- Properties ---

		public virtual int Count {
			get {
				ISet<string> names = new HashSet<string>(CacheValues.Keys);
				names.UnionWith(CacheProvider.Keys);
				return names.Count;
			}
		}

		/// <summary>
		/// A <see cref="IDictionary{TKey, TValue}"/> containing the functions to
		/// calculate the properties with the specified name. The key here resembles
		/// the property name and the value is the formula to find it.
		/// </summary>
		protected virtual IDictionary<string, Func<T, ICache<T>, object>> CacheProvider { get; }
			= new Dictionary<string, Func<T, ICache<T>, object>>();
		/// <summary>
		/// A <see cref="IDictionary{TKey, TValue}"/> containing the different values
		/// of properties stored in the cache.
		/// </summary>
		protected virtual IDictionary<string, object> CacheValues { get; }
			= new Dictionary<string, object>();

		public virtual object this[string name] {
			get => GetValue(name, default);
			set => AddValue(name, value);
		}

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new cache with no stored values or provider methods.
		/// </summary>
		public Cache() { }

		/// <summary>
		/// Initialises a new cache with the given saved methods.
		/// </summary>
		/// <param name="values">The collection of values to add to the cache.</param>
		public Cache(params (string, object)[] values) {
			foreach ((string name, object value) in values)
				if (!string.IsNullOrEmpty(name))
					CacheValues.Add(name, value);
		}

		/// <summary>
		/// Initialises a new cache with the given functions to calculate values.
		/// </summary>
		/// <param name="providers">The collection of methods to add to the cache.</param>
		public Cache(params (string, Func<object>)[] providers) {
			foreach ((string name, Func<object> provider) in providers)
				if (!string.IsNullOrEmpty(name) && provider != null)
					CacheProvider.Add(name, (_, __) => provider());
		}

		/// <summary>
		/// Initialises a new cache with the given functions to calculate values.
		/// The provided functions may use the cache for help.
		/// </summary>
		/// <param name="providers">The collection of methods to add to the cache.</param>
		public Cache(params (string, Func<T, ICache<T>, object>)[] providers) {
			foreach ((string name, Func<T, ICache<T>, object> provider) in providers)
				if (!string.IsNullOrEmpty(name) && provider != null)
					CacheProvider.Add(name, provider);
		}

		#endregion

		#region --- Instance Methods ---

		public virtual ICache<T> AddLazyValue(string name, Func<T, ICache<T>, object> provider) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

			CacheProvider.Add(name, provider);
			return this;
		}

		public virtual ICache<T> AddValue(string name, object value) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			CacheValues.Add(name, value);
			return this;
		}


		public virtual object GetValue(string name, T helper) {
			TryGetValue(name, helper, out object value);
			return value;
		}

		public virtual bool TryGetValue(string name, T helper, out object value) {
			if (CacheValues.TryGetValue(name, out value))
				return true;
			if (!CacheProvider.TryGetValue(name, out Func<T, ICache<T>, object> provider))
				return false;

			value = provider(helper, this);
			AddValue(name, value);
			return true;
		}


		public virtual void Clear() {
			CacheValues.Clear();
			//CacheProvider.Clear();
		}

		#endregion

	}
}

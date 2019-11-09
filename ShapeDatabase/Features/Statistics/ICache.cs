using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeDatabase.Properties;

namespace ShapeDatabase.Features.Statistics {

	/// <summary>
	/// An interface to describe an object which can cache specific values for
	/// later operations. The values in the cache can be lazily computed.
	/// </summary>
	public interface ICache<T> {

		/// <summary>
		/// The total number of properties which are saved in the cache.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Provides the value which is saved in the cache with the given name
		/// or <see langword="null"/> if there is no such value.
		/// It is possible that a value will be calculated for the first time when
		/// calling this method, giving slower performance.
		/// </summary>
		/// <param name="name">The name of the property to retrieve.</param>
		/// <returns>The object which is saved (or newly generated) in the cache,
		/// otherwise <see langword="null"/> if there is no such property.</returns>
		/// <exception cref="ArgumentNullException">If the given name is <see langword="null"/>.</exception>
		object this[string name] { get; set; }

		/// <summary>
		/// Puts the given value in the cache overwriting the previous result.
		/// </summary>
		/// <param name="name">The name of the property to be stored in the cache.</param>
		/// <param name="value">The value of the object to be stored with this property.</param>
		/// <returns>The same cache for streaming purposes.</returns>
		/// <exception cref="ArgumentNullException">If the given name or value are <see langword="null"/>.</exception>
		ICache<T> AddValue(string name, object value);

		/// <summary>
		/// Specifies a formula to calculate the given property if it does not exist.
		/// </summary>
		/// <param name="name">The name of the property to be stored in the cache.</param>
		/// <param name="provider">The formula to calculate the object to be stored
		/// with this property.</param>
		/// <returns>The same cache for streaming purposes.</returns>
		/// <exception cref="ArgumentNullException">If the given name or provider
		/// are <see langword="null"/>.</exception>
		ICache<T> AddLazyValue(string name, Func<T, ICache<T>, object> provider);

		/// <summary>
		/// Provides the value which is saved in the cache with the given name
		/// or <see langword="null"/> if there is no such values.
		/// It is possible that a value will be calculated for the first time when
		/// calling this method, giving slower performance.
		/// </summary>
		/// <param name="name">The name of the property to retrieve.</param>
		/// <returns>The object which is saved (or newly generated) in the cache,
		/// otherwise <see langword="null"/> if there is no such property.</returns>
		/// <exception cref="ArgumentNullException">If the given name is
		/// <see langword="null"/>.</exception>
		object GetValue(string name, T helper);

		/// <summary>
		/// Attempts to retrieve the value linked to the provided name.
		/// It is possible that a value will be calculated for the first time when
		/// calling this method, giving slower performance.
		/// </summary>
		/// <param name="name">The name of the property to retrieve.</param>
		/// <param name="value">The object which is saved (or newly generated) in the cache,
		/// otherwise the default value if there is no such property.</param>
		/// <returns>If the value was retrieved from the cache.</returns>
		bool TryGetValue(string name, T helper, out object value);

		/// <summary>
		/// Resets the current cache to its initial position as if it was just created.
		/// </summary>
		void Clear();

	}

	/// <summary>
	/// A class containing extension and helper methods for caches.
	/// </summary>
	public static class CacheEx {

		/// <summary>
		/// Puts the given values in the cache overwriting the previous result.
		/// </summary>
		/// <param name="cache">The cache to put the values in.</param>
		/// <param name="values">A collection of different values for the cache.</param>
		/// <returns>The same cache for streaming purposes.</returns>
		/// <exception cref="ArgumentNullException">If the given cache is <see langword="null"/>.</exception>
		public static ICache<T> AddValue<T>(this ICache<T> cache,
									  params (string, object)[] values) {
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			foreach ((string name, object value) in values)
				if (!string.IsNullOrEmpty(name) && value != null)
					cache.AddValue(name, value);

			return cache;
		}

		/// <summary>
		/// Puts the given values in the cache overwriting the previous result.
		/// </summary>
		/// <param name="cache">The cache to put the values in.</param>
		/// <param name="providers">A collection of providers to find the values
		/// for certain properties.</param>
		/// <returns>The same cache for streaming purposes.</returns>
		/// <exception cref="ArgumentNullException">If the given cache is <see langword="null"/>.</exception>
		public static ICache<T> AddLazyValue<T>(this ICache<T> cache,
			params (string, Func<T, object>)[] providers) {
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			foreach ((string name, Func<T, object> provider) in providers)
				if (!string.IsNullOrEmpty(name) && provider != null)
					cache.AddLazyValue(name, (value, _) => provider(value));

			return cache;
		}
		/// <summary>
		/// Puts the given values in the cache overwriting the previous result.
		/// </summary>
		/// <param name="cache">The cache to put the values in.</param>
		/// <param name="providers">A collection of providers to find the values
		/// for certain properties.</param>
		/// <returns>The same cache for streaming purposes.</returns>
		/// <exception cref="ArgumentNullException">If the given cache is <see langword="null"/>.</exception>
		public static ICache<T> AddLazyValue<T>(this ICache<T> cache,
			params (string, Func<T, ICache<T>, object>)[] providers) {
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			foreach ((string name, Func<T, ICache<T>, object> provider) in providers)
				if (!string.IsNullOrEmpty(name) && provider != null)
					cache.AddLazyValue(name, provider);

			return cache;
		}

		/// <summary>
		/// Puts the given providers of values in the cache overwriting the previous
		/// results.
		/// </summary>
		/// <param name="cache">The cache to put the values in.</param>
		/// <param name="providers">A collection of providers to find the values
		/// for certain properties.</param>
		/// <returns>The same cache for streaming purposes.</returns>
		/// <exception cref="ArgumentNullException">If the given cache is <see langword="null"/>.</exception>
		public static ICache<T> AddLazyValue<T>(this ICache<T> cache,
			params (string, Func<ICache<T>, object>)[] providers) {
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			foreach ((string name, Func<ICache<T>, object> provider) in providers)
				if (!string.IsNullOrEmpty(name) && provider != null)
					cache.AddLazyValue(name, (_, c) => provider(c));

			return cache;
		}
		/// <summary>
		/// Puts the given providers of values in the cache overwriting the previous results.
		/// </summary>
		/// <param name="cache">The cache to put the values in.</param>
		/// <param name="providers">A collection of providers to find the values
		/// for certain properties.</param>
		/// <returns>The same cache for streaming purposes.</returns>
		/// <exception cref="ArgumentNullException">If the given cache is <see langword="null"/>.</exception>
		public static ICache<T> AddLazyValue<T>(this ICache<T> cache,
			params (string, Func<object>)[] providers) {
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			foreach ((string name, Func<object> provider) in providers)
				if (!string.IsNullOrEmpty(name) && provider != null)
					cache.AddLazyValue(name, provider);

			return cache;
		}
		/// <summary>
		/// Specifies a formula to calculate the given property if it does not exist.
		/// </summary>
		/// <param name="cache">The cache to retrieve the values from.</param>
		/// <param name="name">The name of the property to be stored in the cache.</param>
		/// <param name="provider">The formula to calculate the object to be stored
		/// with this property.</param>
		/// <returns>The same cache for streaming purposes.</returns>
		/// <exception cref="ArgumentNullException">If any of the given properties 
		/// is <see langword="null"/>.</exception>
		public static ICache<T> AddLazyValue<T>(this ICache<T> cache,
										  string name, Func<object> provider) {
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

			return cache.AddLazyValue(name, (_, __) => provider());
		}

		/// <summary>
		/// Gets a cached value by name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cache">The cache from which the value should be retrieved.</param>
		/// <param name="name">The name of the cache value.</param>
		/// <returns>The value.</returns>
		public static object GetValue<T>(this ICache<T> cache, string name) {
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			return cache.GetValue(name, default);
		}

		public static TV GetValue<TV, TC>(this ICache<TC> cache, string name, TC helper) {
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			object value = cache.GetValue(name, helper);
			if (value is TV)
				return (TV) value;
			else
				throw CastException<TV>(value);
		}

		public static TV GetValue<TV, TC>(this ICache<TC> cache, string name) {
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			return cache.GetValue<TV, TC>(name, default);
		}

		public static bool TryGetValue<T>(this ICache<T> cache,
										  string name, out object value) {
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			return cache.TryGetValue(name, default, out value);
		}

		public static bool TryGetValue<TV, TC>(this ICache<TC> cache, string name, out TV value) {
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			value = default;
			if (cache.TryGetValue(name, out object obj))
				if (obj is TV) {
					value = (TV) obj;
					return true;
				}
			return false;
		}


		/// <summary>
		/// Throws an error if it is not possible to cast the given object
		/// to the provided type.
		/// </summary>
		/// <typeparam name="T">The type to which the object should've been cast.
		/// </typeparam>
		/// <param name="value">The actual value of the object in this cast.</param>
		/// <returns>The generated exception.</returns>
		private static InvalidCastException CastException<T>(object value) {
			return new InvalidCastException(
				string.Format(Settings.Culture,
					Resources.EX_Cast,
					typeof(T).Name,
					value == null ? "NULL" : value.GetType().Name)
			);
		}

	}

}

using System;
using System.Runtime.Serialization;

namespace ShapeDatabase.Util {

	/// <summary>
	/// A class to provide more generic methods for the Serializer from C#.
	/// </summary>
	public static class SerializationInfoEx {

		/// <summary>
		/// Retrieves a value from the <see cref="SerializationInfo"/> storage with
		/// the given type.
		/// </summary>
		/// <typeparam name="T">The type which this object should be.</typeparam>
		/// <param name="info">The info holder which should give the value.</param>
		/// <param name="name">The name of the item which is stored.</param>
		/// <returns>The result of the found object as the given type.</returns>
		/// <exception cref="ArgumentNullException">When the provided parameters
		/// are null.</exception>
		/// <exception cref="InvalidCastException">When the type stored at the given
		/// location is not of the form of <typeparamref name="T"/></exception>
		/// <exception cref="SerializationException">Internal error in the serializer.
		/// check <see cref="SerializationInfo.AddValue(string, object, Type)"/>
		/// for more information about this exception.</exception>
		public static T GetValue<T>(this SerializationInfo info, string name) {
			if (info == null)
				throw new ArgumentNullException(nameof(info));
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			return (T) info.GetValue(name, typeof(T));
		}

		/// <summary>
		/// Tries to retrieve a value from the <see cref="SerializationInfo"/> storage with the given type. If it could not retrieve it then it gives the default value.
		/// </summary>
		/// <typeparam name="T">The type which this object should be.</typeparam>
		/// <param name="info">The info holder which should give the value.</param>
		/// <param name="name">The name of the item which is stored.</param>
		/// <param name="value">The parameter which will contain the return value or the default value.</param>
		/// <returns>Specified if the retrieval succeeded. If this is false then the default value is being used.</returns>
		/// <exception cref="ArgumentNullException">When the provided parameters
		/// are null.</exception>
		/// <exception cref="InvalidCastException">When the type stored at the given
		/// location is not of the form of <typeparamref name="T"/></exception>
		public static bool TryGetValue<T>(this SerializationInfo info, string name, out T value) {
			if (info == null)
				throw new ArgumentNullException(nameof(info));
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			try {
				value = (T) info.GetValue(name, typeof(T));
				return true;
			} catch (SerializationException) {
				value = default;
				return false;
			}
		}

		/// <summary>
		/// Retrieves a value from the <see cref="SerializationInfo"/> storage with
		/// the given type. If it could not retrieve it then it gives the default value.
		/// </summary>
		/// <typeparam name="T">The type which this object should be.</typeparam>
		/// <param name="info">The info holder which should give the value.</param>
		/// <param name="name">The name of the item which is stored.</param>
		/// <param name="def">The value which should be used when it is not present.</param>
		/// <returns>The value from the <see cref="SerializationInfo"/> object or the specified default value if it was not present.</returns>
		/// <exception cref="ArgumentNullException">When the provided parameters
		/// are null.</exception>
		/// <exception cref="InvalidCastException">When the type stored at the given
		/// location is not of the form of <typeparamref name="T"/></exception>
		public static T GetValueOrDefault<T>(this SerializationInfo info, string name, Lazy<T> def) {
			if (info == null)
				throw new ArgumentNullException(nameof(info));
			if (def == null)
				throw new ArgumentNullException(nameof(def));

			if (!info.TryGetValue(name, out T value))
				value = def.Value;
			return value;
		}

		/// <summary>
		/// Retrieves a value from the <see cref="SerializationInfo"/> storage with
		/// the given type. If it could not retrieve it then it gives the default value.
		/// </summary>
		/// <typeparam name="T">The type which this object should be.</typeparam>
		/// <param name="info">The info holder which should give the value.</param>
		/// <param name="name">The name of the item which is stored.</param>
		/// <param name="def">The value which should be used when it is not present.</param>
		/// <returns>The value from the <see cref="SerializationInfo"/> object or the specified default value if it was not present.</returns>
		/// <exception cref="ArgumentNullException">When the provided parameters
		/// are null.</exception>
		/// <exception cref="InvalidCastException">When the type stored at the given
		/// location is not of the form of <typeparamref name="T"/></exception>
		public static T GetValueOrDefault<T>(this SerializationInfo info, string name, T def) {
			return GetValueOrDefault(info, name, new Lazy<T>(() => def));
		}

	}
}

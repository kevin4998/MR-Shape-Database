using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ShapeDatabase.Features.Descriptors {

	/// <summary>
	/// A simpel implementation to unify all the methods which are similar for
	/// descriptors such as the name.
	/// </summary>
	public abstract class BaseDescriptor<T> : IDescriptor<T>
		where T : class, IDescriptor<T> {

		#region --- Properties ---

		protected const char NameSeperator = ';';

		public string Name { get; }

		#endregion

		#region --- Constructor Methods ---

		/// <summary>
		/// Initialises a new descriptor with common values such as the name.
		/// </summary>
		/// <param name="name">The unique name for each descriptor.</param>
		/// <exception cref="ArgumentNullException">If the name is <see langword="null"/>.
		/// </exception>
		public BaseDescriptor(string name) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			Name = name;
		}

		#endregion

		#region --- Methods ---

		#region -- Instance Methods --

		public double Compare(IDescriptor desc) => Compare(desc as T);
		public abstract double Compare(T desc);

		public string Serialise() {
			StringBuilder builder = new StringBuilder()
				.Append(GetType())
				.Append(NameSeperator)
				.Append(Name)
				.Append(NameSeperator);

			return SubSerialise(builder).ToString();
		}
		protected abstract StringBuilder SubSerialise(StringBuilder builder);

		#endregion

		#region -- Static Methods --

		private const string EX_INVALID_TYPE =
			"Could not reconstruct the type from the serialised string.";
		private const string EX_INVALID_IMPL =
			"The class '{0}' does not correctly implement ISerialisable.\n" +
			"It is missing the method 'Deserialise(string)'.";
		private const string EX_INCORRECT_IMPL =
			"The class '{0}' does not correctly implement ISerialisable.\n" +
			"It is invalid parameters and must only use: 'Deserialise(string)'.";
		private const string DeserialiseName = "Deserialise";
		private const BindingFlags flags =
			BindingFlags.Public |
			BindingFlags.Static |
			BindingFlags.IgnoreCase |
			BindingFlags.FlattenHierarchy;

		public static IDescriptor Deserialise(string serialised) {
			if (string.IsNullOrEmpty(serialised))
				return null;

			// Find the class which should deserialise this type.
			string[] split = serialised.Split(NameSeperator);
			Type clazz = Type.GetType(split[0]);
			if (clazz == null)
				throw new ArgumentException(EX_INVALID_TYPE);

			// Find the deserialisation method in the class.
			MethodInfo method = clazz.GetMethod(DeserialiseName,
												new Type[]{ typeof(string) });
			if (method == null)
				throw new ArgumentException(EX_INVALID_IMPL);

			// Collect the remaining string to deserialise.
			string remainings = string.Join(NameSeperator.ToString(Settings.Culture),
											split, 1, split.Length - 1);

			try { 
				// Call the method and return the descriptor return type.
				object result = method.Invoke(null, new object[] { remainings });
				return result as IDescriptor;

				// The elements of the parametersarray do not match the signature
				// of the method or constructor reflected by this instance.
			} catch (ArgumentException ex) {
				throw new ArgumentException(EX_INCORRECT_IMPL, ex);

				// The invoked method or constructor throws an exception.
			} catch (TargetInvocationException ex) {
				throw new ArgumentException(ex.Message, ex);

				// The parameters array does not have the correct number of arguments.
			} catch (TargetParameterCountException ex) {
				throw new ArgumentException(EX_INCORRECT_IMPL, ex);

				// The caller does not have permission to execute the method
				// or constructor that is represented by the current instance.
			} catch (MethodAccessException ex) {
				throw new ArgumentException(ex.Message, ex);

				// The current instance is a System.Reflection.Emit.MethodBuilder.
			} catch (NotSupportedException ex) {
				throw new ArgumentException(EX_INCORRECT_IMPL, ex);
			}

		}

		#endregion

		#endregion

	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using OpenTK.Input;

namespace ShapeDatabase.UI {

	/// <summmary>
	/// An object with the purpose of managing Keyboard interaction.
	/// This object will notify listeners when a specific key is pressed
	/// so that its actions can be performed.
	/// </summary>
	[DebuggerDisplay("EventCount [Down:{DownKeybinds.Count}, Up:{UpKeybinds.Count}, Hold:{HoldKeybinds.Count}]")]
	public class KeyController {

		/// <summary>
		/// The action to perform when a specific button has been pressed.
		/// </summary>
		/// <param name="input">The keyboard in its current states.
		/// This can be used for checking Shifts or Controls.</param>
		/// <param name="e">The event args for the current frame.
		/// This can be used for checking timings.</param>
		public delegate void KeyBindAction(KeyboardState input, FrameEventArgs e);

		private IDictionary<Key, ICollection<KeyBindAction>> DownKeybinds { get; }
		private IDictionary<Key, ICollection<KeyBindAction>> UpKeybinds { get; }
		private IDictionary<Key, ICollection<KeyBindAction>> HoldKeybinds { get; }

		private readonly HashSet<Key> activeKeys;
		private readonly HashSet<Key> onKeys;


		/// <summary>
		/// Initialises a new Controller to manage keypresses and events.
		/// </summary>
		public KeyController() {
			DownKeybinds = new Dictionary<Key, ICollection<KeyBindAction>>();
			UpKeybinds = new Dictionary<Key, ICollection<KeyBindAction>>();
			HoldKeybinds = new Dictionary<Key, ICollection<KeyBindAction>>();

			activeKeys = new HashSet<Key>();
			onKeys = new HashSet<Key>();
		}


		/// <summary>
		/// The method to call when a change in Keyboard state has happened.
		/// This state difference can be pressing a key or releasing one or many.
		/// </summary>
		/// <param name="input">The changed ButtonState of the board after pressing.</param>
		/// <exception cref="ArgumentNullException">If the provided parameter is null.</exception>
		public void OnKeyPress(KeyboardState input, FrameEventArgs e) {
			if (input == null)
				throw new ArgumentNullException(nameof(input));
			if (!input.IsAnyKeyDown)
				return;

			// Check all the keys which have been on to see if they have turned off.
			foreach (Key key in activeKeys)
				if (input.IsKeyUp(key))
					if (UpKeybinds.TryGetValue(key, out ICollection<KeyBindAction> actions))
						foreach (KeyBindAction action in actions)
							action(input, e);

			// Check all the keys which have listeners about being pressed or held down.
			foreach (Key key in onKeys)
				if (input.IsKeyDown(key)) {
					if (activeKeys.Contains(key)) { // Continues press
						if (HoldKeybinds.TryGetValue(key, out ICollection<KeyBindAction> actions))
							foreach (KeyBindAction action in actions)
								action(input, e);
					} else {
						activeKeys.Add(key);        // First time press
						if (DownKeybinds.TryGetValue(key, out ICollection<KeyBindAction> actions))
							foreach (KeyBindAction action in actions)
								action(input, e);
					}
				}

			// Remove the keys that are no longer active.
			activeKeys.RemoveWhere(key => input.IsKeyUp(key));
		}


		/// <summary>
		/// Registers that if the specified button is pressed than the given action
		/// should be performed.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed after that
		/// specific button has been pressed. </param>
		public void RegisterDown(Key button, KeyBindAction action) {
			DicAdd(DownKeybinds, button, action);
			onKeys.Add(button);
		}
		/// <summary>
		/// Registers that if the specified button is pressed than the given action
		/// should be performed.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed after that
		/// specific button has been pressed. </param>
		public void RegisterDown(Key button, Action<FrameEventArgs> action) {
			RegisterDown(button, (x, y) => action(y));
		}
		/// <summary>
		/// Registers that if the specified button is pressed than the given action
		/// should be performed.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed after that
		/// specific button has been pressed. </param>
		public void RegisterDown(Key button, Action<KeyboardState> action) {
			RegisterDown(button, (x, y) => action(x));
		}
		/// <summary>
		/// Registers that if the specified button is pressed than the given action
		/// should be performed.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed after that
		/// specific button has been pressed. </param>
		public void RegisterDown(Key button, Action action) {
			RegisterDown(button, (x, y) => action());
		}


		/// <summary>
		/// Registers that if the specified button is released than the given action
		/// should be performed.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed after that
		/// specific button has been released. </param>
		public void RegisterUp(Key button, KeyBindAction action) {
			DicAdd(UpKeybinds, button, action);
		}
		/// <summary>
		/// Registers that if the specified button is released than the given action
		/// should be performed.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed after that
		/// specific button has been released. </param>
		public void RegisterUp(Key button, Action<FrameEventArgs> action) {
			RegisterUp(button, (x, y) => action(y));
		}
		/// <summary>
		/// Registers that if the specified button is released than the given action
		/// should be performed.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed after that
		/// specific button has been released. </param>
		public void RegisterUp(Key button, Action<KeyboardState> action) {
			RegisterUp(button, (x, y) => action(x));
		}
		/// <summary>
		/// Registers that if the specified button is released than the given action
		/// should be performed.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed after that
		/// specific button has been released. </param>
		public void RegisterUp(Key button, Action action) {
			RegisterUp(button, (x, y) => action());
		}


		/// <summary>
		/// Registers that an action will keep happening when a specified
		/// button is held down.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed as long as
		/// s specific button is held down. </param>
		public void RegisterHold(Key button, KeyBindAction action) {
			DicAdd(DownKeybinds, button, action);
			DicAdd(HoldKeybinds, button, action);
			onKeys.Add(button);
		}
		/// <summary>
		/// Registers that an action will keep happening when a specified
		/// button is held down.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed as long as
		/// s specific button is held down. </param>
		public void RegisterHold(Key button, Action<FrameEventArgs> action) {
			RegisterHold(button, (x, y) => action(y));
		}
		/// <summary>
		/// Registers that an action will keep happening when a specified
		/// button is held down.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed as long as
		/// s specific button is held down. </param>
		public void RegisterHold(Key button, Action<KeyboardState> action) {
			RegisterHold(button, (x, y) => action(x));
		}
		/// <summary>
		/// Registers that an action will keep happening when a specified
		/// button is held down.
		/// </summary>
		/// <param name="button">The button which will trigger the action.</param>
		/// <param name="action">The operation which will be performed as long as
		/// s specific button is held down. </param>
		public void RegisterHold(Key button, Action action) {
			RegisterHold(button, (x, y) => action());
		}


		private void DicAdd<K, V>(IDictionary<K, ICollection<V>> dic, K key, V value) {
			if (!dic.TryGetValue(key, out ICollection<V> col))
				col = new List<V>();

			if (!col.Contains(value))
				col.Add(value);

			dic.Add(key, col);
		}

	}
}

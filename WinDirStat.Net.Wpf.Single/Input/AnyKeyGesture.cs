using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;

namespace WinDirStat.Net.Wpf.Input {
	/// <summary>
	/// AnyKeyGesture - Key and Modifier combination.
	///              Can be set on properties of KeyBinding and RoutedCommand.
	/// </summary>
	[TypeConverter(typeof(AnyKeyGestureConverter))]
	[ValueSerializer(typeof(KeyGestureValueSerializer))]
	public class AnyKeyGesture : InputGesture {
		//------------------------------------------------------
		//
		//  Constructors
		//
		//------------------------------------------------------
		#region Constructors
		/// <summary>
		///  constructor
		/// </summary>
		/// <param name="key">key</param>
		public AnyKeyGesture(Key key)
			: this(key, ModifierKeys.None) {
		}

		/// <summary>
		///  constructor
		/// </summary>
		/// <param name="modifiers">modifiers</param>
		/// <param name="key">key</param>
		public AnyKeyGesture(Key key, ModifierKeys modifiers)
			: this(key, modifiers, string.Empty, true) {
		}

		/// <summary>
		///  constructor
		/// </summary>
		/// <param name="modifiers">modifiers</param>
		/// <param name="key">key</param>
		/// <param name="displayString">display string</param>
		public AnyKeyGesture(Key key, ModifierKeys modifiers, string displayString)
			: this(key, modifiers, displayString, true) {
		}


		/// <summary>
		/// Internal constructor used by KeyBinding to avoid key and modifier validation
		/// This allows setting KeyBinding.Key and KeyBinding.Modifiers without regard
		/// to order.
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="modifiers">Modifiers</param>
		/// <param name="validateGesture">If true, throws an exception if the key and modifier are not valid</param>
		private AnyKeyGesture(Key key, ModifierKeys modifiers, bool validateGesture)
			: this(key, modifiers, string.Empty, validateGesture) {
		}

		/// <summary>
		/// Private constructor that does the real work.
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="modifiers">Modifiers</param>
		/// <param name="displayString">display string</param>
		/// <param name="validateGesture">If true, throws an exception if the key and modifier are not valid</param>
		private AnyKeyGesture(Key key, ModifierKeys modifiers, string displayString, bool validateGesture) {
			if (!ModifierKeysConverter.IsDefinedModifierKeys(modifiers))
				throw new InvalidEnumArgumentException("modifiers", (int) modifiers, typeof(ModifierKeys));

			if (!IsDefinedKey(key))
				throw new InvalidEnumArgumentException("key", (int) key, typeof(Key));

			if (validateGesture && !IsValid(key, modifiers)) {
				throw new NotSupportedException($"Unsupported key gesture \"{this}\"!");
			}

			_modifiers = modifiers;
			_key = key;
			_displayString = displayString ?? string.Empty;
		}
		#endregion Constructors

		//------------------------------------------------------
		//
		//  Public Methods
		//
		//------------------------------------------------------
		#region Public Methods
		/// <summary>
		/// Modifier
		/// </summary>
		public ModifierKeys Modifiers {
			get {
				return _modifiers;
			}
		}

		/// <summary>
		/// Key
		/// </summary>
		public Key Key {
			get {
				return _key;
			}
		}

		/// <summary>
		/// DisplayString
		/// </summary>
		public string DisplayString {
			get {
				return _displayString;
			}
		}

		/// <summary>
		/// Returns a string that can be used to display the AnyKeyGesture.  If the
		/// DisplayString was set by the constructor, it is returned.  Otherwise
		/// a suitable string is created from the Key and Modifiers, with any
		/// conversions being governed by the given CultureInfo.
		/// </summary>
		/// <param name="culture">the culture used when creating a string from Key and Modifiers</param>
		public string GetDisplayStringForCulture(CultureInfo culture) {
			// return the DisplayString, if it was set by the ctor
			if (!string.IsNullOrEmpty(_displayString)) {
				return _displayString;
			}

			// otherwise use the type converter
			return (string) _keyGestureConverter.ConvertTo(null, culture, this, typeof(string));
		}

		/// <summary>
		/// Compares InputEventArgs with current Input
		/// </summary>
		/// <param name="targetElement">the element to receive the command</param>
		/// <param name="inputEventArgs">inputEventArgs to compare to</param>
		/// <returns>True - AnyKeyGesture matches, false otherwise.
		/// </returns>
		public override bool Matches(object targetElement, InputEventArgs inputEventArgs) {
			KeyEventArgs keyEventArgs = inputEventArgs as KeyEventArgs;
			if (keyEventArgs != null && IsDefinedKey(keyEventArgs.Key)) {
				return (((int) Key == (int) keyEventArgs.Key) && (this.Modifiers == Keyboard.Modifiers));
			}
			return false;
		}

		// Check for Valid enum, as any int can be casted to the enum.
		internal static bool IsDefinedKey(Key key) {
			return (key >= Key.None && key <= Key.OemClear);
		}

		#endregion Public Methods

		//------------------------------------------------------
		//
		//  Internal Methods
		//
		//------------------------------------------------------
		#region Internal Methods
		///<summary>
		/// Is Valid Keyboard input to process for commands
		///</summary>
		internal static bool IsValid(Key key, ModifierKeys modifiers) {
			//
			//  Don't enforce any rules on the Function keys or on the number pad keys.
			//
			/*if (!((key >= Key.F1 && key <= Key.F24) || (key >= Key.NumPad0 && key <= Key.Divide))) {
				//
				//  We check whether Control/Alt/Windows key is down for modifiers. We don't check
				//  for shift at this time as Shift with any combination is already covered in above check.
				//  Shift alone as modifier case, we defer to the next condition to avoid conflicing with
				//  TextInput.

				if ((modifiers & (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows)) != 0) {
					switch (key) {
					case Key.LeftCtrl:
					case Key.RightCtrl:
					case Key.LeftAlt:
					case Key.RightAlt:
					case Key.LWin:
					case Key.RWin:
						return false;

					default:
						return true;
					}
				}
				else if ((key >= Key.D0 && key <= Key.D9) || (key >= Key.A && key <= Key.Z)) {
					return false;
				}
			}*/
			return true;
		}

		/// <summary>
		/// Decode the strings keyGestures and displayStrings, creating a sequence
		/// of KeyGestures.  Add each AnyKeyGesture to the given InputGestureCollection.
		/// The two input strings typically come from a resource file.
		/// </summary>
		internal static void AddGesturesFromResourceStrings(string keyGestures, string displayStrings, InputGestureCollection gestures) {
			while (!string.IsNullOrEmpty(keyGestures)) {
				string keyGestureToken;
				string keyDisplayString;

				// break apart first gesture from the rest
				int index = keyGestures.IndexOf(MULTIPLEGESTURE_DELIMITER, StringComparison.Ordinal);
				if (index >= 0) {   // multiple gestures exist
					keyGestureToken  = keyGestures.Substring(0, index);
					keyGestures   = keyGestures.Substring(index + 1);
				}
				else {
					keyGestureToken = keyGestures;
					keyGestures = string.Empty;
				}

				// similarly, break apart first display string from the rest
				index = displayStrings.IndexOf(MULTIPLEGESTURE_DELIMITER, StringComparison.Ordinal);
				if (index >= 0) {   // multiple display strings exist
					keyDisplayString  = displayStrings.Substring(0, index);
					displayStrings   = displayStrings.Substring(index + 1);
				}
				else {
					keyDisplayString = displayStrings;
					displayStrings = string.Empty;
				}

				AnyKeyGesture keyGesture = CreateFromResourceStrings(keyGestureToken, keyDisplayString);

				if (keyGesture != null) {
					gestures.Add(keyGesture);
				}
			}
		}

		internal static AnyKeyGesture CreateFromResourceStrings(string keyGestureToken, string keyDisplayString) {
			// combine the gesture and the display string, producing a string
			// that the type converter will recognize
			if (!string.IsNullOrEmpty(keyDisplayString)) {
				keyGestureToken += AnyKeyGestureConverter.DISPLAYSTRING_SEPARATOR + keyDisplayString;
			}

			return _keyGestureConverter.ConvertFromInvariantString(keyGestureToken) as AnyKeyGesture;
		}

		#endregion Internal Methods

		//------------------------------------------------------
		//
		//   Private Fields
		//
		//------------------------------------------------------
		#region Private Fields
		private readonly ModifierKeys _modifiers = ModifierKeys.None;
		private readonly Key _key = Key.None;
		private readonly string _displayString;
		private const string MULTIPLEGESTURE_DELIMITER = ";";
		private static readonly TypeConverter _keyGestureConverter = new AnyKeyGestureConverter();
		//private static bool    _classRegistered = false;
		#endregion Private Fields
	}
}

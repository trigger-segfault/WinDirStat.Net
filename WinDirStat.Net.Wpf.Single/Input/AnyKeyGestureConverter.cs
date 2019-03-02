using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinDirStat.Net.Wpf.Input {
	public class AnyKeyGestureConverter : TypeConverter {
		private const char MODIFIERS_DELIMITER = '+';
		internal const char DISPLAYSTRING_SEPARATOR = ',';

		///<summary>
		///CanConvertFrom()
		///</summary>
		///<param name="context">ITypeDescriptorContext</param>
		///<param name="sourceType">type to convert from</param>
		///<returns>true if the given type can be converted, false otherwise</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			// We can only handle string.
			if (sourceType == typeof(string)) {
				return true;
			}
			else {
				return false;
			}
		}


		///<summary>
		///TypeConverter method override.
		///</summary>
		///<param name="context">ITypeDescriptorContext</param>
		///<param name="destinationType">Type to convert to</param>
		///<returns>true if conversion	is possible</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			// We can convert to an InstanceDescriptor or to a string.
			if (destinationType == typeof(string)) {
				// When invoked by the serialization engine we can convert to string only for known type
				if (context != null && context.Instance != null) {
					AnyKeyGesture keyGesture = context.Instance as AnyKeyGesture;
					if (keyGesture != null) {
						return (ModifierKeysConverter.IsDefinedModifierKeys(keyGesture.Modifiers)
								&& IsDefinedKey(keyGesture.Key));
					}
				}
			}
			return false;
		}

		/// <summary>
		/// ConvertFrom()
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source) {
			if (source != null && source is string) {
				string fullName = ((string) source).Trim();
				if (fullName == string.Empty)
					return new AnyKeyGesture(Key.None);

				string keyToken;
				string modifiersToken;
				string displayString;

				// break apart display string
				int index = fullName.IndexOf(DISPLAYSTRING_SEPARATOR);
				if (index >= 0) {
					displayString = fullName.Substring(index + 1).Trim();
					fullName      = fullName.Substring(0, index).Trim();
				}
				else {
					displayString = string.Empty;
				}

				// break apart key and modifiers
				index = fullName.LastIndexOf(MODIFIERS_DELIMITER);
				if (index >= 0) {   // modifiers exists
					modifiersToken = fullName.Substring(0, index);
					keyToken       = fullName.Substring(index + 1);
				}
				else {
					modifiersToken = string.Empty;
					keyToken       = fullName;
				}

				ModifierKeys modifiers = ModifierKeys.None;
				object resultkey = keyConverter.ConvertFrom(context, culture, keyToken);
				if (resultkey != null) {
					object temp = modifierKeysConverter.ConvertFrom(context, culture, modifiersToken);
					if (temp != null) {
						modifiers = (ModifierKeys) temp;
					}
					return new AnyKeyGesture((Key) resultkey, modifiers, displayString);
				}
			}
			throw GetConvertFromException(source);
		}

		/// <summary>
		/// ConvertTo()
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <param name="destinationType"></param>
		/// <returns></returns>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == null)
				throw new ArgumentNullException("destinationType");

			if (destinationType == typeof(string)) {
				if (value != null) {
					AnyKeyGesture keyGesture = value as AnyKeyGesture;
					if (keyGesture != null) {
						if (keyGesture.Key == Key.None)
							return string.Empty;

						string strBinding = "";
						string strKey = (string) keyConverter.ConvertTo(context, culture, keyGesture.Key, destinationType) as string;
						if (strKey != string.Empty) {
							strBinding += modifierKeysConverter.ConvertTo(context, culture, keyGesture.Modifiers, destinationType) as string;
							if (strBinding != string.Empty) {
								strBinding += MODIFIERS_DELIMITER;
							}
							strBinding += strKey;

							if (!string.IsNullOrEmpty(keyGesture.DisplayString)) {
								strBinding += DISPLAYSTRING_SEPARATOR + keyGesture.DisplayString;
							}
						}
						return strBinding;
					}
				}
				else {
					return string.Empty;
				}
			}
			throw GetConvertToException(value, destinationType);
		}

		// Check for Valid enum, as any int can be casted to the enum.
		internal static bool IsDefinedKey(Key key) {
			return (key >= Key.None && key <= Key.OemClear);
		}

		private static KeyConverter keyConverter = new KeyConverter();
		private static ModifierKeysConverter modifierKeysConverter = new ModifierKeysConverter();
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using WinDirStat.Net.Wpf.Input;

namespace WinDirStat.Net.Wpf.Commands {
	/// <summary>
	/// A class for storing visual command info in XAML.
	/// </summary>
	public partial class RelayInfo : ObservableObject {

		#region Fields

		private string name;
		private string text;
		private ImageSource icon;
		private Key key;
		private ModifierKeys modifiers;
		private string inputGestureDisplayText;

		#endregion

		#region Properties

		/// <summary>Gets the display the name of the command to assign this info to.</summary>
		public string Name {
			get => name;
			set => SetProperty(ref name, value);
		}
		/// <summary>Gets the display text for the command.</summary>
		public string Text {
			get => text;
			set => SetProperty(ref text, value);
		}
		/// <summary>Gets the display icon for the command.</summary>
		public ImageSource Icon {
			get => icon;
			set => SetProperty(ref icon, value);
		}
		/// <summary>Gets or sets the key for <see cref="InputGesture"/>.</summary>
		public Key Key {
			get => key;
			set {
				if (SetProperty(ref key, value))
                    OnPropertyChanged(nameof(InputGesture));
			}
		}
		/// <summary>Gets or sets the modifier keys for <see cref="InputGesture"/>.</summary>
		public ModifierKeys Modifiers {
			get => modifiers;
			set {
				if (SetProperty(ref modifiers, value))
                    OnPropertyChanged(nameof(InputGesture));
			}
		}
		/// <summary>Gets or sets the override display text for <see cref="InputGesture"/>.</summary>
		public string InputGestureDisplayText {
			get => inputGestureDisplayText;
			set {
				if (SetProperty(ref inputGestureDisplayText, value))
					OnPropertyChanged(nameof(InputGesture));
			}
		}

		/// <summary>Gets the input gesture for the command.</summary>
		public AnyKeyGesture InputGesture {
			get {
				if (Key == Key.None)
					return null;
				if (InputGestureDisplayText != null)
					return new AnyKeyGesture(Key, Modifiers, InputGestureDisplayText);
				return new AnyKeyGesture(Key, Modifiers);
			}
		}

		#endregion
	}
}

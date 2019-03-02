using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.Wpf.Services.Structures {
	public class WpfShortcut : IShortcut {

		#region Fields

		/// <summary>The actual shortcut object.</summary>
		public KeyGesture Shortcut { get; }
		
		#endregion

		#region Constructors

		public WpfShortcut(Key key) : this(key, ModifierKeys.None) { }
		public WpfShortcut(Key key, string displayString) : this(key, ModifierKeys.None, displayString) { }
		public WpfShortcut(Key key, ModifierKeys modifiers) {
			Shortcut = new KeyGesture(key, modifiers);
		}
		public WpfShortcut(Key key, ModifierKeys modifiers, string displayString) {
			Shortcut = new KeyGesture(key, modifiers, displayString);
		}

		#endregion

		/// <summary>The actual shortcut object.</summary>
		object IShortcut.Shortcut => Shortcut;

		/// <summar>Gets the display text for the shortcut.</summar>
		public string DisplayText => Shortcut.GetDisplayStringForCulture(CultureInfo.CurrentCulture);

		/// <summar>Gets the display image for the shortcut.</summar>
		public IImage DisplayImage => null;
	}
}

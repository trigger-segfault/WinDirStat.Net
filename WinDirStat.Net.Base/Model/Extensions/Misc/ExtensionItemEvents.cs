using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.Extensions {
	/// <summary>The event action type for <see cref="ExtensionItem.Changed"/>.</summary>
	public enum ExtensionItemAction {
		// State
		/// <summary>The preview color of the extension changed.</summary>
		ColorChanged,

		// Accessors
		/// <summary>Requests the view watching this node to show itself.</summary>
		GetViewModel,
	}

	/// <summary>The event arguments for <see cref="ExtensionItem.Changed"/>.</summary>
	public class ExtensionItemEventArgs {
		/// <summary>The action to notify the view model of.</summary>
		public ExtensionItemAction Action { get; }
		/// <summary>Gets the index of the extension item in the sorted list.</summary>
		public int Index { get; }
		/// <summary>
		/// The view model if <see cref="Action"/> is <see cref="ExtensionItemAction.GetViewModel"/>.
		/// </summary>
		public object ViewModel { get; set; }

		/// <summary>
		/// Constructs the <see cref="ExtensionItemEventArgs"/> that just require an action.
		/// </summary>
		public ExtensionItemEventArgs(ExtensionItemAction action) {
			Action = action;
			Index = -1;
		}

		/// <summary>
		/// Constructs the <see cref="ExtensionItemEventArgs"/> that require an action and index.
		/// </summary>
		public ExtensionItemEventArgs(ExtensionItemAction action, int index) {
			Action = action;
			Index = index;
		}
	}

	/// <summary>The event handler for <see cref="ExtensionItem.Changed"/>.</summary>
	/// 
	/// <param name="sender">The <see cref="ExtensionItem"/> that sent this changed event.</param>
	/// <param name="e">The <see cref="ExtensionItemEventArgs"/> for this changed event.</param>
	public delegate void ExtensionItemEventHandler(ExtensionItem sender, ExtensionItemEventArgs e);
}

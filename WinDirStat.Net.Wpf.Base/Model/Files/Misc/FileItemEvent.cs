using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.Files {
	/// <summary>The event action type for <see cref="FileItemBase.Changed"/>.</summary>
	public enum FileItemAction {

		// State
		/// <summary>The node state has been marked is invalidated.</summary>
		Invalidated,
		/// <summary>The node state has been validated to reflect its children.</summary>
		Validated,
		/// <summary>Properties that require a visual resort have changed during validation.</summary>
		ValidatedSortOrder,
		/// <summary>The node and all children have been fully scanned.</summary>
		Done,
		/// <summary>The file's existance state has changed.</summary>
		Exists,
		/// <summary>The node was refreshed, and non-standard sort properties may have changed.</summary>
		/// <remarks>Exists may also have been changed when this action is received.</remarks>
		Refreshed,

		// Children
		/// <summary>Children have been added to the node.</summary>
		ChildrenAdded,
		/// <summary>Children have been removed from the node.</summary>
		ChildrenRemoved,
		/// <summary>Children have been cleared.</summary>
		ChildrenCleared,

		// Accessors
		/// <summary>Requests the view model watching this node to show itself.</summary>
		GetViewModel,
	}

	/// <summary>The event arguments for <see cref="FileItemBase.Changed"/>.</summary>
	public class FileItemEventArgs {
		/// <summary>The action to notify the view model of.</summary>
		public FileItemAction Action { get; }
		/// <summary>The children that were introduced or removed.</summary>
		public List<FileItemBase> Children { get; }
		/// <summary>The index of the introduced children.</summary>
		public int Index { get; }
		/// <summary>
		/// The view model if <see cref="Action"/> is <see cref="FileItemAction.GetViewModel"/>.
		/// </summary>
		public object ViewModel { get; set; }

		/// <summary>Constructs the <see cref="FileItemEventArgs"/> that just require an action.</summary>
		public FileItemEventArgs(FileItemAction action) : this(action, -1) {
		}

		/// <summary>
		/// Constructs the <see cref="FileItemEventArgs"/> that require an action and index.
		/// </summary>
		public FileItemEventArgs(FileItemAction action, int index) {
			Action = action;
			Index = index;
		}

		/// <summary>
		/// Constructs the <see cref="FileItemEventArgs"/> that represent changes to children.
		/// </summary>
		public FileItemEventArgs(FileItemAction action, List<FileItemBase> children, int index = -1)
			: this(action, index)
		{
			Children = children;
		}

		/// <summary>
		/// Constructs the <see cref="FileItemEventArgs"/> that represent changes to children.
		/// </summary>
		public FileItemEventArgs(FileItemAction action, IEnumerable<FileItemBase> children, int index = -1)
			: this(action, index)
		{
			Children = children.ToList();
		}

		/// <summary>
		/// Constructs the <see cref="FileItemEventArgs"/> that represent changes to a child.
		/// </summary>
		public FileItemEventArgs(FileItemAction action, FileItemBase child, int index = -1)
			: this(action, index)
		{
			Children = new List<FileItemBase> { child };
		}
	}

	/// <summary>The event handler for <see cref="FileItemBase.Changed"/>.</summary>
	/// 
	/// <param name="sender">The <see cref="FileItemBase"/> that sent this changed event.</param>
	/// <param name="e">The <see cref="FileItemEventArgs"/> for this changed event.</param>
	public delegate void FileItemEventHandler(FileItemBase sender, FileItemEventArgs e);
}

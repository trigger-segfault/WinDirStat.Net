using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Structures;

namespace WinDirStat.Net.Model.Extensions {
	/// <summary>A container for information about a file extension.</summary>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class ExtensionItem : IComparable<ExtensionItem>, IComparable {

		#region Constants

		/// <summary>The <see cref="ExtensionItem"/> used for file tree items that are not files.</summary>
		public static readonly ExtensionItem NotAFile = new ExtensionItem();

		/// <summary>The string used to represent an empty extension.</summary>
		public const string EmptyExtension = "*";

		#endregion

		#region Fields

		/// <summary>The collection containing this extension.</summary>
		private readonly ExtensionItems extensions;

		/// <summary>Gets the name of the extension with the dot.</summary>
		public string Extension { get; }
		/// <summary>Gets the total size of all the files that use this extension.</summary>
		public long Size { get; private set; }
		/// <summary>Gets the number of files that use this extension.</summary>
		public int FileCount { get; private set; }
		/// <summary>Gets the color to use in the treemap for files with this extension.</summary>
		private Rgb24Color color;

		#endregion

		#region Constructors

		/// <summary>Constructs the not-a-file extension.</summary>
		private ExtensionItem() {
			extensions = null;
			Extension = string.Empty;
			color = Rgb24Color.Black;
		}

		/// <summary>Constructs an <see cref="ExtensionItem"/> with the specified extension.</summary>
		/// 
		/// <param name="extensions">The collection containing this extension.</param>
		/// <param name="normalizedExtension">The pre-normalized extension for this item.</param>
		internal ExtensionItem(ExtensionItems extensions, string normalizedExtension) {
			this.extensions = extensions;
			Extension = normalizedExtension;
			color = new Rgb24Color(150, 150, 150);
		}

		#endregion

		#region ViewModel Events

		/// <summary>Notifies any view models watching this item of important changes to the item.</summary>
		[field: NonSerialized]
		public event ExtensionItemEventHandler Changed;

		/// <summary>Gets if the item is being watched by a view model.</summary>
		public bool IsWatched => Changed != null;

		protected void RaiseChanged(ExtensionItemEventArgs e) {
			Changed?.Invoke(this, e);
		}
		protected void RaiseChanged(ExtensionItemAction action) {
			Changed?.Invoke(this, new ExtensionItemEventArgs(action));
		}
		protected void RaiseChanged(ExtensionItemAction action, int index) {
			Changed?.Invoke(this, new ExtensionItemEventArgs(action, index));
		}

		public object GetViewModel() {
			ExtensionItemEventArgs e = new ExtensionItemEventArgs(ExtensionItemAction.GetViewModel);
			RaiseChanged(e);
			return e.ViewModel;
		}

		public TViewModel GetViewModel<TViewModel>() where TViewModel : class {
			ExtensionItemEventArgs e = new ExtensionItemEventArgs(ExtensionItemAction.GetViewModel);
			RaiseChanged(e);
			return e.ViewModel as TViewModel;
		}

		#endregion

		#region Properties

		/// <summary>Gets the color to use in the treemap for files with this extension.</summary>
		public Rgb24Color Color {
			get => color;
			/*internal set {
				color = value;
				RaiseChanged(ExtensionItemAction.ColorChanged);
			}*/
		}

		/// <summary>Gets this extension's size relative to the total used space.</summary>
		public double Percent {
			get => (double) Size / extensions.TotalSize;
		}

		/// <summary>Gets if this extension is the empty extension with nothing after the dot.</summary>
		public bool IsEmptyExtension {
			get => Extension == EmptyExtension;
		}

		#endregion

		#region Color

		/// <summary>Sets the new color of the extension item.</summary>
		/// 
		/// <param name="color">The new color.</param>
		/// <param name="index">The index of the extension in the sorted list.</param>
		internal void SetColor(Rgb24Color color, int index) {
			this.color = color;
			RaiseChanged(ExtensionItemAction.ColorChanged, index);
		}

		#endregion

		#region Files

		/// <summary>Adds the file to the extension data.</summary>
		/// 
		/// <param name="size">The size of the file to add.</param>
		internal void AddFile(long size) {
			FileCount++;
			Size += size;
			extensions.TotalSize += size;
			extensions.TotalFileCount++;
		}

		/// <summary>Refreshes the file size with the extension.</summary>
		/// 
		/// <param name="size">The new size of the file to refresh.</param>
		/// <param name="oldSize">The old size of the file to refresh.</param>
		internal void RefreshFile(long size, long oldSize) {
			long diff = size - oldSize;
			Size += diff;
			extensions.TotalSize += diff;
		}

		/// <summary>Removes the file from the extension data.</summary>
		/// 
		/// <param name="size">The size of the file to remove.</param>
		internal void RemoveFile(long size) {
			Debug.Assert(FileCount > 0);
			FileCount--;
			Size -= size;
			extensions.TotalSize -= size;
			extensions.TotalFileCount--;
			if (FileCount == 0)
				extensions.Remove(this);
		}

		/// <summary>
		/// Removes all files associated with this extension..
		/// </summary>
		internal void ClearFiles() {
			extensions.TotalSize -= Size;
			extensions.TotalFileCount -= FileCount;
			Size = 0;
			FileCount = 0;
			extensions.Remove(this);
		}

		/// <summary>
		/// Removes all files associated with this extension. Only call this during <see cref="ExtensionItems
		/// .Clear"/>.
		/// </summary>
		internal void ClearFilesMinimal() {
			Size = 0;
			FileCount = 0;
		}

		#endregion

		#region IComparable Implementation

		/// <summary>Compares this extension to another based on size in descending order.</summary>
		/// 
		/// <param name="other">The other extension to compare to.</param>
		/// <returns>The comparison result.</returns>
		public int CompareTo(ExtensionItem other) {
			int diff = other.Size.CompareTo(Size);
			if (diff == 0)
				return string.Compare(Extension, other.Extension, true);
			return diff;
		}

		/// <summary>Compares this extension to another based on size in descending order.</summary>
		/// 
		/// <param name="obj">The other extension to compare to.</param>
		/// <returns>The comparison result.</returns>
		int IComparable.CompareTo(object obj) {
			return CompareTo((ExtensionItem) obj);
		}

		#endregion

		#region ToString/DebuggerDisplay

		/// <summary>Gets the string representation of this item.</summary>
		public override sealed string ToString() => $"[{FileCount:N0}]: {Extension}";

		/// <summary>Gets the string used to represent the file in the debugger.</summary>
		private string DebuggerDisplay => $"[{FileCount:N0}]: {Extension}";

		#endregion

		#region Static Methods

		/// <summary>Normalizes the extension for consistency.</summary>
		/// 
		/// <param name="extension">The extension to normalize.</param>
		/// <returns>A normalized extension.</returns>
		public static string NormalizeExtension(string extension) {
			int length = extension.Length;
			if (length > 0) {
				if (extension[0] == '.') {
					if (length > 1)
						return extension.ToLower();
					else // '.' is equivilant to an empty extension
						return EmptyExtension;
				}
				else if (extension[0] != '*' || length > 1) {
					// Needs a dot
					return "." + extension.ToLower();
				}
			}
			// Empty string, empty extension
			return EmptyExtension;
		}

		/// <summary>Normalizes the extension for consistency.</summary>
		/// 
		/// <param name="extension">The extension to normalize.</param>
		public static void NormalizeExtension(ref string extension) {
			extension = NormalizeExtension(extension);
		}

		/// <summary>Gets the extension from the file path and normalizes it.</summary>
		/// 
		/// <param name="path">The file path to normalize the extension of.</param>
		/// <returns>A normalized extension.</returns>
		public static string GetAndNormalizeExtension(string path) {
			// Code modified from Path.cs: Path.GetExtension(string)
			int length = path.Length;
			for (int i = length; --i >= 0;) {
				char ch = path[i];
				if (ch == '.') {
					if (i != length - 1)
						return path.Substring(i, length - i).ToLower();
					else
						return EmptyExtension;
				}
				if (ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar)
					break;
			}
			return EmptyExtension;
		}

		#endregion
	}
}

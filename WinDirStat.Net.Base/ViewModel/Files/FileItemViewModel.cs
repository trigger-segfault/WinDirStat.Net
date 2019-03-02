using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;
using WinDirStat.Net.Utils;
using WinDirStat.Net.ViewModel.Extensions;

namespace WinDirStat.Net.ViewModel.Files {
	/// <summary>A view model for displaying <see cref="FileItem"/> models.</summary>
	public partial class FileItemViewModel : ObservableObject, IDisposable {

		#region Constants

		/// <summary>The collection used for empty children to avoid wasting memory.</summary>
		private static readonly FileItemViewModelCollection EmptyChildren = new FileItemViewModelCollection();

		#endregion

		#region Fields

		/// <summary>The loaded, visible children of the view model item.</summary>
		private FileItemViewModelCollection children;
		/// <summary>The parent of the view model item.</summary>
		private FileItemViewModel parent;
		/// <summary>The loaded icon of the view model item.</summary>
		private IImage icon;
		/// <summary>The loaded display name of the view model item. Only used with certain types.</summary>
		private string displayName;

		/// <summary> The main view model containing this item.</summary>
		public MainViewModel ViewModel { get; }
		/// <summary>The model this item represents.</summary>
		public FileItemBase Model { get; }
		/// <summary>The extension item view model associated with this item.</summary>
		public ExtensionItemViewModel ExtensionItem { get; }

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="FileItemViewModel"/>.</summary>
		/// 
		/// <param name="viewModel">The main view model containing this item.</param>
		/// <param name="model">The model this item represents.</param>
		public FileItemViewModel(MainViewModel viewModel, FileItemBase model) {
			ViewModel = viewModel;
			Model = model;
			if (model.Type == FileItemType.File)
				ExtensionItem = model.ExtensionItem.GetViewModel<ExtensionItemViewModel>();
			else
				ExtensionItem = ExtensionItemViewModel.NotAFile;
			Debug.Assert(ExtensionItem != null);

			exists = model.Exists;
			HookEvents();
			LoadIcon();
		}

		/// <summary>Disposes of the <see cref="FileItemViewModel"/>.</summary>
		/*~FileItemViewModel() {
			Dispose();
		}*/

		#endregion

		#region Properties

		/// <summary>Gets the parent of the item.</summary>
		public FileItemViewModel Parent {
			get => parent;
			private set => Set(ref parent, value);
		}
		/// <summary>Gets the icon of the item.</summary>
		public IImage Icon {
			get => icon;
			private set => Set(ref icon, value);
		}

		/// <summary>Gets the overlay icon if there is one.</summary>
		public IImage OverlayIcon {
			get {
				if (!Exists)
					return Images.Missing;
				else if (IsShortcut)
					return IconCache.ShortcutIcon;
				return null;
			}
		}

		/// <summary>Gets the display name of the item. (This may be null)</summary>
		public string DisplayName {
			get => displayName;
			private set {
				if (Set(ref displayName, value))
					RaisePropertyChanged(nameof(Header));
			}
		}

		/// <summary>Gets the extension of the item.</summary>
		public string Extension => Model.Extension;

		/// <summary>Gets the name of the file.</summary>
		public string Name => Model.Name;

		/// <summary>Gets the path of the file.</summary>
		public string FullName {
			get {
				if (Model.Parent != null || Model.IsAbsoluteRootType)
					return Model.FullName;
				return "";
			}
		}

		/// <summary>Gets the type of this file item.</summary>
		public FileItemType Type => Model.Type;

		/// <summary>Gets the total size of the file and all of its children.</summary>
		public long Size => Model.Size;

		/// <summary>Gets this file's size relative to the parent's size.</summary>
		public double Percent {
			get {
				if (Model.Parent != null || Model.IsAbsoluteRootType)
					return Model.Percent;
				return 0d;
			}
		}

		/// <summary>
		/// Gets the local time of when the file was last written to.<para/>
		/// If this is a container, it returns the most recent time of all children.
		/// </summary>
		public DateTime LastWriteTime => Model.LastWriteTime;

		/// <summary>
		/// Gets the number of files and directories this folder contains. Returns -1 if this is not a
		/// container.
		/// </summary>
		public int ItemCount => Model.ItemCount;
		/// <summary>
		/// Gets the number of files this folder contains. Returns -1 if this is not a container.
		/// </summary>
		public int FileCount => Model.FileCount;
		/// <summary>
		/// Gets the number of directories this folder contains. Returns -1 if this is not a container.
		/// </summary>
		public int SubdirCount => Model.SubdirCount;

		/// <summary>Gets the attributes for this file.</summary>
		public FileAttributes Attributes => Model.Attributes;

		/// <summary>Gets the attributes for this file.</summary>
		public string AttributesString => Model.AttributesString;

		/// <summary>Gets if the item should display a shortcut overlay.</summary>
		public bool IsShortcut => Model.IsShortcut;

		/// <summary>Gets the root view model item containing this view model item.</summary>
		public FileItemViewModel Root {
			get {
				if (parent == null)
					return this;
				return parent.Root;
			}
		}

		/// <summary>Gets the display tooltip for the view model item.</summary>
		public string ToolTip {
			get {
				if (Model.Type == FileItemType.Computer)
					return StringConstants.ComputerName;
				if (Model.Parent != null || Model.IsAbsoluteRootType)
					return Model.FullName;
				return "";
			}
		}

		/// <summary>Gets the display header for the view model item.</summary>
		public string Header {
			get {
				if (Type == FileItemType.Volume || Type == FileItemType.Computer)
					return displayName ?? $"({PathUtils.TrimSeparatorEnd(Name)})";
				else
					return Model.Name;
			}
		}

		/// <summary>Gets if file still exists in the system.</summary>
		public bool Exists {
			get => exists;
			private set {
				if (exists != value) {
					exists = value;
					RaisePropertyChanged();
					RaisePropertyChanged(nameof(OverlayIcon));
				}
			}
		}

		/// <summary>Gets the method for how icons are cached and displayed.</summary>
		private IconCacheMode CacheMode => ViewModel.Settings.IconCacheMode;

		/// <summary>Gets the UI service.</summary>
		private IUIService UI => ViewModel.UI;

		/// <summary>Gets the images service.</summary>
		private ImagesServiceBase Images => ViewModel.Images;

		/// <summary>Gets the icon cache service.</summary>
		private IIconCacheService IconCache => ViewModel.IconCache;
		
		/// <summary>Gets if the UI refreshing should be supressed due to validation.</summary>
		private bool SuppressRefresh => ViewModel.SuppressFileTreeRefresh;

		#endregion

		#region CacheIcon

		/// <summary>Caches the file's icon.</summary>
		private void LoadIcon(bool basicLoad = false) {
			IconCacheMode cacheMode = CacheMode;
			if (cacheMode == IconCacheMode.None)
				return;
			IIconAndName iconName;
			switch (Type) {
			case FileItemType.File:
				if (cacheMode >= IconCacheMode.FileType) {
					SetIcon(ExtensionItem.Icon);
					if (cacheMode >= IconCacheMode.Individual) {
						IconCache.CacheIconAsync(FullName, OnCacheFileIcon);
					}
					else if (ExtensionItem.CacheState != IconCacheState.Cached) {
						// Hook an event to wait for the cache state to change
						isWaitingForExtensionIcon = true;
						ExtensionItem.PropertyChanged += OnExtensionItemPropertyChanged;
					}
				}
				else {
					SetIcon(IconCache.FileIcon);
				}
				break;
			case FileItemType.Directory:
				SetIcon(IconCache.FolderIcon);
				if (cacheMode >= IconCacheMode.Individual)
					IconCache.CacheIconAsync(FullName, OnCacheFolderIcon);
				break;
			case FileItemType.Volume:
				if (cacheMode >= IconCacheMode.Individual) {
					iconName = IconCache.CacheIconAndDisplayName(FullName);
					SetIconAndDisplayName(iconName, IconCache.VolumeIcon, Name);
				}
				else {
					SetIcon(IconCache.VolumeIcon);
				}
				break;
			case FileItemType.Computer:
				if (CacheMode != IconCacheMode.None) {
					iconName = IconCache.CacheSpecialFolder(Environment.SpecialFolder.MyComputer);
					SetIconAndDisplayName(iconName, null, Name);
				}
				break;
			case FileItemType.FileCollection:
				SetIcon(Images.FileCollection);
				break;
			case FileItemType.FreeSpace:
				SetIcon(Images.FreeSpace);
				break;
			case FileItemType.Unknown:
				SetIcon(Images.UnknownSpace);
				break;
			}
		}

		/// <summary>
		/// The callback for when a property of the extension item has changed.<para/>
		/// This is only used to wait for the file type icon to be cached.
		/// </summary>
		private void OnExtensionItemPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == nameof(ExtensionItem.CacheState)) {
				if (ExtensionItem.CacheState == IconCacheState.Cached) {
					SetIcon(ExtensionItem.Icon);
					// Unhook event
					isWaitingForExtensionIcon = false;
					ExtensionItem.PropertyChanged -= OnExtensionItemPropertyChanged;
				}
			}
		}

		/// <summary>Sets the icon and raises the associated property change.</summary>
		/// 
		/// <param name="icon">The icon to use.</param>
		/// <returns>True if the <paramref name="icon"/> was non-null and was assigned.</returns>
		private bool SetIcon(IImage icon) {
			if (icon != null) {
				this.icon = icon;
				RaisePropertyChanged(nameof(Icon));
				return true;
			}
			return false;
		}

		/// <summary>Sets the icon and raises the associated property change.</summary>
		/// 
		/// <param name="icon">The icon to use.</param>
		/// <param name="defaultIcon">The default icon to use if the result is null.</param>
		/// <returns>True if the <paramref name="icon"/> was non-null and was assigned.</returns>
		private bool SetIcon(IImage icon, IImage defaultIcon) {
			if (icon != null) {
				this.icon = icon;
				RaisePropertyChanged(nameof(Icon));
				return true;
			}
			else if (defaultIcon != null) {
				icon = defaultIcon;
				RaisePropertyChanged(nameof(Icon));
			}
			return false;
		}

		/// <summary>Sets the icon and display name and raises the associated property changes.</summary>
		/// 
		/// <param name="iconName">The icon and name to use.</param>
		/// <param name="defaultIcon">The default icon to use if the result is null.</param>
		/// <param name="defaultName">The default name to use if the result is null.</param>
		/// <returns>True if the <paramref name="iconName"/> was non-null and was assigned.</returns>
		private bool SetIconAndDisplayName(IIconAndName iconName, IImage defaultIcon, string defaultName) {
			if (iconName != null) {
				icon = iconName.Icon;
				displayName = iconName.Name;
				RaisePropertyChanged(nameof(Icon));
				RaisePropertyChanged(nameof(DisplayName));
				RaisePropertyChanged(nameof(Header));
				return true;
			}
			else {
				if (defaultIcon != null) {
					icon = defaultIcon;
					RaisePropertyChanged(nameof(Icon));
				}
				if (defaultName != null) {
					displayName = defaultName;
					RaisePropertyChanged(nameof(DisplayName));
					RaisePropertyChanged(nameof(Header));
				}
			}
			return false;
		}

		/// <summary>The callback for caching a an individual file icon.</summary>
		/// 
		/// <param name="icon">The newly cached icon.</param>
		private void OnCacheFileIcon(IImage icon) {
			SetIcon(icon, ExtensionItem.Icon);
		}

		/// <summary>The callback for caching a an individual folder icon.</summary>
		/// 
		/// <param name="icon">The newly cached icon.</param>
		private void OnCacheFolderIcon(IImage icon) {
			SetIcon(icon, IconCache.FolderIcon);
		}

		/*private void OnCacheIconAndDisplayName(IconAndName iconName) {
			this.icon = icon ?? IconCache.FileIcon;
			if (icon == null) {
				if (Type == FileItemType.Volume)
					icon = IconCache.VolumeIcon;
			}
			else {
				displayName = Name;
			}
			RaisePropertyChanged(nameof(Icon));
			RaisePropertyChanged(nameof(Header));
		}*/

		#endregion

		#region Sort

		/// <summary>Sorts the children and all subchildren based on the comparison.</summary>
		/// 
		/// <param name="comparison">The comparison to sort with.</param>
		public void Sort(Comparison<FileItemViewModel> comparison) {
			Sort(comparison, true);
		}

		/// <summary>Sorts the children based on the comparison.</summary>
		/// 
		/// <param name="comparison">The comparison to sort with.</param>
		/// <param name="recursive">True if all subchildren should be recursively sorted.</param>
		private void Sort(Comparison<FileItemViewModel> comparison, bool recursive) {
			// We don't wanna go locking empty children.
			if (Model is FolderItem folder && !folder.IsEmpty) {
				List<FileItemBase> modelChildren = folder.Children;
				lock (modelChildren) {
					children.Sort(comparison);
					if (recursive) {
						int count = children.Count;
						for (int i = 0; i < count; i++) {
							FileItemViewModel child = children[i];
							if (child.Model.IsContainerType) {
								child.Sort(comparison, true);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Children

		/// <summary>Ensures that the chidren collection is created and ready for population.</summary>
		private void EnsureChildren() {
			if (children == null)
				children = new FileItemViewModelCollection(this);
		}
		
		/// <summary>The callback for when the model has changed in some way.</summary>
		private void OnModelChanged(FileItemBase sender, FileItemEventArgs e) {
			switch (e.Action) {
			case FileItemAction.Invalidated:
			case FileItemAction.Done:
			//case FileItemAction.Refreshed:
				break;
			case FileItemAction.Exists:
				Exists = Model.Exists;
				break;
			case FileItemAction.Validated:
			case FileItemAction.ValidatedSortOrder:
				OnModelValidated(e.Action == FileItemAction.ValidatedSortOrder);
				break;
			case FileItemAction.ChildrenAdded:
				OnModelChildrenAdded(e.Children, e.Index);
				break;
			case FileItemAction.ChildrenRemoved:
				OnModelChildrenRemoved(e.Children);
				break;
			case FileItemAction.ChildrenCleared:
				OnModelChildrenCleared();
				break;
			case FileItemAction.GetViewModel:
				e.ViewModel = this;
				break;
			}
		}

		/// <summary>Raises changes in the view model due to model validation.</summary>
		private void OnModelValidated(bool sortOrder) {
			UI.Invoke(() => {
				RaisePropertyChanged(nameof(Percent));
				RaisePropertyChanged(nameof(Size));
				RaisePropertyChanged(nameof(LastWriteTime));
				if (Model.IsContainerType) {
					RaisePropertyChanged(nameof(ItemCount));
					RaisePropertyChanged(nameof(FileCount));
					RaisePropertyChanged(nameof(SubdirCount));
				}
				if (children != null) {
					int count = children.Count;
					for (int i = 0; i < count; i++)
						children[i].RaisePropertyChanged(nameof(Percent));
					if (sortOrder && isExpanded)
						children.Sort(ViewModel.FileComparer.Compare);
				}
			});
		}

		/// <summary>Adds new children to the view model collection.</summary>
		private void OnModelChildrenAdded(List<FileItemBase> newChildren, int index) {
			if (isExpanded) {
				UI.Invoke(() => {
					EnsureChildren();
					int count = newChildren.Count;
					for (int i = 0; i < count; i++) {
						children.Add(new FileItemViewModel(ViewModel, newChildren[i]));
					}
					children.Sort(ViewModel.FileComparer.Compare);
				});
			}
			else if (newChildren.Count == Model.ChildCount) {
				// We had no children before
				UI.Invoke(() => RaisePropertyChanged(nameof(ShowExpander)));
			}
		}

		/// <summary>Removes old children from the view model collection.</summary>
		private void OnModelChildrenRemoved(List<FileItemBase> oldChildren) {
			if (isExpanded) {
				UI.Invoke(() => {
					int count = oldChildren.Count;
					for (int i = 0; i < count; i++) {
						int index = children.IndexOf(oldChildren[i]);
						FileItemViewModel oldItem = children[index];
						oldItem.UnhookEvents();
						children.RemoveAt(index);
						//children.Remove(e.Children[i]);
					}
				});
			}
			else if (oldChildren.Count > 0 && Model.ChildCount == 0) {
				// We have no children now
				UI.Invoke(() => RaisePropertyChanged(nameof(ShowExpander)));
			}
		}

		/// <summary>Clears all children from the view model collection.</summary>
		private void OnModelChildrenCleared() {
			UI.Invoke(() => {
				if (isExpanded) {
					if (children != null)
						children.Clear();
				}
				RaisePropertyChanged(nameof(ShowExpander));
			});
		}

		#endregion

		#region Expanding


		public static FileItemViewModel ExpandTo(FileItemBase item) {
			Stack<FileItemBase> ancestores = new Stack<FileItemBase>();

			while (item.GetViewModel() == null) {
				ancestores.Push(item);
				item = item.Parent;
			}

			FileItemViewModel view = item.GetViewModel<FileItemViewModel>();
			while (ancestores.Count > 0) {
				item = ancestores.Pop();
				view = view.ExpandFind(item);
			}
			return view;
		}

		protected void OnExpanding() {
			if (Model is FolderItem folder && !folder.IsEmpty) {
				List<FileItemBase> modelChildren = folder.Children;
				lock (modelChildren) {
					EnsureChildren();
					int count = modelChildren.Count;
					for (int i = 0; i < count; i++)
						children.Add(new FileItemViewModel(ViewModel, modelChildren[i]));
					children.Sort(ViewModel.FileComparer.Compare);
				}
			}
		}
		private FileItemViewModel ExpandFind(FileItemBase node) {
			FileItemViewModel result = null;
			if (Model is FolderItem folder && !folder.IsEmpty) {
				List<FileItemBase> modelChildren = folder.Children;
				lock (modelChildren) {
					int count = modelChildren.Count;
					for (int i = 0; i < count; i++) {
						FileItemBase child = modelChildren[i];
						FileItemViewModel view = new FileItemViewModel(ViewModel, child);
						if (child == node)
							result = view;
						children.Add(view);
					}
					children.Sort(ViewModel.FileComparer.Compare);
				}
			}
			UpdateChildIsVisible(true);
			return result;
		}
		protected void OnCollapsing() {
			if (Model is FolderItem folder && !folder.IsEmpty) {
				List<FileItemBase> children = folder.Children;
				lock (children) {
					int count = this.children.Count;
					for (int i = 0; i < count; i++)
						this.children[i].UnhookEvents();
					this.children.Clear();
				}
			}
		}

		#endregion

		#region IDisposable Implementation

		/// <summary>Disposes of the <see cref="FileItemViewModel"/>.</summary>
		public void Dispose() {
			UnhookEvents();
		}

		/// <summary>Hooks the required events for the view model.</summary>
		private void HookEvents() {
			if (!isEventsHooked) {
				isEventsHooked = true;
				Model.Changed += OnModelChanged;
			}
		}

		/// <summary>Unhooks all events from the view model.</summary>
		private void UnhookEvents() {
			if (isEventsHooked) {
				isEventsHooked = false;
				Model.Changed -= OnModelChanged;
			}
			if (isWaitingForExtensionIcon) {
				isWaitingForExtensionIcon = false;
				ExtensionItem.PropertyChanged -= OnExtensionItemPropertyChanged;
			}
			// Recursively unhook all events, we forgot to do that in the last implementation
			if (children != null) {
				int count = children.Count;
				for (int i = 0; i < count; i++) {
					children[i].UnhookEvents();
				}
			}
		}

		#endregion


		public FileItemViewModel FindView(FileItemBase node, bool expand) {
			Stack<FileItemBase> ancestors = new Stack<FileItemBase>();

			while (node.Parent != null && node != Model) {
				ancestors.Push(node);
				node = node.Parent;
			}

			FileItemViewModel view = this;
			while (ancestors.Count > 0) {
				node = ancestors.Pop();
				if (!view.IsExpanded) {
					if (!expand) {
						return null;
					}
					else {
						view.isExpanded = true;
						view = view.ExpandFind(node);
					}
				}
				else {
					view = view.children.Find(node);
				}
			}
			return view;
		}
	}
}

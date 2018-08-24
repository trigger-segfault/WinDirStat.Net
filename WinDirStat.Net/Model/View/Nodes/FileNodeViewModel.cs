using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WinDirStat.Net.Model.Data;
using WinDirStat.Net.Model.Data.Nodes;
using WinDirStat.Net.Model.View.Extensions;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Model.View.Nodes {
	public partial class FileNodeViewModel : ObservableObject, IDisposable {

		private readonly FileNodeBase model;

		private ImageSource icon;
		private ExtensionRecordViewModel extRecordView;
		private readonly WinDirStatViewModel viewModel;
		//private int modelIndex;

		private string displayName;

		private static Dispatcher Dispatcher {
			get => Application.Current.Dispatcher;
		}

		public FileNodeViewModel(WinDirStatViewModel viewModel, FileNodeBase model)
			: this(viewModel, model, -1)
		{
		}

		public FileNodeViewModel(WinDirStatViewModel viewModel, FileNodeBase model, int modelIndex) {
			this.viewModel = viewModel;
			this.model = model;
			//this.modelIndex = modelIndex;
			if (model.Type == FileNodeType.File)
				//extRecordView = model.ExtensionRecord.GetView<ExtensionRecordView>();
				extRecordView = viewModel.Extensions[model.Extension];
			else
				extRecordView = ExtensionRecordViewModel.Empty;

			HookEvents();
			LoadIcon();
		}

		private void OnModelChanged(FileNodeBase sender, FileNodeEventArgs e) {
			switch (e.Action) {
			case FileNodeAction.Invalidated:
			case FileNodeAction.Done:
			case FileNodeAction.Refreshed:
				break;
			case FileNodeAction.Validated:
			case FileNodeAction.ValidatedSortOrder:
				OnModelValidated(e.Action == FileNodeAction.ValidatedSortOrder);
				break;
			case FileNodeAction.ChildrenAdded:
				OnModelChildrenAdded(e.Children, e.Index);
				break;
			case FileNodeAction.ChildrenRemoved:
				OnModelChildrenRemoved(e.Children);
				break;
			case FileNodeAction.ChildrenCleared:
				OnModelChildrenCleared();
				break;
			case FileNodeAction.GetView:
				e.View = this;
				break;
			case FileNodeAction.IndexChanged:
				//modelIndex = e.Index;
				break;
			}
		}

		~FileNodeViewModel() {
			Dispose();
		}

		public FileNodeViewModel FindView(FileNodeBase node, bool expand) {
			Stack<FileNodeBase> ancestors = new Stack<FileNodeBase>();
			
			while (node.VirtualParent != null && node != model) {
				ancestors.Push(node);
				node = node.VirtualParent;
			}

			FileNodeViewModel view = this;
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

		private void OnModelValidated(bool sortOrder) {
			Dispatcher.Invoke(() => {
				RaisePropertiesChanged(
					nameof(Percent),
					nameof(Size),
					nameof(LastChangeTime));
				if (model.IsContainerType) {
					RaisePropertiesChanged(
						nameof(ItemCount),
						nameof(FileCount),
						nameof(SubdirCount));
				}
				if (children != null) {
					int count = children.Count;
					for (int i = 0; i < count; i++)
						children[i].RaisePropertyChanged(nameof(Percent));
					if (sortOrder && isExpanded)
						children.Sort(viewModel.FileComparer.Compare);
				}
			});
		}

		private void OnModelChildrenAdded(List<FileNodeBase> newChildren, int index) {
			if (isExpanded) {
				Dispatcher.Invoke(() => {
					EnsureChildren();
					int count = newChildren.Count;
					for (int i = 0; i < count; i++) {
						children.Add(new FileNodeViewModel(viewModel, newChildren[i], index + i));
					}
					children.Sort(viewModel.FileComparer.Compare);
				});
			}
		}

		private void OnModelChildrenRemoved(List<FileNodeBase> oldChildren) {
			if (isExpanded) {
				Dispatcher.Invoke(() => {
					int count = oldChildren.Count;
					for (int i = 0; i < count; i++) {
						int index = children.IndexOf(oldChildren[i]);
						FileNodeViewModel oldNode = children[index];
						oldNode.UnhookEvents();
						children.RemoveAt(index);
						//children.Remove(e.Children[i]);
					}
				});
			}
		}

		private void OnModelChildrenCleared() {
			if (isExpanded) {
				if (children != null)
					Dispatcher.Invoke(() => children.Clear());
			}
		}

		private void EnsureChildren() {
			if (children == null)
				children = new FileNodeCollection(this);
		}

		private void HookEvents() {
			if (!isEventsHooked) {
				isEventsHooked = true;
				model.Changed += OnModelChanged;
			}
		}

		private void UnhookEvents() {
			if (isEventsHooked) {
				isEventsHooked = false;
				model.Changed -= OnModelChanged;
			}
		}

		public void Dispose() {
			UnhookEvents();
		}

		public static FileNodeViewModel ExpandTo(FileNodeBase node) {
			Stack<FileNodeBase> ancestores = new Stack<FileNodeBase>();

			while (node.GetView() == null) {
				ancestores.Push(node);
				node = node.VirtualParent;
			}

			FileNodeViewModel view = node.GetView<FileNodeViewModel>();
			while (ancestores.Count > 0) {
				node = ancestores.Pop();
				view = view.ExpandFind(node);
			}
			return view;
		}

		protected void OnExpanding() {
			if (model is FolderNode folder && !folder.IsEmpty) {
				List<FileNodeBase> children = folder.VirtualChildren;
				lock (children) {
					EnsureChildren();
					int count = children.Count;
					for (int i = 0; i < count; i++)
						this.children.Add(new FileNodeViewModel(viewModel, children[i], i));
					this.children.Sort(viewModel.FileComparer.Compare);
				}
			}
		}
		private FileNodeViewModel ExpandFind(FileNodeBase node) {
			FileNodeViewModel result = null;
			if (model is FolderNode folder && !folder.IsEmpty) {
				List<FileNodeBase> children = folder.VirtualChildren;
				lock (children) {
					int count = children.Count;
					for (int i = 0; i < count; i++) {
						FileNodeBase child = children[i];
						FileNodeViewModel view = new FileNodeViewModel(viewModel, child, i);
						if (child == node)
							result = view;
						this.children.Add(view);
					}
					this.children.Sort(viewModel.FileComparer.Compare);
				}
			}
			UpdateChildIsVisible(true);
			return result;
		}
		protected void OnCollapsing() {
			if (model is FolderNode folder && !folder.IsEmpty) {
				List<FileNodeBase> children = folder.VirtualChildren;
				lock (children) {
					int count = this.children.Count;
					for (int i = 0; i < count; i++)
						this.children[i].UnhookEvents();
					this.children.Clear();
				}
			}
		}

		public FileNodeBase Model {
			get => model;
		}

		public WinDirStatViewModel ViewModel {
			get => viewModel;
		}

		public string Extension {
			get => model.Extension;
		}
		
		public ExtensionRecordViewModel ExtensionRecordView {
			get => extRecordView;
		}

		public string Name {
			get => model.Name;
		}

		public string FullName {
			get => model.Path;
		}

		public FileNodeType Type {
			get => model.Type;
		}

		public long Size {
			get => model.Size;
		}

		public double Percent {
			get {
				if (model.virtualParent != null || model.IsAbsoluteRootType)
					return model.Percent;
				return 0d;
			}
		}

		public DateTime LastChangeTime {
			get => model.LastChangeTime;
		}

		public int ItemCount {
			get => model.ItemCount;
		}

		public int FileCount {
			get => model.FileCount;
		}

		public int SubdirCount {
			get => model.SubdirCount;
		}

		public FileAttributes Attributes {
			get => model.Attributes;
		}

		public FileNodeViewModel Root {
			get {
				if (parent == null)
					return this;
				return parent.Root;
			}
		}

		public string ToolTip {
			//get => model.Path;
			get {
				if (model.virtualParent != null || model.IsAbsoluteRootType)
					return model.Path;
				return "";
			}
		}

		public string Header {
			get {
				if (Type == FileNodeType.Volume || Type == FileNodeType.Computer)
					return displayName ?? $"({PathUtils.TrimSeparatorEnd(Name)})";
				else
					return model.Name;
			}
		}

		public bool IsShortcut {
			get => model.IsShortcut;
		}

		private void LoadIcon(bool basicLoad = false) {
			string name;
			switch (Type) {
			case FileNodeType.FileCollection:
				icon = IconCache.FileCollectionIcon;
				RaisePropertyChanged(nameof(Icon));
				return;
			case FileNodeType.FreeSpace:
				icon = IconCache.FreeSpaceIcon;
				RaisePropertyChanged(nameof(Icon));
				return;
			case FileNodeType.Unknown:
				icon = IconCache.UnknownSpaceIcon;
				RaisePropertyChanged(nameof(Icon));
				return;
			case FileNodeType.Computer:
				icon = viewModel.Icons.CacheSpecialFolder(Environment.SpecialFolder.MyComputer, out name);
				if (icon != null)
					displayName = name;
				RaisePropertiesChanged(
					nameof(Icon),
					nameof(Header));
				return;
			case FileNodeType.Volume:
				icon = IconCache.VolumeIcon;
				//RaisePropertyChanged(nameof(Icon));
				//root.Document.Icons.CacheIconAsync(Path, Attributes, LoadIconAndDisplayNameCallback);
				icon = viewModel.Icons.CacheIcon(FullName, Attributes, out name);
				if (icon == null)
					icon = IconCache.VolumeIcon;
				else
					displayName = name;
				RaisePropertiesChanged(
					nameof(Icon),
					nameof(Header));
				return;
			case FileNodeType.File:
				icon = ExtensionRecordView.Icon ?? IconCache.FileIcon;
				break;
			default:
				icon = IconCache.FolderIcon;
				break;
			}
			RaisePropertyChanged(nameof(Icon));
			if (!basicLoad)
				viewModel.Icons.CacheIconAsync(FullName, Attributes, OnIconCache);
		}

		private void OnIconCache(ImageSource icon) {
			this.icon = icon;
			if (icon == null) {
				if (Type == FileNodeType.File)
					this.icon = IconCache.FileIcon;
				else if (Type == FileNodeType.Directory)
					this.icon = IconCache.FolderIcon;
			}
			RaisePropertyChanged(nameof(Icon));
		}

		private void OnIconAndDisplayNameCache(ImageSource icon, string name) {
			this.icon = icon ?? IconCache.FileIcon;
			if (icon == null) {
				if (Type == FileNodeType.Volume)
					this.icon = IconCache.VolumeIcon;
			}
			else {
				displayName = name;
			}
			RaisePropertiesChanged(
				nameof(Icon),
				nameof(Header));
		}

		public void Sort(Comparison<FileNodeViewModel> comparison) {
			Sort(comparison, true);
		}

		private void Sort(Comparison<FileNodeViewModel> comparison, bool recursive) {
			if (model is FolderNode folder && !folder.IsEmpty) {
				List<FileNodeBase> children = folder.VirtualChildren;
				lock (children) {
					this.children.Sort(comparison);
				}
			}
		}
	}
}

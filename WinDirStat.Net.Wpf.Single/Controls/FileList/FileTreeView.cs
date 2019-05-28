// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WinDirStat.Net.Wpf.Controls.SortList;
using WinDirStat.Net.Utils;
using WinDirStat.Net.ViewModel.Files;

namespace WinDirStat.Net.Wpf.Controls.FileList {
	public class FileTreeView : SortListView {
		static FileTreeView() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(FileTreeView),
													 new FrameworkPropertyMetadata(typeof(FileTreeView)));

			SelectionModeProperty.OverrideMetadata(typeof(FileTreeView),
												   new FrameworkPropertyMetadata(SelectionMode.Extended));

			AlternationCountProperty.OverrideMetadata(typeof(FileTreeView),
													  new FrameworkPropertyMetadata(2));

			DefaultItemContainerStyleKey =
				new ComponentResourceKey(typeof(FileTreeView), "DefaultItemContainerStyleKey");

			VirtualizingStackPanel.VirtualizationModeProperty.OverrideMetadata(typeof(FileTreeView),
																			   new FrameworkPropertyMetadata(VirtualizationMode.Recycling));

			RegisterCommands();
		}

		public static readonly RoutedEvent ActivateEvent =
			EventManager.RegisterRoutedEvent("Activate", RoutingStrategy.Bubble,
				typeof(RoutedEventHandler), typeof(FileTreeView));

		public event RoutedEventHandler Activate {
			add => AddHandler(ActivateEvent, value);
			remove => RemoveHandler(ActivateEvent, value);
		}

		public static ResourceKey DefaultItemContainerStyleKey { get; private set; }

		public FileTreeView() {
			SetResourceReference(ItemContainerStyleProperty, DefaultItemContainerStyleKey);
		}

		public static readonly DependencyProperty RootProperty =
			DependencyProperty.Register("Root", typeof(FileItemViewModel), typeof(FileTreeView));

		public FileItemViewModel Root {
			get => (FileItemViewModel) GetValue(RootProperty);
			set => SetValue(RootProperty, value);
		}

		public static readonly DependencyProperty ShowRootProperty =
			DependencyProperty.Register("ShowRoot", typeof(bool), typeof(FileTreeView),
										new FrameworkPropertyMetadata(true));

		public bool ShowRoot {
			get => (bool) GetValue(ShowRootProperty);
			set => SetValue(ShowRootProperty, value);
		}

		public static readonly DependencyProperty ShowRootExpanderProperty =
			DependencyProperty.Register("ShowRootExpander", typeof(bool), typeof(FileTreeView),
										new FrameworkPropertyMetadata(false));

		public bool ShowRootExpander {
			get => (bool) GetValue(ShowRootExpanderProperty);
			set => SetValue(ShowRootExpanderProperty, value);
		}

		public static readonly DependencyProperty AllowDropOrderProperty =
			DependencyProperty.Register("AllowDropOrder", typeof(bool), typeof(FileTreeView));

		public bool AllowDropOrder {
			get => (bool) GetValue(AllowDropOrderProperty);
			set => SetValue(AllowDropOrderProperty, value);
		}

		public static readonly DependencyProperty ShowLinesProperty =
			DependencyProperty.Register("ShowLines", typeof(bool), typeof(FileTreeView),
										new FrameworkPropertyMetadata(true));

		public bool ShowLines {
			get => (bool) GetValue(ShowLinesProperty);
			set => SetValue(ShowLinesProperty, value);
		}

		public static bool GetShowAlternation(DependencyObject obj) {
			return (bool) obj.GetValue(ShowAlternationProperty);
		}

		public static void SetShowAlternation(DependencyObject obj, bool value) {
			obj.SetValue(ShowAlternationProperty, value);
		}

		public static readonly DependencyProperty ShowAlternationProperty =
			DependencyProperty.RegisterAttached("ShowAlternation", typeof(bool), typeof(FileTreeView),
												new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);
			if (e.Property == RootProperty ||
				e.Property == ShowRootProperty ||
				e.Property == ShowRootExpanderProperty) {
				Reload();
			}
		}

		FileTreeFlattener flattener;

		void Reload() {
			if (flattener != null) {
				flattener.Stop();
				flattener = null;
			}
			if (Root != null) {
				if (!(ShowRoot && ShowRootExpander)) {
					Root.IsExpanded = true;
				}
				flattener = new FileTreeFlattener(Root, ShowRoot);
				flattener.CollectionChanged += Flattener_CollectionChanged;
				this.ItemsSource = flattener;
			}
			else {
				this.ItemsSource = null;
			}
		}

		void Flattener_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			// Deselect nodes that are being hidden, if any remain in the tree
			if (e.Action == NotifyCollectionChangedAction.Remove && Items.Count > 0) {
				List<FileItemViewModel> selectedOldItems = null;
				foreach (FileItemViewModel node in e.OldItems) {
					if (node.IsSelected) {
						if (selectedOldItems == null)
							selectedOldItems = new List<FileItemViewModel>();
						selectedOldItems.Add(node);
					}
				}
				if (selectedOldItems != null) {
					var list = SelectedItems.Cast<FileItemViewModel>().Except(selectedOldItems).ToList();
					SetSelectedItems(list);
					if (SelectedItem == null && this.IsKeyboardFocusWithin) {
						// if we removed all selected nodes, then move the focus to the node
						// preceding the first of the old selected nodes
						SelectedIndex = Math.Max(0, e.OldStartingIndex - 1);
						if (SelectedIndex >= 0)
							FocusNode((FileItemViewModel) SelectedItem);
					}
				}
			}
		}

		protected override DependencyObject GetContainerForItemOverride() {
			return new FileTreeViewItem();
		}

		protected override bool IsItemItsOwnContainerOverride(object item) {
			return item is FileTreeViewItem;
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
			base.PrepareContainerForItemOverride(element, item);
			FileTreeViewItem container = element as FileTreeViewItem;
			container.ParentTreeView = this;
			// Make sure that the line renderer takes into account the new bound data
			if (container.NodeView != null) {
				container.NodeView.LinesRenderer.InvalidateVisual();
			}
		}

		bool doNotScrollOnExpanding;

		/// <summary>
		/// Handles the node expanding event in the tree view.
		/// This method gets called only if the node is in the visible region (a SharpTreeNodeView exists).
		/// </summary>
		internal void HandleExpanding(FileItemViewModel node) {
			if (doNotScrollOnExpanding)
				return;
			/*FileNode lastVisibleChild = node;
			while (true) {
				FileNode tmp = lastVisibleChild.Children.LastOrDefault(c => c.IsVisible);
				if (tmp != null) {
					lastVisibleChild = tmp;
				}
				else {
					break;
				}
			}
			if (lastVisibleChild != node) {
				// Make the the expanded children are visible; but don't scroll down
				// to much (keep node itself visible)
				base.ScrollIntoView(lastVisibleChild);
				// For some reason, this only works properly when delaying it...
				Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(
					delegate {
						base.ScrollIntoView(node);
					}));
			}*/
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			FileItemViewModel selectedItem = null;
			if (e.OriginalSource is FileTreeViewItem container &&
				ItemsControl.ItemsControlFromItemContainer(container) == this)
			{
				selectedItem = container.Node;
			}
			else if (e.OriginalSource == this) {
				// When the FileTreeFlattener collection is reset, focus will be shifted to the FileTreeView.
				selectedItem = SelectedItem as FileItemViewModel;
				if (selectedItem != null)
					FocusNode(selectedItem);
			}
			else {
				// If we're reaching this, then we may be in some subcontrol of a file item, that control may want
				// focus. We'll keep this uncommented out for now until we determine handling needs to be changed.
				selectedItem = SelectedItem as FileItemViewModel;
				if (selectedItem != null)
					FocusNode(selectedItem);
			}

			if (selectedItem != null) {
				switch (e.Key) {
				case Key.Left:
					if (selectedItem.IsExpanded) {
						selectedItem.IsExpanded = false;
					}
					else if (selectedItem.Parent != null) {
						FocusNode(selectedItem.Parent);
					}
					e.Handled = true;
					break;
				case Key.Right:
					if (!selectedItem.IsExpanded && selectedItem.ShowExpander) {
						selectedItem.IsExpanded = true;
					}
					else if (selectedItem.Children.Count > 0) {
						// jump to first child:
						var firstChild = selectedItem.Children.FirstOrDefault();
						if (firstChild != null) {
							FocusNode(firstChild);
						}
					}
					e.Handled = true;
					break;
				case Key.Return:
				case Key.Space:
					if (Keyboard.Modifiers == ModifierKeys.None && SelectedItems.Count == 1) {
						e.Handled = true;
						selectedItem.ActivateItem();
					}
					break;
				/*case Key.Space:
					// Normally Space has special functionality, but we don't support checkboxes.
					e.Handled = true;
					selectedItem.ActivateItem();
					if (Keyboard.Modifiers == ModifierKeys.None && SelectedItems.Count == 1) {
						e.Handled = true;
						if (selectedItem.IsCheckable) {
							// If partially selected, we want to select everything
							selectedItem.IsChecked = !(selectedItem.IsChecked ?? false);
						}
						else {
							selectedItem.ActivateItem();
						}
					}
					break;*/
				case Key.Add:
					if (selectedItem.ShowExpander) {
						selectedItem.IsExpanded = true;
					}
					e.Handled = true;
					break;
				case Key.Subtract:
					selectedItem.IsExpanded = false;
					e.Handled = true;
					break;
				case Key.Multiply:
					if (selectedItem.ShowExpander) {
						selectedItem.IsExpanded = true;
						ExpandRecursively(selectedItem);
					}
					e.Handled = true;
					break;
				case Key.Divide:
					if (selectedItem.Parent != null) {
						FocusNode(selectedItem.Parent);
					}
					e.Handled = true;
					break;
				}
			}
			if (!e.Handled)
				base.OnKeyDown(e);
		}

		void ExpandRecursively(FileItemViewModel node) {
			if (node.CanExpandRecursively) {
				node.IsExpanded = true;
				foreach (FileItemViewModel child in node.Children) {
					ExpandRecursively(child);
				}
			}
		}

		/// <summary>
		/// Scrolls the specified node in view and sets keyboard focus on it.
		/// </summary>
		public void FocusNode(FileItemViewModel node) {
			if (node == null)
				throw new ArgumentNullException("node");
			ScrollIntoView(node);
			// WPF's ScrollIntoView() uses the same if/dispatcher construct, so we call OnFocusItem() after the item was brought into view.
			if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated) {
				OnFocusItem(node);
			}
			else {
				this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(this.OnFocusItem), node);
			}
		}

		public void ScrollIntoView(FileItemViewModel node) {
			if (node == null)
				throw new ArgumentNullException("node");
			doNotScrollOnExpanding = true;
			foreach (FileItemViewModel ancestor in node.Ancestors())
				ancestor.IsExpanded = true;
			doNotScrollOnExpanding = false;
			base.ScrollIntoView(node);
		}

		object OnFocusItem(object item) {
			if (ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement element) {
				element.Focus();
				Keyboard.Focus(element);
			}
			return null;
		}

		#region Track selection

		protected override void OnSelectionChanged(SelectionChangedEventArgs e) {
			foreach (FileItemViewModel node in e.RemovedItems) {
				node.IsSelected = false;
			}
			foreach (FileItemViewModel node in e.AddedItems) {
				node.IsSelected = true;
			}
			base.OnSelectionChanged(e);
		}

		#endregion

		#region Drag and Drop (Disabled)
		/*protected override void OnDragEnter(DragEventArgs e) {
			OnDragOver(e);
		}

		protected override void OnDragOver(DragEventArgs e) {
			e.Effects = DragDropEffects.None;

			if (Root != null && !ShowRoot) {
				e.Handled = true;
				e.Effects = Root.GetDropEffect(e, Root.Children.Count);
			}
		}

		protected override void OnDrop(DragEventArgs e) {
			e.Effects = DragDropEffects.None;

			if (Root != null && !ShowRoot) {
				e.Handled = true;
				e.Effects = Root.GetDropEffect(e, Root.Children.Count);
				if (e.Effects != DragDropEffects.None)
					Root.InternalDrop(e, Root.Children.Count);
			}
		}

		internal void HandleDragEnter(WinDirTreeViewItem item, DragEventArgs e) {
			HandleDragOver(item, e);
		}

		internal void HandleDragOver(WinDirTreeViewItem item, DragEventArgs e) {
			HidePreview();
			e.Effects = DragDropEffects.None;

			var target = GetDropTarget(item, e);
			if (target != null) {
				e.Handled = true;
				e.Effects = target.Effect;
				ShowPreview(target.Item, target.Place);
			}
		}

		internal void HandleDrop(WinDirTreeViewItem item, DragEventArgs e) {
			try {
				HidePreview();

				var target = GetDropTarget(item, e);
				if (target != null) {
					e.Handled = true;
					e.Effects = target.Effect;
					target.Node.InternalDrop(e, target.Index);
				}
			}
			catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
				throw;
			}
		}

		internal void HandleDragLeave(WinDirTreeViewItem item, DragEventArgs e) {
			HidePreview();
			e.Handled = true;
		}

		class DropTarget {
			public WinDirTreeViewItem Item;
			public DropPlace Place;
			public double Y;
			public WinDirNode Node;
			public int Index;
			public DragDropEffects Effect;
		}

		DropTarget GetDropTarget(WinDirTreeViewItem item, DragEventArgs e) {
			var dropTargets = BuildDropTargets(item, e);
			var y = e.GetPosition(item).Y;
			foreach (var target in dropTargets) {
				if (target.Y >= y) {
					return target;
				}
			}
			return null;
		}

		List<DropTarget> BuildDropTargets(WinDirTreeViewItem item, DragEventArgs e) {
			var result = new List<DropTarget>();
			var node = item.Node;

			if (AllowDropOrder) {
				TryAddDropTarget(result, item, DropPlace.Before, e);
			}

			TryAddDropTarget(result, item, DropPlace.Inside, e);

			if (AllowDropOrder) {
				if (node.IsExpanded && node.Children.Count > 0) {
					var firstChildItem = ItemContainerGenerator.ContainerFromItem(node.Children[0]) as WinDirTreeViewItem;
					TryAddDropTarget(result, firstChildItem, DropPlace.Before, e);
				}
				else {
					TryAddDropTarget(result, item, DropPlace.After, e);
				}
			}

			var h = item.ActualHeight;
			var y1 = 0.2 * h;
			var y2 = h / 2;
			var y3 = h - y1;

			if (result.Count == 2) {
				if (result[0].Place == DropPlace.Inside &&
					result[1].Place != DropPlace.Inside) {
					result[0].Y = y3;
				}
				else if (result[0].Place != DropPlace.Inside &&
						 result[1].Place == DropPlace.Inside) {
					result[0].Y = y1;
				}
				else {
					result[0].Y = y2;
				}
			}
			else if (result.Count == 3) {
				result[0].Y = y1;
				result[1].Y = y3;
			}
			if (result.Count > 0) {
				result[result.Count - 1].Y = h;
			}
			return result;
		}

		void TryAddDropTarget(List<DropTarget> targets, WinDirTreeViewItem item, DropPlace place, DragEventArgs e) {
			WinDirNode node;
			int index;

			GetNodeAndIndex(item, place, out node, out index);

			if (node != null) {
				var effect = node.GetDropEffect(e, index);
				if (effect != DragDropEffects.None) {
					DropTarget target = new DropTarget() {
						Item = item,
						Place = place,
						Node = node,
						Index = index,
						Effect = effect
					};
					targets.Add(target);
				}
			}
		}

		void GetNodeAndIndex(WinDirTreeViewItem item, DropPlace place, out WinDirNode node, out int index) {
			node = null;
			index = 0;

			if (place == DropPlace.Inside) {
				node = item.Node;
				index = node.Children.Count;
			}
			else if (place == DropPlace.Before) {
				if (item.Node.Parent != null) {
					node = item.Node.Parent;
					index = node.Children.IndexOf(item.Node);
				}
			}
			else {
				if (item.Node.Parent != null) {
					node = item.Node.Parent;
					index = node.Children.IndexOf(item.Node) + 1;
				}
			}
		}

		WinDirNodeView previewNodeView;
		InsertMarker insertMarker;
		DropPlace previewPlace;

		enum DropPlace {
			Before, Inside, After
		}

		void ShowPreview(WinDirTreeViewItem item, DropPlace place) {
			previewNodeView = item.NodeView;
			previewPlace = place;

			if (place == DropPlace.Inside) {
				previewNodeView.TextBackground = SystemColors.HighlightBrush;
				previewNodeView.Foreground = SystemColors.HighlightTextBrush;
			}
			else {
				if (insertMarker == null) {
					var adornerLayer = AdornerLayer.GetAdornerLayer(this);
					var adorner = new GeneralAdorner(this);
					insertMarker = new InsertMarker();
					adorner.Child = insertMarker;
					adornerLayer.Add(adorner);
				}

				insertMarker.Visibility = Visibility.Visible;

				var p1 = previewNodeView.TransformToVisual(this).Transform(new Point());
				var p = new Point(p1.X + previewNodeView.CalculateIndent() + 4.5, p1.Y - 3);

				if (place == DropPlace.After) {
					p.Y += previewNodeView.ActualHeight;
				}

				insertMarker.Margin = new Thickness(p.X, p.Y, 0, 0);

				WinDirNodeView secondNodeView = null;
				var index = flattener.IndexOf(item.Node);

				if (place == DropPlace.Before) {
					if (index > 0) {
						secondNodeView = (ItemContainerGenerator.ContainerFromIndex(index - 1) as WinDirTreeViewItem).NodeView;
					}
				}
				else if (index + 1 < flattener.Count) {
					secondNodeView = (ItemContainerGenerator.ContainerFromIndex(index + 1) as WinDirTreeViewItem).NodeView;
				}

				var w = p1.X + previewNodeView.ActualWidth - p.X;

				if (secondNodeView != null) {
					var p2 = secondNodeView.TransformToVisual(this).Transform(new Point());
					w = Math.Max(w, p2.X + secondNodeView.ActualWidth - p.X);
				}

				insertMarker.Width = w + 10;
			}
		}

		void HidePreview() {
			if (previewNodeView != null) {
				previewNodeView.ClearValue(WinDirNodeView.TextBackgroundProperty);
				previewNodeView.ClearValue(WinDirNodeView.ForegroundProperty);
				if (insertMarker != null) {
					insertMarker.Visibility = Visibility.Collapsed;
				}
				previewNodeView = null;
			}
		}*/
		#endregion

		#region Cut / Copy / Paste / Delete Commands (Disabled)

		static void RegisterCommands() {
			/*CommandManager.RegisterClassCommandBinding(typeof(WinDirTreeView),
													   new CommandBinding(ApplicationCommands.Cut, HandleExecuted_Cut, HandleCanExecute_Cut));

			CommandManager.RegisterClassCommandBinding(typeof(WinDirTreeView),
													   new CommandBinding(ApplicationCommands.Copy, HandleExecuted_Copy, HandleCanExecute_Copy));

			CommandManager.RegisterClassCommandBinding(typeof(WinDirTreeView),
													   new CommandBinding(ApplicationCommands.Paste, HandleExecuted_Paste, HandleCanExecute_Paste));
			
			CommandManager.RegisterClassCommandBinding(typeof(WinDirTreeView),
													   new CommandBinding(ApplicationCommands.Delete, HandleExecuted_Delete, HandleCanExecute_Delete));*/
		}

		/*static void HandleExecuted_Cut(object sender, ExecutedRoutedEventArgs e) {
			e.Handled = true;
			WinDirTreeView treeView = (WinDirTreeView) sender;
			var nodes = treeView.GetTopLevelSelection().ToArray();
			if (nodes.Length > 0)
				nodes[0].Cut(nodes);
		}

		static void HandleCanExecute_Cut(object sender, CanExecuteRoutedEventArgs e) {
			WinDirTreeView treeView = (WinDirTreeView) sender;
			var nodes = treeView.GetTopLevelSelection().ToArray();
			e.CanExecute = nodes.Length > 0 && nodes[0].CanCut(nodes);
			e.Handled = true;
		}

		static void HandleExecuted_Copy(object sender, ExecutedRoutedEventArgs e) {
			e.Handled = true;
			WinDirTreeView treeView = (WinDirTreeView) sender;
			var nodes = treeView.GetTopLevelSelection().ToArray();
			if (nodes.Length > 0)
				nodes[0].Copy(nodes);
		}

		static void HandleCanExecute_Copy(object sender, CanExecuteRoutedEventArgs e) {
			WinDirTreeView treeView = (WinDirTreeView) sender;
			var nodes = treeView.GetTopLevelSelection().ToArray();
			e.CanExecute = nodes.Length > 0 && nodes[0].CanCopy(nodes);
			e.Handled = true;
		}

		static void HandleExecuted_Paste(object sender, ExecutedRoutedEventArgs e) {
			WinDirTreeView treeView = (WinDirTreeView) sender;
			var data = Clipboard.GetDataObject();
			if (data != null) {
				var selectedNode = (treeView.SelectedItem as WinDirNode) ?? treeView.Root;
				if (selectedNode != null)
					selectedNode.Paste(data);
			}
			e.Handled = true;
		}

		static void HandleCanExecute_Paste(object sender, CanExecuteRoutedEventArgs e) {
			WinDirTreeView treeView = (WinDirTreeView) sender;
			var data = Clipboard.GetDataObject();
			if (data == null) {
				e.CanExecute = false;
			}
			else {
				var selectedNode = (treeView.SelectedItem as WinDirNode) ?? treeView.Root;
				e.CanExecute = selectedNode != null && selectedNode.CanPaste(data);
			}
			e.Handled = true;
		}

		static void HandleExecuted_Delete(object sender, ExecutedRoutedEventArgs e) {
			e.Handled = true;
			WinDirTreeView treeView = (WinDirTreeView) sender;
			var nodes = treeView.GetTopLevelSelection().ToArray();
			if (nodes.Length > 0)
				nodes[0].Delete(nodes);
		}

		static void HandleCanExecute_Delete(object sender, CanExecuteRoutedEventArgs e) {
			WinDirTreeView treeView = (WinDirTreeView) sender;
			var nodes = treeView.GetTopLevelSelection().ToArray();
			e.CanExecute = nodes.Length > 0 && nodes[0].CanDelete(nodes);
			e.Handled = true;
		}*/

		/// <summary>
		/// Gets the selected items which do not have any of their ancestors selected.
		/// </summary>
		public IEnumerable<FileItemViewModel> GetTopLevelSelection() {
			var selection = this.SelectedItems.OfType<FileItemViewModel>();
			var selectionHash = new HashSet<FileItemViewModel>(selection);
			return selection.Where(item => item.Ancestors().All(a => !selectionHash.Contains(a)));
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinDirStat.Net.Wpf.Controls.SortList {
	public class SortViewEventArgs : RoutedEventArgs {

		public object Mode { get; }
		public ListSortDirection Direction { get; }
		public bool IsDescending {
			get => Direction == ListSortDirection.Descending;
		}

		public TEnum ParseMode<TEnum>() {
			return (TEnum) Enum.Parse(typeof(TEnum), Mode as string);
		}
		
		public SortViewEventArgs(RoutedEvent routedEvent, object mode, ListSortDirection direction)
			: base(routedEvent)
		{
			Mode = mode;
			Direction = direction;
		}
	}

	public delegate void SortViewEventHandler(object sender, SortViewEventArgs e);

	public class SortListView : ListView {
		private struct ColumnPair {
			public SortViewColumn Column { get; }
			public GridViewColumnHeader Header { get; }

			public ColumnPair(SortViewColumn column, GridViewColumnHeader header) {
				Column = column;
				Header = header;
			}
		}


		//public static ResourceKey CellTemplateKey { get; } =
		//	new ComponentResourceKey(typeof(SortListView), "CellTemplateKey");
		//public static ResourceKey ColumnHeaderContainerStyleKey { get; } =
		//	new ComponentResourceKey(typeof(SortListView), "ColumnHeaderContainerStyleKey");
		/*internal static readonly DependencyPropertyKey HeaderSortDirectionPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("HeaderSortDirection", typeof(ListSortDirection?), typeof(SortListView),
				new FrameworkPropertyMetadata(null));*/

		public static readonly DependencyProperty CellTemplateProperty =
			GridViewColumn.CellTemplateProperty.AddOwner(typeof(SortListView));

		public DataTemplate CellTemplate {
			get => (DataTemplate) GetValue(CellTemplateProperty);
			set => SetValue(CellTemplateProperty, value);
		}

		public static readonly DependencyProperty HeaderContainerStyleProperty =
			GridViewColumn.HeaderContainerStyleProperty.AddOwner(typeof(SortListView));

		public Style HeaderContainerStyle {
			get => (Style) GetValue(HeaderContainerStyleProperty);
			set => SetValue(HeaderContainerStyleProperty, value);
		}

		public static readonly RoutedEvent SortEvent =
			EventManager.RegisterRoutedEvent("Sort", RoutingStrategy.Bubble, typeof(SortViewEventHandler), typeof(SortListView));

		public event SortViewEventHandler Sort {
			add => AddHandler(SortEvent, value);
			remove => RemoveHandler(SortEvent, value);
		}


		internal static readonly DependencyPropertyKey ColumnSortDirectionPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("ColumnSortDirection", typeof(ListSortDirection?), typeof(SortListView),
				new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ColumnSortDirectionProperty =
			ColumnSortDirectionPropertyKey.DependencyProperty;

		public static ListSortDirection? GetColumnSortDirection(DependencyObject d) {
			return (ListSortDirection?) d.GetValue(ColumnSortDirectionProperty);
		}

		internal static void SetColumnSortDirection(DependencyObject d, ListSortDirection? value) {
			d.SetValue(ColumnSortDirectionPropertyKey, value);
		}

		internal static readonly DependencyPropertyKey ColumnHeaderTextAlignmentPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("ColumnHeaderTextAlignment", typeof(TextAlignment), typeof(SortListView),
				new FrameworkPropertyMetadata(TextAlignment.Left));

		public static readonly DependencyProperty ColumnHeaderTextAlignmentProperty =
			ColumnHeaderTextAlignmentPropertyKey.DependencyProperty;

		public static TextAlignment GetColumnHeaderTextAlignment(DependencyObject d) {
			return (TextAlignment) d.GetValue(ColumnHeaderTextAlignmentProperty);
		}

		internal static void SetColumnHeaderTextAlignment(DependencyObject d, TextAlignment value) {
			d.SetValue(ColumnHeaderTextAlignmentPropertyKey, value);
		}

		public static readonly DependencyProperty SortDirectionProperty =
			DependencyProperty.Register("SortDirection", typeof(ListSortDirection), typeof(SortListView),
				new FrameworkPropertyMetadata(ListSortDirection.Ascending, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSortChanged));

		public ListSortDirection SortDirection {
			get => (ListSortDirection) GetValue(SortDirectionProperty);
			set => SetValue(SortDirectionProperty, value);
		}

		public static readonly DependencyProperty SortModeProperty =
			DependencyProperty.Register("SortMode", typeof(string), typeof(SortListView),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string SortMode {
			get => (string) GetValue(SortModeProperty);
			set => SetValue(SortModeProperty, value);
		}

		private static void OnSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is SortListView listView && !listView.supressSortEvent) {
				listView.UpdateColumns();
				listView.OnSort();
			}
		}

		private static void OnViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is SortListView listView) {
				if (e.OldValue is SortView oldSortView) {
					oldSortView.Columns.CollectionChanged -= listView.OnColumnsChanged;
				}
				if (e.NewValue is SortView newSortView) {
					newSortView.Columns.CollectionChanged += listView.OnColumnsChanged;
					//listView.Style = (Style) listView.FindResource(typeof(SortListView));
				}
				else if (e.NewValue != null) {
					throw new InvalidOperationException($"View is not a {nameof(SortView)}!");
				}
				listView.UpdateColumns();
			}
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			UpdateColumns();
		}

		private void UpdateColumns() {
			if (!IsLoaded)
				return;
			object mode = SortMode;
			ListSortDirection sortDirection = SortDirection;
			foreach (ColumnPair pair in ColumnsPairs) {
				if (mode == null || !object.Equals(mode, pair.Column.SortMode)) {
					SetColumnSortDirection(pair.Column, null);
					SetColumnSortDirection(pair.Header, null);
				}
				else {
					SetColumnSortDirection(pair.Column, sortDirection);
					SetColumnSortDirection(pair.Header, sortDirection);
				}
			}
		}

		private void OnColumnsChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.Action == NotifyCollectionChangedAction.Move)
				return;

			object mode = SortMode;
			ListSortDirection sortDirection = SortDirection;
			if (!IsLoaded)
				return;
			if (e.NewItems != null) {
				var presenter = Presenter;
				if (presenter == null)
					return;

				for (int i = 0; i < e.NewItems.Count; i++) {
					ColumnPair pair = GetColumnPairAt(presenter, i + e.NewStartingIndex);
					//if (pair.Column.HeaderContainerStyle == null)
					//	pair.Column.HeaderContainerStyle = SortViewKeys.HeaderContainerLeftAlignStyle;
					if (mode == null || !object.Equals(mode, pair.Column.SortMode)) {
						SetColumnSortDirection(pair.Column, null);
						SetColumnSortDirection(pair.Header, null);
					}
					else {
						SetColumnSortDirection(pair.Column, sortDirection);
						SetColumnSortDirection(pair.Header, sortDirection);
					}
				}
			}
		}

		static SortListView() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SortListView),
				new FrameworkPropertyMetadata(typeof(SortListView)));
			ViewProperty.OverrideMetadata(typeof(SortListView),
				new FrameworkPropertyMetadata(null, OnViewChanged));
			VirtualizingStackPanel.VirtualizationModeProperty.OverrideMetadata(typeof(SortListView),
				new FrameworkPropertyMetadata(VirtualizationMode.Recycling));
			Grid.IsSharedSizeScopeProperty.OverrideMetadata(typeof(SortListView),
				new FrameworkPropertyMetadata(true));


			EventManager.RegisterClassHandler(typeof(SortListView),
				GridViewColumnHeader.ClickEvent, new RoutedEventHandler(OnGridViewColumnHeaderClick));
		}
		
		private static void OnGridViewColumnHeaderClick(object sender, RoutedEventArgs e) {
			if (e.OriginalSource is GridViewColumnHeader header && sender is SortListView listView) {
				if (!(header.Column is SortViewColumn column))
					return;

				string oldSortMode = listView.SortMode;
				string newSortMode = column.SortMode;
				
				if (!object.Equals(oldSortMode, newSortMode)) {
					listView.SetSort(newSortMode, column.DefaultSortDirection);
				}
				else if (newSortMode != null) {
					listView.ToggleSortDirection();
				}
			}

			e.Handled = true;
		}

		private bool supressSortEvent;


		public void ToggleSortDirection() {
			if (SortDirection == ListSortDirection.Ascending)
				SortDirection = ListSortDirection.Descending;
			else
				SortDirection = ListSortDirection.Ascending;
		}

		public SortListView() {
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			UpdateColumns();
		}

		public void SetSort(string mode, ListSortDirection direction) {
			// Do all this extra work to avoid triggering a sort 'only' once, if needed
			bool newMode = !object.Equals(mode, SortMode);
			bool newDirection = direction != SortDirection;
			if (newMode) {
				supressSortEvent = true;
				if (newDirection) {
					SortDirection = direction;
				}
				SortMode = mode;
				supressSortEvent = false;
				UpdateColumns();
				OnSort();
			}
			else if (newDirection) {
				supressSortEvent = true;
				SortDirection = direction;
				supressSortEvent = false;
				UpdateColumns();
				OnSort();
			}
		}

		protected virtual void OnSort() {
			RaiseEvent(new SortViewEventArgs(SortEvent, SortMode, SortDirection));
		}

		private SortView SortView {
			get => View as SortView;
		}

		/*private IEnumerable<SortViewColumn> SortColumns {
			get {
				if (View is SortView sortView) {
					return sortView.Columns.Cast<SortViewColumn>();
				}
				return Enumerable.Empty<SortViewColumn>();
			}
		}*/

		private GridViewHeaderRowPresenter Presenter {
			get => GetDescendantByType<GridViewHeaderRowPresenter>();
		}

		private IEnumerable<ColumnPair> ColumnsPairs {
			get {
				if (View is SortView sortView) {
					var presenter = Presenter;
					if (presenter == null)
						yield break;

					for (int i = 0; i < sortView.Columns.Count; i++)
						yield return GetColumnPairAt(sortView, presenter, i);
				}
			}
		}

		private ColumnPair GetColumnPairAt(GridViewHeaderRowPresenter presenter, int index) {
			return new ColumnPair(GetColumnAt(index), GetHeader(SortView, presenter, index));
		}

		private ColumnPair GetColumnPairAt(SortView sortView, GridViewHeaderRowPresenter presenter, int index) {
			return new ColumnPair(GetColumnAt(sortView, index), GetHeader(sortView, presenter, index));
		}

		private SortViewColumn GetColumnAt(int index) {
			return (SortViewColumn) SortView.Columns[index];
		}

		private SortViewColumn GetColumnAt(SortView sortView, int index) {
			return (SortViewColumn) sortView.Columns[index];
		}

		private GridViewColumnHeader GetHeader(SortView sortView, GridViewHeaderRowPresenter presenter, int index) {
			return (GridViewColumnHeader) VisualTreeHelper.GetChild(presenter, sortView.Columns.Count - index);
		}

		private T GetDescendantByType<T>() where T : Visual {
			return GetDescendantByType<T>(this);
		}

		private static T GetDescendantByType<T>(Visual element) where T : Visual {
			if (element == null)
				return null;
			if (element is T t)
				return t;

			T foundElement = null;
			if (element is FrameworkElement frameworkElement)
				frameworkElement.ApplyTemplate();

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++) {
				Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
				foundElement = GetDescendantByType<T>(visual);
				if (foundElement != null)
					break;
			}
			return foundElement;
		}
	}
}

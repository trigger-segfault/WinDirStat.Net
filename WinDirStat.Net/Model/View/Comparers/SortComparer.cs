using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using WinDirStat.Net.SortingView;

namespace WinDirStat.Net.Model.View.Comparers {
	public abstract class SortComparer<T, TSortMode> : ObservableObject, IComparer<T>, IComparer {

		#region Fields

		private ListSortDirection direction;
		private TSortMode mode;
		private Comparison<T> comparison;

		#endregion

		#region Constructor

		public SortComparer(TSortMode mode) {
			Mode = mode;
		}

		public SortComparer(TSortMode mode, ListSortDirection direction) {
			Mode = mode;
			Direction = direction;
		}

		#endregion

		#region SetSort

		public void SetSort(SortViewEventArgs e) {
			Mode = e.ParseMode<TSortMode>();
			Direction = e.Direction;
		}

		#endregion

		#region IComparer

		public int Compare(T a, T b) {
			int diff = comparison(a, b);
			// Always sort alphabetically after initial sort
			if (diff == 0)
				return SecondaryCompare(a, b);
			if (direction == ListSortDirection.Ascending)
				return diff;
			else
				return -diff;
		}

		int IComparer.Compare(object a, object b) {
			return Compare((T) a, (T) b);
		}

		#endregion

		#region Sort Settings

		public TSortMode Mode {
			get => mode;
			set {
				if (!mode.Equals(value)) {
					mode = value;
					comparison = GetSortComparison(mode);
					AutoRaisePropertyChanged();
				}
			}
		}

		public ListSortDirection Direction {
			get => direction;
			set {
				if (direction != value) {
					direction = value;
					AutoRaisePropertyChanged();
					RaisePropertyChanged(nameof(IsDescending));
				}
			}
		}

		public bool IsDescending {
			get => direction == ListSortDirection.Descending;
			set {
				if (IsDescending != value) {
					if (value)
						direction = ListSortDirection.Descending;
					else
						direction = ListSortDirection.Ascending;
					RaisePropertyChanged(nameof(Direction));
					AutoRaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Abstract Methods

		protected abstract int SecondaryCompare(T a, T b);

		protected abstract Comparison<T> GetSortComparison(TSortMode mode);

		#endregion
	}
}

using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace WinDirStat.Net.ViewModel.Comparers {
    /// <summary>A helper class for view model comparers.</summary>
    /// <typeparam name="T">The type of the view model.</typeparam>
    /// <typeparam name="TSortMode">The type that representings the sort mode.</typeparam>
    public abstract class SortComparer<T, TSortMode> : ObservableObject, IComparer<T>, IComparer {

        #region Fields

        /// <summary>The current sort direction.</summary>
        private ListSortDirection direction;
        /// <summary>The current sort mode.</summary>
        private TSortMode mode;
        /// <summary>The current sort comparison.</summary>
        private Comparison<T> comparison;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs the <see cref="SortComparer"/> with the specified sort mode in ascending order.
        /// </summary>
        /// 
        /// <param name="mode">The sort mode to start with.</param>
        protected SortComparer(TSortMode mode) {
            Mode = mode;
        }

        /// <summary>
        /// Constructs the <see cref="SortComparer"/> with the specified sort mode and direction.
        /// </summary>
        /// 
        /// <param name="mode">The sort mode to start with.</param>
        /// <param name="direction">The sort direction to start with.</param>
        protected SortComparer(TSortMode mode, ListSortDirection direction) {
            Mode = mode;
            Direction = direction;
        }

        #endregion

        #region IComparer

        /// <summary>Compares the two items based on the sort mode and direction.</summary>
        /// 
        /// <param name="a">The first item to compare.</param>
        /// <param name="b">The second item to compare.</param>
        /// <returns>The comparison result.</returns>
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
        /// <summary>Compares the two items based on the sort mode and direction.</summary>
        /// 
        /// <param name="a">The first item to compare.</param>
        /// <param name="b">The second item to compare.</param>
        /// <returns>The comparison result.</returns>
        int IComparer.Compare(object a, object b) {
            return Compare((T) a, (T) b);
        }

        #endregion

        #region Sort Settings

        /// <summary>Gets the mode that determines the primary comparison method.</summary>
        public TSortMode Mode {
            get => mode;
            set {
                if (!mode.Equals(value)) {
                    mode = value;
                    comparison = GetSortComparison(mode);
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>Gets the direction of the sorting.</summary>
        public ListSortDirection Direction {
            get => direction;
            set {
                if (SetProperty(ref this.direction, value)) {
                    OnPropertyChanged(nameof(IsDescending));
                }
            }
        }
        /// <summary>Gets if the sort direction is in descending order.</summary>
        public bool IsDescending {
            get => direction == ListSortDirection.Descending;
            set => this.Direction = value ? ListSortDirection.Descending : ListSortDirection.Ascending;
        }

        #endregion

        #region Abstract Methods

        /// <summary>A secondary comparison that's called when the sort mode comparison returns 0.</summary>
        /// 
        /// <param name="a">The first item to compare.</param>
        /// <param name="b">The second item to compare.</param>
        /// <returns>The comparison result.</returns>
        protected abstract int SecondaryCompare(T a, T b);

        /// <summary>Gets the comparison method for the specified sort mode.</summary>
        /// 
        /// <param name="mode">The mode to get the comparison of.</param>
        /// <returns>The comparison to use.</returns>
        protected abstract Comparison<T> GetSortComparison(TSortMode mode);

        #endregion
    }
}

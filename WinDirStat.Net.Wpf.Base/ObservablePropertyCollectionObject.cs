using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace WinDirStat.Net {
    /// <summary>A base class for objects of which the collection must be observable.</summary>
    public abstract class ObservablePropertyCollectionObject : ObservableObject, INotifyCollectionChanged {

		#region Fields

		/// <summary>True if the collection is raising a <see cref="CollectionChanged"/> event.</summary>
		protected bool IsRaisingEvent { get; set; }

		#endregion

		#region Events

		/// <summary>Occurs after the collection's items change.</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		#region RaiseCollectionChanged (EventArgs)

		/// <summary>Raises a new <see cref="CollectionChanged"/> event.</summary>
		/// 
		/// <param name="e">The arguments for the event.</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e) {
			//Debug.Assert(!isRaisingEvent);
			ThrowOnReentrancy();
			IsRaisingEvent = true;
			try {
				CollectionChanged?.Invoke(this, e);
			}
			finally {
				IsRaisingEvent = false;
			}
		}

		#endregion

		#region RaiseCollectionChanged (Reset)

		/// <summary>
		/// Raises a new <see cref="CollectionChanged"/> event that describes a <see cref=
		/// "NotifyCollectionChangedAction.Reset"/> change.
		/// </summary>
		/// 
		/// <param name="action">The action that caused the event (must be Reset).</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedAction action) {
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action));
		}

		#endregion

		#region RaiseCollectionChanged (Change)

		/// <summary>
		/// Raises a new <see cref="CollectionChanged"/> event that describes a one-item change.
		/// </summary>
		/// 
		/// <param name="action">
		/// The action that caused the event; can only be Reset, Add or Remove action.
		/// </param>
		/// <param name="changedItem">The item affected by the change.</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, object changedItem) {
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItem));
		}

		/// <summary>
		/// Raises a new <see cref="CollectionChanged"/> event that describes a one-item change.
		/// </summary>
		/// 
		/// <param name="action">The action that caused the event.</param>
		/// <param name="changedItem">The item affected by the change.</param>
		/// <param name="index">The index where the change occurred.</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, object changedItem,
			int index) {
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItem, index));
		}

		/// <summary>
		/// Raises a new <see cref="CollectionChanged"/> event that describes a multi-item change.
		/// </summary>
		/// 
		/// <param name="action">The action that caused the event.</param>
		/// <param name="changedItems">The items affected by the change.</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, IList changedItems) {
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems));
		}

		/// <summary>
		/// Raises a new <see cref="CollectionChanged"/> event that describes a multi-item change.
		/// </summary>
		/// 
		/// <param name="action">The action that caused the event.</param>
		/// <param name="changedItems">The items affected by the change.</param>
		/// <param name="startingIndex">The index where the change occurred.</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, IList changedItems,
			int startingIndex)
		{
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems, startingIndex));
		}

		#endregion

		#region RaiseCollectionChanged (Replace)

		/// <summary>
		/// Raises a new <see cref="CollectionChanged"/> event that describes a one-item Replace event.
		/// </summary>
		/// 
		/// <param name="action">Can only be a Replace action.</param>
		/// <param name="newItem">The new item replacing the original item.</param>
		/// <param name="oldItem">The original item that is replaced.</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, object newItem,
			object oldItem)
		{
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
		}

		/// <summary>
		/// Raises a new <see cref="CollectionChanged"/> event that describes a one-item Replace event.
		/// </summary>
		/// 
		/// <param name="action">Can only be a Replace action.</param>
		/// <param name="newItem">The new item replacing the original item.</param>
		/// <param name="oldItem">The original item that is replaced.</param>
		/// <param name="index">The index of the item being replaced.</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, object newItem,
			object oldItem, int index)
		{
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
		}

		/// <summary>
		/// Raises a new <see cref="CollectionChanged"/> event that describes a multi-item Replace event.
		/// </summary>
		/// 
		/// <param name="action">Can only be a Replace action.</param>
		/// <param name="newItems">The new items replacing the original items.</param>
		/// <param name="oldItems">The original items that are replaced.</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, IList newItems,
			IList oldItems)
		{
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItems, oldItems));
		}

		/// <summary>
		/// Raises a new <see cref="CollectionChanged"/> event that describes a multi-item Replace event.
		/// </summary>
		/// 
		/// <param name="action">Can only be a Replace action.</param>
		/// <param name="newItems">The new items replacing the original items.</param>
		/// <param name="oldItems">The original items that are replaced.</param>
		/// <param name="startingIndex">The starting index of the items being replaced.</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, IList newItems,
			IList oldItems, int index)
		{
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItems, oldItems, index));
		}

		#endregion

		#region RaiseCollectionChanged (Move)

		/// <summary>
		/// Raises a new <see cref="CollectionChanged"/> event that describes a one-item Move event.
		/// </summary>
		/// 
		/// <param name="action">Can only be a Move action.</param>
		/// <param name="changedItem">The item affected by the change.</param>
		/// <param name="index">The new index for the changed item.</param>
		/// <param name="oldIndex">The old index for the changed item.</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, object changedItem,
			int index, int oldIndex)
		{
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItem, index, oldIndex));
		}

		/// <summary>
		/// Raises a new <see cref="CollectionChanged"/> event that describes a multi-item Move event.
		/// </summary>
		/// 
		/// <param name="action">Can only be a Replace action.</param>
		/// <param name="newItems">The new items replacing the original items.</param>
		/// <param name="oldItems">The original items that are replaced.</param>
		/// <param name="startingIndex">The starting index of the items being replaced.</param>
		protected void RaiseCollectionChanged(NotifyCollectionChangedAction action, IList changedItems,
			int index, int oldIndex)
		{
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems, index, oldIndex));
		}

		#endregion

		#region ThrowOnReentrancy

		/// <summary>
		/// Throws an exception if the user is attempting to modify the list during a <see cref=
		/// "CollectionChanged"/> event.
		/// </summary>
		/// 
		/// <exception cref="InvalidOperationException">
		/// The user is attempting to modify the list during a <see cref="CollectionChanged"/> event.
		/// </exception>
		protected void ThrowOnReentrancy() {
			if (IsRaisingEvent)
				throw new InvalidOperationException($"Collection cannot be modified during a " +
					$"{nameof(CollectionChanged)} event!");
		}

		#endregion
	}
}

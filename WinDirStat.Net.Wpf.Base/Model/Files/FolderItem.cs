using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Model.Files {
	/// <summary>The base class for containing all other file items.</summary>
	/// <remarks>
	/// Rules for Folder structure:<para/>
	/// Folders/Volumes can store files in 3 possible ways:<para/>
	/// A) 0-1 files, unlimited containers<para/>
	/// B) unlimited files, 0 containers<para/>
	/// C) 1 file collection (with 2+ files), 1+ containers<para/>
	/// If more than one file is found then we know containers are not being stored.
	/// </remarks>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FolderItem : FileItemBase {
		
		#region Fields

		/// <summary>
		/// The list of children in the folder.<para/>
		/// Always use the list modification methods to add or remove from the list. This is important
		/// because they account for if <see cref="FileItemBase.EmptyChildren"/> is being used.
		/// </summary>
		protected volatile List<FileItemBase> children = EmptyChildren;

		/// <summary>Gets the number of files this folder contains.</summary>
		public override sealed int FileCount { get; protected set; }
		/// <summary>Gets the number of directories this folder contains.</summary>
		public override sealed int SubdirCount { get; protected set; }

		#endregion

		#region Constructors

		/// <summary>Constructs a file collection <see cref="FolderItem"/>.</summary>
		private FolderItem()
			: base(StringConstants.FileCollectionName, FileItemType.FileCollection,
				  FileItemFlags.ContainerType)
		{
			// File collections are only created when adding children to them
			EnsureChildren();
		}

		/// <summary>Constructs the <see cref="FolderItem"/> without any file info.</summary>
		/// 
		/// <param name="name">The name of the item.</param>
		/// <param name="type">The type of the item.</param>
		/// <param name="flags">The flags for the item (except <see cref="FileAttributes"/>).</param>
		private protected FolderItem(string name, FileItemType type, FileItemFlags flags)
			: base(name, type, flags)
		{
		}

		/// <summary>Constructs the <see cref="FolderItem"/> with a <see cref="FileSystemInfo"/>.</summary>
		/// 
		/// <param name="info">The file information.</param>
		/// <param name="type">The type of the item.</param>
		/// <param name="flags">The flags for the item (except <see cref="FileAttributes"/>).</param>
		private protected FolderItem(FileSystemInfo info, FileItemType type, FileItemFlags flags)
			: base(info, type, flags)
		{
			//CaseSensitive = DirectoryCaseSensitivity.IsCaseSensitive(info.FullName);
		}

		/// <summary>Constructs the <see cref="FolderItem"/> with a <see cref="IScanFileInfo"/>.</summary>
		/// 
		/// <param name="info">The file information.</param>
		/// <param name="type">The type of the item.</param>
		/// <param name="flags">The flags for the item (except <see cref="FileAttributes"/>).</param>
		private protected FolderItem(IScanFileInfo info, FileItemType type, FileItemFlags flags)
			: base(info, type, flags)
		{
			//CaseSensitive = DirectoryCaseSensitivity.IsCaseSensitive(info.FullName);
		}

		/// <summary>
		/// Constructs the <see cref="FolderItem"/> directory with a <see cref="FileSystemInfo"/>.
		/// </summary>
		/// 
		/// <param name="info">The file information.</param>
		/// <param name="type">The type of the item.</param>
		/// <param name="flags">The flags for the item (except <see cref="FileAttributes"/>).</param>
		public FolderItem(FileSystemInfo info)
			: base(info, FileItemType.Directory, FileItemFlags.ContainerType | FileItemFlags.FileType)
		{
		}

		/// <summary>
		/// Constructs the <see cref="FolderItem"/> directory with a <see cref="IScanFileInfo"/>.
		/// </summary>
		/// 
		/// <param name="info">The file information.</param>
		/// <param name="type">The type of the item.</param>
		/// <param name="flags">The flags for the item (except <see cref="FileAttributes"/>).</param>
		public FolderItem(IScanFileInfo info)
			: base(info, FileItemType.Directory, FileItemFlags.ContainerType | FileItemFlags.FileType)
		{
		}

		#endregion

		#region ViewModel Events (Unused)
		/*
		// TODO: Eventually determine if we can move this to FolderItem

		/// <summary>Notifies any view models watching this item of important changes to the file.</summary>
		[field: NonSerialized]
		public event FileItemEventHandler Changed;

		/// <summary>Gets if the file is being watched by a view model.</summary>
		public bool IsWatched {
			get => Changed != null;
		}

		protected void RaiseChanged(FileItemEventArgs e) {
			Changed?.Invoke(this, e);
		}
		protected void RaiseChanged(FileItemAction action) {
			Changed?.Invoke(this, new FileItemEventArgs(action));
		}
		protected void RaiseChanged(FileItemAction action, int index) {
			Changed?.Invoke(this, new FileItemEventArgs(action, index));
		}
		protected void RaiseChanged(FileItemAction action, List<FileItemBase> children, int index) {
			Changed?.Invoke(this, new FileItemEventArgs(action, children, index));
		}
		protected void RaiseChanged(FileItemAction action, FileItemBase child, int index) {
			Changed?.Invoke(this, new FileItemEventArgs(action, child, index));
		}

		public object GetViewModel() {
			FileItemEventArgs e = new FileItemEventArgs(FileItemAction.GetView);
			RaiseChanged(e);
			return e.ViewModel;
		}

		public TViewModel GetViewModel<TViewModel>() {
			FileItemEventArgs e = new FileItemEventArgs(FileItemAction.GetView);
			RaiseChanged(e);
			return (TViewModel) e.ViewModel;
		}
		*/
		#endregion

		#region FileItemBase Overrides

		/// <summary>Returns true if this folder has any children.</summary>
		public override sealed bool HasChildren => children.Count > 0;
		/// Gets the list of children for this folder.<para/>
		/// This list is only for fast access and should never be modified.
		/// </summary>
		internal override sealed List<FileItemBase> Children => children;
		
		/// <summary>Gets the number of fils and directories this folder contains.</summary>
		public override sealed int ItemCount => FileCount + SubdirCount;

		#endregion

		#region ITreemapItem Overrides

		/// <summary>Gets the number of children in this container.</summary>
		public override sealed int ChildCount => children.Count;

		/// <summary>Gets the child at the specified index in the container.</summary>
		public override sealed FileItemBase this[int index] => children[index];

		#endregion

		#region Helper Properties
		
		/// <summary>Returns true if the folder is storing at least 2 files and 0 containers.</summary>
		private bool IsStoringMultipleFiles {
			get {
				// Always called from a File Folder, for a file folder.
				// This is why the return strange.
				if (Type == FileItemType.FileCollection)
					return false;
				// If true, another file means containers are not being stored.
				bool fileFound = false;
				int count = children.Count;
				for (int i = 0; i < count; i++) {
					FileItemBase child = children[i];
					if (!child.IsContainerType) {
						if (child.Type == FileItemType.File) {
							if (!fileFound)
								fileFound = true;
							else
								return true;
						}
					}
					else {
						return false;
					}
				}
				return false;
			}
		}
		/// <summary>Returns true if the folder is storing any number of containers.</summary>
		private bool IsStoringContainers {
			get {
				// Always called from a File Folder, for a file folder.
				// This is why the return strange.
				if (Type == FileItemType.FileCollection)
					return true;
				// If true, another file means containers are not being stored.
				bool fileFound = false;
				int count = children.Count;
				for (int i = 0; i < count; i++) {
					FileItemBase child = children[i];
					if (!child.IsContainerType) {
						if (child.Type == FileItemType.File) {
							if (!fileFound)
								fileFound = true;
							else
								return false;
						}
					}
					else {
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>Returns true if the folder is storing more than 1 container.</summary>
		private bool IsStoringMultipleContainers {
			get {
				// Always called from a File Folder, for a file folder.
				// This is why the return strange.
				if (Type == FileItemType.FileCollection)
					return true;
				// If true, another file means containers are not being stored.
				bool fileFound = false;
				bool containerFound = false;
				int count = children.Count;
				for (int i = 0; i < count; i++) {
					FileItemBase child = children[i];
					if (!child.IsContainerType) {
						if (child.Type == FileItemType.File) {
							if (!fileFound)
								fileFound = true;
							else
								return false;
						}
					}
					else if (!containerFound) {
						containerFound = true;
					}
					else {
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>Returns true if the folder is storing just 1 container.</summary>
		private bool IsStoringOneContainer {
			get {
				// Always called from a File Folder, for a file folder.
				// This is why the return strange.
				if (Type == FileItemType.FileCollection)
					return true;
				// If true, another file means containers are not being stored.
				bool fileFound = false;
				bool containerFound = false;
				int count = children.Count;
				for (int i = 0; i < count; i++) {
					FileItemBase child = children[i];
					if (!child.IsContainerType) {
						if (child.Type == FileItemType.File) {
							if (!fileFound)
								fileFound = true;
							else
								return false;
						}
					}
					else if (!containerFound) {
						containerFound = true;
					}
					else {
						return false;
					}
				}
				return containerFound;
			}
		}


		/// <summary>
		/// Gets if the folder is using <see cref="FileItemBase.EmptyChildren"/> for its list.
		/// </summary>
		internal protected bool IsEmpty => children == EmptyChildren;

		#endregion

		#region Item Finders

		/// <summary>Gets the file collection for this folder.</summary>
		/// 
		/// <returns>The folder's file collection, or null if it does not exist.</returns>
		public FolderItem GetFileCollection() {
			if (Type == FileItemType.FileCollection)
				return this;
			int count = children.Count;
			for (int i = 0; i < count; i++) {
				FileItemBase child = children[i];
				if (child.Type == FileItemType.FileCollection)
					return (FolderItem) child;
			}
			return null;
		}

		/// <summary>Gets the first file in this folder.</summary>
		/// 
		/// <returns>The folder's first file, or null if one does not exist.</returns>
		public FileItem GetFirstFile() {
			// Always called from a File Folder, for a file folder.
			// This is why the return strange.
			if (Type == FileItemType.FileCollection)
				return null;
			int count = children.Count;
			for (int i = 0; i < count; i++) {
				FileItemBase child = children[i];
				if (child.Type == FileItemType.File)
					return (FileItem) child;
			}
			return null;
		}

		/// <summary>Gets the first file in this folder and checks if there are multiple files.</summary>
		/// 
		/// <param name="multipleFiles">The output result of if there are multiple files.</param>
		/// <returns>The folder's only file, or null if there is not exactly one file.</returns>
		public FileItem GetFirstFile(out bool multipleFiles) {
			multipleFiles = false;
			// Always called from a File Folder, for a file folder.
			// This is why the return strange.
			if (Type == FileItemType.FileCollection)
				return null;
			FileItem firstFile = null;
			int count = children.Count;
			for (int i = 0; i < count; i++) {
				FileItemBase child = children[i];
				if (firstFile == null) {
					if (child.Type == FileItemType.File)
						firstFile = (FileItem) child;
				}
				else if (child.Type == FileItemType.File) {
					// Multiple files encountered, we're not storing a single file
					multipleFiles = true;
					return firstFile;
				}
				else if (child.IsContainerType) {
					// Container encountered, we must be storing a single file
					return firstFile;
				}
			}
			return firstFile;
		}

		/*/// <summary>Gets the only file in this folder.</summary>
		/// 
		/// <returns>The folder's only file, or null if there is not exactly one file.</returns>
		public FileItem GetSingleFile() {
			// Always called from a File Folder, for a file folder.
			// This is why the return strange.
			if (Type == FileItemType.FileCollection)
				return null;
			FileItem firstFile = null;
			int count = children.Count;
			for (int i = 0; i < count; i++) {
				FileItemBase child = children[i];
				if (firstFile == null) {
					if (child.Type == FileItemType.File)
						firstFile = (FileItem) child;
				}
				else if (child.Type == FileItemType.File) {
					// Multiple files encountered, we're not storing a single file
					return null;
				}
				else if (child.IsContainerType) {
					// Container encountered, we must be storing a single file
					return firstFile;
				}
			}
			return firstFile;
		}*/

		#endregion

		#region Add/Remove Item

		/// <summary>Adds the item to the container.</summary>
		/// 
		/// <param name="item">The item to add.</param>
		public void AddItem(FileItemBase item) {
			FolderItem fileCollection = null;
			FileItem firstFile = null;
			// Finding these items is not relevant when adding items like FreeSpace or Unknown
			if (item.Type == FileItemType.File || (item.IsContainerType && !item.IsAnyRootType)) {
				fileCollection = GetFileCollection();
				firstFile = GetFirstFile();
			}
			AddItem(item, ref fileCollection, ref firstFile);
		}

		/// <summary>
		/// Adds the item to the container and keeps track of the file collection and single file.
		/// </summary>
		/// 
		/// <param name="item">The item to add.</param>
		/// <param name="fileCollection">
		/// The container's file collection. Keep track of this when scanning this directory.
		/// </param>
		/// <param name="firstFile">
		/// The container's first file. Keep track of this when scanning this directory.
		/// </param>
		public void AddItem(FileItemBase item, ref FolderItem fileCollection, ref FileItem firstFile) {
			if (Type == FileItemType.FileCollection)
				throw new InvalidOperationException($"Cannot call {nameof(AddItem)} from a File Collection!");

			// We know we're adding an item to children, make sure it's setup
			EnsureChildren();

			lock (children) {
				if (item.Type == FileItemType.File) {
					// Add the file to the extensions
					item.ExtensionItem.AddFile(item.Size);

					if (fileCollection != null) {
						fileCollection.Add(item);
						fileCollection.Invalidate();
						// Item added to file collection, do not continue
						Debug.Assert(IsInvalidated);
						return;
					}
					else if (firstFile == null) {
						// Our first file! Let's celebrate by keeping track of it.
						// If more files are added then it will no longer be tracked.
						firstFile = (FileItem) item;
					}
					else if (IsStoringContainers) {
						// We've hit our limit of only one visible file when a
						// folder is storing non-files. Move to FileCollection.
						fileCollection = new FolderItem();
						Remove(firstFile);
						Add(fileCollection);
						fileCollection.Add(firstFile);
						fileCollection.Add(item);
						fileCollection.Invalidate();
						firstFile = null;
						// Item added to file collection, do not continue
						Debug.Assert(IsInvalidated);
						return;
					}
					// Removed after file -> firstFile (and was unassigned when holding more files)
					// file is only non-null when one file is being stored (with or without other files)
					/*else {
						bool storingContainers = IsStoringContainers;
						//children.Count == 0 is wrong, I forgot about Free/Unknown Space
						if (children.Count == 0 || (firstFile == null && storingContainers)) {
							// Our first file! Let's celebrate by keeping track of it.
							// If more files are added then it will no longer be tracked.
							firstFile = (FileItem) item;
						}
						else if (firstFile != null) {
							if (storingContainers) {
								// We've hit our limit of only one visible file when a
								// folder is storing non-files. Move to FileCollection.
								fileCollection = new FolderItem();
								Remove(firstFile);
								Add(fileCollection);
								fileCollection.Add(firstFile);
								fileCollection.Add(item);
								fileCollection.Invalidate();
								firstFile = null;
								// Item added to file collection, do not continue
								return;
							}
							firstFile = null;
						}
					}*/
				}
				else if (item.IsContainerType && IsStoringMultipleFiles) {
					// Setup file collection if the folder needs to store a container item
					Debug.Assert(fileCollection == null);
					fileCollection = new FolderItem();
					List<FileItemBase> files = ClearAndGetFiles();
					Add(fileCollection);
					fileCollection.AddRange(files);
					fileCollection.Invalidate();
					firstFile = null;
				}

				Add(item);
				Invalidate();

				//Debug.Assert(IsInvalidated);
			}
		}
		
		/// <summary>Removes the item from the container.</summary>
		/// 
		/// <param name="item">The item to remove.</param>
		public void RemoveItem(FileItemBase item) {
			FolderItem fileCollection = null;
			//FileItem firstFile = null;
			// Finding these items is not relevant when adding items like FreeSpace or Unknown
			if (item.Type == FileItemType.File || (item.IsContainerType && !item.IsAnyRootType)) {
				fileCollection = GetFileCollection();
				//firstFile = GetFirstFile();
			}
			RemoveItem(item, ref fileCollection/*, ref firstFile*/);
		}

		/// <summary>
		/// Removes the item from the container and keeps track of the file collection and single file.
		/// </summary>
		/// 
		/// <param name="item">The item to remove.</param>
		/// <param name="fileCollection">
		/// The container's file collection. Keep track of this when scanning this directory.
		/// </param>
		/// <param name="firstFile">
		/// The container's single file. Keep track of this when scanning this directory.
		/// </param>
		public void RemoveItem(FileItemBase item, ref FolderItem fileCollection/*, ref FileItem firstFile*/) {
			if (Type == FileItemType.FileCollection)
				throw new InvalidOperationException($"Cannot call {nameof(AddItem)} from a File Collection!");

			lock (children) {
				if (item.Type == FileItemType.File) {
					// Remove the file from the extensions
					item.ExtensionItem.RemoveFile(item.Size);

					if (fileCollection != null) {
						fileCollection.Remove(item);
						if (fileCollection.children.Count == 1) {
							// Once we go down to one file, we'll no longer need the file collection
							//firstFile = (FileItem) fileCollection.children[0];
							FileItemBase firstFile = fileCollection.children[0];
							fileCollection.RemoveAt(0);
							Remove(fileCollection);
							Add(firstFile);
							Invalidate();
							// It's dead Jim, remove the reference
							fileCollection = null;
						}
						else {
							// Standard procedure, no changes in structure
							fileCollection.Invalidate();
						}
						return;
					}
				}
				else if (item.IsContainerType) {
					// Recursively remove all of the container's children
					((FolderItem) item).RecurseClearChildren();

					if (fileCollection != null && IsStoringContainers) {
						// No more containers, open the gate for files to return back to folder.
						// Also remember to remove the no-longer-in-use file collection.
						Remove(item);
						List<FileItemBase> files = fileCollection.ClearAndGetChildren();
						//firstFile = (FileItem) files[0];
						Remove(fileCollection);
						AddRange(files);
						Invalidate();
						// It's dead Jim, remove the reference
						fileCollection = null;
						return;
					}
				}

				Remove(item);
				Invalidate();

				// Set children back to EmptyChildren if we removed the last item
				EnsureEmptyChildren();
			}
		}

		/// <summary>
		/// Recursively clears this folder's children from the file tree and any other associations.
		/// </summary>
		private protected void RecurseClearChildren() {
			foreach (FileItemBase child in children) {
				if (child is FolderItem subdir && !subdir.IsEmpty)
					subdir.RecurseClearChildren();
				else if (child.Type == FileItemType.File)
					child.ExtensionItem.RemoveFile(child.Size);
			}
			Clear();
		}

		/*/// <summary>
		/// Removes this folder's files from the file tree and any other associations.
		/// <para/>
		/// Handles changes to the folder structure like normally removing an item.
		/// </summary>
		public void ClearFileItems() {
			if (IsEmpty)
				return;

			lock (children) {
				FolderItem fileCollection = GetFileCollection();
				if (fileCollection != null)
					ClearFileCollectionItems(fileCollection);
			}
		}*/

		/// <summary>
		/// Recursively removes this folder's children from the file tree and any other associations.
		/// <para/>
		/// Handles changes to the folder structure like normally removing an item.
		/// </summary>
		public void ClearItems() {
			if (IsEmpty)
				return;

			int count;

			List<FolderItem> subdirs = new List<FolderItem>();
			lock (children) {
				count = children.Count;
				for (int i = 0; i < count; i++) {
					FileItemBase child = children[i];
					if (child is FolderItem subdir && child.Type != FileItemType.FileCollection)
						subdirs.Add(subdir);
				}
			}

			// Clear all subdirectories outside of the children
			// lock to prevent the UI from freezing.
			count = subdirs.Count;
			for (int i = 0; i < count; i++) {
				subdirs[i].ClearItems();
			}

			lock (children) {
				FolderItem fileCollection = null;
				for (int i = 0; i < children.Count; i++) {
					FileItemBase child = children[i];
					/*if (child is FolderItem subdir && !subdir.IsEmpty) {
						if (subdir.Type == FileItemType.FileCollection) {
							ClearFileCollectionItems(ref fileCollection);
							continue;
						}
						else {
							subdir.ClearItems();
						}
					}*/
					// File Collections do not get removed
					// through RemoveItem so let's continue
					if (child.Type == FileItemType.FileCollection)
						fileCollection = (FolderItem) child;
					else if (child.Type == FileItemType.File)
						child.ExtensionItem.RemoveFile(child.Size);
					//RemoveAt(0);
					//RemoveAt(i);
					//RemoveItem(child, ref fileCollection);
					//i--;
				}
				Clear();
				children = EmptyChildren;
				fileCollection?.ClearFileCollectionItems();
				FileCount = 0;
				SubdirCount = 0;
				Size = 0;
			}
		}

		private void ClearFileCollectionItems() {
			lock (children) {
				for (int i = 0; i < children.Count; i++) {
					FileItemBase child = children[i];
					child.ExtensionItem.RemoveFile(child.Size);
				}
				Clear();
				children = EmptyChildren;
			}
		}

		#endregion

		#region List Modification

		/// <summary>Adds the item to the container.</summary>
		/// 
		/// <param name="item">The item to add.</param>
		protected void Add(FileItemBase item) {
			//EnsureChildren();
			int index = children.Count;
			children.Add(item);
			item.Parent = this;
			if (IsWatched)
				RaiseChanged(FileItemAction.ChildrenAdded, item, index);
		}

		/// <summary>Adds or removes the item from the container based on a conditional value.</summary>
		/// 
		/// <param name="item">The item to add or remove.</param>
		/// <param name="addCondition">The condition to add the item instead of removing it.</param>
		protected void AddOrRemove(FileItemBase item, bool addCondition) {
			int index = children.IndexOf(item);
			if (addCondition && index == -1)
				Add(item);
			else if (!addCondition && index != -1)
				RemoveAt(index);
		}

		/// <summary>Adds the items to the container.</summary>
		/// 
		/// <param name="items">The items to add.</param>
		protected void AddRange(IEnumerable<FileItemBase> items) {
			//EnsureChildren();
			int index = children.Count;
			children.AddRange(items);
			foreach (FileItemBase item in items)
				item.Parent = this;
			if (IsWatched)
				RaiseChanged(FileItemAction.ChildrenAdded, items.ToList(), index);
		}

		/// <summary>Removes the items from the container.</summary>
		/// 
		/// <param name="items">The items to remove.</param>
		protected void RemoveRange(IEnumerable<FileItemBase> items) {
			foreach (FileItemBase item in items) {
				children.Remove(item);
				item.Parent = null;
			}
			if (IsWatched)
				RaiseChanged(FileItemAction.ChildrenRemoved, items.ToList(), -1);
		}

		/// <summary>Removes the item from the container.</summary>
		/// 
		/// <param name="items">The item to remove.</param>
		/// <returns>true if the item existed and was removed.</returns>
		protected bool Remove(FileItemBase item) {
			if (children.Remove(item)) {
				item.Parent = null;
				if (IsWatched)
					RaiseChanged(FileItemAction.ChildrenRemoved, item, -1);
				return true;
			}
			return false;
		}

		/// <summary>Removes the item at the specified index in the container.</summary>
		/// 
		/// <param name="index">The index of the item to remove.</param>
		protected void RemoveAt(int index) {
			FileItemBase item = children[index];
			children.RemoveAt(index);
			item.Parent = null;
			if (IsWatched)
				RaiseChanged(FileItemAction.ChildrenRemoved, item, index);
		}

		/// <summary>Removes the items at the specified index in the container.</summary>
		/// 
		/// <param name="index">The index of the items to remove.</param>
		/// <param name="count">The number of items to remove.</param>
		protected void RemoveRange(int index, int count) {
			List<FileItemBase> items = children.GetRange(index, count);
			children.RemoveRange(index, count);
			for (int i = 0; i < count; i++)
				items[i].Parent = null;
			if (IsWatched)
				RaiseChanged(FileItemAction.ChildrenRemoved, items.GetFullRange(), index);
		}

		/// <summary>Clears the list.</summary>
		protected void Clear() {
			int count = children.Count;
			if (count > 0) {
				for (int i = 0; i < count; i++)
					children[i].Parent = null;
				if (IsWatched) {
					List<FileItemBase> oldChildren = children.GetFullRange();
					children.Clear();
					//children = EmptyChildren;
					RaiseChanged(FileItemAction.ChildrenRemoved, oldChildren, 0);
				}
				else {
					children.Clear();
					//children = EmptyChildren;
				}
			}
		}

		/// <summary>Clears the list and returns the range that was cleared.</summary>
		private List<FileItemBase> ClearAndGetChildren() {
			int count = children.Count;
			if (count > 0) {
				for (int i = 0; i < count; i++)
					children[i].Parent = null;
				List<FileItemBase> oldChildren = children.GetFullRange();
				children.Clear();
				if (IsWatched)
					RaiseChanged(FileItemAction.ChildrenRemoved, oldChildren, 0);
				return oldChildren;
			}
			return children;//EmptyChildren;
		}

		/// <summary>Clears the list of files and returns the files that were cleared.</summary>
		private List<FileItemBase> ClearAndGetFiles() {
			int count = children.Count;
			if (count > 0) {
				List<FileItemBase> oldChildren = new List<FileItemBase>();
				for (int i = 0; i < children.Count; i++) {
					FileItemBase child = children[i];
					if (child.Type == FileItemType.File) {
						children[i].Parent = null;
						oldChildren.Add(child);
						children.RemoveAt(i);
						i--;
					}
				}
				if (IsWatched)
					RaiseChanged(FileItemAction.ChildrenRemoved, oldChildren, -1);
				return oldChildren;
			}
			return children;//EmptyChildren;
		}

		/// <summary>Ensures the children are setup so that they can be locked.</summary>
		protected void EnsureChildren() {
			if (IsEmpty)
				children = new List<FileItemBase>(4);
		}

		/// <summary>Ensures the children are setup so that they can be locked.</summary>
		/// 
		/// <param name="capacity">The capacity of the new list.</param>
		protected void EnsureChildren(int capacity) {
			if (IsEmpty)
				children = new List<FileItemBase>(capacity);
		}

		/// <summary>
		/// Ensures the children are assigned to <see cref="FileItemBase.EmptyChildren"/> if nothing is
		/// contained in them.
		/// </summary>
		protected void EnsureEmptyChildren() {
			if (!IsEmpty) {
				lock (children) {
					if (children.Count == 0)
						children = EmptyChildren;
				}
			}
		}

		#endregion

		#region Validation
		
		/// <summary>
		/// Marks the folder as <see cref="FileItemBase.IsDone"/>. Which means all subitems have been
		/// scanned.
		/// </summary>
		public void Finish() {
			IsDone = true;
			if (IsWatched)
				RaiseChanged(FileItemAction.Done);
		}

		/// <summary>Fully validates the file item tree from this folder and above.</summary>
		public void UpdwardsFullValidate() {
			// Something to validate
			if (!IsEmpty) {
				ValidateImpl(true);
				lock (children) {
					children.Sort();
					children.Minimize();
				}
			}
			Parent?.FullValidateImpl();
		}

		/// <summary>
		/// Upwards invalidates this folder and every ancestor and marks it as needing validation in the
		/// future.
		/// </summary>
		protected void Invalidate() {
			if (!IsInvalidated) {
				// This is automatically set inside the same lock when IsInvalidated is set to true
				//IsDone = false;
				IsInvalidated = true;
				if (IsWatched)
					RaiseChanged(FileItemAction.Invalidated);
				Parent?.Invalidate();
			}
		}

		/// <summary>
		/// Validates the container and all invalidated children.<para/>
		/// For use when scanning is still underway, but the UI wants to be updated.
		/// </summary>
		private protected bool ValidateImpl(bool force) {
			if (!IsInvalidated || (IsValidating && !force))
				return false;
			while (IsValidating)
				Thread.Sleep(1);

			// This is automatically set inside the same lock when IsValidating is set to true
			//IsInvalidated = false;
			IsValidating = true;

			if (IsEmpty) {
				// If we're validating and empty, it means something was removed
				Size = 0;
				if (IsFileType) {
					DirectoryInfo directoryInfo = new DirectoryInfo(FullName);
					// Use the directory's LastWriteTime when empty
					LastWriteTimeUtc = directoryInfo.LastWriteTimeUtc;
				}
				else {
					LastWriteTimeUtc = DateTime.MinValue;
				}
				if (IsWatched)
					RaiseChanged(FileItemAction.ValidatedSortOrder);
				IsValidating = false;
				return true;
			}

			// True, if any of the sort orders that can change, have changed
			bool sortOrderChanged = false;
			long oldSize = Size;
			DateTime oldLastChangeTime = LastWriteTimeUtc;
			bool thisIsDone = IsDone;
			bool isDone = true;
			
			lock (children) {
				UnknownItem unknown = null;
				FileItemBase freeSpace = null;
				FileCount = 0;
				SubdirCount = 0;
				Size = 0;
				int count = children.Count;
				for (int i = 0; i < count; i++) {
					FileItemBase child = children[i];
					if (child.Type == FileItemType.Unknown) {
						// Don't count the size for this because it will 
						// be updated based on the remaining size.
						unknown = (UnknownItem) child;
						continue;
					}
					else if (child.Type == FileItemType.FreeSpace) {
						// Don't count the size for this because it will 
						// be excluded when updating unknown's size.
						freeSpace = child;
						continue;
					}
					if (child is FolderItem childFolder) {
						sortOrderChanged |= childFolder.ValidateImpl(force);
						if (!thisIsDone && isDone)
							isDone = childFolder.IsDone;
					}
					Size += child.Size;
					//LastAccessTime = Max(LastAccessTime, child.LastAccessTime);
					LastWriteTimeUtc = MaxDateTime(LastWriteTimeUtc, child.LastWriteTimeUtc);
					if (child.Type == FileItemType.File || child.Type == FileItemType.FreeSpace) {
						FileCount++;
					}
					else {
						FileCount += child.FileCount;
						SubdirCount += child.SubdirCount;
						if (child.Type != FileItemType.FileCollection) {
							SubdirCount++;
						}
					}
				}
				if (unknown != null) {
					unknown.UpdateSize(Size);
					Size += unknown.Size;
				}
				if (freeSpace != null) {
					Size += freeSpace.Size;
				}
				sortOrderChanged |= (oldSize != Size);

				if (!thisIsDone && isDone)
					Finish();
			}

			IsValidating = false;
			if (IsWatched) {
				if (sortOrderChanged)
					RaiseChanged(FileItemAction.ValidatedSortOrder);
				else
					RaiseChanged(FileItemAction.Validated);
			}
			return sortOrderChanged || (oldLastChangeTime != LastWriteTimeUtc);
		}

		/// <summary>Fully validates the file item tree and prepares it for use.</summary>
		private protected void FullValidateImpl() {
			// Nothing to validate
			if (IsEmpty)
				return;

			lock (children) {
				int count = children.Count;
				for (int i = 0; i < count; i++) {
					FileItemBase child = children[i];
					if (child is FolderItem folder)
						folder.FullValidateImpl();
				}
				children.Sort();
				children.Minimize();
			}
		}

		/// <summary>Gets the maximum of two <see cref="DateTime"/>s.</summary>
		private static DateTime MaxDateTime(DateTime a, DateTime b) {
			return (a > b ? a : b);
		}

		#endregion

		#region ToString/DebuggerDisplay

		/// <summary>Gets the string representation of this item.</summary>
		public override string ToString() {
			if (Type == FileItemType.FileCollection)
				return $"{Type}[{children.Count:N0}]";
			return $"{Type}[{children.Count:N0}]: {Name}";
		}

		/// <summary>Gets the string used to represent the file in the debugger.</summary>
		private string DebuggerDisplay {
			get {
				if (Type == FileItemType.FileCollection)
					return $"[{children.Count:N0}]: <Files>";
				return $"[{children.Count:N0}]: {Name}";
			}
		}

		#endregion

		#region Override Methods
		
		/// <summary>Refreshes the file. Returns true if it still exists.</summary>
		/// 
		/// <returns>True if the file still exists.</returns>
		public override bool Refresh() {
			DirectoryInfo info = new DirectoryInfo(FullName);
			// Remove all of our children first, they will be repopulated
			ClearItems();
			if (info.Exists && info.Attributes.HasFlag(FileAttributes.Directory)) {
				//CaseSensitive = DirectoryCaseSensitivity.IsCaseSensitive(info.FullName);
				Attributes = info.Attributes;
				Size = 0L;
				LastWriteTimeUtc = info.LastWriteTimeUtc;

				return RefreshFinal(true);
			}
			return RefreshFinal(false);
		}

		/// <summary>Checks if the file still exists.</summary>
		/// 
		/// <returns>True if the file exists.</returns>
		public override bool CheckExists() {
			if (Type == FileItemType.FileCollection)
				return true;
			
			if (Directory.Exists(FullName)) {
				return CheckExistsFinal(true);
			}
			RecurseClearChildren();
			return CheckExistsFinal(false);
		}

		#endregion

		#region Static Helpers

		/// <summary>
		/// Removes all files in the enumeration whose ancestors are also in the enumeration.
		/// </summary>
		/// 
		/// <returns>The enumeration with ancestor conflicts removed.</returns>
		public static FileItemBase[] IsolateAncestores(IEnumerable<FileItemBase> files) {
			// TODO: This can probably be optimized more
			List<FileItemBase> fileList = files.ToList();
			for (int i = 0; i < fileList.Count; i++) {
				FileItemBase file = fileList[i];
				bool hasAncestor = false;
				FolderItem ancestor = file.FileParent;
				while (ancestor != null && !hasAncestor) {
					for (int j = 0; j < fileList.Count; j++) {
						if (ancestor == fileList[j]) {
							hasAncestor = true;
							fileList.RemoveAt(i);
							i--;
							break;
						}
					}
					ancestor = ancestor.FileParent;
				}
			}
			return fileList.ToArray();
		}

		/*/// <summary>Gets if the file contains the specified ancestor.</summary>
		/// 
		/// <param name="file">The file to check the ancestors of.</param>
		/// <param name="ancestor">The ancestor to check with.</param>
		public bool ContainsAncestor(FileItemBase file, FolderItem ancestor) {

		}*/

		/// <summary>Gets the ancestores for the file from bottom to top.</summary>
		/// 
		/// <param name="file">The file to get the ancestores of.</param>
		/// <returns>The ancestors of the file.</returns>
		public static IEnumerable<FolderItem> GetAncestores(FileItemBase file) {
			FolderItem parent = file.Parent;
			while (parent != null) {
				yield return parent;
				parent = parent.Parent;
			}
		}

		#endregion
	}
}

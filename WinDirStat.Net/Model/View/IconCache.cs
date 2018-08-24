using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WinDirStat.Net.Utils;
using static WinDirStat.Net.Utils.Native.Win32;

namespace WinDirStat.Net.Model.View {
	public delegate void CacheIconCallback(ImageSource icon);
	public delegate void CacheIconAndNameCallback(ImageSource icon, string name);

	public struct IconAndName {
		public ImageSource Icon { get; }
		public string Name { get; }

		public IconAndName(ImageSource icon, string name) {
			Icon = icon;
			Name = name;
		}
	}

	public partial class IconCache : ObservableObject, IReadOnlyList<ImageSource>, INotifyCollectionChanged {
		
		private const SHFileInfoFlags FlagDefaults =
			SHFileInfoFlags.SysIconIndex | SHFileInfoFlags.SmallIcon;

		private const SHFileInfoFlags FileTypeFlags = SHFileInfoFlags.UseFileAttributes;

		//private static ImageSource unknownFileIcon;
		private static ImageSource fileCollectionIcon;
		private static ImageSource freeSpaceIcon;
		private static ImageSource unknownSpaceIcon;
		private static ImageSource fileIcon;
		private static ImageSource folderIcon;
		private static ImageSource volumeIcon;
		private static ImageSource shortcutIcon;

		private static ImageSource LoadResource(string name) {
			return BitmapUtils.FromResource($"Resources/FileIcons/{name}.png");
		}

		public static ImageSource FileCollectionIcon {
			get => fileCollectionIcon ?? (fileCollectionIcon = LoadResource("FileCollection"));
		}
		public static ImageSource FreeSpaceIcon {
			get => freeSpaceIcon ?? (freeSpaceIcon = LoadResource("FreeSpace"));
		}
		public static ImageSource UnknownSpaceIcon {
			get => unknownSpaceIcon ?? (unknownSpaceIcon = LoadResource("UnknownSpace"));
		}

		public static ImageSource FileIcon {
			get => fileIcon ?? (fileIcon = LoadStockIcon(SHStockIconID.DocNoAssoc));
		}
		public static ImageSource FolderIcon {
			get => folderIcon ?? (folderIcon = LoadStockIcon(SHStockIconID.Folder));
		}
		public static ImageSource VolumeIcon {
			get => volumeIcon ?? (volumeIcon = LoadStockIcon(SHStockIconID.DriveFixed));
		}
		public static ImageSource ShortcutIcon {
			get => shortcutIcon ?? (shortcutIcon = LoadStockIcon(SHStockIconID.Link));
		}

		private readonly WinDirStatViewModel viewModel;
		private readonly Dictionary<int, ImageSource> cachedIcons;
		//private readonly Dictionary<string, IconAndName> cachedFileTypes;
		private readonly ObservableCollection<ImageSource> cachedIconList;
		//private readonly Queue<ICacheTask> cacheTasks;
		//private readonly DispatcherTimer queueTimer;
		//private ImageSource defaultDriveIcon;
		//private ImageSource defaultFolderIcon;
		//private ImageSource defaultFileIcon;
		
		public static ResourceKey ShortcutIconKey { get; } =
			new ComponentResourceKey(typeof(IconCache), "ShortcutIconKey");

		static IconCache() {
			Application.Current.Resources.Add(ShortcutIconKey, ShortcutIcon);
		}

		public IconCache(WinDirStatViewModel viewModel) {
			this.viewModel = viewModel;
			cachedIcons = new Dictionary<int, ImageSource>();
			cachedIconList = new ObservableCollection<ImageSource>();
			//cacheTasks = new Queue<ICacheTask>();
			/*queueTimer = new DispatcherTimer(
				TimeSpan.FromSeconds(0.1),
				DispatcherPriority.Normal,
				OnQueueTick,
				Application.Current.Dispatcher);
			queueTimer.Stop();*/
		}

		private void OnQueueTick(object sender, EventArgs e) {
			//Stopwatch sw = Stopwatch.StartNew();
			/*ICacheTask task;
			for (int i = 0; i < 5; i++) {
				lock (cacheTasks) {
					if (cacheTasks.Count == 0)
						break;
					task = cacheTasks.Dequeue();
				}
				task.Run(this);
			}
			lock (cacheTasks) {
				if (cacheTasks.Count == 0)
					queueTimer.Stop();
			}*/
			//Console.WriteLine($"OnQueueTick took: {sw.ElapsedMilliseconds}ms");
		}

		public WinDirStatViewModel ViewModel {
			get => viewModel;
		}

		public int Count {
			get => cachedIconList.Count;
		}

		public void ClearCache() {
			cachedIcons.Clear();
			cachedIconList.Clear();
		}

		/*private void ClearQueue() {
			cacheTasks.Clear();
		}

		private void QueueCacheTask(ICacheTask task) {
			lock (cacheTasks) {
				cacheTasks.Enqueue(task);
				if (cacheTasks.Count == 1)
					queueTimer.Start();
			}
		}*/

		public ImageSource CacheSpecialFolder(Environment.SpecialFolder folder) {
			return CacheSpecialFolder(folder, SHFileInfoFlags.None, out _);
		}

		public ImageSource CacheSpecialFolder(Environment.SpecialFolder folder, out string name) {
			return CacheSpecialFolder(folder, SHFileInfoFlags.DisplayName, out name);
		}

		public void CacheSpecialFolderAsync(Environment.SpecialFolder folder, CacheIconCallback callback) {
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			Application.Current.Dispatcher.BeginInvoke(new Action(() => {
				ImageSource icon = CacheSpecialFolder(folder);
				callback(icon);
			}), DispatcherPriority.Background);
		}

		public void CacheSpecialFolderAsync(Environment.SpecialFolder folder,
			CacheIconAndNameCallback callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			//QueueCacheTask(new SpecialFolderCacheTask(folder, callback));
			Application.Current.Dispatcher.BeginInvoke(new Action(() => {
				ImageSource icon = CacheSpecialFolder(folder, out string name);
				callback(icon, name);
			}), DispatcherPriority.Background);
		}

		public ImageSource CacheIcon(string path, FileAttributes attributes, out string name) {
			return CacheIcon(path, attributes, SHFileInfoFlags.DisplayName, out name, out _);
		}

		public ImageSource CacheIcon(string path, FileAttributes attributes) {
			return CacheIcon(path, attributes, SHFileInfoFlags.None, out _, out _);
		}

		public void CacheIconAsync(string path, FileAttributes attributes, CacheIconCallback callback) {
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			//QueueCacheTask(new IconCacheTask(path, attributes, callback));
			Application.Current.Dispatcher.BeginInvoke(new Action(() => {
				ImageSource icon = CacheIcon(path, attributes);
				callback(icon);
			}), DispatcherPriority.Background);
		}

		public void CacheIconAsync(string path, FileAttributes attributes, CacheIconAndNameCallback callback) {
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			//QueueCacheTask(new IconAndDisplayNameCacheTask(path, attributes, callback));
			Application.Current.Dispatcher.BeginInvoke(new Action(() => {
				ImageSource icon = CacheIcon(path, attributes, out string name);
				callback(icon, name);
			}), DispatcherPriority.Background);
		}

		public ImageSource CacheFileType(string extension) {
			return CacheIcon(extension, FileTypeFlags, out _, out _);
		}

		public ImageSource CacheFileType(string extension, out string name) {
			return CacheIcon(extension, FileTypeFlags | SHFileInfoFlags.TypeName, out _, out name);
		}

		public void CacheFileTypeAsync(string extension, CacheIconCallback callback) {
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			Application.Current.Dispatcher.BeginInvoke(new Action(() => {
				ImageSource icon = CacheFileType(extension);
				callback(icon);
			}), DispatcherPriority.Background);
		}
		
		public void CacheFileTypeAsync(string extension, CacheIconAndNameCallback callback) {
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			//QueueCacheTask(new FileTypeCacheTask(extension, callback));
			Application.Current.Dispatcher.BeginInvoke(new Action(() => {
				ImageSource icon = CacheFileType(extension, out string name);
				callback(icon, name);
			}), DispatcherPriority.Background);
		}

		private static ImageSource LoadStockIcon(SHStockIconID id, SHStockIconFlags flags = 0) {
			flags |= SHStockIconFlags.Icon | SHStockIconFlags.SmallIcon;
			SHStockIconInfo stockInfo = new SHStockIconInfo() {
				cbSize = SHStockIconInfo.CBSize,
			};
			if (SHGetStockIconInfo(id, flags, ref stockInfo))
				return null;
			try {
				return ExtractIcon(stockInfo.hIcon);
			}
			finally {
				DestroyIcon(stockInfo.hIcon);
			}
		}

		private ImageSource CacheStockIcon(SHStockIconID id, SHStockIconFlags flags = 0) {
			flags |= SHStockIconFlags.SysIconIndex | SHStockIconFlags.Icon | SHStockIconFlags.SmallIcon;
			ImageSource icon = null;
			SHStockIconInfo stockInfo = new SHStockIconInfo {
				cbSize = SHStockIconInfo.CBSize,
			};
			if (SHGetStockIconInfo(id, flags, ref stockInfo))
				return null;
			try {
				if (!cachedIcons.TryGetValue(stockInfo.iSysIconIndex, out icon)) {
					try {
						Debug.WriteLine($"Cache[{Count}]: " + id);
						icon = ExtractIcon(stockInfo.hIcon);
					}
					catch {
						Debug.WriteLine($"Failed to load icon for \"{id}\"!");
					}
					RaisePropertyChanged("Count");
					cachedIcons.Add(stockInfo.iSysIconIndex, icon);
					cachedIconList.Add(icon);
				}
			}
			finally {
				DestroyIcon(stockInfo.hIcon);
			}
			return icon;
		}

		private ImageSource CacheSpecialFolder(Environment.SpecialFolder folder, SHFileInfoFlags flags,
			out string displayName)
		{
			displayName = null;
			flags |= FlagDefaults | SHFileInfoFlags.PIDL;

			IntPtr pidl = IntPtr.Zero;
			ImageSource icon = null;
			SHFileInfo fileInfo = new SHFileInfo();
			if (SHGetSpecialFolderLocation(IntPtr.Zero, folder, ref pidl))
				return null;
			IntPtr hImageList = SHGetFileInfo(pidl, 0, ref fileInfo, SHFileInfo.CBSize, flags);
			try {
				if (hImageList == IntPtr.Zero)
					return null;

				if (flags.HasFlag(SHFileInfoFlags.DisplayName))
					displayName = fileInfo.szDisplayName;

				if (!cachedIcons.TryGetValue(fileInfo.iIcon, out icon)) {
					try {
						Debug.WriteLine($"Cache[{Count}]: " + pidl);
						icon = ExtractIcon(hImageList, fileInfo.iIcon);
					}
					catch {
						Debug.WriteLine($"Failed to load icon for \"{pidl}\"!");
					}
					RaisePropertyChanged("Count");
					cachedIcons.Add(fileInfo.iIcon, icon);
					cachedIconList.Add(icon);
				}
			}
			finally {
				ImageList_Destroy(hImageList);
				Marshal.FreeCoTaskMem(pidl);
			}
			return icon;
		}

		private ImageSource CacheIcon(string path, SHFileInfoFlags flags, out string displayName,
			out string typeName)
		{
			return CacheIcon(path, 0, flags, out displayName, out typeName);
		}

		private ImageSource CacheIcon(string path, FileAttributes attributes, SHFileInfoFlags flags,
			out string displayName, out string typeName)
		{
			displayName = null;
			typeName = null;
			flags |= FlagDefaults;

			ImageSource icon = null;
			SHFileInfo fileInfo = new SHFileInfo();
			IntPtr hImageList = SHGetFileInfo(path, 0, ref fileInfo, SHFileInfo.CBSize, flags);
			try {
				if (hImageList == IntPtr.Zero)
					return null;

				if (flags.HasFlag(SHFileInfoFlags.DisplayName))
					displayName = fileInfo.szDisplayName;
				if (flags.HasFlag(SHFileInfoFlags.TypeName))
					typeName = fileInfo.szTypeName;

				if (!cachedIcons.TryGetValue(fileInfo.iIcon, out icon)) {
					try {
						Debug.WriteLine($"Cache[{Count}]: " + path);
						icon = ExtractIcon(hImageList, fileInfo.iIcon);
					}
					catch {
						Debug.WriteLine($"Failed to load icon for \"{path}\"!");
					}
					RaisePropertyChanged("Count");
					cachedIcons.Add(fileInfo.iIcon, icon);
					cachedIconList.Add(icon);
				}
			}
			finally {
				ImageList_Destroy(hImageList);
			}
			return icon;
		}

		private static ImageSource ExtractIcon(IntPtr hIcon) {
			using (Icon icon = Icon.FromHandle(hIcon))
			using (Bitmap bitmap = icon.ToBitmap())
				return BitmapUtils.FromGdiBitmap(bitmap);
		}

		private static ImageSource ExtractIcon(IntPtr hImageList, int index) {
			IntPtr hIcon = ImageList_GetIcon(hImageList, index, ImageListDrawFlags.Normal);
			try {
				return ExtractIcon(hIcon);
			}
			finally {
				if (hIcon != IntPtr.Zero)
					DestroyIcon(hIcon);
			}
		}
		/*private static ImageSource GetRegisteredIcon(string filePath) {
			IntPtr hIcon = IntPtr.Zero;
			try {
				hIcon = GetRegisteredHIcon(filePath);
				using (Icon icon = Icon.FromHandle(hIcon))
				using (Bitmap bitmap = icon.ToBitmap())
					return BitmapUtils.FromGdiBitmap(bitmap);
			}
			finally {
				if (hIcon != IntPtr.Zero)
					DestroyIcon(hIcon);
			}
		}*/

		private static IntPtr GetRegisteredHIcon(string filePath) {
			const SHFileInfoFlags Flags = SHFileInfoFlags.Icon | SHFileInfoFlags.SmallIcon;
			var shinfo = new SHFileInfo();
			SHGetFileInfo(filePath, 0, ref shinfo, SHFileInfo.CBSize, Flags);
			return shinfo.hIcon;
		}
		
		public ImageSource this[int index] {
			get => cachedIconList[index];
		}

		public IEnumerator<ImageSource> GetEnumerator() {
			return cachedIconList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return cachedIconList.GetEnumerator();
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged {
			add => cachedIconList.CollectionChanged += value;
			remove => cachedIconList.CollectionChanged -= value;
		}

		/*public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}*/
	}
}

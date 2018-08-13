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

namespace WinDirStat.Net.Data {
	public delegate void CacheIconCallback(ImageSource icon);
	public delegate void CacheFileTypeCallback(ImageSource icon, string name);

	public class IconCache : IReadOnlyList<ImageSource>, INotifyCollectionChanged {

		private interface ICacheTask {
			void Run(IconCache icons);
		}

		private class IconCacheTask : ICacheTask {
			public string Path { get; }
			public string FileName {
				get => System.IO.Path.GetFileName(Path);
			}
			public FileAttributes Attributes { get; }
			public CacheIconCallback Callback { get; }

			public IconCacheTask(string path, FileAttributes attributes, CacheIconCallback callback) {
				Path = path;
				Attributes = attributes;
				Callback = callback;
			}

			public void Run(IconCache icons) {
				ImageSource icon = icons.CacheIcon(Path, Attributes);
				Callback(icon);
			}

			public override string ToString() {
				return $"Icon: {FileName}";
			}
		}

		private class FileTypeCacheTask : ICacheTask {
			public string Extension { get; }
			public CacheFileTypeCallback Callback { get; }

			public FileTypeCacheTask(string extension, CacheFileTypeCallback callback) {
				Extension = extension;
				Callback = callback;
			}

			public void Run(IconCache icons) {
				ImageSource icon = icons.CacheFileType(Extension, out string name);
				Callback(icon, name);
			}

			public override string ToString() {
				return $"File Type: {Extension}";
			}
		}

		private const SHFileInfoFlags FlagDefaults =
			SHFileInfoFlags.SysIconIndex | SHFileInfoFlags.SmallIcon;// | SHFileInfoFlags.Icon | SHFileInfoFlags.SmallIcon;

		private const SHFileInfoFlags FileTypeFlags = SHFileInfoFlags.UseFileAttributes;

		private static ImageSource unknownFileIcon;
		private static ImageSource fileCollectionIcon;
		private static ImageSource freeSpaceIcon;
		private static ImageSource unknownSpaceIcon;

		public static ImageSource UnknownFileIcon {
			get {
				if (unknownFileIcon == null)
					unknownFileIcon = BitmapUtils.FromResource("Resources/FileIcons/UnknownFile.png");
				return unknownFileIcon;
			}
		}

		public static ImageSource FileCollectionIcon {
			get {
				if (fileCollectionIcon == null)
					fileCollectionIcon = BitmapUtils.FromResource("Resources/FileIcons/FileCollection.png");
				return fileCollectionIcon;
			}
		}

		public static ImageSource FreeSpaceIcon {
			get {
				if (freeSpaceIcon == null)
					freeSpaceIcon = BitmapUtils.FromResource("Resources/FileIcons/FreeSpace.png");
				return freeSpaceIcon;
			}
		}

		public static ImageSource UnknownSpaceIcon {
			get {
				if (unknownSpaceIcon == null)
					unknownSpaceIcon = BitmapUtils.FromResource("Resources/FileIcons/UnknownSpace.png");
				return unknownSpaceIcon;
			}
		}

		private readonly WinDirDocument document;
		private readonly Dictionary<int, ImageSource> cachedIcons;
		private readonly ObservableCollection<ImageSource> cachedIconList;
		private readonly Queue<ICacheTask> cacheTasks;
		//private ImageSource defaultDriveIcon;
		private ImageSource defaultFolderIcon;
		private ImageSource defaultFileIcon;
		

		public IconCache(WinDirDocument document) {
			this.document = document;
			cachedIcons = new Dictionary<int, ImageSource>();
			cachedIconList = new ObservableCollection<ImageSource>();
			cacheTasks = new Queue<ICacheTask>();
		}

		public WinDirDocument Document {
			get => document;
		}

		public int Count {
			get => cachedIconList.Count;
		}

		public void ClearCache() {
			cachedIcons.Clear();
			cachedIconList.Clear();
		}

		public ImageSource CacheIcon(string path, FileAttributes attributes) {
			return CacheIcon(path, attributes, SHFileInfoFlags.None, false, out _);
		}

		public void CacheIconAsync(string path, FileAttributes attributes, CacheIconCallback callback) {
			/*if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			Application.Current.Dispatcher.BeginInvoke(new Action(() => {
				ImageSource icon = CacheIcon(path, attributes);
				callback(icon);
			}), DispatcherPriority.Background);*/
		}

		public ImageSource CacheFileType(string extension) {
			return CacheIcon(extension, FileTypeFlags, false, out _);
		}

		public ImageSource CacheFileType(string extension, out string name) {
			return CacheIcon(extension, FileTypeFlags, true, out name);
		}

		public void CacheFileTypeAsync(string extension, CacheIconCallback callback) {
			/*Application.Current.Dispatcher.BeginInvoke(new Action(() => {
				ImageSource icon = CacheFileType(extension);
				callback(icon);
			}), DispatcherPriority.Background);*/
		}
		
		public void CacheFileTypeAsync(string extension, CacheFileTypeCallback callback) {
			/*Application.Current.Dispatcher.BeginInvoke(new Action(() => {
				ImageSource icon = CacheFileType(extension, out string name);
				callback(icon, name);
			}), DispatcherPriority.Background);*/
		}

		public ImageSource DefaultDriveIcon {
			get {
				if (defaultFolderIcon == null) {
					string tmpDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
					Application.Current.Dispatcher.Invoke(() => {
						defaultFolderIcon = GetRegisteredIcon(tmpDir);
					});
					try {
						Directory.Delete(tmpDir);
					}
					catch { }
				}
				return defaultFolderIcon;
			}
		}

		public ImageSource DefaultFolderIcon {
			get {
				if (defaultFolderIcon == null) {
					string tmpDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
					Application.Current.Dispatcher.Invoke(() => {
						defaultFolderIcon = GetRegisteredIcon(tmpDir);
					});
					try {
						Directory.Delete(tmpDir);
					}
					catch { }
				}
				return defaultFolderIcon;
			}
		}

		public ImageSource DefaultFileIcon {
			get {
				if (defaultFileIcon == null) {
					string tmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
					using (File.Create(tmpFile)) { }
					Application.Current.Dispatcher.Invoke(() => {
						defaultFileIcon = GetRegisteredIcon(tmpFile);
					});
					try {
						File.Delete(tmpFile);
					}
					catch { }
				}
				return defaultFileIcon;
			}
		}

		private ImageSource CacheIcon(string path, SHFileInfoFlags flags, bool setTypeName, out string typeName) {
			return CacheIcon(path, 0, flags, setTypeName, out typeName);
		}

		private ImageSource CacheIcon(string path, FileAttributes attributes, SHFileInfoFlags flags, bool setTypeName, out string typeName) {
			typeName = null;
			flags |= FlagDefaults;
			/*if (!attributes.HasFlag(FileAttributes.Directory) && Path.GetExtension(path).ToLower() == ".lnk") {
				flags |= SHFileInfoFlags.LinkOverlay;
			}*/
			if (setTypeName) {
				flags |= SHFileInfoFlags.TypeName;
			}
			//if (attributes != 0)
			//	flags |= SHFileInfoFlags.UseFileAttributes;
			ImageSource icon = null;
			SHFileInfo fileInfo = new SHFileInfo();
			IntPtr hImageList = hImageList = SHGetFileInfo(path, 0, ref fileInfo, SHFileInfo.CBSize, flags);
			try {
				if (hImageList == IntPtr.Zero) {
					return null;
				}

				if (setTypeName) {
					typeName = fileInfo.szTypeName;
				}

				if (!cachedIcons.TryGetValue(fileInfo.iIcon, out icon)) {
					try {
						Debug.WriteLine($"Cache[{Count}]: " + path);
						icon = ExtractIcon(hImageList, fileInfo.iIcon);
					}
					catch {
						Debug.WriteLine($"Failed to load icon for \"{path}\"!");
					}
					cachedIcons.Add(fileInfo.iIcon, icon);
					cachedIconList.Add(icon);
				}
			}
			finally {
				ImageList_Destroy(hImageList);
				//DestroyIcon(fileInfo.hIcon);
			}
			return icon;
		}

		private ImageSource ExtractIcon(IntPtr hImageList, int index) {
			IntPtr hIcon = IntPtr.Zero;
			try {
				hIcon = ImageList_GetIcon(hImageList, index, ImageListDrawFlags.Normal);
				using (Icon icon = Icon.FromHandle(hIcon))
				using (Bitmap bitmap = icon.ToBitmap())
					return BitmapUtils.FromGdiBitmap(bitmap);
			}
			finally {
				if (hIcon != IntPtr.Zero)
					DestroyIcon(hIcon);
			}
		}
		private ImageSource GetRegisteredIcon(string filePath) {
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
		}

		private static IntPtr GetRegisteredHIcon(string filePath) {
			const SHFileInfoFlags Flags = SHFileInfoFlags.Icon | SHFileInfoFlags.SmallIcon;
			var shinfo = new SHFileInfo();
			SHGetFileInfo(filePath, 0, ref shinfo, SHFileInfo.CBSize, Flags);
			return shinfo.hIcon;
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool DestroyIcon(IntPtr hIcon);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern bool DeleteObject(IntPtr hObject);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern uint GetSystemDirectory(out string buffer, int size);

		[StructLayout(LayoutKind.Sequential)]
		private struct SHFileInfo {
			/// <summary>The marshaled size of the struct.</summary>
			public static readonly int CBSize = Marshal.SizeOf<SHFileInfo>();

			/// <summary>
			/// A handle to the icon that represents the file. You are responsible for destroying this handle
			/// with DestroyIcon when you no longer need it.
			/// </summary>
			public IntPtr hIcon;
			/// <summary>The index of the icon image within the system image list.</summary>
			public int iIcon;
			/// <summary>
			/// An array of values that indicates the attributes of the file object. For information about
			/// these values, see the IShellFolder::GetAttributesOf method.
			/// </summary>
			public uint dwAttributes;
			/// <summary>
			/// A string that contains the name of the file as it appears in the Windows Shell, or the path
			/// and file name of the file that contains the icon representing the file.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			/// <summary>A string that describes the type of file.</summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		[Flags]
		private enum SHFileInfoFlags : uint {
			None = 0,
			LargeIcon = 0x000000000,

			SmallIcon = 0x000000001,
			OpenIcon = 0x000000002,
			ShellIconSize = 0x000000004,
			PIDL = 0x000000008,

			UseFileAttributes = 0x000000010,
			AddOverlays = 0x000000020,
			OverlayIndex = 0x000000040,

			Icon = 0x000000100,
			DisplayName = 0x000000200,
			TypeName = 0x000000400,
			Attributes = 0x000000800,

			IconLocation = 0x000001000,
			ExeType = 0x000002000,
			SysIconIndex = 0x000004000,
			LinkOverlay = 0x000008000,

			Selected = 0x000010000,
			AttrSpecified = 0x000020000,
		}

		[DllImport("shell32.dll")]
		private static extern IntPtr SHGetFileInfo(string path, [MarshalAs(UnmanagedType.U4)] FileAttributes fileAttributes, ref SHFileInfo fileInfo, int sizeofFileInfo, [MarshalAs(UnmanagedType.U4)] SHFileInfoFlags flags);

		[DllImport("comctl32.dll", SetLastError = true)]
		private static extern int ImageList_GetImageCount(IntPtr hImageList);

		[DllImport("comctl32.dll", SetLastError = true)]
		private static extern IntPtr ImageList_Duplicate(IntPtr hImageList);

		[DllImport("comctl32.dll", SetLastError = true)]
		private static extern IntPtr ImageList_Destroy(IntPtr hImageList);

		[DllImport("comctl32.dll", SetLastError = true)]
		private static extern int ImageList_Add(IntPtr hImageList, IntPtr image, IntPtr mask);

		[DllImport("comctl32.dll", SetLastError = true)]
		private static extern IntPtr ImageList_ExtractIcon(IntPtr hInstance, IntPtr hImageList, int i);

		[DllImport("comctl32.dll", SetLastError = true)]
		private static extern IntPtr ImageList_GetIcon(IntPtr hImageList, int i, [MarshalAs(UnmanagedType.U4)] ImageListDrawFlags flags);
		
		[Flags]
		private enum ImageListDrawFlags : uint {
			Normal = 0x00000000,
			Transparent = 0x00000001,
			Blend25 = 0x00000002,
			Focus = 0x00000002,
			Blend50 = 0x00000004,
			Selected = 0x00000004,
			Blend = 0x00000004,
			Mask = 0x00000010,
			Image = 0x00000020,
			Rop = 0x00000040,
			OverlayMask = 0x00000F00,
			PreserveAlpha = 0x00001000,
			Scale = 0x00002000,
			DpiScale = 0x00004000,
			Async = 0x00008000,
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
	}
}

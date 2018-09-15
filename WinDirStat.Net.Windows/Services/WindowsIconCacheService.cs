using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WinDirStat.Net.Model.Extensions;
using WinDirStat.Net.Services;
using static WinDirStat.Net.Native.Win32;

namespace WinDirStat.Net.Windows.Services {
	/// <summary>A service for caching file and folder icons.</summary>
	public class WindowsIconCacheService {

		#region Constants

		private const SHFileInfoFlags CacheIconFlags =
			SHFileInfoFlags.SysIconIndex | SHFileInfoFlags.SmallIcon;

		private const SHFileInfoFlags CacheFileTypeFlags = CacheIconFlags |
			SHFileInfoFlags.UseFileAttributes | SHFileInfoFlags.TypeName;

		private const SHFileInfoFlags CacheSpecialFolderFlags = CacheIconFlags |
			SHFileInfoFlags.PIDL;

		private const SHStockIconFlags CacheStockIconFlags =
			SHStockIconFlags.SysIconIndex | SHStockIconFlags.Icon | SHStockIconFlags.SmallIcon;

		#endregion

		#region Fields
		
		/// <summary>The default file icon.</summary>
		private IBitmap fileIcon;
		/// <summary>The default folder icon.</summary>
		private IBitmap folderIcon;
		/// <summary>The default drive icon.</summary>
		private IBitmap volumeIcon;
		/// <summary>The default shortcut icon.</summary>
		private IBitmap shortcutIcon;

		/// <summary>The service for performing UI actions such as dispatcher invoking.</summary>
		private readonly IUIService ui;
		/// <summary>The service for creating and loading bitmaps.</summary>
		private readonly IBitmapFactory bitmapFactory;
		private readonly Dictionary<int, IBitmap> cachedIcons;
		private readonly Dictionary<Environment.SpecialFolder, IIconAndName> cachedSpecialFolders;
		private readonly Dictionary<string, IIconAndName> cachedFileTypes;
		private readonly ObservableCollection<IBitmap> cachedIconList;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="IconCacheService"/>.</summary>
		public WindowsIconCacheService(IUIService ui,
									   IBitmapFactory bitmapFactory) {
			this.ui = ui;
			this.bitmapFactory = bitmapFactory;

			cachedIcons = new Dictionary<int, IBitmap>();
			cachedSpecialFolders = new Dictionary<Environment.SpecialFolder, IIconAndName>();
			cachedFileTypes = new Dictionary<string, IIconAndName>();
			cachedIconList = new ObservableCollection<IBitmap>();
		}

		#endregion

		#region Properties

		/// <summary>Gets the number of cached icons.</summary>
		public int Count => cachedIcons.Count;

		#endregion

		#region Icon Properties

		/// <summary>Gets the default file icon.</summary>
		public IBitmap FileIcon {
			get => fileIcon ?? (fileIcon = CacheStockIcon(SHStockIconID.DocNoAssoc));
		}
		/// <summary>Gets the default folder icon.</summary>
		public IBitmap FolderIcon {
			get => folderIcon ?? (folderIcon = CacheStockIcon(SHStockIconID.Folder));
		}
		/// <summary>Gets the default drive icon.</summary>
		public IBitmap VolumeIcon {
			get => volumeIcon ?? (volumeIcon = CacheStockIcon(SHStockIconID.DriveFixed));
		}
		/// <summary>Gets the default shortcut icon.</summary>
		public IBitmap ShortcutIcon {
			get => shortcutIcon ?? (shortcutIcon = CacheStockIcon(SHStockIconID.Link));
		}

		#endregion

		#region Icon Caching

		/// <summary>Caches the icon of the specified file.</summary>
		/// 
		/// <param name="path">The path of the file.</param>
		/// <returns>The cached icon on success, otherwise null.</returns>
		public IBitmap CacheIcon(string path) {
			return ui.Invoke(() => CacheIconImpl(path, 0, SHFileInfoFlags.None, out _, out _));
		}
		/// <summary>Asynchronousy caches the icon of the specified file.</summary>
		/// 
		/// <param name="path">The path of the file.</param>
		/// <param name="callback">The method that returns the icon upon completion.</param>
		public void CacheIconAsync(string path, ICacheIconCallback callback) {
			if (callback == null) throw new ArgumentNullException(nameof(callback));
			ui.BeginInvoke(() => callback(CacheIcon(path)), false);
		}

		/// <summary>Caches the icon and display name of the specified file.</summary>
		/// 
		/// <param name="path">The path of the file.</param>
		/// <returns>The cached icon and icon on success, otherwise null.</returns>
		public IIconAndName CacheIconAndDisplayName(string path) {
			return ui.Invoke(() => {
				IBitmap icon = CacheIconImpl(path, 0, SHFileInfoFlags.DisplayName, out string name, out _);
				if (icon != null)
					return new IIconAndName(icon, name);
				return null;
			});
		}
		/// <summary>Asynchronousy caches the icon and display name of the specified file.</summary>
		/// 
		/// <param name="path">The path of the file.</param>
		/// <param name="callback">The method that returns the icon and name upon completion.</param>
		public void CacheIconAndDisplayNameAsync(string path, ICacheIconAndNameCallback callback) {
			if (callback == null) throw new ArgumentNullException(nameof(callback));
			ui.BeginInvoke(() => callback(CacheIconAndDisplayName(path)), false);
		}

		/// <summary>Caches the file type's icon and type name.</summary>
		/// 
		/// <param name="extension">The extension of the file type.</param>
		/// <returns>The cached icon and type name on success, otherwise null.</returns>
		public IIconAndName CacheFileType(string extension) {
			return ui.Invoke(() => {
				if (!cachedFileTypes.TryGetValue(extension, out IIconAndName iconName)) {
					try {
						// Empty extensions will just return the Local Disk instead of a proper file
						string extensionPath = extension;
						if (extension == ExtensionItem.EmptyExtension)
							extensionPath = Guid.NewGuid().ToString();
						IBitmap icon = CacheIconImpl(extensionPath, FileAttributes.Normal,
							CacheFileTypeFlags, out _, out string typeName);
						if (icon != null)
							iconName = new IIconAndName(icon, typeName);
						cachedFileTypes.Add(extension, iconName);
					}
					catch { }
				}
				return iconName;
			});
		}
		/// <summary>Asynchronousy caches the file type's icon and type name.</summary>
		/// 
		/// <param name="extension">The extension of the file type.</param>
		/// <param name="callback">The method that returns the icon and type name upon completion.</param>
		public void CacheFileTypeAsync(string extension, ICacheIconAndNameCallback callback) {
			if (callback == null) throw new ArgumentNullException(nameof(callback));
			ui.BeginInvoke(() => callback(CacheFileType(extension)), false);
		}

		/// <summary>Caches the special folder's icon and display name.</summary>
		/// 
		/// <param name="folder">The special folder type.</param>
		/// <returns>The cached icon and name on success, otherwise null.</returns>
		public IIconAndName CacheSpecialFolder(Environment.SpecialFolder folder) {
			return ui.Invoke(() => {
				if (!cachedSpecialFolders.TryGetValue(folder, out IIconAndName iconName)) {
					try {
						IBitmap icon = CacheSpecialFolderImpl(folder, 0, SHFileInfoFlags.DisplayName,
							out string name, out _);
						if (icon != null)
							iconName = new IIconAndName(icon, name);
						cachedSpecialFolders.Add(folder, iconName);
					}
					catch { }
				}
				return iconName;
			});
		}
		/// <summary>Asynchronousy caches the special folder's icon and display name.</summary>
		/// 
		/// <param name="folder">The special folder type.</param>
		/// <param name="callback">The method that returns the icon and name upon completion.</param>
		public void CacheSpecialFolderAsync(Environment.SpecialFolder folder, ICacheIconAndNameCallback callback) {
			if (callback == null) throw new ArgumentNullException(nameof(callback));
			ui.BeginInvoke(() => callback(CacheSpecialFolder(folder)), false);
		}

		#endregion

		#region Private Icon Caching

		/// <summary>Caches a stock icon.</summary>
		/// 
		/// <param name="id">The id of the stock icon.</param>
		/// <returns>The icon on success, otherwise null.</returns>
		private IBitmap CacheStockIcon(SHStockIconID id) {
			return ui.Invoke(() =>  CacheStockIconImpl(id, SHStockIconFlags.None));
		}

		#endregion

		#region Cache Implementation

		/// <summary>The implementation for caching icons with a path.</summary>
		/// 
		/// <param name="path">The file path or extension.</param>
		/// <param name="attributes">The optional file attributes.</param>
		/// <param name="flags">Get flags for getting the icon.</param>
		/// <param name="displayName">
		/// The output display name if <see cref="SHFileInfoFlags.DisplayName"/> is used in <paramref name=
		/// "flags"/>.
		/// </param>
		/// <param name="typeName">
		/// The output type name if <see cref="SHFileInfoFlags.TypeName"/> is used in <paramref name=
		/// "flags"/>.
		/// </param>
		/// <returns>The icon on success, otherwise null.</returns>
		private IBitmap CacheIconImpl(string path, FileAttributes attributes, SHFileInfoFlags flags,
			out string displayName, out string typeName)
		{
			displayName = null;
			typeName = null;
			flags |= CacheIconFlags;

			IBitmap icon = null;
			SHFileInfo fileInfo = new SHFileInfo();
			IntPtr hImageList = SHGetFileInfo(path, attributes, ref fileInfo, SHFileInfo.CBSize, flags);
			try {
				if (hImageList == IntPtr.Zero)
					return null;

				if (flags.HasFlag(SHFileInfoFlags.DisplayName))
					displayName = fileInfo.szDisplayName;
				if (flags.HasFlag(SHFileInfoFlags.TypeName))
					typeName = fileInfo.szTypeName;

				icon = TryAddIconToCache(fileInfo.iIcon, () => ExtractIcon(hImageList, fileInfo.iIcon));
				if (icon != null)
					Debug.WriteLine($"Cache[{Count}]: " + path);
				else
					Debug.WriteLine($"Failed to load icon for \"{path}\"!");
			}
			finally {
				ImageList_Destroy(hImageList);
			}
			return icon;
		}

		/// <summary>The implementation for caching icons with a special folder.</summary>
		/// 
		/// <param name="folder">The special folder to get the icon of.</param>
		/// <param name="attributes">The optional file attributes.</param>
		/// <param name="flags">Get flags for getting the icon.</param>
		/// <param name="displayName">
		/// The output display name if <see cref="SHFileInfoFlags.DisplayName"/> is used in <paramref name=
		/// "flags"/>.
		/// </param>
		/// <param name="typeName">
		/// The output type name if <see cref="SHFileInfoFlags.TypeName"/> is used in <paramref name=
		/// "flags"/>.
		/// </param>
		/// <returns>The icon on success, otherwise null.</returns>
		private IBitmap CacheSpecialFolderImpl(Environment.SpecialFolder folder, FileAttributes attributes,
			SHFileInfoFlags flags, out string displayName, out string typeName)
		{
			displayName = null;
			typeName = null;
			flags |= CacheSpecialFolderFlags;

			IntPtr pidl = IntPtr.Zero;
			IBitmap icon = null;
			SHFileInfo fileInfo = new SHFileInfo();
			if (SHGetSpecialFolderLocation(IntPtr.Zero, folder, ref pidl))
				return null;
			IntPtr hImageList = SHGetFileInfo(pidl, attributes, ref fileInfo, SHFileInfo.CBSize, flags);
			try {
				if (hImageList == IntPtr.Zero)
					return null;

				if (flags.HasFlag(SHFileInfoFlags.DisplayName))
					displayName = fileInfo.szDisplayName;
				if (flags.HasFlag(SHFileInfoFlags.TypeName))
					typeName = fileInfo.szTypeName;

				icon = TryAddIconToCache(fileInfo.iIcon, () => ExtractIcon(hImageList, fileInfo.iIcon));
				if (icon != null)
					Debug.WriteLine($"Cache[{Count}]: " + pidl);
				else
					Debug.WriteLine($"Failed to load icon for \"{pidl}\"!");
			}
			finally {
				ImageList_Destroy(hImageList);
				Marshal.FreeCoTaskMem(pidl);
			}
			return icon;
		}

		/// <summary>The implementation for caching stock icons.</summary>
		/// 
		/// <param name="id">The id of the stock icon.</param>
		/// <param name="flags">The flags for caching the stock icon.</param>
		/// <returns>The icon on success, otherwise null.</returns>
		private IBitmap CacheStockIconImpl(SHStockIconID id, SHStockIconFlags flags) {
			flags |= CacheStockIconFlags;

			IBitmap icon = null;
			SHStockIconInfo stockInfo = new SHStockIconInfo {
				cbSize = SHStockIconInfo.CBSize,
			};
			if (SHGetStockIconInfo(id, flags, ref stockInfo))
				return null;
			try {
				if (!cachedIcons.TryGetValue(stockInfo.iSysIconIndex, out icon)) {
					icon = TryAddIconToCache(stockInfo.iSysIconIndex, () => ExtractIcon(stockInfo.hIcon));
					if (icon != null)
						Debug.WriteLine($"Cache[{Count}]: " + id);
					else
						Debug.WriteLine($"Failed to load icon for \"{id}\"!");
				}
			}
			finally {
				DestroyIcon(stockInfo.hIcon);
			}
			return icon;
		}

		#endregion

		#region Add/Extracting

		/// <summary>
		/// Tries to add the icon to the cache, if it does exist, <paramref name="extract"/> is called to get
		/// the icon.
		/// </summary>
		/// 
		/// <param name="systemIndex">The system index of the icon.</param>
		/// <param name="extract">The method to extract the icon.</param>
		/// <returns>The extracted icon on success, otherwise null.</returns>
		private IBitmap TryAddIconToCache(int systemIndex, Func<IBitmap> extract) {
			if (!cachedIcons.TryGetValue(systemIndex, out IBitmap icon)) {
				try {
					icon = extract();
					cachedIcons.Add(systemIndex, icon);
					cachedIconList.Add(icon);
				}
				catch { }
			}
			return icon;
		}

		/// <summary>Extracts the icon from the image list.</summary>
		/// 
		/// <param name="hImageList">The handle to the image list.</param>
		/// <param name="systemIndex">The system index of the icon.</param>
		/// <returns>The extracted icon on success, otherwise null.</returns>
		private IBitmap ExtractIcon(IntPtr hImageList, int systemIndex) {
			IntPtr hIcon = ImageList_GetIcon(hImageList, systemIndex, ImageListDrawFlags.Normal);
			try {
				return ExtractIcon(hIcon);
			}
			finally {
				if (hIcon != IntPtr.Zero)
					DestroyIcon(hIcon);
			}
		}

		/// <summary>Extracts the icon from the handle.</summary>
		/// 
		/// <param name="hIcon">The handle to the icon.</param>
		/// <returns>The extracted icon on success, otherwise null.</returns>
		protected IBitmap ExtractIcon(IntPtr hIcon) {
			return bitmapFactory.FromHIcon(hIcon);
		}

		#endregion
	}
}

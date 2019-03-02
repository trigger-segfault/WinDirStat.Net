using System;
using System.Collections.Specialized;
using System.ComponentModel;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.Services {
	/// <summary>A service for caching and looking up icons.</summary>
	public interface IIconCacheService : INotifyPropertyChanged {

		#region Properties

		/// <summary>Gets the number of cached icons.</summary>
		int Count { get; }

		#endregion

		#region Icon Properties

		/// <summary>Gets the default file icon.</summary>
		IImage FileIcon { get; }
		/// <summary>Gets the default folder icon.</summary>
		IImage FolderIcon { get; }
		/// <summary>Gets the default drive icon.</summary>
		IImage VolumeIcon { get; }
		/// <summary>Gets the default shortcut icon.</summary>
		IImage ShortcutIcon { get; }

		#endregion

		#region Icon Caching

		/// <summary>Caches the icon of the specified file.</summary>
		/// 
		/// <param name="path">The path of the file.</param>
		/// <returns>The cached icon on success, otherwise null.</returns>
		IImage CacheIcon(string path);
		/// <summary>Asynchronousy caches the icon of the specified file.</summary>
		/// 
		/// <param name="path">The path of the file.</param>
		/// <param name="callback">The method that returns the icon upon completion.</param>
		void CacheIconAsync(string path, ICacheIconCallback callback);

		/// <summary>Caches the icon and display name of the specified file.</summary>
		/// 
		/// <param name="path">The path of the file.</param>
		/// <returns>The cached icon and icon on success, otherwise null.</returns>
		IIconAndName CacheIconAndDisplayName(string path);
		/// <summary>Asynchronousy caches the icon and display name of the specified file.</summary>
		/// 
		/// <param name="path">The path of the file.</param>
		/// <param name="callback">The method that returns the icon and name upon completion.</param>
		void CacheIconAndDisplayNameAsync(string path, ICacheIconAndNameCallback callback);

		/// <summary>Caches the file type's icon and type name.</summary>
		/// 
		/// <param name="extension">The extension of the file type.</param>
		/// <returns>The cached icon and type name on success, otherwise null.</returns>
		IIconAndName CacheFileType(string extension);
		/// <summary>Asynchronousy caches the file type's icon and type name.</summary>
		/// 
		/// <param name="extension">The extension of the file type.</param>
		/// <param name="callback">The method that returns the icon and type name upon completion.</param>
		void CacheFileTypeAsync(string extension, ICacheIconAndNameCallback callback);

		/// <summary>Caches the special folder's icon and display name.</summary>
		/// 
		/// <param name="folder">The special folder type.</param>
		/// <returns>The cached icon and name on success, otherwise null.</returns>
		IIconAndName CacheSpecialFolder(Environment.SpecialFolder folder);
		/// <summary>Asynchronousy caches the special folder's icon and display name.</summary>
		/// 
		/// <param name="folder">The special folder type.</param>
		/// <param name="callback">The method that returns the icon and name upon completion.</param>
		void CacheSpecialFolderAsync(Environment.SpecialFolder folder, ICacheIconAndNameCallback callback);

		#endregion
	}
}

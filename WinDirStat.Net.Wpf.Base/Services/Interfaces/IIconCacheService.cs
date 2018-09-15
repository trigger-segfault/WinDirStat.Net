using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Services {

	/// <summary>An interface for storing an icon and an associated name.</summary>
	public interface IIconAndName {
		/// <summary>The loaded icon.</summary>
		IBitmap Icon { get; }
		/// <summary>The loaded name.</summary>
		string Name { get; }
	}

	/// <summary>The callback for caching just an icon.</summary>
	/// <param name="icon">The icon result.</param>
	public delegate void ICacheIconCallback(IBitmap icon);
	/// <summary>The callback for caching an icon and a name.</summary>
	/// <param name="iconName">The icon and name result.</param>
	public delegate void ICacheIconAndNameCallback(IIconAndName iconName);

	/// <summary>A service for caching and looking up icons.</summary>
	public interface IIconCacheService : INotifyCollectionChanged, INotifyPropertyChanged {

		IBitmap CacheIcon(string path);
		void CacheIconAsync(string path, ICacheIconCallback callback);

		IIconAndName CacheIconAndDisplayName(string path);
		void CacheIconAndDisplayNameAsync(string path, ICacheIconAndNameCallback callback);

		IIconAndName CacheFileType(string extension);
		void CacheFileTypeAsync(string extension, ICacheIconAndNameCallback callback);

		IIconAndName CacheSpecialFolder(Environment.SpecialFolder folder);
		void CacheSpecialFolderAsync(Environment.SpecialFolder folder, ICacheIconAndNameCallback callback);

		/// <summary>Gets the total number of cached icons.</summary>
		int Count { get; }
	}
}

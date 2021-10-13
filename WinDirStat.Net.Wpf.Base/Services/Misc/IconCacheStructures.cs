using System;
using System.ComponentModel;
using System.Windows.Media;

namespace WinDirStat.Net.Services {
    /// <summary>How icons are stored and displayed.</summary>
    [Serializable]
	public enum IconCacheMode : byte {
		/// <summary>Don't show icons.</summary>
		[Description("Don't show icons")]
		None,
		/// <summary>Use default icons (File/Folder/Drive/Computer).</summary>
		[Description("Use default icons")]
		Basic,
		/// <summary>Use file type icons.</summary>
		[Description("Use file type icons")]
		FileType,
		/// <summary>Use individual icons.</summary>
		[Description("Individual icons")]
		Individual,
		/// <summary>Use individual icons with overlays.</summary>
		//[Description("Individual icons with overlays")]
		//IndividualOverlays,
	}

	/// <summary>The states for caching an icon.</summary>
	public enum IconCacheState : byte {
		/// <summary>The icon has not been cached yet.</summary>
		NotCached,
		/// <summary>An asynchronous operation is running to cache the icon.</summary>
		Caching,
		/// <summary>The icon has been cached at its highest level.</summary>
		Cached,
	}

	/// <summary>A structure containing both a cached icon and name.</summary>
	public record class IconAndName(ImageSource Icon, string Name);

	/// <summary>The callback delegate for caching an icon.</summary>
	/// 
	/// <param name="icon">The returned icon.</param>
	public delegate void CacheIconCallback(ImageSource icon);
	/// <summary>The callback delegate for caching an icon and name.</summary>
	/// 
	/// <param name="iconName">The returned icon and name.</param>
	public delegate void CacheIconAndNameCallback(IconAndName iconName);
}

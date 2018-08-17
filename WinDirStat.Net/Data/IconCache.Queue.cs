using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WinDirStat.Net.Data {
	public partial class IconCache : IReadOnlyList<ImageSource>, INotifyCollectionChanged {
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

		private class IconAndDisplayNameCacheTask : ICacheTask {
			public string Path { get; }
			public string FileName {
				get => System.IO.Path.GetFileName(Path);
			}
			public FileAttributes Attributes { get; }
			public CacheIconAndNameCallback Callback { get; }

			public IconAndDisplayNameCacheTask(string path, FileAttributes attributes,
				CacheIconAndNameCallback callback) {
				Path = path;
				Attributes = attributes;
				Callback = callback;
			}

			public void Run(IconCache icons) {
				ImageSource icon = icons.CacheIcon(Path, Attributes, out string name);
				Callback(icon, name);
			}

			public override string ToString() {
				return $"Icon/Name: {FileName}";
			}
		}

		private class SpecialFolderCacheTask : ICacheTask {
			public Environment.SpecialFolder Folder { get; }
			public CacheIconAndNameCallback Callback { get; }

			public SpecialFolderCacheTask(Environment.SpecialFolder folder,
				CacheIconAndNameCallback callback)
			{
				Folder = folder;
				Callback = callback;
			}

			public void Run(IconCache icons) {
				ImageSource icon = icons.CacheSpecialFolder(Folder, out string name);
				Callback(icon, name);
			}

			public override string ToString() {
				return $"Special Folder/Name: {Folder}";
			}
		}

		private class FileTypeCacheTask : ICacheTask {
			public string Extension { get; }
			public CacheIconAndNameCallback Callback { get; }

			public FileTypeCacheTask(string extension, CacheIconAndNameCallback callback) {
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
	}
}

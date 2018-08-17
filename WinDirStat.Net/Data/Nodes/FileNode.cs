using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Data.Nodes {
	public class FileNode : FileNodeBase {

		internal ExtensionRecord extRecord;

		public override sealed string Extension {
			get => extRecord.Extension;
		}
		public override sealed ExtensionRecord ExtensionRecord {
			get => extRecord;
		}

		public FileNode(INtfsNode node)
			: base(node, FileNodeType.File, FileNodeFlags.None)
		{
		}
		
		public FileNode(FileSystemInfo info)
			: base(info, FileNodeType.File, FileNodeFlags.None)
		{
		}
		
		public FileNode(IFileFindData find)
			: base(find, FileNodeType.File, FileNodeFlags.None)
		{
		}
		
		public FileNode(FileNode node, FolderNode parent)
			: base(node, parent)
		{
			extRecord = node.extRecord;
		}
	}
}

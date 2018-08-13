using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Data.Nodes {
	public abstract class DriveSpaceNode : FileNode {

		private readonly string driveName;

		protected DriveSpaceNode(string driveName, FileNodeType type) {
			this.driveName = driveName;
			//creationTime = DateTime.MinValue;
			lastChangeTime = DateTime.MinValue;
			//lastAccessTime = DateTime.MinValue;
			size = CalculateSize();
		}

		public string DriveName {
			get => driveName;
		}

		public abstract long CalculateSize();
	}
}

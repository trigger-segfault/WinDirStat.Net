using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Data.Nodes {
	public interface IFileNodeCollection : IList<FileNodeBase>, INotifyCollectionChanged {
		void AddRange(IEnumerable<FileNodeBase> nodes);
		
		//void InsertRange(int index, IEnumerable<FileNode> nodes);
		
		void RemoveRange(int index, int count);

		//void RemoveAll(Predicate<FileNode> match);

		//FileNode[] ToArray();
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.View.Nodes {
	public interface IFileNodeCollection : IList<FileNodeViewModel>, INotifyCollectionChanged {
		void AddRange(IEnumerable<FileNodeViewModel> nodes);
		
		//void InsertRange(int index, IEnumerable<FileNode> nodes);
		
		void RemoveRange(int index, int count);

		//void RemoveAll(Predicate<FileNode> match);

		//FileNode[] ToArray();
	}
}

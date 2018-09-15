using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.ViewModel.Files {
	/// <summary>
	/// The public interface for the readonly part of the <see cref="FileItemViewModel"/> collection.
	/// </summary>
	public interface IReadOnlyFileItemViewModelCollection
		: INotifyCollectionChanged, IReadOnlyList<FileItemViewModel>
	{
	}
}

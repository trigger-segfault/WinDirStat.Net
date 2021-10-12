using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinDirStat.Net {
    /// <summary>An observable object with extra raise property changed methods.</summary>
    public class ObservableObjectEx : ObservableObject {

		/// <summary>Raises the property as changed if the condition is true.</summary>
		/// 
		/// <param name="condition">The condition for raising the changed event.</param>
		/// <param name="propertyName">The name of the property.</param>
		/// <returns>True if the property was changed.</returns>
		protected bool OnPropertyChangedIf(bool condition, [CallerMemberName] string propertyName = null) {
            //FIXME: this never checks the condition...
			OnPropertyChanged(propertyName);
			return condition;
		}
	}
}

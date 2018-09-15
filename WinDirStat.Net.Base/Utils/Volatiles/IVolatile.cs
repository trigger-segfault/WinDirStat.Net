using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	/// <summary>An interface for a volatile container type.</summary>
	public interface IVolatile {

		#region Properties

		/// <summary>Gets the <see cref="object"/> value.</summary>
		object Value { get; }
		/// <summary>Gets the lock context.</summary>
		object LockObj { get; }

		#endregion

		#region Locking

		/// <summary>Locks the value.</summary>
		void Lock();
		/// <summary>Unlocks the value.</summary>
		void Unlock();

		#endregion

		#region Volatile Operators

		/// <summary>Sets the value.</summary>
		object Set(object o);

		#endregion

	}
}

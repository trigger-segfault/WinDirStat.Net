using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	/// <summary>A mutable volatile <see cref="bool"/> that supports compound assignments.</summary>
	public class VolatileBoolean : Volatile<bool> {
		
		#region Constructors

		/// <summary>Constructs the default <see cref="VolatileBoolean"/>.</summary>
		public VolatileBoolean() { }
		/// <summary>Constructs the <see cref="VolatileBoolean"/> with the specified value.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		public VolatileBoolean(bool value) : base(value) { }
		/// <summary>Constructs the <see cref="VolatileBoolean"/> with the specified lock.</summary>
		/// 
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileBoolean(object lockObj) : base(lockObj) { }
		/// <summary>Constructs the <see cref="VolatileBoolean"/> with the specified value and lock.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileBoolean(bool value, object lockObj) : base(value, lockObj) { }

		#endregion
		
		#region Volatile Operators
		
		/// <summary>Toggles the value.</summary>
		public bool Toggle() {
			lock (lockObj)
				return value = !value;
		}

		#endregion

		#region Volatile Bitwise Operators

		/// <summary>Bitwise AND's the value.</summary>
		public bool BitwiseAnd(bool b) {
			lock (lockObj)
				return value &= b;
		}
		/// <summary>Bitwise OR's the value.</summary>
		public bool BitwiseOr(bool b) {
			lock (lockObj)
				return value |= b;
		}
		/// <summary>Bitwise XOR's the value.</summary>
		public bool BitwiseXor(bool b) {
			lock (lockObj)
				return value ^= b;
		}

		#endregion
	}
}

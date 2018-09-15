
namespace WinDirStat.Net.Utils {
	/// <summary>A mutable volatile <see cref="int"/> that supports compound assignments.</summary>
	public class VolatileInt32 : Volatile<int> {
		
		#region Constructors

		/// <summary>Constructs the default <see cref="VolatileInt32"/>.</summary>
		public VolatileInt32() { }
		/// <summary>Constructs the <see cref="VolatileInt32"/> with the specified value.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		public VolatileInt32(int value) : base(value) { }
		/// <summary>Constructs the <see cref="VolatileInt32"/> with the specified lock.</summary>
		/// 
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileInt32(object lockObj) : base(lockObj) { }
		/// <summary>Constructs the <see cref="VolatileInt32"/> with the specified value and lock.</summary>
		/// 
		/// <param name="value">The value to use.</param>
		/// <param name="lockObj">The lock context to use.</param>
		public VolatileInt32(int value, object lockObj) : base(value, lockObj) { }

		#endregion
		
		#region Volatile Operators
		
		/// <summary>Adds to the value.</summary>
		public int Add(int i) {
			lock (lockObj)
				return value += i;
		}
		/// <summary>Subtracts from the value.</summary>
		public int Sub(int i) {
			lock (lockObj)
				return value -= i;
		}
		/// <summary>Multiplies the value.</summary>
		public int Mul(int i) {
			lock (lockObj)
				return value *= i;
		}
		/// <summary>Divides the value.</summary>
		public int Div(int i) {
			lock (lockObj)
				return value /= i;
		}
		/// <summary>Negates the value.</summary>
		public int Negate() {
			lock (lockObj)
				return value = -value;
		}

		#endregion

		#region Volatile Bitwise Operators

		/// <summary>Bitwise AND's the value.</summary>
		public int BitwiseAnd(int i) {
			lock (lockObj)
				return value &= i;
		}
		/// <summary>Bitwise OR's the value.</summary>
		public int BitwiseOr(int i) {
			lock (lockObj)
				return value |= i;
		}
		/// <summary>Bitwise XOR's the value.</summary>
		public int BitwiseXor(int i) {
			lock (lockObj)
				return value ^= i;
		}
		/// <summary>Bitwise negates the value.</summary>
		public int BitwiseNegate() {
			lock (lockObj)
				return value = ~value;
		}

		#endregion
	}
}

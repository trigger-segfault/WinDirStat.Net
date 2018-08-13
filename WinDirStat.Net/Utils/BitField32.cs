using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Utils {
	public struct BitField32 {
		/// <summary>The 4-byte signed integer value of the bit field.</summary>
		public int Value { get; set; }

		/// <summary>The 4-byte unsigned integer value of the bit field.</summary>
		public uint UValue {
			get => unchecked((uint) Value);
			set => Value = unchecked((int) value);
		}

		/// <summary>Constructs a bitfield with the base value.</summary>
		/// <param name="value">The base value to use for the bitfield.</param>
		public BitField32(int value) {
			Value = value;
		}

		/// <summary>Constructs a bitfield with the base value.</summary>
		/// <param name="value">The base value to use for the bitfield.</param>
		public BitField32(uint value) {
			Value = unchecked((int) value);
		}

		/// <summary>Gets if a bit is 1.</summary>
		/// 
		/// <param name="bit">The index of the bit.</param>
		/// <returns>True if the bit is 1.</returns>
		public bool GetBit(int bit) {
			return ((Value >> bit) & 0x1) != 0;
		}

		/// <summary>Sets the bit.</summary>
		/// 
		/// <param name="bit">The index of the bit.</param>
		/// <param name="value">True if the bit is 1, otherwise 0.</param>
		public void SetBit(int bit, bool value) {
			if (value)
				Value |= (0x1 << bit);
			else
				Value &= ~(0x1 << bit);
		}

		/// <summary>Gets the bit as 1 or 0.</summary>
		/// 
		/// <param name="bit">The index of the bit.</param>
		/// <returns>The value of the bit as 1 or 0.</returns>
		public int GetBitValue(int bit) {
			return (Value >> bit) & 0x1;
		}

		/// <summary>Sets the bit as 1 or 0.</summary>
		/// 
		/// <param name="bit">The index of the bit.</param>
		/// <param name="value">The new value of the bit as 1 or 0.</returns>
		public void SetBitValue(int bit, int value) {
			if ((value & 0x1) != 0)
				Value |= (0x1 << bit);
			else
				Value &= ~(0x1 << bit);
		}

		public uint GetUInt32(int bit) {
			return unchecked((uint) ((Value >> bit) & -1));
		}

		public uint GetUInt32(int firstBit, int lastBit) {
			int mask = GetMask(firstBit, lastBit);
			return unchecked((uint) ((Value >> firstBit) & mask));
		}

		public void SetUInt32(int bit, uint value) {
			Value &= ~(-1 << bit);
			Value |= (unchecked((int) value) << bit);
		}

		public void SetUInt32(int firstBit, int lastBit, uint value) {
			int mask = GetMask(firstBit, lastBit);
			Value &= ~(mask << firstBit);
			Value |= ((unchecked((int) value) & mask) << firstBit);
		}

		public int GetInt32(int bit) {
			return ((Value >> bit) & -1);
		}

		public int GetInt32(int firstBit, int lastBit) {
			int mask = GetMask(firstBit, lastBit);
			return ((Value >> firstBit) & mask);
		}

		public void SetInt32(int bit, int value) {
			Value &= ~(-1 << bit);
			Value |= (value << bit);
		}

		public void SetInt32(int firstBit, int lastBit, int value) {
			int mask = GetMask(firstBit, lastBit);
			Value &= ~(mask << firstBit);
			Value |= ((value & mask) << firstBit);
		}

		public short GetInt16(int bit) {
			return unchecked((short) ((Value >> bit) & ushort.MaxValue));
		}

		public short GetInt16(int firstBit, int lastBit) {
			int mask = GetMask(firstBit, lastBit);
			return unchecked((short) ((Value >> firstBit) & mask));
		}

		public void SetInt16(int bit, short value) {
			Value &= ~(ushort.MaxValue << bit);
			Value |= ((value & ushort.MaxValue) << bit);
		}

		public void SetInt16(int firstBit, int lastBit, short value) {
			int mask = GetMask(firstBit, lastBit);
			Value &= ~(mask << firstBit);
			Value |= ((value & mask) << firstBit);
		}

		public ushort GetUInt16(int bit) {
			return unchecked((ushort) ((Value >> bit) & ushort.MaxValue));
		}

		public ushort GetUInt16(int firstBit, int lastBit) {
			int mask = GetMask(firstBit, lastBit);
			return unchecked((ushort) ((Value >> firstBit) & mask));
		}

		public void SetUInt16(int bit, ushort value) {
			Value &= ~(ushort.MaxValue << bit);
			Value |= ((value & ushort.MaxValue) << bit);
		}

		public void SetUInt16(int firstBit, int lastBit, ushort value) {
			int mask = GetMask(firstBit, lastBit);
			Value &= ~(mask << firstBit);
			Value |= ((value & mask) << firstBit);
		}

		public byte GetByte(int bit) {
			return unchecked((byte) ((Value >> bit) & byte.MaxValue));
		}

		public byte GetByte(int firstBit, int lastBit) {
			int mask = GetMask(firstBit, lastBit);
			return unchecked((byte) ((Value >> firstBit) & mask));
		}

		public void SetByte(int bit, byte value) {
			Value &= ~(byte.MaxValue << bit);
			Value |= ((value & byte.MaxValue) << bit);
		}

		public void SetByte(int firstBit, int lastBit, byte value) {
			int mask = GetMask(firstBit, lastBit);
			Value &= ~(mask << firstBit);
			Value |= ((value & mask) << firstBit);
		}

		public sbyte GetSByte(int bit) {
			return unchecked((sbyte) ((Value >> bit) & byte.MaxValue));
		}

		public sbyte GetSByte(int firstBit, int lastBit) {
			int mask = GetMask(firstBit, lastBit);
			return unchecked((sbyte) ((Value >> firstBit) & mask));
		}

		public void SetSByte(int bit, sbyte value) {
			Value &= ~(byte.MaxValue << bit);
			Value |= ((value & byte.MaxValue) << bit);
		}

		public void SetSByte(int firstBit, int lastBit, sbyte value) {
			int mask = GetMask(firstBit, lastBit);
			Value &= ~(mask << firstBit);
			Value |= ((value & mask) << firstBit);
		}

		private static int GetMask(int firstBit, int lastBit) {
			int range = lastBit - firstBit + 1;
			return range *  range - 1;
		}
	}
}

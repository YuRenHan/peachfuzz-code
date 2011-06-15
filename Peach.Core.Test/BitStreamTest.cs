﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;

namespace Peach.Core.Test
{
	[TestFixture]
	public class BitStreamTest
	{
		[Test]
		public void Length()
		{
			BitStream bs = new BitStream();

			Assert.AreEqual(0, bs.LengthBits);

			bs.WriteBit(0);
			Assert.AreEqual(1, bs.LengthBits);

			bs = new BitStream();
			for (int i = 1; i < 10000; i++)
			{
				bs.WriteBit(0);
				Assert.AreEqual(i, bs.LengthBits);
			}

			bs = new BitStream();
			bs.WriteByte(1);
			Assert.AreEqual(8, bs.LengthBits);
			Assert.AreEqual(1, bs.LengthBytes);

			bs = new BitStream(new byte[] { 1, 2, 3, 4, 5 });
			Assert.AreEqual(5, bs.LengthBytes);
			Assert.AreEqual(5 * 8, bs.LengthBits);
		}

		[Test]
		public void ReadingBites()
		{
			BitStream bs = new BitStream(new byte[] { 0x41, 0x41 });
			bs.LittleEndian();

			Assert.AreEqual(1, bs.ReadBit()); // 0
			Assert.AreEqual(0, bs.ReadBit()); // 1
			Assert.AreEqual(0, bs.ReadBit()); // 2
			Assert.AreEqual(0, bs.ReadBit()); // 3
			Assert.AreEqual(0, bs.ReadBit()); // 4
			Assert.AreEqual(0, bs.ReadBit()); // 5
			Assert.AreEqual(1, bs.ReadBit()); // 6
			Assert.AreEqual(0, bs.ReadBit()); // 7

			Assert.AreEqual(1, bs.ReadBit()); // 0
			Assert.AreEqual(0, bs.ReadBit()); // 1
			Assert.AreEqual(0, bs.ReadBit()); // 2
			Assert.AreEqual(0, bs.ReadBit()); // 3
			Assert.AreEqual(0, bs.ReadBit()); // 4
			Assert.AreEqual(0, bs.ReadBit()); // 5
			Assert.AreEqual(1, bs.ReadBit()); // 6
			Assert.AreEqual(0, bs.ReadBit()); // 7

			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			bs.BigEndian();

			Assert.AreEqual(0, bs.ReadBit()); // 0
			Assert.AreEqual(1, bs.ReadBit()); // 1
			Assert.AreEqual(0, bs.ReadBit()); // 2
			Assert.AreEqual(0, bs.ReadBit()); // 3
			Assert.AreEqual(0, bs.ReadBit()); // 4
			Assert.AreEqual(0, bs.ReadBit()); // 5
			Assert.AreEqual(0, bs.ReadBit()); // 6
			Assert.AreEqual(1, bs.ReadBit()); // 7

			Assert.AreEqual(0, bs.ReadBit()); // 0
			Assert.AreEqual(1, bs.ReadBit()); // 1
			Assert.AreEqual(0, bs.ReadBit()); // 2
			Assert.AreEqual(0, bs.ReadBit()); // 3
			Assert.AreEqual(0, bs.ReadBit()); // 4
			Assert.AreEqual(0, bs.ReadBit()); // 5
			Assert.AreEqual(0, bs.ReadBit()); // 6
			Assert.AreEqual(1, bs.ReadBit()); // 7
		}

		[Test]
		public void ReadWriteBits()
		{
			BitStream bs = new BitStream();
			bs.LittleEndian();

			bs.WriteBit(0);
			bs.WriteBit(0);
			bs.WriteBit(0);
			bs.WriteBit(1);
			bs.WriteBit(0);
			bs.WriteBit(1);

			bs.WriteBit(0);
			bs.WriteBit(0);
			bs.WriteBit(0);
			bs.WriteBit(1);
			bs.WriteBit(0);
			bs.WriteBit(1);

			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());

			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
		}

		[Test]
		public void ReadWriteNumbers()
		{
			BitStream bs = new BitStream();
			bs.LittleEndian();

			//Max
			bs.WriteInt8(sbyte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteInt16(short.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteInt32(67305985);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt32(Int32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt64(Int64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, bs.ReadInt64());

			bs.Clear();
			bs.WriteUInt8(byte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, bs.ReadUInt8());

			bs.Clear();
			bs.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, bs.ReadUInt16());

			bs.Clear();
			bs.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, bs.ReadUInt32());

			bs.Clear();
			bs.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, bs.ReadUInt64());


			//Min
			bs.Clear();
			bs.WriteInt8(sbyte.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteInt16(short.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteInt32(Int32.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt64(Int64.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, bs.ReadInt64());

			// BIG ENDIAN //////////////////////////////////////////

			bs = new BitStream();
			bs.LittleEndian();

			//Max
			bs.WriteInt8(sbyte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteInt16(short.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteInt32(67305985);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt32(Int32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt64(Int64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, bs.ReadInt64());

			bs.Clear();
			bs.WriteUInt8(byte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, bs.ReadUInt8());

			bs.Clear();
			bs.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, bs.ReadUInt16());

			bs.Clear();
			bs.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, bs.ReadUInt32());

			bs.Clear();
			bs.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, bs.ReadUInt64());

			//Min
			bs.Clear();
			bs.WriteInt8(sbyte.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteInt16(short.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteInt32(Int32.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt64(Int64.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, bs.ReadInt64());
		}

		[Test]
		public void ReadWriteNumbersOddOffset()
		{
			BitStream bs = new BitStream();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);

			bs.LittleEndian();

			//Max
			bs.WriteInt8(sbyte.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt16(short.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(67305985);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(Int32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt64(Int64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, bs.ReadInt64());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt8(byte.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, bs.ReadUInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, bs.ReadUInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, bs.ReadUInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, bs.ReadUInt64());


			//Min
			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt8(sbyte.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt16(short.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(Int32.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt64(Int64.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, bs.ReadInt64());

			// BIG ENDIAN //////////////////////////////////////////

			bs = new BitStream();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.LittleEndian();

			//Max
			bs.WriteInt8(sbyte.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt16(short.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(67305985);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(Int32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt64(Int64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, bs.ReadInt64());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt8(byte.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, bs.ReadUInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, bs.ReadUInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, bs.ReadUInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, bs.ReadUInt64());

			//Min
			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt8(sbyte.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt16(short.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(Int32.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt64(Int64.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, bs.ReadInt64());
		}
	}
}

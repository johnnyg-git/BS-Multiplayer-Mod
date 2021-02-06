using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BSMP
{
	public enum PacketTypes
	{
		welcome
	}

	public class Packet : IDisposable
	{
		private List<byte> buffer;

		private byte[] readableBuffer;
		private int readPos;
		private bool disposed;

		public Packet()
		{
			buffer = new List<byte>();
			readPos = 0;
		}

		public Packet(int _id)
		{
			buffer = new List<byte>();
			readPos = 0;
			Write(_id);
		}

		public Packet(byte[] _data)
		{
			buffer = new List<byte>();
			readPos = 0;
			SetBytes(_data);
		}

		public void SetBytes(byte[] _data)
		{
			Write(_data);
			readableBuffer = buffer.ToArray();
		}

		public void InsertInt(int _value, int pos = 0)
		{
			buffer.InsertRange(pos, BitConverter.GetBytes(_value));
		}

		public byte[] ToArray()
		{
			readableBuffer = buffer.ToArray();
			return readableBuffer;
		}

		public int Length()
		{
			return buffer.Count;
		}

		public int UnreadLength()
		{
			return Length() - readPos;
		}

		public void Reset(bool _shouldReset = true)
		{
			if (_shouldReset)
			{
				buffer.Clear();
				readableBuffer = null;
				readPos = 0;
				return;
			}
			readPos -= 4;
		}

		#region writing
		public void WriteLength()
		{
			buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
		}

		public void Write(byte _value)
		{
			buffer.Add(_value);
		}

		public void Write(byte[] _value)
		{
			buffer.AddRange(_value);
		}

		public void Write(short _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}

		public void Write(int _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}

		public void Write(long _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}

		public void Write(float _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}

		public void Write(bool _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}

		public void Write(string _value)
		{
			Write(_value.Length);
			buffer.AddRange(Encoding.ASCII.GetBytes(_value));
		}

		#endregion

		#region reading
		public byte ReadByte(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				byte result = readableBuffer[readPos];
				if (_moveReadPos)
				{
					readPos++;
				}
				return result;
			}
			throw new Exception("Could not read value of type 'byte'!");
		}

		public byte[] ReadBytes(int _length, bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				byte[] result = buffer.GetRange(readPos, _length).ToArray();
				if (_moveReadPos)
				{
					readPos += _length;
				}
				return result;
			}
			throw new Exception("Could not read value of type 'byte[]'!");
		}

		public short ReadShort(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				short result = BitConverter.ToInt16(readableBuffer, readPos);
				if (_moveReadPos)
				{
					readPos += 2;
				}
				return result;
			}
			throw new Exception("Could not read value of type 'short'!");
		}

		public int ReadInt(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				int result = BitConverter.ToInt32(readableBuffer, readPos);
				if (_moveReadPos)
				{
					readPos += 4;
				}
				return result;
			}
			throw new Exception("Could not read value of type 'int'!");
		}

		public long ReadLong(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				long result = BitConverter.ToInt64(readableBuffer, readPos);
				if (_moveReadPos)
				{
					readPos += 8;
				}
				return result;
			}
			throw new Exception("Could not read value of type 'long'!");
		}

		public float ReadFloat(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				float result = BitConverter.ToSingle(readableBuffer, readPos);
				if (_moveReadPos)
				{
					readPos += 4;
				}
				return result;
			}
			throw new Exception("Could not read value of type 'float'!");
		}

		public bool ReadBool(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				bool result = BitConverter.ToBoolean(readableBuffer, readPos);
				if (_moveReadPos)
				{
					readPos++;
				}
				return result;
			}
			throw new Exception("Could not read value of type 'bool'!");
		}

		public string ReadString(bool _moveReadPos = true)
		{
			string result;
			try
			{
				int num = ReadInt(true);
				string @string = Encoding.ASCII.GetString(readableBuffer, readPos, num);
				if (_moveReadPos && @string.Length > 0)
				{
					readPos += num;
				}
				result = @string;
			}
			catch
			{
				throw new Exception("Could not read value of type 'string'!");
			}
			return result;
		}
		#endregion

		protected virtual void Dispose(bool _disposing)
		{
			if (!disposed)
			{
				if (_disposing)
				{
					buffer = null;
					readableBuffer = null;
					readPos = 0;
				}
				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}

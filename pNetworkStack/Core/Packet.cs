using System;
using System.Collections.Generic;
using System.Text;

namespace pNetworkStack.Core
{
	public class Packet : IDisposable
	{
		private List<byte> m_Buffer;
		private byte[] m_ReadableBuffer;
		private int m_ReadPos;

		public Packet()
		{
			m_Buffer = new List<byte>();
			m_ReadPos = 0;
		}

		public Packet(int id)
		{
			m_Buffer = new List<byte>();
			m_ReadPos = 0;

			Write(id);
		}

		public Packet(byte[] data)
		{
			m_Buffer = new List<byte>();
			m_ReadPos = 0;

			SetBytes(data);
		}

	#region Functions

		/// <summary>Sets the packet's content and prepares it to be read.</summary>
        /// <param name="data">The bytes to add to the packet.</param>
        public void SetBytes(byte[] data)
        {
            Write(data);
            m_ReadableBuffer = m_Buffer.ToArray();
        }

        /// <summary>Inserts the length of the packet's content at the start of the m_Buffer.</summary>
        public void WriteLength()
        {
            m_Buffer.InsertRange(0, BitConverter.GetBytes(m_Buffer.Count)); // Insert the byte length of the packet at the very beginning
        }

        /// <summary>Inserts the given int at the start of the m_Buffer.</summary>
        /// <param name="value">The int to insert.</param>
        public void InsertInt(int value)
        {
            m_Buffer.InsertRange(0, BitConverter.GetBytes(value)); // Insert the int at the start of the m_Buffer
        }

        /// <summary>Gets the packet's content in array form.</summary>
        public byte[] ToArray()
        {
            m_ReadableBuffer = m_Buffer.ToArray();
            return m_ReadableBuffer;
        }

        /// <summary>Gets the length of the packet's content.</summary>
        public int Length()
        {
            return m_Buffer.Count; // Return the length of m_Buffer
        }

        /// <summary>Gets the length of the unread data contained in the packet.</summary>
        public int UnreadLength()
        {
            return Length() - m_ReadPos; // Return the remaining length (unread)
        }

        /// <summary>Resets the packet instance to allow it to be reused.</summary>
        /// <param name="shouldReset">Whether or not to reset the packet.</param>
        public void Reset(bool shouldReset = true)
        {
            if (shouldReset)
            {
                m_Buffer.Clear(); // Clear m_Buffer
                m_ReadableBuffer = null;
                m_ReadPos = 0; // Reset m_ReadPos
            }
            else
            {
                m_ReadPos -= 4; // "Unread" the last read int
            }
        }

	#endregion
		
	#region Write
		
		public void Write(byte value)
		{
			m_Buffer.Add(value);
		}

		public void Write(byte[] value)
		{
			m_Buffer.AddRange(value);
		}

		public void Write(short value)
		{
			m_Buffer.AddRange(BitConverter.GetBytes(value));
		}

		public void Write(int value)
		{
			m_Buffer.AddRange(BitConverter.GetBytes(value));
		}

		public void Write(long value)
		{
			m_Buffer.AddRange(BitConverter.GetBytes(value));
		}

		public void Write(float value)
		{
			m_Buffer.AddRange(BitConverter.GetBytes(value));
		}

		public void Write(bool value)
		{
			m_Buffer.AddRange(BitConverter.GetBytes(value));
		}

		public void Write(string value)
		{
			Write(value.Length);
			m_Buffer.AddRange(Encoding.ASCII.GetBytes(value));
		}

	#endregion

	#region Read

		/// <summary>Reads a byte from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the m_Buffer's read position.</param>
        public byte ReadByte(bool moveReadPos = true)
        {
            if (m_Buffer.Count > m_ReadPos)
            {
                // If there are unread bytes
                byte _value = m_ReadableBuffer[m_ReadPos]; // Get the byte at m_ReadPos' position
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    m_ReadPos += 1; // Increase m_ReadPos by 1
                }
                return _value; // Return the byte
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        /// <summary>Reads an array of bytes from the packet.</summary>
        /// <param name="length">The length of the byte array.</param>
        /// <param name="moveReadPos">Whether or not to move the m_Buffer's read position.</param>
        public byte[] ReadBytes(int length, bool moveReadPos = true)
        {
            if (m_Buffer.Count > m_ReadPos)
            {
                // If there are unread bytes
                byte[] _value = m_Buffer.GetRange(m_ReadPos, length).ToArray(); // Get the bytes at m_ReadPos' position with a range of _length
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    m_ReadPos += length; // Increase m_ReadPos by _length
                }
                return _value; // Return the bytes
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        /// <summary>Reads a short from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the m_Buffer's read position.</param>
        public short ReadShort(bool moveReadPos = true)
        {
            if (m_Buffer.Count > m_ReadPos)
            {
                // If there are unread bytes
                short _value = BitConverter.ToInt16(m_ReadableBuffer, m_ReadPos); // Convert the bytes to a short
                if (moveReadPos)
                {
                    // If moveReadPos is true and there are unread bytes
                    m_ReadPos += 2; // Increase m_ReadPos by 2
                }
                return _value; // Return the short
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        /// <summary>Reads an int from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the m_Buffer's read position.</param>
        public int ReadInt(bool moveReadPos = true)
        {
            if (m_Buffer.Count > m_ReadPos)
            {
                // If there are unread bytes
                int _value = BitConverter.ToInt32(m_ReadableBuffer, m_ReadPos); // Convert the bytes to an int
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    m_ReadPos += 4; // Increase m_ReadPos by 4
                }
                return _value; // Return the int
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        /// <summary>Reads a long from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the m_Buffer's read position.</param>
        public long ReadLong(bool moveReadPos = true)
        {
            if (m_Buffer.Count > m_ReadPos)
            {
                // If there are unread bytes
                long _value = BitConverter.ToInt64(m_ReadableBuffer, m_ReadPos); // Convert the bytes to a long
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    m_ReadPos += 8; // Increase m_ReadPos by 8
                }
                return _value; // Return the long
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        /// <summary>Reads a float from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the m_Buffer's read position.</param>
        public float ReadFloat(bool moveReadPos = true)
        {
            if (m_Buffer.Count > m_ReadPos)
            {
                // If there are unread bytes
                float _value = BitConverter.ToSingle(m_ReadableBuffer, m_ReadPos); // Convert the bytes to a float
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    m_ReadPos += 4; // Increase m_ReadPos by 4
                }
                return _value; // Return the float
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        /// <summary>Reads a bool from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the m_Buffer's read position.</param>
        public bool ReadBool(bool moveReadPos = true)
        {
            if (m_Buffer.Count > m_ReadPos)
            {
                // If there are unread bytes
                bool _value = BitConverter.ToBoolean(m_ReadableBuffer, m_ReadPos); // Convert the bytes to a bool
                if (moveReadPos)
                {
                    // If moveReadPos is true
                    m_ReadPos += 1; // Increase m_ReadPos by 1
                }
                return _value; // Return the bool
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        /// <summary>Reads a string from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the m_Buffer's read position.</param>
        public string ReadString(bool moveReadPos = true)
        {
            try
            {
                int _length = ReadInt(); // Get the length of the string
                string _value = Encoding.ASCII.GetString(m_ReadableBuffer, m_ReadPos, _length); // Convert the bytes to a string
                if (moveReadPos && _value.Length > 0)
                {
                    // If moveReadPos is true string is not empty
                    m_ReadPos += _length; // Increase m_ReadPos by the length of the string
                }
                return _value; // Return the string
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }

	#endregion
		
		private bool m_Disposed = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!m_Disposed)
			{
				if (disposing)
				{
					m_Buffer = null;
					m_ReadableBuffer = null;
					m_ReadPos = 0;
				}

				m_Disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
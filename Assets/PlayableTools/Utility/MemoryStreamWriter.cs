using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MemoryStreamWriter : IDisposable
{
    public MemoryStream _stream;
    public MemoryStreamWriter()
    {
        _stream = new MemoryStream();
    }

    public MemoryStreamWriter(byte[] bytes)
    {
        _stream = new MemoryStream(bytes);
    }

    public void Write(int value)
    {
        _stream.Write(BitConverter.GetBytes(value));
    }
    public void Write(float value)
    {
        _stream.Write(BitConverter.GetBytes(value));
    }
    public void Write(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        int length = bytes.Length;
        _stream.Write(BitConverter.GetBytes(length));
        _stream.Write(bytes);
    }
    public void Write(bool value)
    {
        _stream.Write(BitConverter.GetBytes(value));
    }

    public int ReadInt()
    {
        byte[] bytes = new byte[sizeof(int)];
        _stream.Read(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }

    public float ReadFloat()
    {
        byte[] bytes = new byte[sizeof(float)];
        _stream.Read(bytes);
        return BitConverter.ToSingle(bytes, 0);
    }

    public string ReadString()
    {
        int length = ReadInt();
        byte[] bytes = new byte[length];
        _stream.Read(bytes);
        return Encoding.UTF8.GetString(bytes);
    }
    public bool ReadBoolean()
    {
        byte[] bytes = new byte[sizeof(bool)];
        _stream.Read(bytes);
        return BitConverter.ToBoolean(bytes, 0);
    }

    public void SetPosition(int pos)
    {
        _stream.Position = pos;
    }

    public byte[] ToBytes()
    {
        return _stream.ToArray();
    }

    public int GetLength()
    {
        return (int)_stream.Length;
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}
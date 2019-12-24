using System;
using System.Collections.Generic;
public class ByteArray
{
    /// <summary>
    /// 默认大小
    /// </summary>
    const int DEFAULT_SIZE = 1024;
    /// <summary>
    /// 初始大小
    /// </summary>
    int initsize = 0;
    /// <summary>
    /// 缓冲区
    /// </summary>
    public byte[] bytes;
    /// <summary>
    /// 读位置
    /// </summary>
    public int readIdx = 0;
    /// <summary>
    /// 写位置
    /// </summary>
    public int writeIdx = 0;
    /// <summary>
    /// 容量
    /// </summary>
    private int capacity = 0;
    /// <summary>
    /// 剩余空间
    /// </summary>
    public int Remain { get { return capacity - writeIdx; } }
    /// <summary>
    /// 有效数据长度
    /// </summary>
    public int Length { get { return writeIdx - readIdx; } }

    public ByteArray(int size = DEFAULT_SIZE)
    {
        bytes = new byte[size];
        capacity = size;
        initsize = size;
        readIdx = 0;
        writeIdx = 0;
    }

    public ByteArray(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        capacity = defaultBytes.Length;
        initsize = defaultBytes.Length;
        readIdx = 0;
        writeIdx = defaultBytes.Length;
    }

    /// <summary>
    /// 重设尺寸，每次翻倍增加数组长度2*n，如果输入1500，缓冲区会设置成2048大小
    /// </summary>
    /// <param name="size">所需数据长度</param>
    public void ReSize(int size)
    {
        if (size < Length) return;
        if (size < initsize) return;
        int n = 1;
        while (n<size)
        {
            n *= 2;
        }
        capacity = n;
        byte[] newbytes = new byte[capacity];
        Array.Copy(bytes, readIdx, newbytes, 0, writeIdx - readIdx);
        bytes = newbytes;
        writeIdx = Length;
        readIdx = 0;
    }

    #region 检查并移动数据
    public void CheckAndMoveBytes()
    {
        if (Length<8)
        {
            MoveBytes();
        }
    }

    public void MoveBytes()
    {
        Array.Copy(bytes, readIdx, bytes, 0, Length);
        writeIdx = Length;
        readIdx = 0;
    }
    #endregion


    #region 读写功能

    /// <summary>
    /// 将数组bs写入缓冲区
    /// </summary>
    /// <param name="bs">要写入缓冲区的数组</param>
    /// <param name="offset">开始写入的位置</param>
    /// <param name="count">写入的数据长度</param>
    /// <returns>成功写入的长度</returns>
    public int Write(byte[] bs,int offset,int count)
    {
        if (Remain < count)
        {
            ReSize(Length + count);
        }
        Array.Copy(bs, offset, bytes, writeIdx, count);
        writeIdx += count;
        return count;
    }

    /// <summary>
    /// 读取数据到bs数组
    /// </summary>
    /// <param name="bs">写入的数组</param>
    /// <param name="offset">从这个位置开始读取</param>
    /// <param name="count">读取的数据长度</param>
    /// <returns>成功读取的数据长度</returns>
    public int Read(byte[] bs, int offset, int count)
    {
        count = Math.Min(count, Length); //确保有效数据 
        Array.Copy(bytes, 0, bs, offset, count);    //复制到bs数组中
        readIdx += count;   
        CheckAndMoveBytes();
        return count;
    }

    public Int16 ReadInt16()
    {
        if (Length < 2) return 0;
        Int16 ret=(Int16)((bytes[1]<<8)|bytes[0]);
        readIdx += 2;
        CheckAndMoveBytes();
        return ret;
    }

    public Int32 ReadInt32()
    {
        if (Length < 4) return 0;
        Int32 ret = (Int32)((bytes[3] << 24) |
                            (bytes[2] << 16) | 
                            (bytes[1] <<  8) | 
                             bytes[0]);

        readIdx += 4;
        CheckAndMoveBytes();
        return ret;
    }
    #endregion
}

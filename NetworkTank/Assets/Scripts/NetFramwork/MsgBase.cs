using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MsgBase
{
    /// <summary>
    /// 协议名
    /// </summary>
    public string protoName = "";

    /// <summary>
    /// 序列化 编码
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] Encode(MsgBase msgBase)
    {
        string str = JsonUtility.ToJson(msgBase);
        return System.Text.Encoding.UTF8.GetBytes(str);
    }

    /// <summary>
    /// 反序列化 解码
    /// </summary>
    /// <param name="protoName">协议名</param>
    /// <param name="bytes">字节流</param>
    /// <param name="offset">开始解码位置</param>
    /// <param name="count">解码字节长度</param>
    /// <returns></returns>
    public static MsgBase Decode(string protoName,byte[] bytes,int offset,int count)
    {
        string str = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
        MsgBase msgBase = (MsgBase)JsonUtility.FromJson(str, System.Type.GetType(protoName));
        return msgBase;
    }

    /// <summary>
    /// 编码协议名（2字节长度+字符串）
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] EncodeName(MsgBase msgBase)
    {
        //名字bytes和长度
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.protoName);
        Int16 len = (Int16)nameBytes.Length;
        //申请bytes数值
        byte[] bytes = new byte[2 + len];
        //组装2字节长度信息
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        //组装名字
        Array.Copy(nameBytes, 0, bytes, 2, len);

        return bytes;

    }

    /// <summary>
    /// 解码协议名（2字节长度+字符串）
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static string DecodeName(byte[] bytes,int offset, out int count)
    {
        count = 0;
        //必须大于2字节
        if (offset+2>bytes.Length)
        {
            return "";
        }
        //读取长度
        Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
        //长度必须足够
        if (offset+2+len>bytes.Length)
        {
            return "";
        }
        //解析
        count = 2 + len;
        string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
        return name;
    }
}

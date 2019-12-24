using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class TestByteArray : MonoBehaviour
{

    void Start()
    {
        
        MemoryStream ms = new MemoryStream();
        //声明数组
        byte[] wb = new byte[] { 1, 2, 3, 4, 5 };
        //写入内存流
        ms.Write(wb, 0, wb.Length);

        Debug.Log("ToArray()    "+BitConverter.ToString(ms.ToArray()));
        Debug.Log("GetBuffer()  " + BitConverter.ToString(ms.GetBuffer()));
        Debug.Log("Position " + ms.Position);
        Debug.Log("Length   " + ms.Length);
        Debug.Log("Capacity "+ms.Capacity);

        ms.Position += 1024;
        Debug.Log("Positon+1024");
        Debug.Log("ToArray()    " + BitConverter.ToString(ms.ToArray()));
        Debug.Log("Position " + ms.Position);
        Debug.Log("Length   " + ms.Length);
        Debug.Log("Capacity   " + ms.Capacity);

        //声明数组
        byte[] newWb = new byte[] { 6, 7, 8, 9, 10, 11, 12 };
        ms.Write(newWb, 0, newWb.Length);
        Debug.Log("再次写入字节数组");
        Debug.Log("ToArray()    " + BitConverter.ToString(ms.ToArray()));
        Debug.Log("GetBuffer()  " + BitConverter.ToString(ms.GetBuffer()));
        Debug.Log("Position " + ms.Position);
        Debug.Log("Length   " + ms.Length);
        Debug.Log("Capacity " + ms.Capacity);
    }

    void ByteArrayTest()
    {
        //[1    创建]
        ByteArray buff = new ByteArray(8);
        //[2    写入]
        byte[] wb = new byte[] { 1, 2, 3, 4, 5 };
        buff.Write(wb, 0, wb.Length);

        Debug.Log(BitConverter.ToString(buff.bytes));
        Debug.Log(buff.writeIdx);

        ////[3 读取]
        //byte[] rb = new byte[4];
        //buff.Read(rb, 0, rb.Length);
        //Debug.Log(BitConverter.ToString(rb));

        //[4 写入+扩容]
        wb = new byte[] { 6, 7, 8, 9, 10, 11 };
        buff.Write(wb, 0, wb.Length);
        Debug.Log(BitConverter.ToString(buff.bytes));
        Debug.Log(buff.writeIdx);
    }
}

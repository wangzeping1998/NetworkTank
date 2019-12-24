using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
public static class NetManager
{
    #region Member

    //套接字
    static Socket socket;

    //
    //  网络状态
    //

    //是否正在连接
    static bool isConnecting = false;
    //是否正在关闭
    static bool isClosing = false;
    //网络事件类型
    public enum NetEvent
    {
        /// <summary>
        /// 连接成功
        /// </summary>
        ConnectSucc = 1,
        /// <summary>
        /// 连接失败
        /// </summary>
        ConnectFail = 2,
        /// <summary>
        /// 断开连接
        /// </summary>
        Close = 3,
    }

    //接收缓冲区
    static ByteArray readBuff;
    //写入队列
    static Queue<ByteArray> writeQueue;
    //消息列表
    static List<MsgBase> msgList = new List<MsgBase>();
    //消息列表长度
    static int msgCount = 0;
    //每一次Update处理的消息量
    readonly static int MAX_MESSAGE_FIRE = 10;

    //
    // 网络心跳机制
    //

    //是否启用心跳
    public static bool isUsePing = true;
    //心跳间隔时间
    public static int pingInterval = 30;
    //上一次发送PING时间
    static float lastPingTime = 0;
    //上一次发送PONG时间
    static float lastPongTime = 0;




    //事件委托类型
    public delegate void EventListener(string err);
    //事件监听列表
    private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();
    //消息委托类型
    public delegate void MsgListener(MsgBase msgBase);
    //消息监听列表
    private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();
    #endregion

    #region Methods

    /// <summary>
    /// 初始化状态 设置socke、 缓冲区
    /// </summary>
    private static void InitState()
    {
        //socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //接收缓冲区
        readBuff = new ByteArray();
        //写入队列
        writeQueue = new Queue<ByteArray>();
        //是否正在连接
        isConnecting = false;
        //是否整个关闭
        isClosing = false;
        //消息列表
        msgList = new List<MsgBase>();
        //消息列表长度
        msgCount = 0;
        //上一次发送PING的时间
        lastPingTime = Time.time;
        //上一次收到PONG的时间
        lastPongTime = Time.time;
        //监听PONG协议
        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
        }
    }



    #region Listen methods

    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void AddEventListener(NetEvent netEvent,EventListener listener)
    {
        //添加事件
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        //新增事件
        else
        {
            eventListeners[netEvent] = listener;
        }
    }

    /// <summary>
    /// 删除监听事件
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            //注销
            eventListeners[netEvent] -= listener;
            //删除
            if (eventListeners[netEvent] == null)
            {
                eventListeners.Remove(netEvent);
            }
        }
    }

    /// <summary>
    /// 分发事件
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="err"></param>
    public static void FireEvent(NetEvent netEvent, string err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }

    /// <summary>
    /// 添加消息监听
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        //添加事件
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
        //新增事件
        else
        {
            msgListeners[msgName] = listener;
        }
    }

    /// <summary>
    /// 删除监听事件
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            //注销
            msgListeners[msgName] -= listener;
            //删除
            if (msgListeners[msgName] == null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }

    /// <summary>
    /// 分发消息
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="err"></param>
    public static void FireMsg(string msgName, MsgBase msgBase)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }
    #endregion

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ip">地址</param>
    /// <param name="port">端口号</param>
    public static void Connect(string ip,int port)
    {
        //判断状态
        if (socket!=null && socket.Connected)
        {
            Debug.Log("Connect fail,already connected");
            return;
        }
        if (isConnecting)
        {
            Debug.Log("Connect fail,isConnecting");
            return;
        }
        //初始化成员
        InitState();
        //参数设置
        socket.NoDelay = true;
        //Connect
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }

    // Connect Callback
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect succ");
            //分发事件
            FireEvent(NetEvent.ConnectSucc, "");
            isConnecting = false;
            //开始接收
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.Remain, 0, ReceiveCallback, socket);
        }
        catch (Exception ex)
        {
            Debug.Log("Socket Connect fail" + ex.Message);
            FireEvent(NetEvent.ConnectFail, ex.Message);
            isConnecting = false;
        }
    }

    //Receive Callback
    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            //获取接收数据长度
            int count = socket.EndReceive(ar);
            if (count == 0)
            {
                Close();
                return;
            }

            readBuff.writeIdx += count;
            //处理二进制消息
            OnReceiveData();
            //继续接收数据
            if (readBuff.Remain < 8)
            {
                readBuff.MoveBytes();
                readBuff.ReSize(readBuff.Length * 2);
            }
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.Remain, 0, ReceiveCallback, socket);
        }
        catch (Exception ex)
        {
            Debug.Log("Socket receive fail" + ex.Message);
        }   
    }

    /// <summary>
    /// 接收完整协议并解析他，如果不完整便继续等下下一波消息
    /// </summary>
    private static void OnReceiveData()
    {
        //消息长度
        if (readBuff.Length <= 2)
        {
            return;
        }
        //获取消息体长度
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLenth = (Int16)(bytes[readIdx + 1] << 8 | bytes[readIdx]);
        if (readBuff.Length < bodyLenth)
        {
            return;
        }
        readBuff.readIdx += 2;
        //解析协议名
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);

        if (protoName=="")
        {
            Debug.Log("OnReceiveData MsgBase.DecodeName fail");
            return;
        }
        readBuff.readIdx += nameCount;
        //解析协议体
        int bodyCount = bodyLenth - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();
        //添加到消息队列
        lock (msgList)
        {
            msgList.Add(msgBase);
        }
        msgCount++;

        //继续读取消息
        if (readBuff.Length>2)
        {
            OnReceiveData();
        }

    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="msg"></param>
    public static void Send(MsgBase msg)
    {
        //状态判断
        if (socket==null||!socket.Connected)
        {
            return;
        }

        if (isConnecting)
        {
            return;
        }

        if (isClosing)
        {
            return;
        }

        //数据编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];
        //组装长度
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        //组装名字
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        //组装消息体
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        //写入队列
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0; //writeQueue的长度
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        //send
        if (count==1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }

    //Send回调
    private static void SendCallback(IAsyncResult ar)
    {
        //获取state、EndSend处理
        Socket socket = (Socket)ar.AsyncState;
        //判断状态
        if (socket==null||!socket.Connected)
        {
            return;
        }

        int count = socket.EndSend(ar);
        //获取写入的第一条数据
        ByteArray ba;
        lock (writeQueue)
        {
            ba = writeQueue.Peek();
        }
        //完整发送
        ba.readIdx += count;
        if (ba.Length==0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                ba = writeQueue.Peek();
            }
        }
        
        if (ba!=null) //继续发送
        {
            socket.BeginSend(ba.bytes, 0, ba.Length, 0, SendCallback, socket);
        }
        else if(isClosing) //正在关闭
        {
            socket.Close();
        }

    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public static void Close()
    {
        //判断状态
        if (socket == null || !socket.Connected)
            return;
        if (isConnecting)
            return;
        //还有数据在发送
        if (writeQueue.Count > 0)
        {
            isConnecting = true;
        }
        //没有数据在发送
        else
        {
            socket.Close();
            FireEvent(NetEvent.Close, "");
        }
            
    }

    public static void Update()
    {
        MsgUpdate();
        PingUpdate();
    }

    /// <summary>
    /// 将消息队列中的消息逐一处理
    /// </summary>
    public static void MsgUpdate()
    {
        //初步判断提升效率
        if (msgCount == 0)
        {
            return;
        }

        //重复处理消息
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
            //获取第一条消息
            MsgBase msgBase = null;
            lock (msgList)
            {
                if (msgList.Count>0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }

            if (msgBase!=null) //分发消息
            {
                FireMsg(msgBase.protoName, msgBase);
            }
            else //没有消息了
            {
                break;
            }
        }
    }

    private static void PingUpdate()
    {
        //是否启用
        if (!isUsePing)
        {
            return;
        }
        //发送PING
        if (Time.time - lastPingTime > pingInterval)
        {
            MsgPing msgPing = new MsgPing();
            Send(msgPing);
            Debug.Log("PING");
            lastPingTime = Time.time;
        }

        //检测PONG时间
        if (Time.time - lastPongTime > pingInterval*4)
        {
            Close();
        }
    }

    private static void OnMsgPong(MsgBase msgBase)
    {
        Debug.Log("PONG");
        lastPongTime = Time.time;
    }
    #endregion

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetManager.isUsePing = true;
        NetManager.Connect("127.0.0.1", 8888);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);

        NetManager.AddMsgListener("MsgMove", OnMsgMove);
    }

    //收到MsgMove协议
    private void OnMsgMove(MsgBase msgBase)
    {
        MsgMove msg = (MsgMove)msgBase;
        //消息处理
        Debug.Log("OnMsgMove msg.x" + msg.x);
        Debug.Log("OnMsgMove msg.y" + msg.y);
        Debug.Log("OnMsgMove msg.z" + msg.z);
    }

    private void Update()
    {
        NetManager.Update();
    }

    //点击连接按钮
    public void OnConnectClick()
    {
        NetManager.Connect("127.0.0.1", 8888);
        //TODO:开始转圈，提示：“连接中”
    }

    //点击关闭按钮
    public void OnCloseClick()
    {
        NetManager.Close();
    }

    //连接成功
    private void OnConnectSucc(string err)
    {
        Debug.Log("OnConnectSucc");
        //TODO:进入游戏
    }

    //连接失败
    private void OnConnectFail(string err)
    {
        Debug.Log("OnConnectFail："+err);
        //TODO:弹出提示框（连接失败，请重试）
    }

    //断开连接
    private void OnConnectClose(string err)
    {
        Debug.Log("OnConnectClose");
        //TODO:弹出提示框（网络断开）
        //TODO:弹出按钮（重新连接）
    }

}

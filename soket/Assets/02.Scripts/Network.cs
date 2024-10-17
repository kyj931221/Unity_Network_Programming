using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Network : MonoBehaviour
{
    public Text DebugMsg;

    bool bServer = false;
    bool bConnect = false;

    Socket socketListen = null;
    Socket socket = null;

    Thread thread = null;
    bool bThreadBegin = false;

    Buffer bufferSend;
    Buffer bufferReceive;

    public string name;

    void Start()
    {
        bufferSend = new Buffer();
        bufferReceive = new Buffer();
    }

    public void ServerStart(int port, int backlog=10)
    {
        socketListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
        socketListen.Bind(ep);

        socketListen.Listen(backlog);
        bServer = true;
        DebugMsg.text = "Server Start";

        StartThread();
    }

    public bool IsServer()
    {
        return bServer;
    }

    bool StartThread() //쓰레드 ThreadProc 함수가 계속 실행.
    {
        ThreadStart threadDelegate = new ThreadStart(ThreadProc);
        thread = new Thread(threadDelegate);
        thread.Start();

        bThreadBegin = true;

        return true;
    }

    public void ThreadProc()
    {
        while (bThreadBegin)
        {
            AcceptClient(); // 매 프레임마다 클라이언트 접속 여부 확인.

            if (socket != null && bConnect == true)
            {
                SendUpdate();
                ReceiveUpdate();
            }

            Thread.Sleep(10);
        }
    }

    public void ClientStart(string address, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        socket.Connect(address, port);

        bConnect = true;
        DebugMsg.text = "Client Start";

        StartThread();
    }

    void AcceptClient()
    {
        if (socketListen != null && socketListen.Poll(0, SelectMode.SelectRead))
        {
            socket = socketListen.Accept();
            bConnect = true;

            Debug.Log("Client Connect");
        }
    }

    public bool IsConnect()
    {
        return bConnect;
    }
    
    // ******************************************************************************
    public int Send(byte[] bytes, int length)
    {
        return bufferSend.Write(bytes, length);
    }

    public int Receive(ref byte[] bytes, int length)
    {
        return bufferReceive.Read(ref bytes, length);
    }
    // ******************************************************************************
    void SendUpdate()
    {
        if (socket.Poll(0, SelectMode.SelectWrite))
        {
            byte[] bytes = new byte[1024];

            int length = bufferSend.Read(ref bytes, bytes.Length);
            while (length > 0)
            {
                socket.Send(bytes, length, SocketFlags.None);
                length = bufferSend.Read(ref bytes, bytes.Length);
            }
        }
    }

    void ReceiveUpdate()
    {
        while (socket.Poll(0, SelectMode.SelectRead))
        {
            byte[] bytes = new byte[1024];

            int length = socket.Receive(bytes, bytes.Length, SocketFlags.None);
            if (length > 0)
            {
                bufferReceive.Write(bytes, length);
            }
        }
    }
}

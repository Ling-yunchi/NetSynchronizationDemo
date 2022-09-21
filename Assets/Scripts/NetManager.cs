using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public static class NetManager
{
    private static Socket _socket;
    private static byte[] _buffer = new byte[1024];

    public delegate void MsgListener(string msg);

    private static Dictionary<string, MsgListener> _listeners = new Dictionary<string, MsgListener>();
    private static List<string> _msgList = new List<string>();

    public static void AddListener(string msgName, MsgListener listener)
    {
        _listeners[msgName] = listener;
    }

    public static string GetDesc()
    {
        return _socket is not { Connected: true } ? "" : _socket.LocalEndPoint.ToString();
    }

    public static void Connect(string ip, int port)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(ip, port);
        _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, _socket);
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            string recvStr = System.Text.Encoding.UTF8.GetString(_buffer, 0, count);
            Debug.Log($"Socket Receive: {recvStr}");
            _msgList.Add(recvStr);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.Log($"Socket Receive Error: {e}");
        }
    }

    public static void Send(string sendStr)
    {
        if (_socket is not { Connected: true }) return;
        var sendBytes = System.Text.Encoding.UTF8.GetBytes(sendStr);
        _socket.BeginSend(sendBytes, 0, sendBytes.Length, SocketFlags.None, ar =>
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndSend(ar);
            }
            catch (SocketException e)
            {
                Debug.Log($"Socket Send Error: {e}");
            }
        }, _socket);
    }

    public static void Update()
    {
        if (_msgList.Count <= 0) return;

        foreach (var msg in _msgList)
        {
            var split = msg.Split('|');
            var msgName = split[0];
            var msgArgs = split[1];
            if (_listeners.ContainsKey(msgName))
            {
                _listeners[msgName](msgArgs);
            }
        }

        _msgList.Clear();
    }

    public static void Disconnect()
    {
        if (_socket is not { Connected: true }) return;
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }
}
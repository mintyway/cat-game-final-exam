using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
		UDPServer udpServer = new UDPServer();
		udpServer.ServerStart();
    }

    //private static void ProcessPlayerMoveData(byte[] receiveBytes)
    //{
    //    byte[] sendBytes = new byte[1];
    //    const byte Stop = 0;
    //    const byte Left = 1;
    //    const byte Right = 2;

    //    switch (receiveBytes[2])
    //    {
    //        case Left:
    //            sendBytes[0] = Left;
    //            Send(sendBytes, latestEP);
    //            Console.WriteLine();
    //            break;

    //        case Right:
    //            sendBytes[0] = Right;
    //            Send(sendBytes, latestEP);
    //            Console.WriteLine();
    //            break;

    //        default:
    //            sendBytes[0] = Stop;
    //            Send(sendBytes, latestEP);
    //            break;
    //    }
    //}

    //private static void ProcessPlayerData(byte[] receiveBytes)
    //{
    //    const byte AliveData = 0;
    //    const byte MovingData = 1;

    //    switch (receiveBytes[1])
    //    {
    //        case AliveData:
    //            GenerateNewPlayer(receiveBytes);
    //            break;

    //        case MovingData:
    //            ProcessPlayerMoveData(receiveBytes);
    //            break;

    //        default:
    //            break;
    //    }
    //}

    //private static void ProcessPlayerAliveData(byte[] receiveBytes)
    //{
    //}

    //private static void GenerateNewPlayer(byte[] receiveBytes)
    //{
    //    byte[] sendBytes = new byte[1];

    //    clientEPList.Add(latestEP);

    //    Console.WriteLine($"{clientEPList.Count - 1}번 플레이어 입장");
    //    sendBytes[0] = (byte)clientEPList.Count;

    //    Send(sendBytes, clientEPList[clientEPList.Count - 1]);
    //}

    //private static void ProcessGameData(byte[] receiveBytes)
    //{
    //    if (receiveBytes[1] == 0)
    //    {
    //        GenerateNewPlayer(receiveBytes);
    //    }
    //    else
    //    {
    //        ProcessPlayerAliveData(receiveBytes);
    //    }
    //}

    //private static void Receive()
    //{
    //    const byte GameData = 0;

    //    byte[] receiveBytes = new byte[1024];

    //    while (true)
    //    {
    //        int receiveBytesCount = socket.ReceiveFrom(receiveBytes, ref latestEP);
    //        Console.Write($"[{latestEP.ToString()}]로부터 수신된 데이터: ");

    //        for (int i = 0; i < receiveBytesCount; i++)
    //        {
    //            Console.Write($"[{receiveBytes[i]}]");
    //        }

    //        Console.WriteLine();

    //        int dataType = receiveBytes[0];

    //        if (dataType == GameData)
    //        {
    //            ProcessGameData(receiveBytes);
    //        }
    //        else
    //        {
    //            ProcessPlayerData(receiveBytes);
    //        }
    //    }
    //}

    //private static void Send(byte[] sendBytes, EndPoint sendEP)
    //{
    //    socket.SendTo(sendBytes, sendEP);
    //    Console.Write($"[{sendEP.ToString()}]로 송신된 데이터: ");

    //    for (int i = 0; i < sendBytes.Length; i++)
    //    {
    //        Console.Write($"[{sendBytes[i]}]");
    //    }

    //    Console.WriteLine();

    //    return;
    //}
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/* 바이트 규칙
 * 0    1   2
 * 0: 게임의 전반적인 데이터
 *      0: 새로운 플레이어 생성 요청
 *      N: N번 플레이어 입장 관련
 *          0: 입장
 *          1: 퇴장
 *      
 * N: N번 플레이어 관련 데이터
 *      0: 생존 관련 데이터
 *          0: 생존
 *          1: 사망
 *      1: 이동 관련 데이터
 *          0: 정지
 *          1: 좌 이동
 *          2: 우 이동
 */

class Program
{
    static Socket socket;
    static IPEndPoint serverEP;
    static EndPoint latestEP;
    static List<EndPoint> clientEPList;

    static void Main(string[] args)
    {
        // UDP소켓 생성입니다.
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverEP = new IPEndPoint(IPAddress.Any, 52000);
        latestEP = new IPEndPoint(IPAddress.None, 0);
        clientEPList = new List<EndPoint>();
        clientEPList.Add(null);

        socket.Bind(serverEP);

        // 수신 스레드 생성입니다.
        Thread receiver = new Thread(Receive);
        receiver.IsBackground = true;
        receiver.Start();
        Thread.Sleep(500);

        Console.WriteLine("야옹야옹이 게임용 서버 프로그램입니다.");
        Console.WriteLine("종료하려면 아무 키나 누르세요...");
        Console.ReadLine();

        socket.Close();
    }

    private static void ProcessPlayerMoveData(byte[] receiveBytes)
    {
        byte[] sendBytes = new byte[1];
        const byte Stop = 0;
        const byte Left = 1;
        const byte Right = 2;

        switch (receiveBytes[2])
        {
            case Left:
                sendBytes[0] = Left;
                Send(sendBytes, latestEP);
                Console.WriteLine();
                break;

            case Right:
                sendBytes[0] = Right;
                Send(sendBytes, latestEP);
                Console.WriteLine();
                break;

            default:
                sendBytes[0] = Stop;
                Send(sendBytes, latestEP);
                break;
        }
    }

    private static void ProcessPlayerData(byte[] receiveBytes)
    {
        const byte AliveData = 0;
        const byte MovingData = 1;

        switch (receiveBytes[1])
        {
            case AliveData:
                GenerateNewPlayer(receiveBytes);
                break;

            case MovingData:
                ProcessPlayerMoveData(receiveBytes);
                break;

            default:
                break;
        }
    }

    private static void ProcessPlayerAliveData(byte[] receiveBytes)
    {
    }

    private static void GenerateNewPlayer(byte[] receiveBytes)
    {
        byte[] sendBytes = new byte[1];

        clientEPList.Add(latestEP);

        Console.WriteLine($"{clientEPList.Count - 1}번 플레이어 입장");
        sendBytes[0] = (byte)clientEPList.Count;

        Send(sendBytes, clientEPList[clientEPList.Count - 1]);
    }

    private static void ProcessGameData(byte[] receiveBytes)
    {
        if (receiveBytes[1] == 0)
        {
            GenerateNewPlayer(receiveBytes);
        }
        else
        {
            ProcessPlayerAliveData(receiveBytes);
        }
    }

    private static void Receive()
    {
        const byte GameData = 0;

        byte[] receiveBytes = new byte[1024];

        while (true)
        {
            int receiveBytesCount = socket.ReceiveFrom(receiveBytes, ref latestEP);
            Console.Write($"[{latestEP.ToString()}]로부터 수신된 데이터: ");

            for (int i = 0; i < receiveBytesCount; i++)
            {
                Console.Write($"[{receiveBytes[i]}]");
            }

            Console.WriteLine();

            int dataType = receiveBytes[0];

            if (dataType == GameData)
            {
                ProcessGameData(receiveBytes);
            }
            else
            {
                ProcessPlayerData(receiveBytes);
            }
        }
    }

    private static void Send(byte[] sendBytes, EndPoint sendEP)
    {
        socket.SendTo(sendBytes, sendEP);
        Console.Write($"[{sendEP.ToString()}]로 송신된 데이터: ");

        for (int i = 0; i < sendBytes.Length; i++)
        {
            Console.Write($"[{sendBytes[i]}]");
        }

        Console.WriteLine();

        return;
    }
}

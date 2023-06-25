using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UdonDLL;

internal class UDPServer
{
	private UdpClient unicastServer;
	private UdpClient multicastServer;
	private IPEndPoint unicastServerEP;
	private IPEndPoint multicastEP;
	private IPEndPoint remoteEP;
	private static List<IPEndPoint> clientEPList;

	public void OnJoin(byte[] receiveBuffer)
	{
		// JoinPacket의 역직렬화 멤버함수를 사용 하기 위한 객체 생성
		UdonDLL.JoinPacket joinPacket = new UdonDLL.JoinPacket();

		// 연결 체크, 클라이언트 접속 리스트에 추가, 플레이어 넘버 할당 수행
		joinPacket.Deserialize(receiveBuffer);
		joinPacket.isCheck = true;
		clientEPList.Add(remoteEP);
		joinPacket.playerNumber = (PlayerNumber)(clientEPList.Count - 1);

		byte[] sendBuffer = joinPacket.Serialize();
		unicastServer.Send(sendBuffer, sendBuffer.Length, remoteEP);

		// 로그 출력
		Console.WriteLine($"[Log] [{remoteEP}] {joinPacket.playerNumber} 접속");
	}

	private void OnChat(byte[] receiveBuffer)
	{
		throw new NotImplementedException();
	}

	private void OnKeyInput(byte[] receiveBuffer)
	{
		// KeyInputPacket의 역직렬화 멤버함수를 사용 하기 위한 객체 생성
		UdonDLL.KeyInputPacket keyInputPacket = new UdonDLL.KeyInputPacket();

		// 해당 플레이어가 조작하려는 방향을 로그로 출력하기 위한 코드
		keyInputPacket.Deserialize(receiveBuffer);
		string direction;

		if (keyInputPacket.direction == UdonDLL.Direction.Left)
			direction = "좌";
		else
			direction = "우";

		// 받은 플레이어 이동 데이터를 다시 클라이언트들에게 송신
		multicastServer.Send(receiveBuffer, receiveBuffer.Length, multicastEP);

		// 로그 출력
		Console.WriteLine($"[Log] {keyInputPacket.playerNumber} {direction}이동");
	}

	private void OnPlayerStatus(byte[] receiveBuffer)
	{
		throw new NotImplementedException();
	}

	private void OnArrowSeed(byte[] receiveBuffer)
	{
		throw new NotImplementedException();
	}

	public void ReceiveWait()
	{
		while (true)
		{
			try
			{
				// 수신한 데이터의 맨 앞 1바이트는 어떤 데이터를 다루는지 기입되어 있음
				byte[] receiveBuffer = unicastServer.Receive(ref remoteEP);
				UdonDLL.PacketType receiveBufferType = (UdonDLL.PacketType)receiveBuffer[0];

				// 어떤 데이터인지 구별한 뒤 그에 맞는 로직 실행
				switch (receiveBufferType)
				{
					case UdonDLL.PacketType.Join:
						OnJoin(receiveBuffer);

						continue;

					case UdonDLL.PacketType.Chat:
						OnChat(receiveBuffer);
						continue;

					case UdonDLL.PacketType.KeyInput:
						OnKeyInput(receiveBuffer);

						continue;

					case UdonDLL.PacketType.PlayerStatus:
						OnPlayerStatus(receiveBuffer);

						continue;

					case UdonDLL.PacketType.ArrowSeed:
						OnArrowSeed(receiveBuffer);

						continue;

					default:
						continue;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}

	public void ServerStart()
	{
		// 수신 대기 스레드 실행
		Thread receiver = new Thread(ReceiveWait) { IsBackground = true };
		receiver.Start();

		Console.WriteLine("Cat Game 게임용 서버 프로그램입니다.");
		Console.WriteLine("서버가 정상적으로 실행되었습니다.");
		Console.WriteLine("서버를 종료하시려면 아무키나 누르십시오...");
		Console.ReadLine();

		// 종료시 스레드를 강제종료하고 유니캐스트, 멀티캐스트 클라이언트 닫기
		receiver.Abort();
		unicastServer.Close();
		multicastServer.Close();
	}

	public UDPServer()
	{
		// 데이터 송수신을 위한 유니캐스트, 멀티캐스트, 원격 엔드포인트 설정
		unicastServerEP = new IPEndPoint(IPAddress.Any, 52000);
		multicastEP = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 52001);
		remoteEP = new IPEndPoint(IPAddress.None, 0);

		// 데이터 송수신을 위한 유니캐스트, 멀티캐스트 클라이언트 설정
		unicastServer = new UdpClient(unicastServerEP);
		multicastServer = new UdpClient();

		// 접속한 클라이언트를 저장할 리스트
		clientEPList = new List<IPEndPoint>();

		// 멀티캐스트 그룹 참가(송신만 할 것이기에 바인드는 필요 없음)
		multicastServer.JoinMulticastGroup(multicastEP.Address);
	}
}
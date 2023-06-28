using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Udon;

internal class CatGameServer
{
	private IPEndPoint unicastServerEndPoint;
	private UdpClient unicastClient;
	private IPEndPoint multicastGroupEndPoint;
	private UdpClient multicastClient;

	private static List<IPEndPoint> clientEndPointList;

	private async Task OnJoin(byte[] receiveBuffer, IPEndPoint remoteEndPoint)
	{
		JoinPacket joinPacket = new JoinPacket(receiveBuffer);

		// 연결 체크, 클라이언트 접속 리스트에 추가, 플레이어 넘버 할당 수행
		joinPacket.isCheck = true;
		clientEndPointList.Add(remoteEndPoint);
		joinPacket.playerNumber = (PlayerNumber)((clientEndPointList.Count - 1) % 2);

		byte[] sendBuffer = joinPacket.Serialize();
		await unicastClient.SendAsync(sendBuffer, sendBuffer.Length, remoteEndPoint);

		// 접속 로그 출력
		Console.WriteLine($"[Log] [{remoteEndPoint}] {joinPacket.playerNumber} 접속");

		if (clientEndPointList.Count >= 2)
		{
			GameStatusPacket gameStatusPacket = new GameStatusPacket() { gameStatus = GameStatus.Running };

			await Task.Delay(3000);

			sendBuffer = gameStatusPacket.Serialize();
			await multicastClient.SendAsync(sendBuffer, sendBuffer.Length, multicastGroupEndPoint);

			Console.WriteLine($"[Log] 게임 시작");
		}
	}

	private async Task OnKeyInput(byte[] receiveBuffer)
	{
		KeyInputPacket keyInputPacket = new KeyInputPacket(receiveBuffer);

		byte[] sendBuffer = keyInputPacket.Serialize();
		// 받은 플레이어 이동 데이터를 다시 클라이언트들에게 송신
		await multicastClient.SendAsync(sendBuffer, sendBuffer.Length, multicastGroupEndPoint);

		// 플레이어로부터 받은 입력 방향 로그 출력
		Console.WriteLine($"[Log] {keyInputPacket.playerNumber} {keyInputPacket.direction} Move");
	}

	private async Task OnPlayerStatus(byte[] receiveBuffer)
	{
		PlayerStatusPacket playerStatusPacket = new PlayerStatusPacket(receiveBuffer);
		byte[] sendBuffer;

		if (!playerStatusPacket.isAlive)
		{
			GameStatusPacket gameStatusPacket = new GameStatusPacket() { gameStatus = GameStatus.GameOver };

			sendBuffer = gameStatusPacket.Serialize();
			await multicastClient.SendAsync(sendBuffer, sendBuffer.Length, multicastGroupEndPoint);

			Console.WriteLine($"[Log] 게임 오버");
		}

		sendBuffer = playerStatusPacket.Serialize();
		await multicastClient.SendAsync(sendBuffer, sendBuffer.Length, multicastGroupEndPoint);

		Console.WriteLine($"[Log] {playerStatusPacket.playerNumber}의 HP: {playerStatusPacket.hp}");
	}

	private async Task ReceiveWaitAsync()
	{
		while (true)
		{
			try
			{
				UdpReceiveResult receiveResult = await unicastClient.ReceiveAsync();
				PacketType packetType = (PacketType)receiveResult.Buffer[0];

				Task taskHandle;

				// 데이터 구별 후 맞는 로직 실행
				switch (packetType)
				{
					case PacketType.Join:
						taskHandle = Task.Run(() => OnJoin(receiveResult.Buffer, receiveResult.RemoteEndPoint));

						continue;

					case PacketType.KeyInput:
						taskHandle = Task.Run(() => OnKeyInput(receiveResult.Buffer));

						continue;

					case PacketType.PlayerStatus:
						taskHandle = Task.Run(() => OnPlayerStatus(receiveResult.Buffer));

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
		Task receiveWaitAsyncTask = Task.Run(() => ReceiveWaitAsync());

		Console.WriteLine("Cat Game 게임용 서버 프로그램입니다.");
		Console.WriteLine("서버가 정상적으로 실행되었습니다.");
		Console.WriteLine("서버를 종료하시려면 아무키나 누르십시오...");
		Console.ReadLine();

		// 종료시 스레드를 강제종료하고 유니캐스트, 멀티캐스트 클라이언트 닫기
		unicastClient.Close();
		multicastClient.Close();
	}

	public CatGameServer()
	{
		// 유니캐스트용 클라이언트 설정
		unicastServerEndPoint = new IPEndPoint(IPAddress.Any, 52000);
		unicastClient = new UdpClient(unicastServerEndPoint);

		// 멀티캐스트용 클라이언트 설정(송신만 할 것이기에 바인드는 필요 없음)
		multicastGroupEndPoint = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 52001);
		multicastClient = new UdpClient();
		multicastClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		multicastClient.Client.ExclusiveAddressUse = false;
		multicastClient.JoinMulticastGroup(multicastGroupEndPoint.Address);

		// 접속한 클라이언트를 저장할 리스트
		clientEndPointList = new List<IPEndPoint>();
	}
}
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;
using System;
using System.Threading.Tasks;
using UnityEngine.Analytics;
using Unity.VisualScripting;
using Udon;

public class NetworkManager : MonoBehaviour
{
	private IPEndPoint unicastServerEndPoint;
	private UdpClient unicastClient;
	private IPEndPoint multicastGroupEndPoint;
	private UdpClient multicastClient;
	private Task receiveWaitAsyncTask;

	private GameObject playerManager;
	public string serverIP;

	void Start()
	{
		if (string.IsNullOrEmpty(serverIP))
			serverIP = "127.0.0.1";

		playerManager = GameObject.Find("PlayerManager");

		unicastServerEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), 52000);
		unicastClient = new UdpClient();

		multicastGroupEndPoint = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 52001);
		multicastClient = new UdpClient(new IPEndPoint(IPAddress.Any, 52001));
		multicastClient.JoinMulticastGroup(multicastGroupEndPoint.Address);

		Application.targetFrameRate = 120;
	}

	void Update()
	{
		receiveWaitAsyncTask = MulticastReceiveWaitAsync();
	}

	public async Task SendKeyInputAsync(Udon.PlayerNumber playerNumber, Udon.Direction direction)
	{
		Udon.KeyInputPacket keyInputPacket = new Udon.KeyInputPacket();

		// 패킷에 플레이어 번호와 방향 할당
		keyInputPacket.playerNumber = playerNumber;
		keyInputPacket.direction = direction;

		byte[] sendBuffer = keyInputPacket.Serialize();
		await unicastClient.SendAsync(sendBuffer, sendBuffer.Length, unicastServerEndPoint);
	}

	public async Task<Udon.PlayerNumber> OnJoinAsync()
	{
		while (true)
		{
			Udon.JoinPacket joinPacket = new Udon.JoinPacket() { isCheck = false };

			// 서버에 접속 요청을 전송
			byte[] sendBuffer = joinPacket.Serialize();
			await unicastClient.SendAsync(sendBuffer, sendBuffer.Length, unicastServerEndPoint);

			// 서버 접속 확인 수신
			UdpReceiveResult receiveResult = await unicastClient.ReceiveAsync();
			joinPacket.Deserialize(receiveResult.Buffer);

			// 서버 접속 실패 시 재시도
			if (joinPacket.isCheck == false)
				continue;

			return joinPacket.playerNumber;
		}
	}

	private void OnKeyInput(byte[] receiveBuffer)
	{
		Udon.KeyInputPacket keyInputPacket = new Udon.KeyInputPacket(receiveBuffer);

		// 플레이어 번호와 방향을 MovePlayer의 인자로 넘기는 코드
		if (keyInputPacket.direction == Udon.Direction.Left)
			playerManager.GetComponent<PlayerManager>().MovePlayer(keyInputPacket.playerNumber, Udon.Direction.Left);
		else
			playerManager.GetComponent<PlayerManager>().MovePlayer(keyInputPacket.playerNumber, Udon.Direction.Right);
	}

	//멀티캐스트 수신대기
	private async Task MulticastReceiveWaitAsync()
	{
		while (multicastClient.Available != 0)
		{
			try
			{
				UdpReceiveResult receiveResult = await multicastClient.ReceiveAsync();
				Udon.PacketType packetType = (Udon.PacketType)receiveResult.Buffer[0];

				// 데이터 구별 후 맞는 로직 실행
				switch (packetType)
				{
					case PacketType.Chat:
						break;

					case PacketType.KeyInput:
						OnKeyInput(receiveResult.Buffer);

						break;

					case PacketType.PlayerStatus:
						break;

					case PacketType.ArrowSeed:
						break;

					default:
						break;
				}
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}
	}
}
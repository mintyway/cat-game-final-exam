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
	private GameManager gameManager;
	private PlayerManager playerManager;
	public string serverIP;
	public bool IsRunning { get; private set; } = false;

	private IPEndPoint unicastServerEndPoint;
	private UdpClient unicastClient;
	private IPEndPoint multicastGroupEndPoint;
	private UdpClient multicastClient;

	void Start()
	{
		if (string.IsNullOrEmpty(serverIP))
			serverIP = "127.0.0.1";

		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

		unicastServerEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), 52000);
		unicastClient = new UdpClient();

		multicastGroupEndPoint = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 52001);
		multicastClient = new UdpClient();
		multicastClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		multicastClient.Client.ExclusiveAddressUse = false;
		multicastClient.Client.Bind(new IPEndPoint(IPAddress.Any, 52001));
		multicastClient.JoinMulticastGroup(multicastGroupEndPoint.Address);

		// 디버그용
		//IsRunning = true;
	}

	async void Update()
	{
		await MulticastReceiveWaitAsync();
	}

	/* 함수 설명:
	 * 다른 스크립트에서 서버로 데이터를 전송해야할 때 사용하는 함수이다.
	 * 
	 * 입출력 설명:
	 * 직렬화한 패킷이 담긴 byte 배열을 매개변수로 사용한다.
	 */
	public void SendServer(byte[] serializedPacket)
	{
		try
		{
			unicastClient.Send(serializedPacket, serializedPacket.Length, unicastServerEndPoint);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}

	/* 함수 설명:
	 * 서버에 접속을 요청하고 플레이어 번호를 할당 받는 함수이다.
	 * 
	 * 입출력 설명:
	 * 서버로부터 할당 받은 플레이어 번호를 PlayerNumber 타입으로 반환하는 비동기 함수이다. 
	 * 
	 * 사용 예시:
	 * Task task = OnJoinAsync();
	 * PlayerNumber playerNumber = await task;
	 */
	public PlayerNumber JoinAndAllocatePlayerNumber()
	{
		while (true)
		{
			JoinPacket joinPacket = new JoinPacket() { isCheck = false };

			byte[] sendBuffer = joinPacket.Serialize();
			unicastClient.Send(sendBuffer, sendBuffer.Length, unicastServerEndPoint);

			IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.None, 0);
			byte[] receiveBuffer = unicastClient.Receive(ref remoteEndPoint);
			joinPacket.Deserialize(receiveBuffer);

			if (!joinPacket.isCheck)        // 할당 실패시 다시 루프
				continue;

			return joinPacket.playerNumber;
		}
	}

	/* 함수 설명:
	 * 서버에서 받아온 게임 상태 데이터를 클라이언트로 반영하기위해 해당 스크립트로 넘기는 기능을 하는 함수이다.
	 * 
	 * 입출력 설명:
	 * GameStatusPacket이 직렬화 되어 있는 바이트 배열을 매개변수로 사용한다.
	 */
	private async Task OnGameStatus(byte[] serializedGamaStatusPacket)
	{
		GameStatusPacket gameStatusPacket = new GameStatusPacket(serializedGamaStatusPacket);

		switch (gameStatusPacket.gameStatus)
		{
			case GameStatus.Waiting:
				gameManager.RenderWaiting();
				IsRunning = false;

				break;

			case GameStatus.Running:
				gameManager.RenderGame();
				await Task.Delay(3000);

				IsRunning = true;

				break;

			case GameStatus.GameOver:
				IsRunning = false;

				break;

			default:
				break;
		}
	}

	/* 함수 설명:
	 * 서버에서 받아온 키입력 데이터를 클라이언트에 반영하는 함수이다.
	 * 
	 * 입출력 설명:
	 * KeyInputPacket이 직렬화 되어 있는 바이트 배열을 매개변수로 사용한다
	 */
	private void OnKeyInput(byte[] serializedKeyInputPacket)
	{
		KeyInputPacket keyInputPacket = new KeyInputPacket(serializedKeyInputPacket);

		if (keyInputPacket.direction == Direction.Left)
			playerManager.Players[keyInputPacket.playerNumber].transform.Translate(-playerManager.Speed, 0, 0);
		else
			playerManager.Players[keyInputPacket.playerNumber].transform.Translate(playerManager.Speed, 0, 0);
	}

	/* 함수 설명:
	 * 서버로부터 받아온 HP 정보나 생존 정보를 클라이언트에 반영하는 함수이다.
	 * 만약 한 플레이어가 죽으면 그대로 승패를 출력한다.
	 * 
	 * 입출력 설명:
	 * PlayerStatusPacket이 직렬화 되어 있는 바이트 배열을 매개변수로 사용한다.
	 */
	private void OnPlayerStatus(byte[] serializedPlayerStatusPacket)
	{
		PlayerStatusPacket playerStatusPacket = new PlayerStatusPacket(serializedPlayerStatusPacket);

		playerManager.PlayerHPs[playerStatusPacket.playerNumber] = playerStatusPacket.hp;
		playerManager.IsAlive[playerStatusPacket.playerNumber] = playerStatusPacket.isAlive;
		gameManager.RenderPlayerHP(playerStatusPacket.playerNumber, playerManager.PlayerHPs[playerStatusPacket.playerNumber]);

		if (!playerStatusPacket.isAlive)		// 한 플레이어가 죽게되면 승패 출력
		{
			gameManager.RenderGameOver();
		}
	}

	/* 함수 설명:
	 * 서버로부터 비동기 수신대기를 하며, 수신한 데이터를 패킷 종류별로 구분해 그에 맞는 로직을 실행할 수 있도록 도와주는 함수이다.
	 */
	private async Task MulticastReceiveWaitAsync()
	{
		while (multicastClient.Available != 0)
		{
			try
			{
				UdpReceiveResult receiveResult = await multicastClient.ReceiveAsync();
				PacketType packetType = (PacketType)receiveResult.Buffer[0];        // 패킷 타입 헤더를 enum에 할당

				switch (packetType)
				{
					case PacketType.GameStatus:
						await OnGameStatus(receiveResult.Buffer);
						break;

					case PacketType.KeyInput:
						OnKeyInput(receiveResult.Buffer);

						break;

					case PacketType.PlayerStatus:
						OnPlayerStatus(receiveResult.Buffer);

						break;

					case PacketType.ArrowRandomSeed:
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
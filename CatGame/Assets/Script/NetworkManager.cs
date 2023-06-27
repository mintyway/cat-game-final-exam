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
	private Task receiveWaitAsyncTask;

	void Start()
	{
		if (string.IsNullOrEmpty(serverIP))
			serverIP = "127.0.0.1";

		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

		unicastServerEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), 52000);
		unicastClient = new UdpClient();

		multicastGroupEndPoint = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 52001);
		multicastClient = new UdpClient(new IPEndPoint(IPAddress.Any, 52001));
		multicastClient.JoinMulticastGroup(multicastGroupEndPoint.Address);
	}

	void Update()
	{
		receiveWaitAsyncTask = MulticastReceiveWaitAsync();
	}

	public async Task SendPlayerStatusAsync(PlayerNumber playerNumber, float hp)
	{
		PlayerStatusPacket playerStatusPacket = new PlayerStatusPacket() { playerNumber = playerNumber, isAlive = true, hp = hp };

		if (playerStatusPacket.hp <= 0)
		{
			playerStatusPacket.hp = 0f;
			playerStatusPacket.isAlive = false;
		}

		byte[] sendBuffer = playerStatusPacket.Serialize();
		await unicastClient.SendAsync(sendBuffer, sendBuffer.Length, unicastServerEndPoint);
	}

	/* 함수 설명
	 * PlayerManager로부터 전달된 enum 타입 플레이어 번호와 enum 타입 이동 방향을 매개변수로 사용하여 서버로 전송하는 비동기 함수이다.
	 * 플레이어 번호는 나중에 서버로 부터 키 입력 데이터를 다시 받을 때 어떤 캐릭터를 움직여야할 지 구별하기위해 사용한다.
	 * 
	 * 사용 예시:
	 * Task task = SendKeyInputAsync(playerNumber, direction);
	 */
	public async Task SendKeyInputAsync(PlayerNumber playerNumber, Direction direction)
	{
		KeyInputPacket keyInputPacket = new KeyInputPacket() { playerNumber = playerNumber, direction = direction };

		byte[] sendBuffer = keyInputPacket.Serialize();
		await unicastClient.SendAsync(sendBuffer, sendBuffer.Length, unicastServerEndPoint);
	}

	/* 함수 설명
	 * 서버로부터 할당 받은 enum 타입 플레이어 번호를 반환하는 비동기 함수이다. 
	 * 서버에 접속을 요청하고 플레이어 번호를 할당 받는데 사용한다.
	 * 번호 할당받기에 실패하면 무한 루프를 통해 할당 받을때까지 시도한다.
	 * 비동기 함수로 구현한 이유는 혹시 무한 루프를 돌게 되었을때 메인 스레드가 블로킹되지 않기 위함이다.
	 * 
	 * 사용 예시:
	 * Task task = OnJoinAsync();
	 * PlayerNumber playerNumber = await task;
	 */
	public async Task<PlayerNumber> OnJoinAsync()
	{
		while (true)
		{
			JoinPacket joinPacket = new JoinPacket() { isCheck = false };

			byte[] sendBuffer = joinPacket.Serialize();
			await unicastClient.SendAsync(sendBuffer, sendBuffer.Length, unicastServerEndPoint);

			UdpReceiveResult receiveResult = await unicastClient.ReceiveAsync();
			joinPacket.Deserialize(receiveResult.Buffer);

			if (joinPacket.isCheck == false)        // isCheck는 서버와 접속에 성공했는지를 나타내는 멤버변수
				continue;

			return joinPacket.playerNumber;
		}
	}

	private void OnGameStatus(byte[] receiveBuffer)
	{
		GameStatusPacket gameStatusPacket = new GameStatusPacket(receiveBuffer);

		switch (gameStatusPacket.gameStatus)
		{
			case GameStatus.Waiting:
				gameManager.RenderWaiting(true);
				gameManager.RenderGameOver(false);

				IsRunning = false;

				break;

			case GameStatus.Running:
				gameManager.RenderWaiting(false);
				gameManager.RenderGameOver(false);

				IsRunning = true;

				break;

			case GameStatus.GameOver:
				gameManager.RenderWaiting(false);
				gameManager.RenderGameOver(true);

				IsRunning = false;

				break;

			default:
				break;
		}
	}

	/* 함수 설명
	 * 직렬화되어 있는 byte 배열 타입의 키입력 패킷을 입력받고
	 * 역직렬화 후 PlayerManger의 MovePlayer 함수에 필요한 인자를 넣어 호출하는 함수이다.
	 * 이 함수는 서버에서 받아온 키입력 데이터를 실제로 플레이어를 움직일 수 있도록
	 * PlayerManger의 MovePlayer를 호출하는 함수이다.
	 * 
	 * 사용 예시:
	 * OnInputKey(receiveBuffer);
	 */
	private void OnKeyInput(byte[] receiveBuffer)
	{
		KeyInputPacket keyInputPacket = new KeyInputPacket(receiveBuffer);

		if (keyInputPacket.direction == Direction.Left)
			playerManager.MovePlayer(keyInputPacket.playerNumber, Direction.Left);
		else
			playerManager.MovePlayer(keyInputPacket.playerNumber, Direction.Right);
	}

	private void OnPlayerStatus(byte[] receiveBuffer)
	{
		PlayerStatusPacket playerStatusPacket = new PlayerStatusPacket(receiveBuffer);

		playerManager.ResponseDecreaseHP(playerStatusPacket.playerNumber, playerStatusPacket.hp);
	}

	/* 함수 설명
	 * 멀티캐스트 주소로 오는 데이터를비동기 수신을 하고,
	 * 수신받은 데이터 타입에 따라 그에 맞는 로직으로 실행할 수 있도록 도와주는 핸들링을 지원하는 함수입니다.
	 * 
	 * 사용 예시:
	 * Task task = MulticastReceiveWaitAsync();
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
						OnGameStatus(receiveResult.Buffer);
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
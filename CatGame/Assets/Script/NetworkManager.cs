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

	/* �Լ� ����
	 * PlayerManager�κ��� ���޵� enum Ÿ�� �÷��̾� ��ȣ�� enum Ÿ�� �̵� ������ �Ű������� ����Ͽ� ������ �����ϴ� �񵿱� �Լ��̴�.
	 * �÷��̾� ��ȣ�� ���߿� ������ ���� Ű �Է� �����͸� �ٽ� ���� �� � ĳ���͸� ���������� �� �����ϱ����� ����Ѵ�.
	 * 
	 * ��� ����:
	 * Task task = SendKeyInputAsync(playerNumber, direction);
	 */
	public async Task SendKeyInputAsync(PlayerNumber playerNumber, Direction direction)
	{
		KeyInputPacket keyInputPacket = new KeyInputPacket() { playerNumber = playerNumber, direction = direction };

		byte[] sendBuffer = keyInputPacket.Serialize();
		await unicastClient.SendAsync(sendBuffer, sendBuffer.Length, unicastServerEndPoint);
	}

	/* �Լ� ����
	 * �����κ��� �Ҵ� ���� enum Ÿ�� �÷��̾� ��ȣ�� ��ȯ�ϴ� �񵿱� �Լ��̴�. 
	 * ������ ������ ��û�ϰ� �÷��̾� ��ȣ�� �Ҵ� �޴µ� ����Ѵ�.
	 * ��ȣ �Ҵ�ޱ⿡ �����ϸ� ���� ������ ���� �Ҵ� ���������� �õ��Ѵ�.
	 * �񵿱� �Լ��� ������ ������ Ȥ�� ���� ������ ���� �Ǿ����� ���� �����尡 ���ŷ���� �ʱ� �����̴�.
	 * 
	 * ��� ����:
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

			if (joinPacket.isCheck == false)        // isCheck�� ������ ���ӿ� �����ߴ����� ��Ÿ���� �������
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

	/* �Լ� ����
	 * ����ȭ�Ǿ� �ִ� byte �迭 Ÿ���� Ű�Է� ��Ŷ�� �Է¹ް�
	 * ������ȭ �� PlayerManger�� MovePlayer �Լ��� �ʿ��� ���ڸ� �־� ȣ���ϴ� �Լ��̴�.
	 * �� �Լ��� �������� �޾ƿ� Ű�Է� �����͸� ������ �÷��̾ ������ �� �ֵ���
	 * PlayerManger�� MovePlayer�� ȣ���ϴ� �Լ��̴�.
	 * 
	 * ��� ����:
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

	/* �Լ� ����
	 * ��Ƽĳ��Ʈ �ּҷ� ���� �����͸��񵿱� ������ �ϰ�,
	 * ���Ź��� ������ Ÿ�Կ� ���� �׿� �´� �������� ������ �� �ֵ��� �����ִ� �ڵ鸵�� �����ϴ� �Լ��Դϴ�.
	 * 
	 * ��� ����:
	 * Task task = MulticastReceiveWaitAsync();
	 */
	private async Task MulticastReceiveWaitAsync()
	{
		while (multicastClient.Available != 0)
		{
			try
			{
				UdpReceiveResult receiveResult = await multicastClient.ReceiveAsync();
				PacketType packetType = (PacketType)receiveResult.Buffer[0];        // ��Ŷ Ÿ�� ����� enum�� �Ҵ�

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
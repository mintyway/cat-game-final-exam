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

		// ����׿�
		//IsRunning = true;
	}

	async void Update()
	{
		await MulticastReceiveWaitAsync();
	}

	/* �Լ� ����:
	 * �ٸ� ��ũ��Ʈ���� ������ �����͸� �����ؾ��� �� ����ϴ� �Լ��̴�.
	 * 
	 * ����� ����:
	 * ����ȭ�� ��Ŷ�� ��� byte �迭�� �Ű������� ����Ѵ�.
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

	/* �Լ� ����:
	 * ������ ������ ��û�ϰ� �÷��̾� ��ȣ�� �Ҵ� �޴� �Լ��̴�.
	 * 
	 * ����� ����:
	 * �����κ��� �Ҵ� ���� �÷��̾� ��ȣ�� PlayerNumber Ÿ������ ��ȯ�ϴ� �񵿱� �Լ��̴�. 
	 * 
	 * ��� ����:
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

			if (!joinPacket.isCheck)        // �Ҵ� ���н� �ٽ� ����
				continue;

			return joinPacket.playerNumber;
		}
	}

	/* �Լ� ����:
	 * �������� �޾ƿ� ���� ���� �����͸� Ŭ���̾�Ʈ�� �ݿ��ϱ����� �ش� ��ũ��Ʈ�� �ѱ�� ����� �ϴ� �Լ��̴�.
	 * 
	 * ����� ����:
	 * GameStatusPacket�� ����ȭ �Ǿ� �ִ� ����Ʈ �迭�� �Ű������� ����Ѵ�.
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

	/* �Լ� ����:
	 * �������� �޾ƿ� Ű�Է� �����͸� Ŭ���̾�Ʈ�� �ݿ��ϴ� �Լ��̴�.
	 * 
	 * ����� ����:
	 * KeyInputPacket�� ����ȭ �Ǿ� �ִ� ����Ʈ �迭�� �Ű������� ����Ѵ�
	 */
	private void OnKeyInput(byte[] serializedKeyInputPacket)
	{
		KeyInputPacket keyInputPacket = new KeyInputPacket(serializedKeyInputPacket);

		if (keyInputPacket.direction == Direction.Left)
			playerManager.Players[keyInputPacket.playerNumber].transform.Translate(-playerManager.Speed, 0, 0);
		else
			playerManager.Players[keyInputPacket.playerNumber].transform.Translate(playerManager.Speed, 0, 0);
	}

	/* �Լ� ����:
	 * �����κ��� �޾ƿ� HP ������ ���� ������ Ŭ���̾�Ʈ�� �ݿ��ϴ� �Լ��̴�.
	 * ���� �� �÷��̾ ������ �״�� ���и� ����Ѵ�.
	 * 
	 * ����� ����:
	 * PlayerStatusPacket�� ����ȭ �Ǿ� �ִ� ����Ʈ �迭�� �Ű������� ����Ѵ�.
	 */
	private void OnPlayerStatus(byte[] serializedPlayerStatusPacket)
	{
		PlayerStatusPacket playerStatusPacket = new PlayerStatusPacket(serializedPlayerStatusPacket);

		playerManager.PlayerHPs[playerStatusPacket.playerNumber] = playerStatusPacket.hp;
		playerManager.IsAlive[playerStatusPacket.playerNumber] = playerStatusPacket.isAlive;
		gameManager.RenderPlayerHP(playerStatusPacket.playerNumber, playerManager.PlayerHPs[playerStatusPacket.playerNumber]);

		if (!playerStatusPacket.isAlive)		// �� �÷��̾ �װԵǸ� ���� ���
		{
			gameManager.RenderGameOver();
		}
	}

	/* �Լ� ����:
	 * �����κ��� �񵿱� ���Ŵ�⸦ �ϸ�, ������ �����͸� ��Ŷ �������� ������ �׿� �´� ������ ������ �� �ֵ��� �����ִ� �Լ��̴�.
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
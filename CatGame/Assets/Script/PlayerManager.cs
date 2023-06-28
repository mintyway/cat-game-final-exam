using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using Udon;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerManager : MonoBehaviour
{
	private NetworkManager networkManager;
	public Dictionary<PlayerNumber, GameObject> Players { get; private set; } = new Dictionary<PlayerNumber, GameObject>();

	private readonly float initialHP = 10f;
	public Dictionary<PlayerNumber, float> PlayerHPs { get; private set; } = new Dictionary<PlayerNumber, float>();
	public float Speed { get; private set; } = 0.2f;

	public PlayerNumber PlayingPlayerNumber { get; private set; }
	public PlayerNumber NonPlayingPlayerNumber { get; private set; }

	private readonly float targetFrameTime = 1f / 60f;
	private float accumulatedTime = 0f;

	void Start()
	{
		networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

		Players[PlayerNumber.Player1] = GameObject.Find("Player1");
		Players[PlayerNumber.Player2] = GameObject.Find("Player2");

		PlayerHPs[PlayerNumber.Player1] = initialHP;
		PlayerHPs[PlayerNumber.Player2] = initialHP;

		PlayingPlayerNumber = networkManager.JoinAndAllocatePlayerNumber();
		NonPlayingPlayerNumber = (PlayerNumber)(((byte)PlayingPlayerNumber + 1) % 2);
	}

	void Update()
	{
		if (!networkManager.IsRunning)
			return;

		accumulatedTime += Time.deltaTime;

		// 1/60�ʿ� �ѹ� �Է��� ���� �� �ֵ��� �ϴ� �ڵ�
		if (accumulatedTime >= targetFrameTime)
		{
			if (PlayerHPs[PlayingPlayerNumber] <= 0)
				return;

			SendInputKeyboard();

			accumulatedTime -= targetFrameTime;
		}
	}

	/* �Լ� ����:
	 * Ŭ���̾�Ʈ���� �Էµ� Ű�� ������ �����ϴ� �Լ��̴�.
	 * 
	 * ����� ����:
	 * ���� Ŭ���̾�Ʈ���� �÷��� ���� ��ȣ �÷��̾� ��ȣ PlayerNumber�� �Էµ� ���� Direction�� �Ű������� ����Ѵ�.
	 */
	private void SendInputKeyboard()
	{
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			KeyInputPacket keyInputPacket = new KeyInputPacket() { playerNumber = PlayingPlayerNumber, direction = Direction.Left };

			networkManager.SendServer(keyInputPacket.Serialize());
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			KeyInputPacket keyInputPacket = new KeyInputPacket() { playerNumber = PlayingPlayerNumber, direction = Direction.Right };

			networkManager.SendServer(keyInputPacket.Serialize());
		}
	}

	/* �Լ� ����:
	 * Ŭ���̾�Ʈ���� �߻��� HP���Ҹ� ������ �����ϴ� �Լ��̴�.
	 * 
	 * ����� ����:
	 * HP�� ���ҽ�Ű�� float Ÿ���� �Ű������� ����Ѵ�.
	 */
	public void SendDecreaseHP(float damage)
	{
		PlayerStatusPacket playerStatusPacket = new PlayerStatusPacket()
		{
			playerNumber = PlayingPlayerNumber, isAlive = true, hp = PlayerHPs[PlayingPlayerNumber] - damage
		};

		if (playerStatusPacket.hp <= 0)
		{
			playerStatusPacket.hp = 0f;
			playerStatusPacket.isAlive = false;
		}

		networkManager.SendServer(playerStatusPacket.Serialize());
	}
}
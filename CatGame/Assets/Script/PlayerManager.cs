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

		// 1/60초에 한번 입력을 받을 수 있도록 하는 코드
		if (accumulatedTime >= targetFrameTime)
		{
			if (PlayerHPs[PlayingPlayerNumber] <= 0)
				return;

			SendInputKeyboard();

			accumulatedTime -= targetFrameTime;
		}
	}

	/* 함수 설명:
	 * 클라이언트에서 입력된 키를 서버로 전송하는 함수이다.
	 * 
	 * 입출력 설명:
	 * 현재 클라이언트에서 플레이 중인 번호 플레이어 번호 PlayerNumber와 입력된 방향 Direction을 매개변수로 사용한다.
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

	/* 함수 설명:
	 * 클라이언트에서 발생한 HP감소를 서버로 전송하는 함수이다.
	 * 
	 * 입출력 설명:
	 * HP를 감소시키는 float 타입을 매개변수로 사용한다.
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
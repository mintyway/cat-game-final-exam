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
	private GameManager gameManager;
	private NetworkManager networkManager;
	public Dictionary<PlayerNumber, GameObject> Players { get; private set; } = new Dictionary<PlayerNumber, GameObject>();
	public PlayerNumber PlayingPlayerNumber { get; private set; }
	public PlayerNumber NonPlayingPlayerNumber { get; private set; }

	public float initialHP;
	public Dictionary<PlayerNumber, float> PlayerHPs { get; private set; } = new Dictionary<PlayerNumber, float>();
	public float speed;

	private float targetFrameTime = 1f / 60f;
	private float accumulatedTime = 0f;

	Task taskDecreaseHP;
	Task taskKeyInputAsync;
	Task taskLeftKeyInputAsync;
	Task taskRightKeyInputAsync;

	async void Start()
	{
		if (initialHP == 0)
			initialHP = 10f;

		if (speed == 0)
			speed = 0.2f;

		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

		Players[PlayerNumber.Player1] = GameObject.Find("Player1");
		Players[PlayerNumber.Player2] = GameObject.Find("Player2");

		PlayerHPs[PlayerNumber.Player1] = initialHP;
		PlayerHPs[PlayerNumber.Player2] = initialHP;

		Task<PlayerNumber> joinTask = networkManager.OnJoinAsync();
		PlayingPlayerNumber = await joinTask;
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

			taskKeyInputAsync = RequestInputKeyboard();

			accumulatedTime -= targetFrameTime;
		}
	}

	/* 함수 설명
	 * 클라이언트로부터 입력된 키를 NetworkManager의 SendKeyInputAsync함수의 인자로 사용해 호출하는 비동기 함수이다.
	 * 
	 * 사용 예시:
	 * Task task = InputKeyboard();
	 */
	private async Task RequestInputKeyboard()
	{
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			taskLeftKeyInputAsync = networkManager.SendKeyInputAsync(PlayingPlayerNumber, Direction.Left);
			await taskLeftKeyInputAsync;
		}

		else if (Input.GetKey(KeyCode.RightArrow))
		{
			taskRightKeyInputAsync = networkManager.SendKeyInputAsync(PlayingPlayerNumber, Direction.Right);
			await taskRightKeyInputAsync;
		}
	}

	/* 함수 설명
	 * 서버로부터 되받은 enum 타입 플레이어 번호와 enum 타입 방향을 매개변수로 사용해 플레이중인 캐릭터를 움직이는 함수이다.
	 * 서버로 전송했던 키입력을 그대로 다시 되받은 NetworkManager에서 캐릭터를 움직이는데 사용한다.
	 * 
	 * 사용 예시:
	 * networkManager.MovePlayer(playerNumber, direction);
	 */
	public void MovePlayer(PlayerNumber playerNumber, Direction direction)
	{
		if (direction == Direction.Left)
		{
			Players[playerNumber].transform.Translate(-speed, 0, 0);
		}
		else
		{
			Players[playerNumber].transform.Translate(speed, 0, 0);
		}
	}

	public void RequestDecreaseHP(float damage)
	{
		taskDecreaseHP = networkManager.SendPlayerStatusAsync(PlayingPlayerNumber, PlayerHPs[PlayingPlayerNumber] - damage);
	}

	public void ResponseDecreaseHP(PlayerNumber playerNumber, float hp)
	{
		PlayerHPs[playerNumber] = hp;
		gameManager.RenderPlayerHP(playerNumber, PlayerHPs[playerNumber]);
	}

	/* 함수 설명
	 * 플레이어 둘 중 누군가 하나라도 죽으면 true를 반환하는 함수이다.
	 * 게임을 멈추고 결과를 출력할때 사용한다.
	 * 
	 * 사용 예시:
	 * if (playerManager.IsAnyPlayerDead())
	 */
	public bool IsAnyPlayerDead()
	{
		if ((PlayerHPs[PlayerNumber.Player1] <= 0) || (PlayerHPs[PlayerNumber.Player1] <= 0))
			return true;

		return false;
	}
}
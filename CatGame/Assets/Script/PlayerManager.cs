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

		// 1/60�ʿ� �ѹ� �Է��� ���� �� �ֵ��� �ϴ� �ڵ�
		if (accumulatedTime >= targetFrameTime)
		{
			if (PlayerHPs[PlayingPlayerNumber] <= 0)
				return;

			taskKeyInputAsync = RequestInputKeyboard();

			accumulatedTime -= targetFrameTime;
		}
	}

	/* �Լ� ����
	 * Ŭ���̾�Ʈ�κ��� �Էµ� Ű�� NetworkManager�� SendKeyInputAsync�Լ��� ���ڷ� ����� ȣ���ϴ� �񵿱� �Լ��̴�.
	 * 
	 * ��� ����:
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

	/* �Լ� ����
	 * �����κ��� �ǹ��� enum Ÿ�� �÷��̾� ��ȣ�� enum Ÿ�� ������ �Ű������� ����� �÷������� ĳ���͸� �����̴� �Լ��̴�.
	 * ������ �����ߴ� Ű�Է��� �״�� �ٽ� �ǹ��� NetworkManager���� ĳ���͸� �����̴µ� ����Ѵ�.
	 * 
	 * ��� ����:
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

	/* �Լ� ����
	 * �÷��̾� �� �� ������ �ϳ��� ������ true�� ��ȯ�ϴ� �Լ��̴�.
	 * ������ ���߰� ����� ����Ҷ� ����Ѵ�.
	 * 
	 * ��� ����:
	 * if (playerManager.IsAnyPlayerDead())
	 */
	public bool IsAnyPlayerDead()
	{
		if ((PlayerHPs[PlayerNumber.Player1] <= 0) || (PlayerHPs[PlayerNumber.Player1] <= 0))
			return true;

		return false;
	}
}
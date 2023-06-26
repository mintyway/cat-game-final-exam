using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Udon;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerManager : MonoBehaviour
{
	private GameObject networkManager;
	private GameObject gameManager;
	private GameObject[] player = new GameObject[2];
	private Udon.PlayerNumber playerNumber;

	public float initialHP;
	private float[] hp = new float[2];
	public float speed;

	Task taskKeyInputAsync;
	Task taskLeftKeyInputAsync;
	Task taskRightKeyInputAsync;

	async void Start()
	{
		if (initialHP == 0)
			initialHP = 10f;

		if (speed == 0)
			speed = 0.2f;

		networkManager = GameObject.Find("NetworkManager");
		gameManager = GameObject.Find("GameManager");

		player[(byte)Udon.PlayerNumber.Player1] = GameObject.Find("Player1");
		player[(byte)Udon.PlayerNumber.Player2] = GameObject.Find("Player2");

		hp[(byte)Udon.PlayerNumber.Player1] = initialHP;
		hp[(byte)Udon.PlayerNumber.Player2] = initialHP;

		Task<Udon.PlayerNumber> joinTask = networkManager.GetComponent<NetworkManager>().OnJoinAsync();
		playerNumber = await joinTask;
	}

	void Update()
	{
		if (hp[(byte)playerNumber] <= 0)
			return;

		taskKeyInputAsync = InputKeyboard();
	}

	// 키보드 입력을 반영하는 코드
	private async Task InputKeyboard()
	{
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			taskLeftKeyInputAsync = networkManager.GetComponent<NetworkManager>().SendKeyInputAsync(playerNumber, Udon.Direction.Left);
			await taskLeftKeyInputAsync;
		}

		else if (Input.GetKey(KeyCode.RightArrow))
		{
			taskRightKeyInputAsync = networkManager.GetComponent<NetworkManager>().SendKeyInputAsync(playerNumber, Udon.Direction.Right);
			await taskRightKeyInputAsync;
		}
	}

	public void MovePlayer(Udon.PlayerNumber playerNumber, Udon.Direction direction)
	{
		if (direction == Direction.Left)
		{
			player[(byte)playerNumber].transform.Translate(-speed, 0, 0);
		}
		else
		{
			player[(byte)playerNumber].transform.Translate(speed, 0, 0);
		}
	}

	// 체력을 깎는 함수
	//public void DecreaseHP(float damage)
	//{
	//	hp -= damage;
	//	gameManager.GetComponent<GameManager>().RenderPlayerHP(hp);

	//	if (hp <= 0)
	//		gameManager.GetComponent<GameManager>().RenderGameOver();
	//}
}
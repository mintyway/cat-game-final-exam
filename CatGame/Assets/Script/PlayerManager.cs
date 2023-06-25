using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using UdonDLL;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerManager : MonoBehaviour
{
	private UdonDLL.PlayerNumber playerNumber;

	private GameObject networkManager;
	private GameObject gameManager;
	private GameObject[] player = new GameObject[2];

	public float initialHP;
	private float[] hp = new float[2];

	public float speed;

	void Start()
	{
		networkManager = GameObject.Find("NetworkManager");
		gameManager = GameObject.Find("GameManager");

		player[(byte)UdonDLL.PlayerNumber.Player1] = GameObject.Find("Player1");
		player[(byte)UdonDLL.PlayerNumber.Player2] = GameObject.Find("Player2");

		if (initialHP == 0)
			initialHP = 10f;

		hp[(byte)UdonDLL.PlayerNumber.Player1] = initialHP;
		hp[(byte)UdonDLL.PlayerNumber.Player2] = initialHP;

		if (speed == 0)
			speed = 0.2f;

		playerNumber = networkManager.GetComponent<NetworkManager>().OnJoin();
	}

	void Update()
	{
		if (hp[(byte)playerNumber] <= 0)
			return;

		InputKeyboard();
	}

	// 키보드 입력을 반영하는 코드
	private void InputKeyboard()
	{
		if (Input.GetKey(KeyCode.LeftArrow))
			networkManager.GetComponent<NetworkManager>().OnKeyInput(playerNumber, UdonDLL.Direction.Left);
		else if (Input.GetKey(KeyCode.RightArrow))
			networkManager.GetComponent<NetworkManager>().OnKeyInput(playerNumber, UdonDLL.Direction.Right);
	}

	public void MovePlayer(UdonDLL.PlayerNumber playerNumber, UdonDLL.Direction direction)
	{
		if (direction == Direction.Left)
			player[(byte)playerNumber].transform.Translate(-speed, 0, 0);
		else
			player[(byte)playerNumber].transform.Translate(speed, 0, 0);
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
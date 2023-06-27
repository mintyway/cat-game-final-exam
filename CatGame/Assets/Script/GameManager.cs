using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using Udon;

public class GameManager : MonoBehaviour
{
	private PlayerManager playerManager;
	private NetworkManager networkManager;

	private GameObject waiting;
	private GameObject gameOver;
	public Dictionary<PlayerNumber, GameObject> HPGauges { get; private set; }

	private RectTransform waitingRectTransform;
	private RectTransform gameOverRectTransform;

	void Start()
	{
		playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
		networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

		waiting = GameObject.Find("Waiting");
		gameOver = GameObject.Find("GameOver");
		HPGauges[PlayerNumber.Player1] = GameObject.Find("Player1HPGauge");
		HPGauges[PlayerNumber.Player2] = GameObject.Find("Player2HPGauge");

		waitingRectTransform = waiting.GetComponent<RectTransform>();
		gameOverRectTransform = gameOver.GetComponent<RectTransform>();
	}

	void Update()
	{
	}

	// 수정 필요
	public void RenderPlayerHP(float playerHP)
	{
		if (playerHP <= 0)
			playerHP = 0;

		HPGauges[PlayerNumber.Player1].GetComponent<Image>().fillAmount = playerHP / 10;
	}

	public void RenderWaiting(bool isRender)
	{
		if (isRender)
			waitingRectTransform.anchoredPosition = Vector2.zero;
		else
			waitingRectTransform.anchoredPosition = new Vector2(0, -1080);
	}

	public void RenderGameOver(bool isRender)
	{
		if (isRender)
			gameOverRectTransform.anchoredPosition = Vector2.zero;
		else
			gameOverRectTransform.anchoredPosition = new Vector2(0, -1080);
	}
}

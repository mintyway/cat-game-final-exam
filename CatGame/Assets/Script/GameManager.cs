using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
	private GameObject hpGauge;
	private GameObject player;
	private GameObject gameOver;
	private RectTransform gameOverRectTransform;

	void Start()
	{
		hpGauge = GameObject.Find("HPGauge1");
		player = GameObject.Find("Player1");
		gameOver = GameObject.Find("GameOver");
		gameOverRectTransform = gameOver.GetComponent<RectTransform>();
	}

	void Update()
	{

	}

	public void RenderPlayerHP(float playerHP)
	{
		if (playerHP <= 0)
			playerHP = 0;

		hpGauge.GetComponent<Image>().fillAmount = playerHP / 10;
	}

	public void RenderGameOver()
	{
		gameOverRectTransform.anchoredPosition = Vector2.zero;
	}
}

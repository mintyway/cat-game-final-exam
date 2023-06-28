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

	private GameObject waiting;
	private GameObject victory;
	private GameObject defeat;
	public Dictionary<PlayerNumber, GameObject> HPGauges { get; private set; } = new Dictionary<PlayerNumber, GameObject>();

	private RectTransform waitingRectTransform;
	private RectTransform victoryRectTransform;
	private RectTransform defeatRectTransform;

	void Start()
	{
		playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

		waiting = GameObject.Find("Waiting");
		victory = GameObject.Find("Victory");
		defeat = GameObject.Find("Defeat");

		HPGauges[PlayerNumber.Player1] = GameObject.Find("Player1HPGauge");
		HPGauges[PlayerNumber.Player2] = GameObject.Find("Player2HPGauge");

		waitingRectTransform = waiting.GetComponent<RectTransform>();
		victoryRectTransform = victory.GetComponent<RectTransform>();
		defeatRectTransform = defeat.GetComponent<RectTransform>();
	}

	/* 함수 설명:
	 * 플레이어의 체력을 출력해주는 함수입니다.
	 */
	public void RenderPlayerHP(PlayerNumber playerNumber, float playerHP)
	{
		HPGauges[playerNumber].GetComponent<Image>().fillAmount = playerHP / 10;
	}

	/* 함수 설명:
	 * 대기 화면을 출력해주는 함수입니다.
	 */
	public void RenderWaiting()
	{
		victoryRectTransform.anchoredPosition = new Vector2(0, -1080);
		defeatRectTransform.anchoredPosition = new Vector2(0, -1080);

		waitingRectTransform.anchoredPosition = Vector2.zero;

	}

	/* 함수 설명:
	 * 게임 화면을 출력해주는 함수입니다.
	 */
	public void RenderGame()
	{
		waitingRectTransform.anchoredPosition = new Vector2(0, -1080);
		victoryRectTransform.anchoredPosition = new Vector2(0, -1080);
		defeatRectTransform.anchoredPosition = new Vector2(0, -1080);
	}

	/* 함수 설명:
	 * 승, 패를 화면에 출력해주는 함수입니다.
	 */
	public void RenderGameOver()
	{
		waitingRectTransform.anchoredPosition = new Vector2(0, -1080);

		if (playerManager.IsAlive[playerManager.PlayingPlayerNumber])
		{
			defeatRectTransform.anchoredPosition = new Vector2(0, -1080);
			victoryRectTransform.anchoredPosition = Vector2.zero;
		}
		else
		{
			victoryRectTransform.anchoredPosition = new Vector2(0, -1080);
			defeatRectTransform.anchoredPosition = Vector2.zero;
		}

	}
}

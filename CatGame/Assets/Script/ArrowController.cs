using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Udon;

public class ArrowController : MonoBehaviour
{
	private NetworkManager networkManager;
	private PlayerManager playerManager;

	private readonly float speed = 12f;
	private readonly float damage = 1f;
	private readonly float playerCollisionRadius = 0.75f;
	private readonly float arrowCollisionRadius = 0.5f;

	private const float floorPosition = -4.55f;
	private Vector2 arrowPosition;
	private Vector2 playingPlayerPosition;
	private Vector2 nonPlayingPlayerPosition;
	private Vector2 playingPlayerDeltaPosition;
	private Vector2 nonPlayingPlayerDeltaPosition;
	private float playingPlayerArrowDistance;
	private float nonPlayingPlayerArrowDistance;

	void Start()
	{
		networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
		playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

		playerManager.Players[PlayerNumber.Player1] = GameObject.Find("Player1");
		playerManager.Players[PlayerNumber.Player2] = GameObject.Find("Player2");
	}

	void Update()
	{
		if (!networkManager.IsRunning)
			return;

		// 화살을 초당 speed의 속도로 등속 낙하시켜주는 코드(델타 타임으로 수정 필요)
		transform.Translate(0, -speed * Time.deltaTime, 0);

		// 캐릭터와 화살의 거리 계산하는 코드
		arrowPosition = transform.position;

		playingPlayerPosition = playerManager.Players[playerManager.PlayingPlayerNumber].transform.position;
		nonPlayingPlayerPosition = playerManager.Players[playerManager.NonPlayingPlayerNumber].transform.position;

		playingPlayerDeltaPosition = arrowPosition - playingPlayerPosition;
		nonPlayingPlayerDeltaPosition = arrowPosition - nonPlayingPlayerPosition;

		playingPlayerArrowDistance = playingPlayerDeltaPosition.magnitude;
		nonPlayingPlayerArrowDistance = nonPlayingPlayerDeltaPosition.magnitude;

		// 플레이 중인 캐릭터와 화살이 충돌 시
		if (playingPlayerArrowDistance < playerCollisionRadius + arrowCollisionRadius)
		{
			Destroy(gameObject);
			playerManager.SendDecreaseHP(damage);
		}

		// 플레이 중이 아닌 캐릭터와 화살이 충돌 시
		if (nonPlayingPlayerArrowDistance < playerCollisionRadius + arrowCollisionRadius)
			Destroy(gameObject);

		// 바닥에 닿는 화살을 제거하는 코드
		if (transform.position.y - arrowCollisionRadius < floorPosition)
			Destroy(gameObject);
	}
}

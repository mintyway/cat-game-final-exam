using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Udon;

public class ArrowController : MonoBehaviour
{
	private GameManager gameManager;
	private NetworkManager networkManager;
	private PlayerManager playerManager;

	// ���� ���������� �� ������ �ʿ��� ��
	public float speed;
	public float damage;
	private const float playerCollisionRadius = 0.8f;
	private const float arrowCollisionRadius = 0.5f;

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
		if (speed == 0)
			speed = 1f;

		if (damage == 0)
			damage = 1f;

		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
		playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
		playerManager.Players[PlayerNumber.Player1] = GameObject.Find("Player1");
		playerManager.Players[PlayerNumber.Player2] = GameObject.Find("Player2");
		playerManager.Players[PlayerNumber.Player1] = GameObject.Find("Player1");
	}

	void Update()
	{
		if (networkManager.IsRunning)
			return;

		// ȭ���� speed ���� 1/10 �ӵ��� ��� ���Ͻ����ִ� �ڵ�
		transform.Translate(0, -speed / 10, 0);

		// ĳ���Ϳ� ȭ���� �Ÿ� ����ϴ� �ڵ�
		arrowPosition = transform.position;

		playingPlayerPosition = playerManager.Players[playerManager.PlayingPlayerNumber].transform.position;
		nonPlayingPlayerPosition = playerManager.Players[playerManager.NonPlayingPlayerNumber].transform.position;

		playingPlayerDeltaPosition = arrowPosition - playingPlayerPosition;
		nonPlayingPlayerDeltaPosition = arrowPosition - nonPlayingPlayerPosition;

		playingPlayerArrowDistance = playingPlayerDeltaPosition.magnitude;
		nonPlayingPlayerArrowDistance = nonPlayingPlayerDeltaPosition.magnitude;

		// �÷��� ���� ĳ���Ϳ� ȭ���� �浹 ��
		if (playingPlayerArrowDistance < playerCollisionRadius + arrowCollisionRadius)
		{
			Destroy(gameObject);
			playerManager.DecreaseHP(damage);
		}

		// �÷��� ���� �ƴ� ĳ���Ϳ� ȭ���� �浹 ��
		if (nonPlayingPlayerArrowDistance < playerCollisionRadius + arrowCollisionRadius)
			Destroy(gameObject);

		// �ٴڿ� ��� ȭ���� �����ϴ� �ڵ�
		if (transform.position.y - arrowCollisionRadius < floorPosition)
			Destroy(gameObject);
	}
}

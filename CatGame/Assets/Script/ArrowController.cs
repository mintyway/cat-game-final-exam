using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
	private GameObject player;
	private GameObject gameManager;

	// 게임 레벨디자인 시 조정이 필요한 값
	public float speed;
	public float damage;
	private const float playerCollisionRadius = 0.8f;
	private const float arrowCollisionRadius = 0.5f;

	private const float floorPosition = -4.55f;
	private Vector2 arrowPosition;
	private Vector2 playerPosition;
	private Vector2 deltaPosition;
	private float PlayerArrowDistance;

	void Start()
	{
		player = GameObject.Find("Player");

		if (speed == 0)
			speed = 1f;

		if (damage == 0)
			damage = 1f;
	}

	void Update()
	{
		if (player.GetComponent<PlayerManager>().hp <= 0)
			return;

		// 화살을 speed 값의 10분의 1 속도로 등속 낙하시켜주는 코드
		transform.Translate(0, -speed / 10, 0);

		// 바닥에 닿는 화살을 제거하는 코드
		if (transform.position.y - arrowCollisionRadius < floorPosition)
			Destroy(gameObject);

		// 플레이어와 화살의 거리 계산하는 코드
		arrowPosition = transform.position;
		playerPosition = player.transform.position;
		deltaPosition = arrowPosition - playerPosition;
		PlayerArrowDistance = deltaPosition.magnitude;

		// 플레이어가 화살에 맞았을 시 플레이어의 체력을 감소시키고 화살 오브젝트를 파괴하는 코드
		if (PlayerArrowDistance < playerCollisionRadius + arrowCollisionRadius)
		{
			player.GetComponent<PlayerManager>().DecreaseHP(damage);

			Destroy(gameObject);
		}
	}
}

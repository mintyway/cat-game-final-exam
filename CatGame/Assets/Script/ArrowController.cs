using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
	private GameObject player;
	private GameObject gameManager;

	// ���� ���������� �� ������ �ʿ��� ��
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

		// ȭ���� speed ���� 10���� 1 �ӵ��� ��� ���Ͻ����ִ� �ڵ�
		transform.Translate(0, -speed / 10, 0);

		// �ٴڿ� ��� ȭ���� �����ϴ� �ڵ�
		if (transform.position.y - arrowCollisionRadius < floorPosition)
			Destroy(gameObject);

		// �÷��̾�� ȭ���� �Ÿ� ����ϴ� �ڵ�
		arrowPosition = transform.position;
		playerPosition = player.transform.position;
		deltaPosition = arrowPosition - playerPosition;
		PlayerArrowDistance = deltaPosition.magnitude;

		// �÷��̾ ȭ�쿡 �¾��� �� �÷��̾��� ü���� ���ҽ�Ű�� ȭ�� ������Ʈ�� �ı��ϴ� �ڵ�
		if (PlayerArrowDistance < playerCollisionRadius + arrowCollisionRadius)
		{
			player.GetComponent<PlayerManager>().DecreaseHP(damage);

			Destroy(gameObject);
		}
	}
}

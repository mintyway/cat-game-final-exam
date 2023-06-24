using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
	private GameObject gameManager;

	private int playerNumber;
	public float hp;
	public float speed;

	void Start()
	{
		gameManager = GameObject.Find("GameManager");

		hp = 10f;

		if (speed == 0)
			speed = 0.2f;
	}

	void Update()
	{
		if (hp <= 0)
			return;

		KeyboardControll();
	}

	// 키보드 입력을 반영하는 코드
	private void KeyboardControll()
	{
		if (Input.GetKey(KeyCode.LeftArrow))
			transform.Translate(-speed, 0, 0);
		else if (Input.GetKey(KeyCode.RightArrow))
			transform.Translate(speed, 0, 0);
	}

	// 체력을 깎는 함수
	public void DecreaseHP(float damage)
	{
		hp -= damage;
		gameManager.GetComponent<GameManager>().RenderPlayerHP(hp);

		if (hp <= 0)
			gameManager.GetComponent<GameManager>().RenderGameOver();
	}
}
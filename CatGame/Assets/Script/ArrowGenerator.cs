using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowGenerator : MonoBehaviour
{
	private NetworkManager networkManager;

	public GameObject arrowPrefab;

	private readonly float screenHalfWidth = 8.4f;

	// 화살 생성 좌표
	private float positionX;
	private readonly float positionY = 6.0f;

	private readonly float arrowSpawnInterval = 0.1f;
	private float accumulatedTime = 0f;

	void Start()
	{
		networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

		Random.InitState(0);
	}

	void Update()
	{
		if (!networkManager.IsRunning)
			return;

		accumulatedTime += Time.deltaTime;

		// 화살을 일정 주기로 랜덤한 위치에 생성합니다.
		if (accumulatedTime >= arrowSpawnInterval)
		{
			GameObject newArrow = Instantiate(arrowPrefab);

			positionX = Random.Range(-screenHalfWidth, screenHalfWidth);
			newArrow.transform.position = new Vector3(positionX, positionY, 0);

			accumulatedTime -= arrowSpawnInterval;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowGenerator : MonoBehaviour
{
	private NetworkManager networkManager;
	private PlayerManager playerManager;

	public GameObject arrowPrefab;

    // 게임 레벨디자인 시 조정이 필요한 값
    public float arrowSpawnInterval;

    float delta;
    const float screenHalfWidth = 8.4f;

    // 화살 생성 좌표
    float positionX;
    float positionY;

    void Start()
    {
		if (arrowSpawnInterval == 0)
            arrowSpawnInterval = 1.0f;

        positionY = 6.0f;

		networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
		playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

		Random.InitState(0);
	}

	void Update()
	{
		if (!networkManager.IsRunning)
			return;

		delta += Time.deltaTime;

		// 화살을 일정 주기로 랜덤한 위치에 생성합니다.
		if (delta > arrowSpawnInterval)
		{
			delta = 0;

			GameObject newArrow = Instantiate(arrowPrefab);

			positionX = Random.Range(-screenHalfWidth, screenHalfWidth);
			newArrow.transform.position = new Vector3(positionX, positionY, 0);
		}
	}
}

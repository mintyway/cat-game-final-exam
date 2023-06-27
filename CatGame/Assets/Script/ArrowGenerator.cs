using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowGenerator : MonoBehaviour
{
	private NetworkManager networkManager;
	private PlayerManager playerManager;

	public GameObject arrowPrefab;

    // ���� ���������� �� ������ �ʿ��� ��
    public float arrowSpawnInterval;

    float delta;
    const float screenHalfWidth = 8.4f;

    // ȭ�� ���� ��ǥ
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

		// ȭ���� ���� �ֱ�� ������ ��ġ�� �����մϴ�.
		if (delta > arrowSpawnInterval)
		{
			delta = 0;

			GameObject newArrow = Instantiate(arrowPrefab);

			positionX = Random.Range(-screenHalfWidth, screenHalfWidth);
			newArrow.transform.position = new Vector3(positionX, positionY, 0);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowGenerator : MonoBehaviour
{
    public GameObject arrowPrefab;
	private NetworkManager networkManager;
	private GameObject playerManager;
	private GameObject player;

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

        player = GameObject.Find("Player");

        positionY = 6.0f;

		//playerManager = GameObject.Find("PlayerManager");
		// �̷������� ���� ��� ���ƾ��� + �Լ����� /**/�� ����� ���� ����!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1
		networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
		PlayerManager playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
	}

	void Update()
	{
		if (networkManager.IsRunning)
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

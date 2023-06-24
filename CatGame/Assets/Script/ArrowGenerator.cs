using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowGenerator : MonoBehaviour
{
    public GameObject arrowPrefab;
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
        player = GameObject.Find("Player");

		if (arrowSpawnInterval == 0)
            arrowSpawnInterval = 1.0f;

        positionY = 6.0f;
    }

    void Update()
    {
        if (player.GetComponent<PlayerManager>().hp <= 0)
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

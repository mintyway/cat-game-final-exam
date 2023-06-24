using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;
using System;
using UnityEngine.Analytics;

/* 바이트 규칙
 * 0    1   2
 * 0: 게임의 전반적인 데이터
 *      0: 새로운 플레이어 생성 요청
 *      N: N번 플레이어 입장 관련
 *          0: 입장
 *          1: 퇴장
 *      
 * N: N번 플레이어 관련 데이터
 *      0: 생존 관련 데이터
 *          0: 생존
 *          1: 사망
 *      1: 이동 관련 데이터
 *          0: 정지
 *          1: 좌 이동
 *          2: 우 이동
 */

public class NetworkManager : MonoBehaviour
{
    Socket socket;
    IPEndPoint endPoint;
    IPEndPoint serverEP;
    EndPoint latestEP;
    GameObject player;

    void Start()
    {
        string myIP = "192.168.35.54";
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        endPoint = new IPEndPoint(IPAddress.Parse(myIP), 52001);
        serverEP = new IPEndPoint(IPAddress.Loopback, 52000);
        latestEP = new IPEndPoint(IPAddress.None, 0);

        player = GameObject.Find("Player");
    }

    void Update()
    {

    }
}
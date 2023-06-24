using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;
using System;
using UnityEngine.Analytics;

/* ����Ʈ ��Ģ
 * 0    1   2
 * 0: ������ �������� ������
 *      0: ���ο� �÷��̾� ���� ��û
 *      N: N�� �÷��̾� ���� ����
 *          0: ����
 *          1: ����
 *      
 * N: N�� �÷��̾� ���� ������
 *      0: ���� ���� ������
 *          0: ����
 *          1: ���
 *      1: �̵� ���� ������
 *          0: ����
 *          1: �� �̵�
 *          2: �� �̵�
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
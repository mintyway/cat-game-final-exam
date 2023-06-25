using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;
using System;
using UnityEngine.Analytics;
using Unity.VisualScripting;
using UdonDLL;

public class NetworkManager : MonoBehaviour
{
	private GameObject playerManager;

	public string serverIP;

	private UdpClient unicastClient;
	private UdpClient multicastClient;
	private IPEndPoint unicastServerEP;
	private IPEndPoint multicastEP;
	private IPEndPoint remoteEP;

	void Start()
	{
		playerManager = GameObject.Find("PlayerManager");

		if (string.IsNullOrEmpty(serverIP))
			serverIP = "127.0.0.1";

		unicastServerEP = new IPEndPoint(IPAddress.Parse(serverIP), 52000);
		multicastEP = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 52001);
		remoteEP = new IPEndPoint(IPAddress.None, 0);

		unicastClient = new UdpClient();
		multicastClient = new UdpClient();

		multicastClient.JoinMulticastGroup(multicastEP.Address);
		multicastClient.Client.Bind(new IPEndPoint(IPAddress.Any, 52001));
	}

	void Update() { }

	public void OnKeyInput(UdonDLL.PlayerNumber playerNumber, UdonDLL.Direction direction)
	{
		UdonDLL.KeyInputPacket keyInputPacket = new UdonDLL.KeyInputPacket();

		keyInputPacket.playerNumber = playerNumber;
		keyInputPacket.direction = direction;

		byte[] sendBuffer = keyInputPacket.Serialize();
		unicastClient.Send(sendBuffer, sendBuffer.Length, unicastServerEP);

		byte[] receiveBuffer = multicastClient.Receive(ref remoteEP);
		keyInputPacket.Deserialize(receiveBuffer);

		if (keyInputPacket.direction == UdonDLL.Direction.Left)
			playerManager.GetComponent<PlayerManager>().MovePlayer(keyInputPacket.playerNumber, UdonDLL.Direction.Left);
		else
			playerManager.GetComponent<PlayerManager>().MovePlayer(keyInputPacket.playerNumber, UdonDLL.Direction.Right);
	}

	public UdonDLL.PlayerNumber OnJoin()
	{
		while (true)
		{
			UdonDLL.JoinPacket joinPacket = new UdonDLL.JoinPacket() { isCheck = false };

			byte[] sendBuffer = joinPacket.Serialize();
			unicastClient.Send(sendBuffer, sendBuffer.Length, unicastServerEP);

			byte[] receiveBuffer = unicastClient.Receive(ref remoteEP);
			joinPacket.Deserialize(receiveBuffer);

			if (joinPacket.isCheck == false)
				continue;

			return joinPacket.playerNumber;
		}
	}
}
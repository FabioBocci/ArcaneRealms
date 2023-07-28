using System;
using ArcaneRealms.Scripts.Systems;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingHostClientUI : MonoBehaviour {
	[SerializeField] private Button hostButton;
	[SerializeField] private Button clientButton;

	private void Awake() {
		hostButton.onClick.AddListener(() => {
			NetworkManager.Singleton.StartHost();
			NetworkManagerHelper.Instance.OnClientConnectCallback += OnClientConnect;
			gameObject.SetActive(false);
		});
		clientButton.onClick.AddListener(() => {
			NetworkManager.Singleton.StartClient();
			gameObject.SetActive(false);
		});

	}

	private void OnDestroy()
	{
		NetworkManagerHelper.Instance.OnClientConnectCallback -= OnClientConnect;
	}

	private void OnClientConnect()
	{
		if (NetworkManagerHelper.Instance.ConnectedClients >= 2)
		{
			NetworkManagerHelper.Instance.LoadMainGameScene();
		}
	}
}

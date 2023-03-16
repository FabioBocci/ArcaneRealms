using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingHostClientUI : MonoBehaviour {
	[SerializeField] private Button hostButton;
	[SerializeField] private Button clientButton;

	private void Awake() {
		hostButton.onClick.AddListener(() => {
			NetworkManager.Singleton.StartHost();
			gameObject.SetActive(false);
		});
		clientButton.onClick.AddListener(() => {
			NetworkManager.Singleton.StartClient();
			gameObject.SetActive(false);
		});

	}

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}
}

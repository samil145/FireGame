using UnityEngine.UI;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private InputField inputAddress;
        private GameObject escapePanel;
        private GameObject settingsPanel;
        [SerializeField] private InputField nameInput;
        [SerializeField] private InputField prefabId;

        //Xuynya
        private void Start()
        {
            inputAddress.onSubmit.AddListener(ChangeAddress);
            nameInput.onSubmit.AddListener(OnNameChanged);
            prefabId.onSubmit.AddListener(OnPrefabIdChanged);
        }

        private void Update()
        {
            if (escapePanel != null && Input.GetKeyUp(KeyCode.Escape))
            {
                escapePanel.SetActive(!escapePanel.activeSelf);
                Cursor.lockState = escapePanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
            }
        }

        public void ChangeAddress(string newAddress)
        {
            Debug.Log("fff");
            Connection.ConnectionHandler.Instance.Address = newAddress;
        }

        public void OnNameChanged(string str)
        {
            Connection.ConnectionHandler.Instance.PlayerName = str;
        }

        public void OnPrefabIdChanged(string str)
        {
            if (int.TryParse(str, out int id))
            {
                Connection.ConnectionHandler.Instance.UserPlayerPrefabId = id;
            }
        }

        public void Resume()
        {
            escapePanel?.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
        }

        public void OpenSettings()
        {
            escapePanel.SetActive(false);
            settingsPanel?.SetActive(true);
        }

        public void CloseSettings()
        {
            settingsPanel.SetActive(false);
            escapePanel.SetActive(true);
        }

        public void ExitToMainMenu()
        {
            Connection.ConnectionHandler.Instance.DisconectLocalClient();
        }
    }
}


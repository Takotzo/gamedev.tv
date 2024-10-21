using NetWorking.Client;
using NetWorking.Host;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace UI
{
    public class GameHUD : NetworkBehaviour
    {
        [SerializeField] private TMP_Text lobbyCodeText;

        private NetworkVariable<FixedString32Bytes> lobbyCode = new NetworkVariable<FixedString32Bytes>("lobbyCode");
        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                lobbyCode.OnValueChanged += HandleLobbyCodeChanged;
                HandleLobbyCodeChanged("", lobbyCode.Value);
            }
            
            if (!IsHost) return;
            
            lobbyCode.Value = HostSingleton.Instance.GameManager.joinCode;
        }

        

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                lobbyCode.OnValueChanged -= HandleLobbyCodeChanged;
            }
        }

        public void LeaveGame()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                HostSingleton.Instance.GameManager.ShutDown();
            }

            ClientSingleton.Instance.GameManager.Disconnect();
        }
        
        private void HandleLobbyCodeChanged(FixedString32Bytes oldCode, FixedString32Bytes newCode)
        {
            lobbyCodeText.text = newCode.ToString();
        }
    }
}

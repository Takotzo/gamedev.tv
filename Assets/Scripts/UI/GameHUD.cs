using NetWorking.Client;
using NetWorking.Host;
using Unity.Netcode;
using UnityEngine;

namespace UI
{
    public class GameHUD : MonoBehaviour
    { 
        public void LeaveGame()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                HostSingleton.Instance.GameManager.ShutDown();
            }

            ClientSingleton.Instance.GameManager.Disconnect();
        }
    }
}

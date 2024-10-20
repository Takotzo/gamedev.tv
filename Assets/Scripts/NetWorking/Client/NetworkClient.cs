using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace NetWorking.Client
{
    public class NetworkClient : IDisposable
    {
        private NetworkManager networkManager;

        private const string MENU_SCENE_NAME = "Menu";
        
        public NetworkClient(NetworkManager networkManager)
        {
            this.networkManager = networkManager;

            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
        

        private void OnClientDisconnected(ulong clientId)
        {
            if (clientId != 0 && clientId != networkManager.LocalClientId) {return;}

            Disconnect();
        }
        
        
        public void Disconnect()
        {
            if (SceneManager.GetActiveScene().name != MENU_SCENE_NAME)
            {
                SceneManager.LoadScene(MENU_SCENE_NAME);
            }

            if (networkManager.IsConnectedClient)
            {
                networkManager.Shutdown();
            }
        }
        

        public void Dispose()
        {
            if (networkManager != null)
            {
                networkManager.OnClientDisconnectCallback -= OnClientDisconnected;  
            }
        }

       
    }
}

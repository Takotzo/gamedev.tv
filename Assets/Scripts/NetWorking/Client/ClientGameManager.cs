using System;
using System.Text;
using System.Threading.Tasks;
using NetWorking.Client.Services;
using NetWorking.Shared;
using UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetWorking.Client
{
    public class ClientGameManager : IDisposable
    {
        private const string MENU_SCENE_NAME = "Menu";

        private JoinAllocation allocation;

        private NetworkClient networkClient;
        private MatchplayMatchmaker matchplayMatchmaker;
        private UserData userData;

        public async Task<bool> InitAsync()
        {
            await UnityServices.InitializeAsync();
        
            networkClient = new NetworkClient(NetworkManager.Singleton);
            matchplayMatchmaker = new MatchplayMatchmaker();

            AuthState authState = await AuthenticationWrapper.DoAuth();

            if (authState == AuthState.Authenticated)
            {
                userData = new UserData()
                {
                    userName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Missing Name"), 
                    userAuthId = AuthenticationService.Instance.PlayerId,
                };
                return true;
            }
            return false;
        }

        public void GoToMenu()
        {
            SceneManager.LoadScene(MENU_SCENE_NAME);
        }

        public void StartClient(string ip, int port)
        {
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(ip, (ushort)port);
            ConnectClient();
        }

        public async Task StartClientAsync(string joinCode)
        {
            try
            {
                allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }
        
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            ConnectClient();
        }

        public async void MatchmakeAsync(bool isTeamQueue, Action<MatchmakerPollingResult> onMatchmakeResponse)
        {
            if (matchplayMatchmaker.IsMatchmaking) { return;}

            userData.userGamePreferences.gameQueue = isTeamQueue ? GameQueue.Team : GameQueue.Solo;
            MatchmakerPollingResult matchResult = await GetMatchAsync();
            onMatchmakeResponse?.Invoke(matchResult);
        }
        

        private async Task<MatchmakerPollingResult> GetMatchAsync()
        {
            MatchmakingResult matchmakingResult = await matchplayMatchmaker.Matchmake(userData);

            if (matchmakingResult.result == MatchmakerPollingResult.Success)
            {
                StartClient(matchmakingResult.ip, matchmakingResult.port);
            }
            
            return matchmakingResult.result;
        }

        private void ConnectClient()
        {
            string payload = JsonUtility.ToJson(userData); 
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        
            NetworkManager.Singleton.StartClient();
        }
        
        public async Task CancelMatchmaking()
        {
            await matchplayMatchmaker.CancelMatchmaking();
        }
        
        public void Disconnect()
        {
            networkClient.Disconnect();
        }
        

        public void Dispose()
        {
            networkClient?.Dispose();
        }


    
    }
}

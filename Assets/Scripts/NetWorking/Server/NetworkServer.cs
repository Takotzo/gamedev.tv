using System;
using System.Collections.Generic;
using Core;
using NetWorking.Shared;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace NetWorking.Server
{
    public class NetworkServer : IDisposable
    {
        private NetworkManager networkManager;

        public Action<UserData> OnUserJoined;
        public Action<UserData> OnUserLeft;
        
        public Action<string> OnClientLeft;
        
        private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
        private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();
        public NetworkServer(NetworkManager networkManager)
        {
            this.networkManager = networkManager;

            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnServerStarted += OnNetworkReady;
        }

        public bool OpenConnection(string ip, int port)
        {
            UnityTransport transport= networkManager.gameObject.GetComponent<UnityTransport>();
            transport.SetConnectionData(ip, (ushort)port);
            return networkManager.StartServer();
        }
      
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
            UserData userData = JsonUtility.FromJson<UserData>(payload);

            clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
            authIdToUserData[userData.userAuthId] = userData;
            OnUserJoined?.Invoke(userData);

            response.Approved = true;
            response.Position = SpawnPoint.GetRandomSpawnPos();
            response.Rotation = quaternion.identity;
            response.CreatePlayerObject = true;
        }
        
        private void OnNetworkReady()
        {
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (clientIdToAuth.TryGetValue(clientId, out string authId))
            {
                OnUserLeft?.Invoke(authIdToUserData[authId]);
                clientIdToAuth.Remove(clientId);
                authIdToUserData.Remove(authId);
                OnClientLeft?.Invoke(authId);
            }
        }
        
        public UserData GetUserDataByClientId(ulong clientId)
        {
            if (clientIdToAuth.TryGetValue(clientId, out string authId))
            {
                if (authIdToUserData.TryGetValue(authId, out UserData userData))
                {
                    return userData;
                }
            }

            return null;
        }

        public void Dispose()
        {
            if (networkManager != null)
            {
                networkManager.ConnectionApprovalCallback -= ApprovalCheck;
                networkManager.OnServerStarted -= OnNetworkReady;
                networkManager.OnClientDisconnectCallback -= OnClientDisconnected;

                if (networkManager.IsListening)
                {
                    networkManager.Shutdown();
                }
            }
        }
        
        
    }
}

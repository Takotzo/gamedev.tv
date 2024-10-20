using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NetWorking.Client.Services;
using NetWorking.Server;
using NetWorking.Server.Services;
using NetWorking.Shared;
using UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetWorking.Server
{
    public class ServerGameManager : IDisposable
    {
        private string serverIP;
        private int serverPort;
        private int queryPort;
        private MatchplayBackfiller backfiller;
        private MultiplayAllocationService multiplayAllocationService;
        
        public NetworkServer NetworkServer { get; private set; }

        private const string GAME_SCENE_NAME = "Game";

        public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager manager)
        {
            this.serverIP = serverIP;
            this.serverPort = serverPort;
            this.queryPort = queryPort;
            NetworkServer = new NetworkServer(manager);
            multiplayAllocationService = new MultiplayAllocationService();
        }
        
        public async Task StartGameServerAsync()
        {
            await multiplayAllocationService.BeginServerCheck();

            try
            {
                MatchmakingResults matchamkerPayload = await GetMatchmakerPayload();

                if (matchamkerPayload != null)
                {
                    await StaterBackfill(matchamkerPayload);
                    NetworkServer.OnUserJoined += UserJoined;
                    NetworkServer.OnUserLeft += UserLeft;
                }
                else
                {
                    Debug.LogWarning("Matchmaker payload timed out");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

            if (!NetworkServer.OpenConnection(serverIP, serverPort))
            {
                Debug.LogWarning("Cannot start game server because network server could not be started.");
                return;
            }
            
            NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_NAME, LoadSceneMode.Single);
        }

        private async Task StaterBackfill(MatchmakingResults payload)
        {
            backfiller = new MatchplayBackfiller($"{serverIP}:{serverPort}", payload.QueueName, payload.MatchProperties, 20);

            if (backfiller.NeedsPlayers())
            {
                await backfiller.BeginBackfilling();
            }
        }

        private void UserJoined(UserData user)
        {
            backfiller.AddPlayerToMatch(user);
            multiplayAllocationService.AddPlayer();
            if (!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
            {
                _ = backfiller.StopBackfill();
            }
        }
        
        private void UserLeft(UserData user)
        {
            int playerCount = backfiller.RemovePlayerFromMatch(user.userAuthId);
            multiplayAllocationService.RemovePlayer();

            if (playerCount <= 0)
            {
                CloseServer();
                return;
            }

            if (backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
            {
                _ = backfiller.BeginBackfilling();
            }
        }

        private async void CloseServer()
        {
            await backfiller.StopBackfill();
            Dispose();
            Application.Quit();
        }


        

        private async Task<MatchmakingResults> GetMatchmakerPayload()
        {
            Task<MatchmakingResults> matchmakerPayloadTask = multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

            if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask)
            {
                return matchmakerPayloadTask.Result;
            }

            return null;
        }

        public void Dispose()
        {
            NetworkServer.OnUserJoined -= UserJoined;
            NetworkServer.OnUserLeft -= UserLeft;
            
            backfiller?.Dispose();
            multiplayAllocationService?.Dispose();
            NetworkServer?.Dispose();
        }
    }
}

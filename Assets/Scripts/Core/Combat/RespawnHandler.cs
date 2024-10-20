using System.Collections;
using Core.Player;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Core.Combat
{
    public class RespawnHandler : NetworkBehaviour
    {
        [SerializeField] private TankPlayer playerPrefab;
        [SerializeField] private float keptCoinPercentage;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) {return;}
            
            TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (TankPlayer player in players)
            {
                HandlePlayerSpawned(player);
            }
            
            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerSpawned += HandlePlayerDespawned;
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsServer) {return;}
            
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerSpawned -= HandlePlayerDespawned;
        }
        
        private void HandlePlayerSpawned(TankPlayer player)
        {
            player.Health.OnDie += (health) => HandlePlayerDie(player);
        }

        
        private void HandlePlayerDespawned(TankPlayer player)
        {
            player.Health.OnDie -= (health) => HandlePlayerDie(player);

        }

        private void HandlePlayerDie(TankPlayer player)
        {
            float coinKept = player.Wallet.TotalCoins.Value *(keptCoinPercentage / 100);
            
            
            Destroy(player.gameObject);

            StartCoroutine(RespawnPlayer(player.OwnerClientId, (int)coinKept));
        }

        private IEnumerator RespawnPlayer(ulong ownerClientID, int playerCoins = 0)
        {
            yield return null;

            TankPlayer playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), quaternion.identity);
            
            playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientID);
            
            playerInstance.Wallet.TotalCoins.Value = playerCoins;
        }

        
    }
}

using System.Collections.Generic;
using System.Linq;
using Core.Player;
using NetWorking.Client;
using NetWorking.Shared;
using Unity.Netcode;
using UnityEngine;

namespace UI.Leaderboard
{
    public class Leaderboard : NetworkBehaviour
    {
        [SerializeField] private Transform leaderboardEntityHolder;
        [SerializeField] private Transform teamLeaderboardEntityHolder;
        [SerializeField] private GameObject teamLeaderboardBackground;
        [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;
        [SerializeField] private int entitiesToDisplay = 8;
        [SerializeField] private Color ownerColor;
        [SerializeField] private string[] teamNames;
        [SerializeField] private TeamColorLookUp teamColorLookUp;

        private NetworkList<LeaderboardEntityState> leaderboardEntities;
        
        private List<LeaderboardEntityDisplay> entityDisplays = new List<LeaderboardEntityDisplay>();
        private List<LeaderboardEntityDisplay> teamEntityDisplays = new List<LeaderboardEntityDisplay>();


        private void Awake()
        {
            leaderboardEntities = new NetworkList<LeaderboardEntityState>();
        }


        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                if (ClientSingleton.Instance.GameManager.userData.userGamePreferences.gameQueue == GameQueue.Team)
                {
                    teamLeaderboardBackground.SetActive(true);

                    for (int i = 0; i < teamNames.Length; i++)
                    {
                        LeaderboardEntityDisplay teamLeaderboardEntity = Instantiate(leaderboardEntityPrefab, teamLeaderboardEntityHolder);

                        LeaderboardEntityState leaderboardEntityState = default;
                        leaderboardEntityState.SetValues(0, i, teamNames[i], 0);

                        teamLeaderboardEntity.Initialise(leaderboardEntityState);
                        
                        Color teamColor = teamColorLookUp.GetTeamColor(i);
                        teamLeaderboardEntity.SetColor(teamColor);
                        
                        teamEntityDisplays.Add(teamLeaderboardEntity);
                        
                    }
                }
                leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
                foreach (LeaderboardEntityState entity in leaderboardEntities)
                {
                    HandleLeaderboardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState>
                    {
                        Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                        Value = entity,
                    });
                }
            }
            
            if (IsServer)
            {
                TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
                foreach (TankPlayer player in players)
                {
                    HandlePlayerSpawned(player);
                }
                
                TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
                TankPlayer.OnPlayerDespawn += HandlePlayerDespawned;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
                
            }

            if (IsServer)
            {
                TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
                TankPlayer.OnPlayerDespawn -= HandlePlayerDespawned;
            }
        }

        private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
        {
            if (!gameObject.scene.isLoaded) return;
            
            switch (changeEvent.Type)
            {
                case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                    if (entityDisplays.All(x => x.LeaderboardEntityState.ClientId != changeEvent.Value.ClientId))
                    {
                        LeaderboardEntityDisplay leaderboardEntity = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                        leaderboardEntity.Initialise(changeEvent.Value); //This could be wrong
                        if (NetworkManager.Singleton.LocalClientId == changeEvent.Value.ClientId)
                        {
                            leaderboardEntity.SetColor(ownerColor);
                        }
                        entityDisplays.Add(leaderboardEntity);
                    }
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                    LeaderboardEntityDisplay displayToRemove = entityDisplays.FirstOrDefault(x =>
                        x.LeaderboardEntityState.ClientId == changeEvent.Value.ClientId);
                    if (displayToRemove != null)
                    {
                        displayToRemove.transform.SetParent(null);
                        Destroy(displayToRemove.gameObject);
                        entityDisplays.Remove(displayToRemove);
                    }
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                    LeaderboardEntityDisplay displayToUpdate = entityDisplays.FirstOrDefault(x =>
                        x.LeaderboardEntityState.ClientId == changeEvent.Value.ClientId);
                    if (displayToUpdate != null)
                    {
                        displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                    }

                    break;
            }
            
            entityDisplays.Sort((x, y) => y.LeaderboardEntityState.Coins.CompareTo(x.LeaderboardEntityState.Coins));

            for (int i = 0; i < entityDisplays.Count; i++)
            {
                entityDisplays[i].transform.SetSiblingIndex(i);
                entityDisplays[i].UpdateText();
                entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay - 1);
            }
            
            LeaderboardEntityDisplay myDisplay = 
                entityDisplays.FirstOrDefault(x => x.LeaderboardEntityState.ClientId == NetworkManager.Singleton.LocalClientId);

            if (myDisplay != null)
            {
                if (myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
                {
                    leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                    myDisplay.gameObject.SetActive(true);
                }
            }

            if (!teamLeaderboardBackground.activeSelf) return;
            
            LeaderboardEntityDisplay teamDisplay = 
                teamEntityDisplays.FirstOrDefault(x => x.LeaderboardEntityState.TeamIndex == changeEvent.Value.TeamIndex);

            if (teamDisplay != null)
            {
                if (changeEvent.Type == NetworkListEvent<LeaderboardEntityState>.EventType.Remove)
                {
                    teamDisplay.UpdateCoins(teamDisplay.LeaderboardEntityState.Coins - changeEvent.Value.Coins);
                }
                else
                {
                    teamDisplay.UpdateCoins(teamDisplay.LeaderboardEntityState.Coins + (changeEvent.Value.Coins - changeEvent.PreviousValue.Coins));
                }
                teamEntityDisplays.Sort((x, y) => y.LeaderboardEntityState.Coins.CompareTo(x.LeaderboardEntityState.Coins));

                for (int i = 0; i < teamEntityDisplays.Count; i++)
                {
                    teamEntityDisplays[i].transform.SetSiblingIndex(i);
                    teamEntityDisplays[i].UpdateText();
                }
            }
        }


        private void HandlePlayerSpawned(TankPlayer player)
        {
            leaderboardEntities.Add(new LeaderboardEntityState
            {
                ClientId = player.OwnerClientId,
                PlayerName = player.playerName.Value,
                TeamIndex = player.teamIndex.Value,
                Coins = 0,
            });
            
            player.Wallet.TotalCoins.OnValueChanged += (oldCoins, newCoins) => HandleCoinsChanged(player.OwnerClientId, newCoins);
        }
        
        
        private void HandlePlayerDespawned(TankPlayer player)
        {
            if (leaderboardEntities == null) { return;}
       
            foreach (LeaderboardEntityState entity in leaderboardEntities)
            {
                if (entity.ClientId != player.OwnerClientId) {continue;}

                leaderboardEntities.Remove(entity);
                break;
            }
            
            player.Wallet.TotalCoins.OnValueChanged -= (oldCoins, newCoins) => HandleCoinsChanged(player.OwnerClientId, newCoins);

        }


        private void HandleCoinsChanged(ulong clientId, int newCoins)
        {
            for (int i = 0; i < leaderboardEntities.Count; i++)
            {
                if (leaderboardEntities[i].ClientId != clientId) {continue;}

                leaderboardEntities[i] = new LeaderboardEntityState
                {
                    ClientId = leaderboardEntities[i].ClientId,
                    PlayerName = leaderboardEntities[i].PlayerName,
                    TeamIndex = leaderboardEntities[i].TeamIndex,
                    Coins = newCoins,
                };
                
                return;
            }
        }

    }
}

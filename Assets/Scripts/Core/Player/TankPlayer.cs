using System;
using Cinemachine;
using Core.Coins;
using Core.Combat;
using NetWorking.Host;
using NetWorking.Server;
using NetWorking.Shared;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    public class TankPlayer : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private Texture2D crosshair;
        [field:SerializeField] public Health Health { get; private set; }
        
        [field:SerializeField] public CoinWallet Wallet { get; private set; }
        [SerializeField] SpriteRenderer minimapIconRenderer;

    
        [Header("Settings")]
        [SerializeField] private int ownerPriority = 15;

        [SerializeField] private Color ownerColor;
        
        public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
    

        public static event Action<TankPlayer> OnPlayerSpawned;
        public static event Action<TankPlayer> OnPlayerDespawn;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                UserData userData = null;
                if (IsHost)
                {
                     userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);

                }
                else
                {
                    userData = ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
                }

                playerName.Value = userData.userName;
                
                OnPlayerSpawned?.Invoke(this);
            }
            
            if (IsOwner)
            {
                virtualCamera.Priority = ownerPriority;
                
                minimapIconRenderer.color = ownerColor;
                
                Cursor.SetCursor(crosshair, new Vector2(crosshair.width/2, crosshair.height/2), CursorMode.Auto);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                OnPlayerDespawn?.Invoke(this);
            }
        }
    }
}

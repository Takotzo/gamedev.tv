using Core.Coins;
using Core.Combat;
using Input;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Player
{
    public class ProjectileLauncher : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private TankPlayer player;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private CoinWallet wallet;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private GameObject serverProjectilePrefab;
        [SerializeField] private GameObject clientProjectilePrefab;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private Collider2D playerCollider;

        [Header("Settings")] 
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float fireRate;
        [SerializeField] private float muzzleFlashDuration;
        [SerializeField] private int costToFire;

        private bool isPointerOverUI;
        private bool shouldFire;
        private float timer;
        private float muzzleFlashTimer;
    

        public override void OnNetworkSpawn()
        {
            if(!IsOwner) {return;}

            inputReader.PrimaryFireEvent += HandlePrimaryFire;
        }

        public override void OnNetworkDespawn()
        {
            if(!IsOwner) {return;}

            inputReader.PrimaryFireEvent -= HandlePrimaryFire;

        }

        private void HandlePrimaryFire(bool shouldFire)
        {
            if (shouldFire)
            {
                if (isPointerOverUI)
                {
                    return;
                }
            }
            this.shouldFire = shouldFire;
        }
    
        [ServerRpc]
        private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
        {
            if (wallet.TotalCoins.Value < costToFire) { return; }

            wallet.SpendCoin(costToFire);
            
            GameObject projectileInstance = Instantiate(serverProjectilePrefab, spawnPos, quaternion.identity);
            projectileInstance.transform.up = direction;
        
        
            Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
            
            if (projectileInstance.TryGetComponent(out  Projectile projectile))
            {
                projectile.Initialise(player.teamIndex.Value);
            }
        
            if (projectileInstance.TryGetComponent(out Rigidbody2D rb))
            {
                rb.velocity = rb.transform.up * projectileSpeed;
            }
        
            SpawnDummyProjectileClientRpc(spawnPos, direction, player.teamIndex.Value);

        }

        [ClientRpc]
        private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction, int teamIndex)
        {
            if (IsOwner) { return; }
            SpawnDummyProjectile(spawnPos, direction, teamIndex);
        }


        private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction, int teamIndex)
        {
            muzzleFlash.SetActive(true);
            muzzleFlashTimer = muzzleFlashDuration;
        
            GameObject projectileInstance = Instantiate(clientProjectilePrefab, spawnPos, quaternion.identity);
            projectileInstance.transform.up = direction;
        
            Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

            if (projectileInstance.TryGetComponent(out Projectile projectile))
            {
                projectile.Initialise(teamIndex);
            }
            if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.velocity = rb.transform.up * projectileSpeed;
            }
        }
    
    
        private void Update()
        {
            if (muzzleFlashTimer > 0f)
            {
                muzzleFlashTimer -= Time.deltaTime;
                if (muzzleFlashTimer <= 0f)
                {
                    muzzleFlash.SetActive(false);
                }
            }
        
            timer -= Time.deltaTime;
        
            if(!shouldFire || !IsOwner) {return;}

            isPointerOverUI = EventSystem.current.IsPointerOverGameObject();

            if (timer > 0) { return; }

            if (wallet.TotalCoins.Value < costToFire) { return; }
            
            
            PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
        
            SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up, player.teamIndex.Value);
        
            timer = 1/fireRate;
        }
    }
}

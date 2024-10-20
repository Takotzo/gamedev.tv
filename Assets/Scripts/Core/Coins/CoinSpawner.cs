using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Coins
{
    public class CoinSpawner : NetworkBehaviour
    {
        [SerializeField] private RespawningCoin coinPrefab;

        [SerializeField] private int maxCoins = 50;

        [SerializeField] private int coinValue = 10;

        [SerializeField] private Vector2 xSpawnRange;
        [SerializeField] private Vector2 ySpawnRange;
        [SerializeField] private LayerMask layerMask;

        private Collider2D[] coinBuffer = new Collider2D[1];
    
        private float coinRadius;
    
        public override void OnNetworkSpawn()
        {
            if (!IsServer) {return;}

            coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

            for (int i = 0; i < maxCoins; i++)
            {
                SpawnCoin();
            }
        }

        private void SpawnCoin()
        {
            RespawningCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), quaternion.identity);
        
            coinInstance.SetValue(coinValue);
            coinInstance.GetComponent<NetworkObject>().Spawn();
            coinInstance.transform.parent = gameObject.transform;
            coinInstance.OnCollected += HandleCoinCollected;
        }

        private void HandleCoinCollected(RespawningCoin coin)
        {
            coin.transform.position = GetSpawnPoint();
            coin.Reset();
        }

        private Vector2 GetSpawnPoint()
        {
            float x = 0;
            float y = 0;

            while (true)
            {
                x = Random.Range(xSpawnRange.x, xSpawnRange.y);
                y = Random.Range(ySpawnRange.x, ySpawnRange.y);

                Vector2 spawnPoint = new Vector2(x, y);

                int numbColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);

                if (numbColliders == 0)
                {
                    return spawnPoint;
                }
            }
        }

    }
}

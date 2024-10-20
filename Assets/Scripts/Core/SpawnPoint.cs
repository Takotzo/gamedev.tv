using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public class SpawnPoint : MonoBehaviour
    {
        private static List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    
    
        public static Vector3 GetRandomSpawnPos()
        {
            if (spawnPoints.Count == 0)
            {
                return Vector3.zero;
            }
        
            return spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }

        private void OnEnable()
        {
            spawnPoints.Add(this);
        }

        private void OnDisable()
        {
            spawnPoints.Remove(this);
        }
    }
}

using Unity.Mathematics;
using UnityEngine;

namespace Utils
{
    public class SpawOnDestroy : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        private void OnDestroy()
        {
            Instantiate(prefab, transform.position, quaternion.identity);
        }
    }
}

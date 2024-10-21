using Unity.Mathematics;
using UnityEngine;

namespace Utils
{
    public class SpawOnDestroy : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        private void OnDestroy()
        {
            if (gameObject.scene.isLoaded) return;
            Instantiate(prefab, transform.position, quaternion.identity);
        }
    }
}

using UnityEngine;

namespace Utils
{
    public class Lifetime : MonoBehaviour
    {
        [SerializeField] private float lifeTime;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }
    }
}

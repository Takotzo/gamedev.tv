using System.Threading.Tasks;
using UnityEngine;

namespace NetWorking.Client
{
    public class ClientSingleton : MonoBehaviour
    {
        private static ClientSingleton instance;

        public ClientGameManager GameManager { get; private set; }

        public static ClientSingleton Instance
        {
            get
            {
                if (instance != null){return instance;}

                instance = FindObjectOfType<ClientSingleton>();

                return instance == null ? null : instance;
            }
        }
    
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async Task<bool> CreateClient()
        {
            GameManager = new ClientGameManager();

            return await GameManager.InitAsync();
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}

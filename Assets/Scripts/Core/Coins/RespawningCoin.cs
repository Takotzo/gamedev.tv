using System;

namespace Core.Coins
{
    public class RespawningCoin : Coin
    {
        public event Action<RespawningCoin> OnCollected; 
    
        public override int Collect()
        {
            if (!IsServer || AlreadyCollected){ Show(false);    return 0; }

            AlreadyCollected = true;
        
            OnCollected?.Invoke(this);
        
            return CoinValue;

        }

        public void Reset()
        {
            AlreadyCollected = false;
            Show(true);
        }
    }
}

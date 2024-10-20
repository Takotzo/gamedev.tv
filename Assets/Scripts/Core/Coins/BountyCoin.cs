namespace Core.Coins
{
    public class BountyCoin : Coin
    {

        public override int Collect()
        {
            if (!IsServer || AlreadyCollected){ Show(false);    return 0; }

            AlreadyCollected = true;
        
            Destroy(gameObject);
        
            return CoinValue;
        }
    }
}

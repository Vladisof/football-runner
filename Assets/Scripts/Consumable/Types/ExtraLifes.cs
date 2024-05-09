using System.Collections;
using Characters;

namespace Consumable.Types
{
    public class ExtraLifes : Consumables
    {
        private const int k_MaxLives = 3;
        private const int k_CoinValue = 10;

        public override string GetConsumableName()
        {
            return "Life";
        }

        public override ConsumableType GetConsumableType()
        {
            return ConsumableType.CHARACTER_LIFE;
        }

        public override int GetPrice()
        {
            return 2000;
        }

        public override int GetPremiumCost()
        {
            return 5;
        }

        public override bool CanBeUsed(CharactersInputController c)
        {
            return c.currentLife != c.maxLife;

        }

        public override IEnumerator Started(CharactersInputController c)
        {
            yield return base.Started(c);
            if (c.currentLife < k_MaxLives)
                c.currentLife += 1;
            else
                c.coins += k_CoinValue;
        }
    }
}

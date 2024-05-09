using System.Collections;
using Characters;

namespace Consumable.Types
{
    public class Invisibility : Consumables
    {
        public override string GetConsumableName()
        {
            return "Invincible";
        }

        public override ConsumableType GetConsumableType()
        {
            return ConsumableType.INVINCIBILITY;
        }

        public override int GetPrice()
        {
            return 1500;
        }

        public override int GetPremiumCost()
        {
            return 5;
        }

        public override void Tick(CharactersInputController c)
        {
            base.Tick(c);

            c.CharactersCollider.SetInvincibleExplicit(true);
        }

        public override IEnumerator Started(CharactersInputController c)
        {
            yield return base.Started(c);
            c.CharactersCollider.SetInvincible(duration);
        }

        public override void Ended(CharactersInputController c)
        {
            base.Ended(c);
            c.CharactersCollider.SetInvincibleExplicit(false);
        }
    }
}

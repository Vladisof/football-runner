using System.Collections;
using Characters;

namespace Consumable.Types
{
    public class ScoreMultiplier : Consumables
    {
        public override string GetConsumableName()
        {
            return "x2";
        }

        public override ConsumableType GetConsumableType()
        {
            return ConsumableType.SCORE_MULTIPLAYER;
        }

        public override int GetPrice()
        {
            return 750;
        }

        public override int GetPremiumCost()
        {
            return 0;
        }

        public override IEnumerator Started(CharactersInputController c)
        {
            yield return base.Started(c);

            MSinceStart = 0;

            c.TracksManager.modifyMultiply += MultiplyModify;
        }

        public override void Ended(CharactersInputController c)
        {
            base.Ended(c);

            c.TracksManager.modifyMultiply -= MultiplyModify;
        }

        protected int MultiplyModify(int multi)
        {
            return multi * 2;
        }
    }
}

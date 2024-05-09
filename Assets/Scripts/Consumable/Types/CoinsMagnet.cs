using Characters;
using UnityEngine;
namespace Consumable.Types
{
	public class CoinsMagnet : Consumables
	{
		private readonly Vector3 _halfExtentsBox = new Vector3 (20.0f, 1.0f, 1.0f);
		private const int k_LayerMask = 1 << 8;

		public override string GetConsumableName()
		{
			return "Magnet";
		}

		public override ConsumableType GetConsumableType()
		{
			return ConsumableType.COIN_MAG;
		}

		public override int GetPrice()
		{
			return 750;
		}

		public override int GetPremiumCost()
		{
			return 0;
		}

		private readonly Collider[] _returnCalls = new Collider[20];

		public override void Tick(CharactersInputController c)
		{
			base.Tick(c);

			int nb = Physics.OverlapBoxNonAlloc(c.CharactersCollider.transform.position, _halfExtentsBox, _returnCalls, c.CharactersCollider.transform.rotation, k_LayerMask);

			for(int i = 0; i< nb; ++i)
			{
				Money returnMoney = _returnCalls[i].GetComponent<Money>();

				if (returnMoney != null && !returnMoney.isPremium && !c.CharactersCollider.magnetCoins.Contains(returnMoney.gameObject))
				{
					_returnCalls[i].transform.SetParent(c.transform);
					c.CharactersCollider.magnetCoins.Add(_returnCalls[i].gameObject);
				}
			}
		}
	}
}

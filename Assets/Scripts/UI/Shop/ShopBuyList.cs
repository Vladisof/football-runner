using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UI.Shop
{
	public abstract class ShopBuyList : MonoBehaviour
	{
		public AssetReference prefabItem;
		public RectTransform listRoot;

		protected delegate void RefreshCallback();

		protected RefreshCallback MRefreshCallback;

		public void Open()
		{
			Populate();
			gameObject.SetActive(true);
		}

		public void Close()
		{
			gameObject.SetActive(false);
			MRefreshCallback = null;
		}

		protected void Refresh()
		{
			MRefreshCallback();
		}

		protected abstract void Populate();
	}
}

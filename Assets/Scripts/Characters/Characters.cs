using UnityEngine;
namespace Characters
{
	public class Characters : MonoBehaviour
	{
		public string characterName;
		public int cost;
		public int premiumCost;

		public CharacterAccessor[] accessories;

		public Animator animator;
		public Sprite icon;

		[Header("Sound")]
		public AudioClip jumpSound;
		public AudioClip hitSound;
		public AudioClip deathSound;

		public void SetupAccessor(int accessory)
		{
			for (int i = 0; i < accessories.Length; ++i)
			{
				accessories[i].gameObject.SetActive(i == PlayerSaveData.instance.usedAccessory);
			}
		}
	}
}

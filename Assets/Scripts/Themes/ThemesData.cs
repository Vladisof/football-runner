using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Themes
{
	[System.Serializable]
	public struct ThemeZones
	{
		public int length;
		public AssetReference[] prefabList;
	}

	[CreateAssetMenu(fileName ="themeData", menuName ="Footbal-runner/Theme Data")]
	public class ThemesData : ScriptableObject
	{
		[Header("Theme Data")]
		public string themeName;
		public int cost;
		public int premiumCost;
		public Sprite themeIcon;

		[Header("Objects")]
		public ThemeZones[] zones;
		public GameObject collectiblePrefab;
		public GameObject premiumCollectible;

		[Header("Decoration")]
		public GameObject[] cloudPrefabs;
		public Vector3 cloudMinimumDistance = new Vector3(0, 20.0f, 15.0f);
		public Vector3 cloudSpread = new Vector3(5.0f, 0.0f, 1.0f);
		public int cloudNumber = 10;
		public Mesh skyMesh;
		public Mesh UIGroundMesh;
		public Color fogColor;
	}
}
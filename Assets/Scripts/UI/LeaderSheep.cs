using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
	public class LeaderSheep : MonoBehaviour
	{
		[FormerlySerializedAs("entriesRoot")]
		public RectTransform entriesLeadRoot;
		public int entriesCount;

		public HighScore playerEntry;
		public bool forcePlayerDisplay;
		[FormerlySerializedAs("displayPlayer")]
		public bool displayLeadPlayer = true;

		public void Close()
		{
			gameObject.SetActive(false);
		}

		public void Populate()
		{
			playerEntry.transform.SetAsLastSibling();
			for(int i = 0; i < entriesCount; ++i)
			{
				entriesLeadRoot.GetChild(i).gameObject.SetActive(true);
			}
			
			const int LOCAL_START = 0;
			int place = -1;
			int localPlace = -1;

			if (displayLeadPlayer)
			{
				place = PlayerSaveData.instance.GetScorePlace(int.Parse(playerEntry.score.text));
				localPlace = place - LOCAL_START;
			}

			if (localPlace >= 0 && localPlace < entriesCount && displayLeadPlayer)
			{
				playerEntry.gameObject.SetActive(true);
				playerEntry.transform.SetSiblingIndex(localPlace);
			}

			if (!forcePlayerDisplay || PlayerSaveData.instance.highScores.Count < entriesCount)
				entriesLeadRoot.GetChild(entriesLeadRoot.transform.childCount - 1).gameObject.SetActive(false);

			int currentHighScore = LOCAL_START;

			for (int i = 0; i < entriesCount; ++i)
			{
				HighScore hs = entriesLeadRoot.GetChild(i).GetComponent<HighScore>();

				if (hs == playerEntry || hs == null)
				{
					continue;
				}

				if (PlayerSaveData.instance.highScores.Count > currentHighScore)
				{
					hs.gameObject.SetActive(true);
					hs.playerName.text = PlayerSaveData.instance.highScores[currentHighScore].name;
					hs.number.text = (LOCAL_START + i + 1).ToString();
					hs.score.text = PlayerSaveData.instance.highScores[currentHighScore].score.ToString();

					currentHighScore++;
				} else
					hs.gameObject.SetActive(false);
			}
			
			if (forcePlayerDisplay) 
				playerEntry.gameObject.SetActive(true);

			playerEntry.number.text = (place + 1).ToString();
		}
	}
}

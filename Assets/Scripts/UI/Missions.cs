using System.Collections;
using Missions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace UI
{
    public class Missions : MonoBehaviour
    {
        public RectTransform missionPlace;
        public AssetReference missionEntryPrefab;
        public AssetReference addMissionButtonPrefab;

        public IEnumerator Open()
        {
            gameObject.SetActive(true);

            foreach (Transform t in missionPlace)
                Addressables.ReleaseInstance(t.gameObject);

            for(int i = 0; i < 3; ++i)
            {
                if (PlayerSaveData.instance.missions.Count > i)
                {
                    AsyncOperationHandle op = missionEntryPrefab.InstantiateAsync();
                    yield return op;
                    if (op.Result == null || !(op.Result is GameObject))
                    {
                        Debug.LogWarning($"Unable to load mission entry {missionEntryPrefab.Asset.name}.");
                        yield break;
                    }
                    MissionsEntrys entry = (op.Result as GameObject)?.GetComponent<MissionsEntrys>();

                    if (entry == null)
                    {
                        continue;
                    }

                    entry.transform.SetParent(missionPlace, false);
                    entry.FillWithMission(PlayerSaveData.instance.missions[i], this);

                } else
                {
                    AsyncOperationHandle op = addMissionButtonPrefab.InstantiateAsync();
                    yield return op;
                    if (op.Result == null || !(op.Result is GameObject))
                    {
                        Debug.LogWarning($"Unable to load button {addMissionButtonPrefab.Asset.name}.");
                        yield break;
                    }
                
                }
            }
        }

        public void CallOpen()
        {
            gameObject.SetActive(true);
            StartCoroutine(Open());
        }

        public void Claim(MissionsBase m)
        {
            PlayerSaveData.instance.ClaimMission(m);
            
            StartCoroutine(Open());
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}

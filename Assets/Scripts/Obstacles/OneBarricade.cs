using System.Collections;
using Tracks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Obstacles
{
    public class OneBarricade : ObtObstacles
    {
        private const int k_MinObstacleCount = 1;
        private const int k_MaxObstacleCount = 2;
        private const int k_LeftMostLaneIndex = -1;
        private const int k_RightMostLaneIndex = 1;
    
        public override IEnumerator Spawn(TracksSegment segment, float t)
        {
            bool isTutorialFirst = TracksManager.instance.isTutorial && TracksManager.instance.firstObstacle && segment == segment.manager.currentSegment;

            if (isTutorialFirst)
                TracksManager.instance.firstObstacle = false;
        
            int count = isTutorialFirst ? 1 : Random.Range(k_MinObstacleCount, k_MaxObstacleCount + 1);
            int startLane = isTutorialFirst ? 0 : Random.Range(k_LeftMostLaneIndex, k_RightMostLaneIndex + 1);

            segment.GetPointAt(t, out Vector3 position, out Quaternion rotation);

            for(int i = 0; i < count; ++i)
            {
                int lane = startLane + i;
                lane = lane > k_RightMostLaneIndex ? k_LeftMostLaneIndex : lane;

                AsyncOperationHandle op = Addressables.InstantiateAsync(gameObject.name, position, rotation);
                yield return op;
                if (op.Result == null || !(op.Result is GameObject))
                {
                    Debug.LogWarning($"Unable to load obstacle {gameObject.name}.");
                    yield break;
                }
                GameObject obj = op.Result as GameObject;

                if (obj == null)
                    Debug.Log(gameObject.name);
                else
                {
                    obj.transform.position += obj.transform.right * (lane * segment.manager.laneOffset);

                    obj.transform.SetParent(segment.objectRoot, true);

                    Vector3 oldPos = obj.transform.position;
                    obj.transform.position += Vector3.back;
                    obj.transform.position = oldPos;
                    RegisterObstacle(segment, obj.GetComponent<ObtObstacles>(), t);

                }
            }

        }
    }
}

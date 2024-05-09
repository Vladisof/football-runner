using System.Collections;
using Tracks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Obstacles
{
	public class AllLanesObtObstacles: ObtObstacles
	{
		public override IEnumerator Spawn(TracksSegment segment, float t)
		{
			segment.GetPointAt(t, out Vector3 position, out Quaternion rotation);
			AsyncOperationHandle op = Addressables.InstantiateAsync(gameObject.name, position, rotation);
			yield return op;
			if (op.Result == null || !(op.Result is GameObject))
			{
				Debug.LogWarning($"Unable to load obstacle {gameObject.name}.");
				yield break;
			}
			GameObject obj = op.Result as GameObject;

			if (obj == null)
			{
				yield break;
			}

			obj.transform.SetParent(segment.objectRoot, true);
			
			Vector3 oldPos = obj.transform.position;
			obj.transform.position += Vector3.back;
			obj.transform.position = oldPos;

			RegisterObstacle(segment, obj.GetComponent<ObtObstacles>(), t);

		}
	}
}

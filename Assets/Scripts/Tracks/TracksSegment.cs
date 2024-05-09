using System;
using System.Collections.Generic;
using Obstacles;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Tracks
{
    public class TracksSegment : MonoBehaviour
    {
        public Transform pathParent;
        public TracksManager manager;

        public Transform objectRoot;
        public Transform collectibleTransform;

        public AssetReference[] possibleObstacles;

        public List<ObtObstacles> SpawnedObstacles = new List<ObtObstacles>();

        public readonly Dictionary<float, List<ObtObstacles>> SpawnedObstacleAtPos = new Dictionary<float, List<ObtObstacles>>();


        [SerializeField]
        public float[] obstaclePositions;

        public float worldLength => _mWorldLength;

        private float _mWorldLength;

        private void OnEnable()
        {
            SpawnedObstacles.Capacity = obstaclePositions.Length * 3;
        
            UpdateWorldLength();

            GameObject obj = new GameObject("ObjectRoot");
            obj.transform.SetParent(transform);
            objectRoot = obj.transform;

            obj = new GameObject("Collectibles");
            obj.transform.SetParent(objectRoot);
            collectibleTransform = obj.transform;
        }

        public void GetPointAtInWorldUnit(float wt, out Vector3 pos, out Quaternion rot)
        {
            float t = wt / _mWorldLength;
            GetPointAt(t, out pos, out rot);
        }

        public void GetPointAt(float t, out Vector3 pos, out Quaternion rot)
        {
            float clampedT = Mathf.Clamp01(t);
            float scaledT = (pathParent.childCount - 1) * clampedT;
            int index = Mathf.FloorToInt(scaledT);
            float segmentT = scaledT - index;

            Transform orig = pathParent.GetChild(index);
            if (index == pathParent.childCount - 1)
            {
                pos = orig.position;
                rot = orig.rotation;
                return;
            }

            Transform target = pathParent.GetChild(index + 1);

            pos = Vector3.Lerp(orig.position, target.position, segmentT);
            rot = Quaternion.Lerp(orig.rotation, target.rotation, segmentT);
        }

        private void UpdateWorldLength()
        {
            _mWorldLength = 0;

            for (int i = 1; i < pathParent.childCount; ++i)
            {
                Transform orig = pathParent.GetChild(i - 1);
                Transform end = pathParent.GetChild(i);

                Vector3 vec = end.position - orig.position;
                _mWorldLength += vec.magnitude;
            }
        }

        public void Cleanup()
        {
            SpawnedObstacles.Clear();

            while(collectibleTransform.childCount > 0)
            {
                Transform t = collectibleTransform.GetChild(0);
                t.SetParent(null);
                Money.coinPool.Free(t.gameObject);
            }

            Addressables.ReleaseInstance(gameObject);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (pathParent == null)
                return;

            Color c = Gizmos.color;
            Gizmos.color = Color.red;
            for (int i = 1; i < pathParent.childCount; ++i)
            {
                Transform orig = pathParent.GetChild(i - 1);
                Transform end = pathParent.GetChild(i);

                Gizmos.DrawLine(orig.position, end.position);
            }

            Gizmos.color = Color.blue;
            for (int i = 0; i < obstaclePositions.Length; ++i)
            {
                Vector3 pos;
                Quaternion rot;
                GetPointAt(obstaclePositions[i], out pos, out rot);
                Gizmos.DrawSphere(pos, 0.5f);
            }

            Gizmos.color = c;
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TracksSegment))]
    internal class TracksSeEditor : Editor
    {
        private TracksSegment _mSegment;

        public void OnEnable()
        {
            _mSegment = target as TracksSegment;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Add obstacles"))
            {
                ArrayUtility.Add(ref _mSegment.obstaclePositions, 0.0f);
            }

            if (_mSegment.obstaclePositions == null)
            {
                return;
            }

            int toremove = -1;
            for (int i = 0; i < _mSegment.obstaclePositions.Length; ++i)
            {
                GUILayout.BeginHorizontal();
                _mSegment.obstaclePositions[i] = EditorGUILayout.Slider(_mSegment.obstaclePositions[i], 0.0f, 1.0f);
                if (GUILayout.Button("-", GUILayout.MaxWidth(32)))
                    toremove = i;
                GUILayout.EndHorizontal();
            }


            Array.Sort(_mSegment.obstaclePositions);
            if (toremove != -1)
                ArrayUtility.RemoveAt(ref _mSegment.obstaclePositions, toremove);
        }
    }

#endif
}
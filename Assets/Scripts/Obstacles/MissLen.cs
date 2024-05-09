using System.Collections;
using Tracks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Obstacles
{
	public sealed class MissLen : ObtObstacles
	{
		private static readonly int sDeathHash = Animator.StringToHash("Death");
		private static readonly int sRunHash = Animator.StringToHash("Run");

		public Animator animator;
		public AudioClip[] movingSound;

		private TracksSegment _mOwnSegment;

		private bool mReady { get; set; }
		private bool _mIsMoving;
		private AudioSource _mAudio;

		private const int k_LeftMostLaneIndex = -1;
		private const int k_RightMostLaneIndex = 1;
		private const float k_Speed = 5f;

		public void Awake()
		{
			_mAudio = GetComponent<AudioSource>();
		}

		public override IEnumerator Spawn(TracksSegment segment, float t)
		{
			int lane = Random.Range(k_LeftMostLaneIndex, k_RightMostLaneIndex + 1);

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
			obj.transform.position += obj.transform.right * (lane * segment.manager.laneOffset);

			obj.transform.forward = -obj.transform.forward;
			MissLen missLen = obj.GetComponent<MissLen>();
			missLen._mOwnSegment = segment;

			Vector3 oldPos = obj.transform.position;
			obj.transform.position += Vector3.back;
			obj.transform.position = oldPos;

			missLen.Setup();
			RegisterObstacle(segment, missLen, t);


		}

		private void Setup()
		{
			mReady = true;
		}

		public override void Impacted()
		{
			base.Impacted();

			if (animator != null)
			{
				animator.SetTrigger(sDeathHash);
			}
		}

		public void Update()
		{
			if (!mReady || !_mOwnSegment.manager.isMoving)
			{
				return;
			}

			if (_mIsMoving)
			{
				transform.position += transform.forward * (k_Speed * Time.deltaTime);
			} else
			{
				if (TracksManager.instance.segments[1] != _mOwnSegment)
				{
					return;
				}

				if (animator != null)
				{
					animator.SetTrigger(sRunHash);
				}

				if(_mAudio != null && movingSound != null && movingSound.Length > 0)
				{
					_mAudio.clip = movingSound[Random.Range(0, movingSound.Length)];
					_mAudio.Play();
					_mAudio.loop = true;
				}

				_mIsMoving = true;
			}
		}
	}
}

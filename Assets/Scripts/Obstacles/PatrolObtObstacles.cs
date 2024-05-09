using System.Collections;
using Tracks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Obstacles
{
	public sealed class PatrolObtObstacles : ObtObstacles
	{
		private static readonly int sSpeedRatioHash = Animator.StringToHash("SpeedRatio");
		private static readonly int sDeadHash = Animator.StringToHash("Dead"); 

		[Tooltip("Minimum time to cross all lanes.")]
		public float minTime = 2f;
		[Tooltip("Maximum time to cross all lanes.")]
		public float maxTime = 5f;
		[Tooltip("Leave empty if no animation")]
		public Animator animator;

		public AudioClip[] patrollingSound;

		private TracksSegment _mSegment;

		private Vector3 _mOriginalPosition = Vector3.zero;
		private float _mMaxSpeed;
		private float _mCurrentPos;

		private AudioSource _mAudio;
		private bool _mIsMoving;

		private const float k_LaneOffsetToFullWidth = 2f;

		public override IEnumerator Spawn(TracksSegment segment, float t)
		{
			segment.GetPointAt(t, out Vector3 position, out Quaternion rotation);
			AsyncOperationHandle op = Addressables.InstantiateAsync(gameObject.name, position, rotation);
			yield return op;
			if (op.Result == null || !(op.Result is GameObject o))
			{
				yield break;
			}

			if (o == null)
			{
				yield break;
			}

			o.transform.SetParent(segment.objectRoot, true);

			PatrolObtObstacles po = o.GetComponent<PatrolObtObstacles>();
			po._mSegment = segment;

			
			Vector3 oldPos = o.transform.position;
			o.transform.position += Vector3.back;
			o.transform.position = oldPos;

			po.Setup();

			RegisterObstacle(segment, po, t);
		}

		private void Setup()
		{
			_mAudio = GetComponent<AudioSource>();
			if(_mAudio != null && patrollingSound is
			   {
				   Length: > 0
			   })
			{
				_mAudio.loop = true;
				_mAudio.clip = patrollingSound[Random.Range(0,patrollingSound.Length)];
				_mAudio.Play();
			}

			_mOriginalPosition = transform.localPosition + transform.right * _mSegment.manager.laneOffset;
			transform.localPosition = _mOriginalPosition;

			float actualTime = Random.Range(minTime, maxTime);

			_mMaxSpeed = (_mSegment.manager.laneOffset * k_LaneOffsetToFullWidth * 2) / actualTime;

			if (animator != null)
			{
				AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
				animator.SetFloat(sSpeedRatioHash, clip.length / actualTime);
			}

			_mIsMoving = true;
		}

		public override void Impacted()
		{
			_mIsMoving = false;
			base.Impacted();

			if (animator != null)
			{
				animator.SetTrigger(sDeadHash);
			}
		}

		private void Update()
		{
			if (!_mIsMoving)
				return;

			_mCurrentPos += Time.deltaTime * _mMaxSpeed;

			transform.localPosition = _mOriginalPosition - transform.right * Mathf.PingPong(_mCurrentPos, _mSegment.manager.laneOffset * k_LaneOffsetToFullWidth);
		}
	}
}

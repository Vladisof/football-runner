using UnityEngine;
namespace Sounds
{
	public class CountDownMus : MonoBehaviour
	{
		private AudioSource _mSource;
		private float _mTimeToDisable;

		private const float k_StartDelay = 0.5f;

		private void OnEnable()
		{
			_mSource = GetComponent<AudioSource>();
			_mTimeToDisable = _mSource.clip.length;
			_mSource.PlayDelayed(k_StartDelay);
		}

		private void Update()
		{
			_mTimeToDisable -= Time.deltaTime;

			if (_mTimeToDisable < 0)
				gameObject.SetActive(false);
		}
	}
}

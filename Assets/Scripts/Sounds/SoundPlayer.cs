using System.Collections;
using UnityEngine;
namespace Sounds
{
	public class SoundPlayer : MonoBehaviour
	{
		[System.Serializable]
		public class Stem
		{
			public AudioSource source;
			public AudioClip clip;
			public float startingSpeedRatio;
		}

		private static SoundPlayer _sInstance;
		public static SoundPlayer instance => _sInstance;

		public UnityEngine.Audio.AudioMixer mixer;
		public Stem[] stems;
		public float maxVolume = 0.1f;

		private void Awake()
		{
			if (_sInstance != null)
			{
				Destroy(gameObject);
				return;
			}

			_sInstance = this;
        
			Application.targetFrameRate = 30;
			AudioListener.pause = false;
        
			DontDestroyOnLoad(gameObject);
		}

		private void Start()
		{
			PlayerSaveData.Create ();

			if (PlayerSaveData.instance.masterVolume > float.MinValue) 
			{
				mixer.SetFloat ("MasterVolume", PlayerSaveData.instance.masterVolume);
				mixer.SetFloat ("MusicVolume", PlayerSaveData.instance.musicVolume);
				mixer.SetFloat ("MasterSFXVolume", PlayerSaveData.instance.MasterSfxVolume);
			} else 
			{
				mixer.GetFloat ("MasterVolume", out PlayerSaveData.instance.masterVolume);
				mixer.GetFloat ("MusicVolume", out PlayerSaveData.instance.musicVolume);
				mixer.GetFloat ("MasterSFXVolume", out PlayerSaveData.instance.MasterSfxVolume);

				PlayerSaveData.instance.Save ();
			}

			StartCoroutine(RestartAllStems());
		}

		public void SetStem(int index, AudioClip clip)
		{
			if (stems.Length <= index)
			{
				return;
			}

			stems[index].clip = clip;
		}

		public AudioClip GetStem(int index)
		{
			return stems.Length <= index ? null : stems[index].clip;
		}

		public IEnumerator RestartAllStems()
		{
			foreach (Stem t in stems)
			{
				t.source.clip = t.clip;
				t.source.volume = 0.0f;
				t.source.Play();
			}
			
			yield return new WaitForSeconds(0.05f);

			foreach (Stem t in stems)
			{
				t.source.volume = t.startingSpeedRatio <= 0.0f ? maxVolume : 0.0f;
			}
		}

		public void UpdateVolumes(float currentSpeedRatio)
		{
			const float FADE_SPEED = 0.5f;

			foreach (Stem t in stems)
			{
				float target = currentSpeedRatio >= t.startingSpeedRatio ? maxVolume : 0.0f;
				t.source.volume = Mathf.MoveTowards(t.source.volume, target, FADE_SPEED * Time.deltaTime);
			}
		}
	}
}

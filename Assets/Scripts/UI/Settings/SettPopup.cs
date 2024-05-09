using GameManager;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace UI.Settings
{
    public class SettPopup : MonoBehaviour
    {
        public AudioMixer mixer;

        public Slider masterSlider;
        public Slider musicSlider;
        [FormerlySerializedAs("masterSFXSlider")]
        public Slider MasterSfxSlider;

        [FormerlySerializedAs("loadoutState")]
        public LoadOutState LoadOutState;
        public DataDelConfirm confirmationPopup;

        private float _mMasterVolume;
        private float _mMusicVolume;
        private float _mMasterSfxVolume;

        private const float k_MinVolume = -80f;
        private const string k_MasterVolumeFloatName = "MasterVolume";
        private const string k_MusicVolumeFloatName = "MusicVolume";
        private const string k_MasterSfxVolumeFloatName = "MasterSFXVolume";
    
        public void Open()
        {
            gameObject.SetActive(true);
            UpdateUI();
        }

        public void Close()
        {
            PlayerSaveData.instance.Save ();
            gameObject.SetActive(false);
        }

        private void UpdateUI()
        {
            mixer.GetFloat(k_MasterVolumeFloatName, out _mMasterVolume);
            mixer.GetFloat(k_MusicVolumeFloatName, out _mMusicVolume);
            mixer.GetFloat(k_MasterSfxVolumeFloatName, out _mMasterSfxVolume);

            masterSlider.value = 1.0f - (_mMasterVolume / k_MinVolume);
            musicSlider.value = 1.0f - (_mMusicVolume / k_MinVolume);
            MasterSfxSlider.value = 1.0f - (_mMasterSfxVolume / k_MinVolume);
        }

        public void DeleteData()
        {
            confirmationPopup.Open(LoadOutState);
        }


        public void MasterVolumeChangeValue(float value)
        {
            _mMasterVolume = k_MinVolume * (1.0f - value);
            mixer.SetFloat(k_MasterVolumeFloatName, _mMasterVolume);
            PlayerSaveData.instance.masterVolume = _mMasterVolume;
        }

        public void MusicVolumeChangeValue(float value)
        {
            _mMusicVolume = k_MinVolume * (1.0f - value);
            mixer.SetFloat(k_MusicVolumeFloatName, _mMusicVolume);
            PlayerSaveData.instance.musicVolume = _mMusicVolume;
        }

        public void MasterSfxVolumeChangeValue(float value)
        {
            _mMasterSfxVolume = k_MinVolume * (1.0f - value);
            mixer.SetFloat(k_MasterSfxVolumeFloatName, _mMasterSfxVolume);
            PlayerSaveData.instance.MasterSfxVolume = _mMasterSfxVolume;
        }
    }
}

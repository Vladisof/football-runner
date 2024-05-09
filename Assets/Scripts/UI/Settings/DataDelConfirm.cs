using GameManager;
using UnityEngine;
namespace UI.Settings
{
    public class DataDelConfirm : MonoBehaviour
    {
        private LoadOutState _mLoadOutState;

        public void Open(LoadOutState owner)
        {
            gameObject.SetActive(true);
            _mLoadOutState = owner;
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }

        public void Confirm()
        {
            PlayerSaveData.NewSave();
            _mLoadOutState.UnEquipPowerUp();
            _mLoadOutState.Refresh();
            Close();
        }

        public void Deny()
        {
            Close();
        }
    }
}

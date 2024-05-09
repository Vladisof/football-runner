using UnityEngine;
using UnityEngine.SceneManagement;
namespace UI
{
  public class StartedButtonUI : MonoBehaviour
  {
    public void StartGame()
    {
      if (PlayerSaveData.instance.fTueLevel == 0)
      {
        PlayerSaveData.instance.fTueLevel = 1;
        PlayerSaveData.instance.Save();
      }
      
      SceneManager.LoadScene("main");
    }
  }
}

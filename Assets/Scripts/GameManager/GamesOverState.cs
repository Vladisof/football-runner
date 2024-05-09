using Characters;
using Sounds;
using Tracks;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameManager
{
  public class GamesOverState : SwState
  {
    [FormerlySerializedAs("trackManager")]
    public TracksManager TracksManager;
    public Canvas canvas;
    public UI.Missions missionPopup;

    public AudioClip gameOverTheme;

    [FormerlySerializedAs("MiniLeaderSheap"),FormerlySerializedAs("miniLeaderboard")]
    public LeaderSheep MiniLeaderSheep;

    public override void Enter ()
    {
      canvas.gameObject.SetActive(true);

      MiniLeaderSheep.playerEntry.inputName.text = PlayerSaveData.instance.previousName;

      MiniLeaderSheep.playerEntry.score.text = TracksManager.score.ToString();
      MiniLeaderSheep.Populate();

      if (PlayerSaveData.instance.AnyMissionComplete())
        StartCoroutine(missionPopup.Open());
      else
        missionPopup.gameObject.SetActive(false);

      CreditCoins();

      if (SoundPlayer.instance.GetStem(0) != gameOverTheme)
      {
        SoundPlayer.instance.SetStem(0, gameOverTheme);
        StartCoroutine(SoundPlayer.instance.RestartAllStems());
      }
    }

    public override void Exit (SwState to)
    {
      canvas.gameObject.SetActive(false);
      FinishRun();
    }

    public override string GetName()
    {
      return "GameOver";
    }

    public override void Tick()
    {}

    public void GoToStore()
    {
      UnityEngine.SceneManagement.SceneManager.LoadScene("shop", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }


    public void GoToLoadout()
    {
      TracksManager.isRerun = false;
      manager.SwitchState("Loadout");
    }

    public void RunAgain()
    {
      TracksManager.isRerun = false;
      manager.SwitchState("Game");
    }

    private static void CreditCoins()
    {
      PlayerSaveData.instance.Save();


    }

    private void FinishRun()
    {
      if (MiniLeaderSheep.playerEntry.inputName.text == "")
      {
        MiniLeaderSheep.playerEntry.inputName.text = "Football";
      } else
      {
        PlayerSaveData.instance.previousName = MiniLeaderSheep.playerEntry.inputName.text;
      }

      PlayerSaveData.instance.InsertScore(TracksManager.score, MiniLeaderSheep.playerEntry.inputName.text);

      CharactersCollider.DeathEvent de = TracksManager.CharactersController.CharactersCollider.deathData;

      PlayerSaveData.instance.Save();

      TracksManager.End();
    }

  }
}
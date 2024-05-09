using Characters;
using UnityEngine;

namespace GameManager
{
  public class GamesOverState : SwState
  {
    public TrackManager trackManager;
    public Canvas canvas;
    public MissionUI missionPopup;

    public AudioClip gameOverTheme;

    public Leaderboard miniLeaderboard;

    public override void Enter ()
    {
      canvas.gameObject.SetActive(true);

      miniLeaderboard.playerEntry.inputName.text = PlayerData.instance.previousName;

      miniLeaderboard.playerEntry.score.text = trackManager.score.ToString();
      miniLeaderboard.Populate();

      if (PlayerData.instance.AnyMissionComplete())
        StartCoroutine(missionPopup.Open());
      else
        missionPopup.gameObject.SetActive(false);

      CreditCoins();

      if (MusicPlayer.instance.GetStem(0) != gameOverTheme)
      {
        MusicPlayer.instance.SetStem(0, gameOverTheme);
        StartCoroutine(MusicPlayer.instance.RestartAllStems());
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
      trackManager.isRerun = false;
      manager.SwitchState("Loadout");
    }

    public void RunAgain()
    {
      trackManager.isRerun = false;
      manager.SwitchState("Game");
    }

    private static void CreditCoins()
    {
      PlayerData.instance.Save();


    }

    private void FinishRun()
    {
      if (miniLeaderboard.playerEntry.inputName.text == "")
      {
        miniLeaderboard.playerEntry.inputName.text = "Football";
      } else
      {
        PlayerData.instance.previousName = miniLeaderboard.playerEntry.inputName.text;
      }

      PlayerData.instance.InsertScore(trackManager.score, miniLeaderboard.playerEntry.inputName.text);

      CharactersCollider.DeathEvent de = trackManager.CharactersController.CharactersCollider.deathData;

      PlayerData.instance.Save();

      trackManager.End();
    }

  }
}
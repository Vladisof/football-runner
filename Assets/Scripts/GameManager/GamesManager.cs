using System.Collections.Generic;
using Consumable;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameManager
{
  public class GamesManager : MonoBehaviour
  {
    static public GamesManager instance => _sInstance;
    private static GamesManager _sInstance;

    public SwState [] states;
    public SwState topState
    {
      get
      {
        if (_mStateStack.Count == 0)
          return null;

        return _mStateStack[^1];
      }
    }

    [FormerlySerializedAs("m_ConsumableDatabase")]
    public ConsumablesDatabase MConsumablesDatabase;

    private readonly List<SwState> _mStateStack = new List<SwState>();
    private readonly Dictionary<string, SwState> _mStateDict = new Dictionary<string, SwState>();

    protected void OnEnable()
    {
      PlayerSaveData.Create();

      _sInstance = this;

      MConsumablesDatabase.Load();

      _mStateDict.Clear();

      if (states.Length == 0)
        return;

      foreach (SwState t in states)
      {
        t.manager = this;
        _mStateDict.Add(t.GetName(), t);
      }

      _mStateStack.Clear();

      PushState(states[0].GetName());
    }

    protected void Update()
    {
      if (_mStateStack.Count > 0)
      {
        _mStateStack[^1].Tick();
      }
    }

    public void SwitchState (string newState)
    {
      SwState state = FindState(newState);

      if (state == null)
      {
        Debug.LogError("Can't find the state named " + newState);
        return;
      }

      _mStateStack[^1].Exit(state);
      state.Enter();
      _mStateStack.RemoveAt(_mStateStack.Count - 1);
      _mStateStack.Add(state);
    }

    private SwState FindState (string stateName)
    {
      if (!_mStateDict.TryGetValue(stateName, out SwState state))
      {
        return null;
      }

      return state;
    }

    private void PushState (string nameStat)
    {
      if (!_mStateDict.TryGetValue(nameStat, out SwState state))
      {
        Debug.LogError("Can't find the state " + nameStat);
        return;
      }

      if (_mStateStack.Count > 0)
      {
        _mStateStack[^1].Exit(state);
        state.Enter();
      } else
      {
        state.Enter();
      }

      _mStateStack.Add(state);
    }
  }

  public abstract class SwState : MonoBehaviour
  {
    [HideInInspector]
    public GamesManager manager;

    public abstract void Enter();

    public abstract void Exit (SwState to);

    public abstract void Tick();

    public abstract string GetName();
  }
}
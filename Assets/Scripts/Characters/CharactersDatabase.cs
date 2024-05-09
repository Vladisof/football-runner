using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Characters
{
  public class CharactersDatabase
  {
    private static Dictionary<string, global::Characters.Characters> _mCharactersDict;

    public static Dictionary<string, global::Characters.Characters> dictionary { get { return _mCharactersDict; } }

    private static bool _mLoaded = false;
    public static bool loaded => _mLoaded;

    static public global::Characters.Characters GetCharacter (string type)
    {
      global::Characters.Characters c;

      if (_mCharactersDict == null || !_mCharactersDict.TryGetValue(type, out c))
        return null;

      return c;
    }

    static public IEnumerator LoadDatabase()
    {
      if (_mCharactersDict == null)
      {
        _mCharactersDict = new Dictionary<string, global::Characters.Characters>();

        yield return Addressables.LoadAssetsAsync<GameObject>("characters", op =>
        {
          global::Characters.Characters c = op.GetComponent<global::Characters.Characters>();

          if (c != null)
          {
            _mCharactersDict.Add(c.characterName, c);
          }
        });

        _mLoaded = true;
      }
    }
  }
}
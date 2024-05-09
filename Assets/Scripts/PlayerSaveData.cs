using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Characters;
using Consumable;
using Missions;
using Themes;
using Tracks;
using UI.Shop;
#if UNITY_EDITOR
using UnityEditor;
#endif

public struct HighScoreDataEntry : System.IComparable<HighScoreDataEntry>
{
  public string name;
  public int score;

  public int CompareTo (HighScoreDataEntry other)
  {
    return other.score.CompareTo(score);
  }
}

public class PlayerSaveData
{
  private static PlayerSaveData _mInstance;
  public static PlayerSaveData instance => _mInstance;

  private string _saveFile = "";


  public int coins;
  public int premium;
  public readonly Dictionary<Consumables.ConsumableType, int> consumables = new Dictionary<Consumables.ConsumableType, int>();

  public readonly List<string> characters = new List<string>();
  public int usedCharacter;
  public int usedAccessory = -1;
  public readonly List<string> characterAccessories = new List<string>();
  public readonly List<string> themes = new List<string>();
  public int usedTheme;
  public readonly List<HighScoreDataEntry> highScores = new List<HighScoreDataEntry>();
  public readonly List<MissionsBase> missions = new List<MissionsBase>();

  public string previousName = "Lion";
  
  public bool tutorialDone;

  public float masterVolume = float.MinValue,
    musicVolume = float.MinValue,
    MasterSfxVolume = float.MinValue;
  
  public int fTueLevel;
  public int rank;

  private const int k_MissionCount = 3;

  private const int k_SVersion = 12;

  public void Consume (Consumables.ConsumableType type)
  {
    if (!consumables.ContainsKey(type))
      return;

    consumables[type] -= 1;

    if (consumables[type] == 0)
    {
      consumables.Remove(type);
    }

    Save();
  }

  public void Add (Consumables.ConsumableType type)
  {
    consumables.TryAdd(type, 0);

    consumables[type] += 1;

    Save();
  }

  public void AddCharacter (string name)
  {
    characters.Add(name);
  }

  public void AddTheme (string theme)
  {
    themes.Add(theme);
  }

  public void AddAccessory (string name)
  {
    characterAccessories.Add(name);
  }

  private void CheckMissionsCount()
  {
    while (missions.Count < k_MissionCount)
      AddMission();
  }

  private void AddMission()
  {
    int val = Random.Range(0, (int)MissionsBase.MissionsType.MAXED);

    MissionsBase newMissions = MissionsBase.GetNewMissionFromType((MissionsBase.MissionsType)val);
    newMissions.Created();

    missions.Add(newMissions);
  }

  public void StartRunMissions (TracksManager manager)
  {
    foreach (MissionsBase t in missions)
    {
      t.RunStart(manager);
    }
  }

  public void UpdateMissions (TracksManager manager)
  {
    foreach (MissionsBase t in missions)
    {
      t.Update(manager);
    }
  }

  public bool AnyMissionComplete()
  {
    foreach (MissionsBase t in missions)
    {
      if (t.isComplete)
        return true;
    }

    return false;
  }

  public void ClaimMission (MissionsBase rMissionsBase)
  {
    premium += rMissionsBase.reward;

    this.missions.Remove(rMissionsBase);

    CheckMissionsCount();

    Save();
  }

  public int GetScorePlace (int score)
  {
    HighScoreDataEntry dataEntry = new HighScoreDataEntry
    {
      score = score,
      name = ""
    };

    int index = highScores.BinarySearch(dataEntry);

    return index < 0 ? (~index) : index;
  }

  public void InsertScore (int score, string name)
  {
    HighScoreDataEntry dataEntry = new HighScoreDataEntry();
    dataEntry.score = score;
    dataEntry.name = name;

    highScores.Insert(GetScorePlace(score), dataEntry);

    while (highScores.Count > 10)
      highScores.RemoveAt(highScores.Count - 1);
  }

  public static void Create()
  {
    if (_mInstance == null)
    {
      _mInstance = new PlayerSaveData();
      
      HandlerCoroutineHandler.StartStaticCoroutine(CharactersDatabase.LoadDatabase());
      HandlerCoroutineHandler.StartStaticCoroutine(ThemesDatabases.LoadDatabase());
    }

    _mInstance._saveFile = Application.persistentDataPath + "/save.bin";

    if (File.Exists(_mInstance._saveFile))
    {
      _mInstance.Read();
    } else
    {
      NewSave();
    }

    _mInstance.CheckMissionsCount();
  }

  public static void NewSave()
  {
    _mInstance.characters.Clear();
    _mInstance.themes.Clear();
    _mInstance.missions.Clear();
    _mInstance.characterAccessories.Clear();
    _mInstance.consumables.Clear();

    _mInstance.usedCharacter = 0;
    _mInstance.usedTheme = 0;
    _mInstance.usedAccessory = -1;

    _mInstance.coins = 0;
    _mInstance.premium = 0;

    _mInstance.characters.Add("Lion");
    _mInstance.themes.Add("Day");

    _mInstance.fTueLevel = 0;
    _mInstance.rank = 0;

    _mInstance.CheckMissionsCount();

    _mInstance.Save();
  }

  private void Read()
  {
    BinaryReader r = new BinaryReader(new FileStream(_saveFile, FileMode.Open));

    int ver = r.ReadInt32();

    if (ver < 6)
    {
      r.Close();

      NewSave();
      r = new BinaryReader(new FileStream(_saveFile, FileMode.Open));
      ver = r.ReadInt32();
    }

    coins = r.ReadInt32();

    consumables.Clear();
    int consumableCount = r.ReadInt32();

    for (int i = 0; i < consumableCount; ++i)
    {
      consumables.Add((Consumables.ConsumableType)r.ReadInt32(), r.ReadInt32());
    }
    
    characters.Clear();
    int charCount = r.ReadInt32();

    for (int i = 0; i < charCount; ++i)
    {
      string charName = r.ReadString();

      if (charName.Contains("Raf") && ver < 11)
      {
        charName = charName.Replace("Raf", "Raf");
      }

      characters.Add(charName);
    }

    usedCharacter = r.ReadInt32();
    
    characterAccessories.Clear();
    int accCount = r.ReadInt32();

    for (int i = 0; i < accCount; ++i)
    {
      characterAccessories.Add(r.ReadString());
    }

    themes.Clear();
    int themeCount = r.ReadInt32();

    for (int i = 0; i < themeCount; ++i)
    {
      themes.Add(r.ReadString());
    }

    usedTheme = r.ReadInt32();
    
    if (ver >= 2)
    {
      premium = r.ReadInt32();
    }
    
    if (ver >= 3)
    {
      highScores.Clear();
      int count = r.ReadInt32();

      for (int i = 0; i < count; ++i)
      {
        HighScoreDataEntry dataEntry = new HighScoreDataEntry
        {
          name = r.ReadString(),
          score = r.ReadInt32()
        };

        highScores.Add(dataEntry);
      }
    }

    if (ver >= 4)
    {
      missions.Clear();

      int count = r.ReadInt32();

      for (int i = 0; i < count; ++i)
      {
        MissionsBase.MissionsType type = (MissionsBase.MissionsType)r.ReadInt32();
        MissionsBase tempMissions = MissionsBase.GetNewMissionFromType(type);

        tempMissions.Deserialize(r);

        missions.Add(tempMissions);
      }
    }
    
    if (ver >= 7)
    {
      previousName = r.ReadString();
    }
    

    if (ver >= 9)
    {
      masterVolume = r.ReadSingle();
      musicVolume = r.ReadSingle();
      MasterSfxVolume = r.ReadSingle();
    }

    if (ver >= 10)
    {
      fTueLevel = r.ReadInt32();
      rank = r.ReadInt32();
    }

    if (ver >= 12)
    {
      tutorialDone = r.ReadBoolean();
    }

    r.Close();
  }

  public void Save()
  {
    BinaryWriter w = new BinaryWriter(new FileStream(_saveFile, FileMode.OpenOrCreate));

    w.Write(k_SVersion);
    w.Write(coins);

    w.Write(consumables.Count);

    foreach (KeyValuePair<Consumables.ConsumableType, int> p in consumables)
    {
      w.Write((int)p.Key);
      w.Write(p.Value);
    }

    w.Write(characters.Count);

    foreach (string c in characters)
    {
      w.Write(c);
    }

    w.Write(usedCharacter);

    w.Write(characterAccessories.Count);

    foreach (string a in characterAccessories)
    {
      w.Write(a);
    }

    w.Write(themes.Count);

    foreach (string t in themes)
    {
      w.Write(t);
    }

    w.Write(usedTheme);
    w.Write(premium);
    
    w.Write(highScores.Count);

    for (int i = 0; i < highScores.Count; ++i)
    {
      w.Write(highScores[i].name);
      w.Write(highScores[i].score);
    }
    
    w.Write(missions.Count);

    foreach (MissionsBase t in missions)
    {
      w.Write((int)t.GetMissionType());
      t.Serialize(w);
    }
    
    w.Write(previousName);

    w.Write(masterVolume);
    w.Write(musicVolume);
    w.Write(MasterSfxVolume);

    w.Write(fTueLevel);
    w.Write(rank);

    w.Write(tutorialDone);

    w.Close();
  }


}

#if UNITY_EDITOR
public class PlayerDataEditor : Editor
{
  [MenuItem("Cheat/Clear Save")]
  public static void ClearSave()
  {
    File.Delete(Application.persistentDataPath + "/save.bin");
  }

  [MenuItem("Cheat/Give 1000000 fishbones and 1000 premium")]
  public static void GiveCoins()
  {
    PlayerSaveData.instance.coins += 1000000;
    PlayerSaveData.instance.premium += 1000;
    PlayerSaveData.instance.Save();
  }

  [MenuItem("Cheat/Give 10 Consumables of each types")]
  public static void AddConsumables()
  {

    for (int i = 0; i < BuyItemBuyList.SConsumablesTypes.Length; ++i)
    {
      Consumables c = ConsumablesDatabase.GetConsumbale(BuyItemBuyList.SConsumablesTypes[i]);

      if (c != null)
      {
        PlayerSaveData.instance.consumables[c.GetConsumableType()] = 10;
      }
    }

    PlayerSaveData.instance.Save();
  }
}
#endif
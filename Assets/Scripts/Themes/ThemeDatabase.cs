﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class ThemeDatabase
{
    static protected Dictionary<string, ThemeData> themeDataList;
    static public Dictionary<string, ThemeData> dictionnary { get { return themeDataList; } }

    static protected bool m_Loaded = false;
    static public bool loaded { get { return m_Loaded; } }

    static public ThemeData GetThemeData(string type)
    {
        ThemeData list;
        if (themeDataList == null || !themeDataList.TryGetValue(type, out list))
            return null;

        return list;
    }

    static public IEnumerator LoadDatabase()
    {
        if (themeDataList == null)
        {
            themeDataList = new Dictionary<string, ThemeData>();

Debug.Log("Loading themeData");
            yield return Addressables.LoadAssetsAsync<ThemeData>("themeData", op =>
            {
                Debug.Log("Loaded themeData");
                if (op != null)
                {
                    if(!themeDataList.ContainsKey(op.themeName))
                        themeDataList.Add(op.themeName, op);
                }
            });

            m_Loaded = true;
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Themes
{
    public class ThemesDatabases
    {
        private static Dictionary<string, ThemesData> _themeDataList;
        public static Dictionary<string, ThemesData> dictionary => _themeDataList;

        private static bool _mLoaded = false;
        public static bool loaded => _mLoaded;

        public static ThemesData GetThemeData(string type)
        {
            if (_themeDataList == null || !_themeDataList.TryGetValue(type, out ThemesData list))
                return null;

            return list;
        }

        static public IEnumerator LoadDatabase()
        {
            if (_themeDataList == null)
            {
                _themeDataList = new Dictionary<string, ThemesData>();

                Debug.Log("Loading themeData");
                yield return Addressables.LoadAssetsAsync<ThemesData>("themeData", op =>
                {
                    Debug.Log("Loaded themeData");
                    if (op != null)
                    {
                        _themeDataList.TryAdd(op.themeName, op);
                    }
                });

                _mLoaded = true;
            }

        }
    }
}

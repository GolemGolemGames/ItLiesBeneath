using System.Collections.Generic;
using UnityEditor;

namespace NormTools
{
    /// <summary>
    /// Les ecarts que l'utilisateur a choisi de ne plus voir signaler.
    /// Persistant : un faux positif reste un faux positif d'une session a l'autre.
    /// </summary>
    public static class NormIgnoreList
    {
        private static string _storageKey = "NormChecker.IgnoredEntries";
        private static string _separator = ";;";

        public static bool IsIgnored(NormViolation violation)
        {
            List<string> entries = LoadEntries();
            return entries.Contains(violation.GetIgnoreKey());
        }

        public static void Ignore(NormViolation violation)
        {
            List<string> entries = LoadEntries();
            string key = violation.GetIgnoreKey();

            if (entries.Contains(key))
            {
                return;
            }

            entries.Add(key);
            SaveEntries(entries);
        }

        public static int GetIgnoredCount()
        {
            return LoadEntries().Count;
        }

        public static void ClearAll()
        {
            EditorPrefs.DeleteKey(_storageKey);
        }

        private static List<string> LoadEntries()
        {
            string raw = EditorPrefs.GetString(_storageKey, "");

            if (string.IsNullOrEmpty(raw))
            {
                return new List<string>();
            }

            return new List<string>(raw.Split(new string[] { _separator }, System.StringSplitOptions.RemoveEmptyEntries));
        }

        private static void SaveEntries(List<string> entries)
        {
            EditorPrefs.SetString(_storageKey, string.Join(_separator, entries.ToArray()));
        }
    }
}

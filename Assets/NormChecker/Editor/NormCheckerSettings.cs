using UnityEditor;

namespace NormTools
{
    /// <summary>
    /// Reglages persistants du verificateur d'ecriture.
    /// </summary>
    public static class NormCheckerSettings
    {
        private static string _blockPlayKey = "NormChecker.BlockPlay";
        private static string _checkLocalsKey = "NormChecker.CheckLocals";
        private static string _scanFolderKey = "NormChecker.ScanFolder";
        private static string _bypassSessionKey = "NormChecker.BypassThisSession";

        public static bool IsBlockPlayEnabled()
        {
            return EditorPrefs.GetBool(_blockPlayKey, true);
        }

        public static void SetBlockPlayEnabled(bool value)
        {
            EditorPrefs.SetBool(_blockPlayKey, value);
        }

        /// <summary>
        /// La detection des variables locales est la partie la plus fragile de
        /// l'analyse. Cette option permet de couper ce controle d'un coup si
        /// elle produit trop de faux positifs.
        /// </summary>
        public static bool AreLocalsChecked()
        {
            return EditorPrefs.GetBool(_checkLocalsKey, true);
        }

        public static void SetLocalsChecked(bool value)
        {
            EditorPrefs.SetBool(_checkLocalsKey, value);
        }

        public static string GetScanFolder()
        {
            return EditorPrefs.GetString(_scanFolderKey, "Assets/Scripts");
        }

        public static void SetScanFolder(string value)
        {
            EditorPrefs.SetString(_scanFolderKey, value);
        }

        /// <summary>
        /// Contournement global du blocage, le temps d'une session Unity.
        /// </summary>
        public static bool IsBypassActive()
        {
            return SessionState.GetBool(_bypassSessionKey, false);
        }

        public static void SetBypassActive(bool value)
        {
            SessionState.SetBool(_bypassSessionKey, value);
        }
    }
}

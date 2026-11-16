using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NormTools
{
    /// <summary>
    /// Annule l'entree en mode Play si l'ecriture n'est pas conforme.
    /// </summary>
    [InitializeOnLoad]
    public static class NormPlayModeGuard
    {
        static NormPlayModeGuard()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            if (NormCheckerSettings.IsBlockPlayEnabled() == false)
            {
                return;
            }

            if (NormCheckerSettings.IsBypassActive())
            {
                return;
            }

            List<NormViolation> violations = NormChecker.ScanProject();

            if (violations.Count == 0)
            {
                return;
            }

            EditorApplication.isPlaying = false;

            Debug.LogError("[Norme] Lancement annule : " + violations.Count +
                           " ecart(s) de norme. Ouvre Tools > Verificateur de norme.");

            NormCheckerWindow.LogViolations(violations);
            NormCheckerWindow.ShowWithResults(violations);
        }
    }
}

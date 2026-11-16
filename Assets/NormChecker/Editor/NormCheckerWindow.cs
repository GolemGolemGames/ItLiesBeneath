using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NormTools
{
    /// <summary>
    /// Fenetre principale : liste les ecarts d'ecriture.
    /// Trois actions par ligne : ignorer, aller a l'endroit, voir la regle.
    /// Aucune correction automatique : c'est toi qui renommes.
    /// </summary>
    public class NormCheckerWindow : EditorWindow
    {
        private static List<NormViolation> _violations = new List<NormViolation>();
        private static bool _hasScannedOnce = false;

        private Vector2 _scrollPosition;
        private bool _showSettings = false;

        [MenuItem("Tools/Verificateur de norme %#n")]
        public static void ShowWindow()
        {
            NormCheckerWindow window = GetWindow<NormCheckerWindow>("Norme");
            window.minSize = new Vector2(600f, 320f);
            window.Show();
        }

        public static void ShowWithResults(List<NormViolation> violations)
        {
            _violations = violations;
            _hasScannedOnce = true;

            NormCheckerWindow window = GetWindow<NormCheckerWindow>("Norme");
            window.minSize = new Vector2(600f, 320f);
            window.Show();
            window.Focus();
            window.Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawSettings();
            DrawSummary();
            DrawViolationList();
            DrawFooter();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Analyser", EditorStyles.toolbarButton, GUILayout.Width(90f)))
            {
                RunScan();
            }

            if (GUILayout.Button("Voir la norme", EditorStyles.toolbarButton, GUILayout.Width(110f)))
            {
                NormDocWindow.ShowWindow();
            }

            GUILayout.FlexibleSpace();

            int ignoredCount = NormIgnoreList.GetIgnoredCount();

            if (ignoredCount > 0)
            {
                if (GUILayout.Button("Ignores : " + ignoredCount, EditorStyles.toolbarButton, GUILayout.Width(100f)))
                {
                    RequestClearIgnored();
                }
            }

            _showSettings = GUILayout.Toggle(_showSettings, "Reglages", EditorStyles.toolbarButton, GUILayout.Width(80f));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSettings()
        {
            if (_showSettings == false)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Reglages", EditorStyles.boldLabel);

            GUIContent folderLabel = new GUIContent("Dossier analyse",
                "Chemin du dossier parcouru par l'analyse, relatif a la racine du projet.");
            string folder = EditorGUILayout.TextField(folderLabel, NormCheckerSettings.GetScanFolder());
            NormCheckerSettings.SetScanFolder(folder);

            GUIContent blockLabel = new GUIContent("Bloquer le Play",
                "Empeche d'entrer en mode Play tant qu'un ecart de nommage subsiste.");
            bool blockPlay = EditorGUILayout.Toggle(blockLabel, NormCheckerSettings.IsBlockPlayEnabled());
            NormCheckerSettings.SetBlockPlayEnabled(blockPlay);

            GUIContent localsLabel = new GUIContent("Verifier variables locales",
                "Controle le plus sujet aux faux positifs. Decoche pour le couper d'un coup s'il produit trop de bruit.");
            bool checkLocals = EditorGUILayout.Toggle(localsLabel, NormCheckerSettings.AreLocalsChecked());
            NormCheckerSettings.SetLocalsChecked(checkLocals);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4f);
        }

        private void DrawSummary()
        {
            if (_hasScannedOnce == false)
            {
                EditorGUILayout.HelpBox("Aucune analyse effectuee. Clique sur \"Analyser\".", MessageType.Info);
                return;
            }

            if (_violations.Count == 0)
            {
                EditorGUILayout.HelpBox("Ecriture conforme a la norme.", MessageType.Info);
                return;
            }

            string plural = _violations.Count > 1 ? "s" : "";
            EditorGUILayout.HelpBox(
                _violations.Count + " ecart" + plural + " de norme. Renomme-les toi-meme dans ton editeur, " +
                "puis relance l'analyse.",
                MessageType.Error);
        }

        private void DrawViolationList()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            string lastFile = "";

            foreach (NormViolation violation in _violations)
            {
                if (violation.GetFilePath() != lastFile)
                {
                    EditorGUILayout.Space(6f);
                    EditorGUILayout.LabelField(violation.GetFilePath(), EditorStyles.boldLabel);
                    lastFile = violation.GetFilePath();
                }

                DrawViolationRow(violation);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawViolationRow(NormViolation violation)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Ligne " + violation.GetLineNumber(), EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(violation.GetMessage(), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Norme : " + NormRuleBook.GetShortRule(violation.GetRule()), EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Aller", GUILayout.Width(58f), GUILayout.Height(44f)))
            {
                OpenViolation(violation);
            }

            if (GUILayout.Button("Regle", GUILayout.Width(58f), GUILayout.Height(44f)))
            {
                NormDocWindow.ShowRule(violation.GetRule());
            }

            if (GUILayout.Button("Ignorer", GUILayout.Width(62f), GUILayout.Height(44f)))
            {
                RequestIgnoreSingle(violation);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFooter()
        {
            if (_violations.Count == 0)
            {
                return;
            }

            EditorGUILayout.Space(4f);

            if (NormCheckerSettings.IsBypassActive())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Blocage du Play desactive jusqu'au redemarrage de Unity.", MessageType.Warning);

                if (GUILayout.Button("Reactiver", GUILayout.Width(90f), GUILayout.Height(38f)))
                {
                    NormCheckerSettings.SetBypassActive(false);
                }

                EditorGUILayout.EndHorizontal();
                return;
            }

            if (GUILayout.Button("Lancer quand meme cette session", GUILayout.Height(26f)))
            {
                RequestBypass();
            }
        }

        private void RequestIgnoreSingle(NormViolation violation)
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Ignorer cet ecart ?",
                violation.GetMessage() + "\n\n" +
                "Norme : " + NormRuleBook.GetShortRule(violation.GetRule()) + "\n\n" +
                "Cet ecart ne sera plus signale. Ne l'ignore que s'il s'agit d'un faux positif : " +
                "une erreur de norme divise ta note par deux.",
                "Ignorer",
                "Annuler");

            if (confirmed == false)
            {
                return;
            }

            NormIgnoreList.Ignore(violation);
            RunScan();
        }

        private void RequestClearIgnored()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Reafficher les ecarts ignores ?",
                NormIgnoreList.GetIgnoredCount() + " ecart(s) sont actuellement masques.\n\n" +
                "Veux-tu les reafficher ?",
                "Tout reafficher",
                "Annuler");

            if (confirmed == false)
            {
                return;
            }

            NormIgnoreList.ClearAll();
            RunScan();
        }

        private void RequestBypass()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Lancer malgre les ecarts ?",
                "Il reste " + _violations.Count + " ecart(s) de norme.\n\n" +
                "Le blocage sera reactive au prochain redemarrage de Unity.",
                "Oui, je suis sur",
                "Annuler");

            if (confirmed == false)
            {
                return;
            }

            NormCheckerSettings.SetBypassActive(true);
            Debug.LogWarning("[Norme] Blocage du Play desactive pour cette session.");
        }

        private void RunScan()
        {
            _violations = NormChecker.ScanProject();
            _hasScannedOnce = true;
            LogViolations(_violations);
            Repaint();
        }

        private static void OpenViolation(NormViolation violation)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(violation.GetFilePath());

            if (asset == null)
            {
                Debug.LogWarning("[Norme] Fichier introuvable : " + violation.GetFilePath());
                return;
            }

            AssetDatabase.OpenAsset(asset, violation.GetLineNumber());
        }

        /// <summary>
        /// Ecrit chaque ecart dans la Console. Le format "chemin(ligne,0):"
        /// est celui que Unity reconnait pour rendre la ligne double-cliquable.
        /// </summary>
        public static void LogViolations(List<NormViolation> violations)
        {
            foreach (NormViolation violation in violations)
            {
                Object context = AssetDatabase.LoadAssetAtPath<Object>(violation.GetFilePath());

                string message = violation.GetFilePath() + "(" + violation.GetLineNumber() + ",0): " +
                                 "[Norme] " + violation.GetMessage() +
                                 "  ->  " + NormRuleBook.GetShortRule(violation.GetRule());

                Debug.LogError(message, context);
            }
        }
    }
}

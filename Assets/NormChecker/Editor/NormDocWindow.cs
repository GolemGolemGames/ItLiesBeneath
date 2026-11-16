using UnityEditor;
using UnityEngine;

namespace NormTools
{
    /// <summary>
    /// Affiche le document de norme, avec la ligne concernee mise en evidence.
    /// </summary>
    public class NormDocWindow : EditorWindow
    {
        private static int _highlightedLine = -1;

        private Vector2 _scrollPosition;
        private GUIStyle _lineStyle;
        private GUIStyle _highlightedStyle;
        private GUIStyle _unwatchedStyle;

        [MenuItem("Tools/Document de norme")]
        public static void ShowWindow()
        {
            _highlightedLine = -1;
            OpenWindow();
        }

        public static void ShowRule(ENormRule rule)
        {
            _highlightedLine = NormRuleBook.GetLineIndexForRule(rule);
            OpenWindow();
        }

        private static void OpenWindow()
        {
            NormDocWindow window = GetWindow<NormDocWindow>("Norme");
            window.minSize = new Vector2(420f, 420f);
            window.Show();
            window.Focus();
            window.Repaint();
        }

        private void BuildStyles()
        {
            if (_lineStyle != null)
            {
                return;
            }

            _lineStyle = new GUIStyle(EditorStyles.label);
            _lineStyle.fontSize = 12;
            _lineStyle.richText = false;
            _lineStyle.padding = new RectOffset(8, 4, 2, 2);

            _highlightedStyle = new GUIStyle(_lineStyle);
            _highlightedStyle.fontStyle = FontStyle.Bold;

            _unwatchedStyle = new GUIStyle(_lineStyle);
            _unwatchedStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        }

        private void OnGUI()
        {
            BuildStyles();

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Document de norme du projet", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Les lignes en gris ne sont pas verifiees par le plugin.", EditorStyles.miniLabel);
            EditorGUILayout.Space(4f);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            string[] lines = NormRuleBook.GetDocumentLines();

            for (int i = 0; i < lines.Length; i++)
            {
                DrawLine(lines[i], i);
            }

            EditorGUILayout.EndScrollView();

            if (_highlightedLine >= 0)
            {
                EditorGUILayout.Space(4f);

                if (GUILayout.Button("Retirer la mise en evidence", GUILayout.Height(22f)))
                {
                    _highlightedLine = -1;
                }
            }
        }

        private void DrawLine(string text, int lineIndex)
        {
            bool isHighlighted = lineIndex == _highlightedLine;
            bool isUnwatched = NormRuleBook.IsUnwatchedLine(lineIndex);

            GUIStyle style = _lineStyle;

            if (isHighlighted)
            {
                style = _highlightedStyle;
            }
            else if (isUnwatched)
            {
                style = _unwatchedStyle;
            }

            Rect rect = GUILayoutUtility.GetRect(new GUIContent(text), style);

            if (isHighlighted)
            {
                EditorGUI.DrawRect(rect, new Color(1f, 0.85f, 0.25f, 0.35f));
            }

            GUI.Label(rect, text, style);
        }
    }
}

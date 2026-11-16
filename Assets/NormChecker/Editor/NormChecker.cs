using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NormTools
{
    /// <summary>
    /// Verificateur d'ECRITURE uniquement.
    ///
    /// Ce que le plugin verifie :
    ///   Class          : CamelCase
    ///   Attributs      : _camelCase
    ///   Variables      : camelCase
    ///   Methodes       : CamelCase()
    ///   Enum           : ECamelCase
    ///   Valeur d'enum  : VALUE
    ///   Interfaces     : ICamelCase
    ///   Abstract Class : ACamelCase
    ///   Attributs private
    ///
    /// Ce que le plugin ne regarde JAMAIS :
    ///   les techniques employees. Aucune detection de static, de coroutine,
    ///   de recherche globale ou de quoi que ce soit d'autre. Tu ecris le code
    ///   que tu veux, le plugin ne juge que la facon de nommer.
    ///
    /// Non verifiable automatiquement : "tous les noms doivent etre en anglais".
    /// </summary>
    public static class NormChecker
    {
        private static Regex _typeRegex = new Regex(
            @"(?<mods>(?:\b(?:public|private|protected|internal|abstract|sealed|static|partial|new)\b\s+)*)" +
            @"\b(?<kind>class|struct|interface|enum)\b\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)");

        private static Regex _methodRegex = new Regex(
            @"^\s*(?:\[[^\]]*\]\s*)*(?<mods>(?:\b(?:public|private|protected|internal|static|virtual|override|abstract|async|sealed|extern|partial|new|unsafe)\b\s+)*)" +
            @"(?<ret>[A-Za-z_][A-Za-z0-9_<>,\.\[\]\?]*)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\(");

        private static Regex _fieldRegex = new Regex(
            @"^\s*(?:\[[^\]]*\]\s*)*(?<mods>(?:\b(?:public|private|protected|internal|static|readonly|const|volatile|event)\b\s+)*)" +
            @"(?<type>[A-Za-z_][A-Za-z0-9_<>,\.\[\]\?]*)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*(?:=[^;]*)?;");

        private static Regex _localRegex = new Regex(
            @"^\s*(?<type>[A-Za-z_][A-Za-z0-9_<>,\.\[\]\?]*)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*(?<tail>=[^=]|;)");

        private static Regex _forEachRegex = new Regex(
            @"\bforeach\s*\(\s*(?:[A-Za-z_][A-Za-z0-9_<>,\.\[\]\?]*)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s+in\b");

        private static Regex _forRegex = new Regex(
            @"\bfor\s*\(\s*(?:[A-Za-z_][A-Za-z0-9_<>,\.\[\]\?]*)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=");

        private static Regex _enumValueRegex = new Regex(
            @"^\s*(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*(?:=\s*[^,]+)?\s*,?\s*$");

        private static Regex _pascalCaseRegex = new Regex(@"^[A-Z][A-Za-z0-9]*$");
        private static Regex _camelCaseRegex = new Regex(@"^[a-z][A-Za-z0-9]*$");
        private static Regex _underscoreCamelRegex = new Regex(@"^_[a-z][A-Za-z0-9]*$");
        private static Regex _upperSnakeRegex = new Regex(@"^[A-Z][A-Z0-9_]*$");

        private static HashSet<string> _keywords = new HashSet<string>
        {
            "return", "if", "else", "while", "for", "foreach", "switch", "case", "break",
            "continue", "using", "namespace", "public", "private", "protected", "internal",
            "static", "void", "new", "throw", "yield", "do", "try", "catch", "finally",
            "lock", "in", "out", "ref", "base", "this", "get", "set", "add", "remove",
            "default", "goto", "checked", "unchecked", "fixed", "unsafe", "delegate",
            "class", "struct", "interface", "enum", "where", "select", "from", "partial"
        };

        public static List<NormViolation> ScanProject()
        {
            List<NormViolation> violations = new List<NormViolation>();
            string scanFolder = NormCheckerSettings.GetScanFolder();

            if (Directory.Exists(scanFolder) == false)
            {
                Debug.LogWarning("[Norme] Dossier introuvable : " + scanFolder + ". Verifie le chemin dans les reglages.");
                return violations;
            }

            string[] files = Directory.GetFiles(scanFolder, "*.cs", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string normalizedPath = file.Replace('\\', '/');

                if (ShouldSkipFile(normalizedPath))
                {
                    continue;
                }

                ScanFile(normalizedPath, violations);
            }

            return FilterIgnored(violations);
        }

        private static List<NormViolation> FilterIgnored(List<NormViolation> violations)
        {
            List<NormViolation> result = new List<NormViolation>();

            foreach (NormViolation violation in violations)
            {
                if (NormIgnoreList.IsIgnored(violation))
                {
                    continue;
                }

                result.Add(violation);
            }

            return result;
        }

        private static bool ShouldSkipFile(string path)
        {
            if (path.Contains("/NormChecker") || path.Contains("/Editor/Norm"))
            {
                return true;
            }

            if (path.Contains("/TextMesh Pro/") || path.Contains("/Plugins/") || path.Contains("/TutorialInfo/"))
            {
                return true;
            }

            if (path.EndsWith(".Designer.cs") || path.Contains("/Generated/"))
            {
                return true;
            }

            return false;
        }

        private static void ScanFile(string path, List<NormViolation> violations)
        {
            string[] lines = File.ReadAllLines(path);

            bool inBlockComment = false;
            int braceDepth = 0;
            int memberDepth = -1;
            int enumDepth = -1;
            string pendingTypeKind = "";
            string currentTypeName = "";

            for (int i = 0; i < lines.Length; i++)
            {
                string rawLine = lines[i];
                string line = StripComments(rawLine, ref inBlockComment);

                if (string.IsNullOrEmpty(line.Trim()) == false)
                {
                    bool insideEnumBody = enumDepth >= 0 && braceDepth > enumDepth;
                    bool atMemberLevel = memberDepth >= 0 && braceDepth == memberDepth && insideEnumBody == false;
                    bool insideMethodBody = memberDepth >= 0 && braceDepth > memberDepth && insideEnumBody == false;

                    if (insideEnumBody)
                    {
                        CheckEnumValue(path, i, line, rawLine, violations);
                    }
                    else
                    {
                        Match typeMatch = _typeRegex.Match(line);

                        if (typeMatch.Success)
                        {
                            CheckTypeName(path, i, typeMatch, rawLine, violations);
                            pendingTypeKind = typeMatch.Groups["kind"].Value;
                            currentTypeName = typeMatch.Groups["name"].Value;
                        }
                        else if (atMemberLevel)
                        {
                            CheckMemberLine(path, i, line, rawLine, currentTypeName, violations);
                        }
                        else if (insideMethodBody && NormCheckerSettings.AreLocalsChecked())
                        {
                            CheckLocalVariables(path, i, line, rawLine, violations);
                        }
                    }
                }

                for (int c = 0; c < line.Length; c++)
                {
                    if (line[c] == '{')
                    {
                        braceDepth++;

                        if (pendingTypeKind.Length > 0)
                        {
                            if (pendingTypeKind == "enum")
                            {
                                enumDepth = braceDepth - 1;
                            }
                            else
                            {
                                memberDepth = braceDepth;
                            }

                            pendingTypeKind = "";
                        }
                    }
                    else if (line[c] == '}')
                    {
                        braceDepth--;

                        if (enumDepth >= 0 && braceDepth <= enumDepth)
                        {
                            enumDepth = -1;
                        }

                        if (memberDepth >= 0 && braceDepth < memberDepth)
                        {
                            memberDepth = -1;
                            currentTypeName = "";
                        }
                    }
                }
            }
        }

        private static void CheckTypeName(string path, int lineIndex, Match match, string rawLine, List<NormViolation> violations)
        {
            string kind = match.Groups["kind"].Value;
            string name = match.Groups["name"].Value;
            string mods = match.Groups["mods"].Value;

            if (kind == "enum")
            {
                if (name.StartsWith("E") == false || _pascalCaseRegex.IsMatch(name) == false)
                {
                    Add(violations, path, lineIndex, ENormRule.ENUM,
                        "L'enum \"" + name + "\" ne respecte pas la forme ECamelCase.", rawLine, name);
                }

                return;
            }

            if (kind == "interface")
            {
                if (name.StartsWith("I") == false || _pascalCaseRegex.IsMatch(name) == false)
                {
                    Add(violations, path, lineIndex, ENormRule.INTERFACE,
                        "L'interface \"" + name + "\" ne respecte pas la forme ICamelCase.", rawLine, name);
                }

                return;
            }

            if (mods.Contains("abstract"))
            {
                if (name.StartsWith("A") == false || _pascalCaseRegex.IsMatch(name) == false)
                {
                    Add(violations, path, lineIndex, ENormRule.ABSTRACT_CLASS,
                        "La classe abstraite \"" + name + "\" ne respecte pas la forme ACamelCase.", rawLine, name);
                }

                return;
            }

            if (_pascalCaseRegex.IsMatch(name) == false)
            {
                Add(violations, path, lineIndex, ENormRule.CLASS,
                    "La classe \"" + name + "\" ne respecte pas la forme CamelCase.", rawLine, name);
            }
        }

        private static void CheckMemberLine(string path, int lineIndex, string line, string rawLine, string currentTypeName, List<NormViolation> violations)
        {
            Match methodMatch = _methodRegex.Match(line);

            if (methodMatch.Success)
            {
                string name = methodMatch.Groups["name"].Value;
                string returnType = methodMatch.Groups["ret"].Value;

                if (_keywords.Contains(returnType))
                {
                    return;
                }

                if (name == currentTypeName || returnType == currentTypeName)
                {
                    return;
                }

                if (_pascalCaseRegex.IsMatch(name) == false)
                {
                    Add(violations, path, lineIndex, ENormRule.METHOD,
                        "La methode \"" + name + "()\" ne respecte pas la forme CamelCase().", rawLine, name);
                }

                return;
            }

            Match fieldMatch = _fieldRegex.Match(line);

            if (fieldMatch.Success == false)
            {
                return;
            }

            string fieldName = fieldMatch.Groups["name"].Value;
            string fieldType = fieldMatch.Groups["type"].Value;
            string fieldMods = fieldMatch.Groups["mods"].Value;

            if (_keywords.Contains(fieldType))
            {
                return;
            }

            bool isExposed = fieldMods.Contains("public") || fieldMods.Contains("protected") || fieldMods.Contains("internal");

            if (isExposed)
            {
                Add(violations, path, lineIndex, ENormRule.ATTRIBUTE_PRIVATE,
                    "L'attribut \"" + fieldName + "\" n'est pas private.", rawLine, fieldName);
            }

            if (_underscoreCamelRegex.IsMatch(fieldName) == false)
            {
                Add(violations, path, lineIndex, ENormRule.ATTRIBUTE,
                    "L'attribut \"" + fieldName + "\" ne respecte pas la forme _camelCase.", rawLine, fieldName);
            }
        }

        private static void CheckLocalVariables(string path, int lineIndex, string line, string rawLine, List<NormViolation> violations)
        {
            Match forEachMatch = _forEachRegex.Match(line);

            if (forEachMatch.Success)
            {
                ValidateLocalName(path, lineIndex, forEachMatch.Groups["name"].Value, rawLine, violations);
                return;
            }

            Match forMatch = _forRegex.Match(line);

            if (forMatch.Success)
            {
                ValidateLocalName(path, lineIndex, forMatch.Groups["name"].Value, rawLine, violations);
                return;
            }

            if (line.Contains("(") && line.Contains("=") && line.IndexOf("(") < line.IndexOf("="))
            {
                return;
            }

            Match localMatch = _localRegex.Match(line);

            if (localMatch.Success == false)
            {
                return;
            }

            string type = localMatch.Groups["type"].Value;
            string name = localMatch.Groups["name"].Value;

            if (_keywords.Contains(type) || _keywords.Contains(name) || type.Contains("."))
            {
                return;
            }

            ValidateLocalName(path, lineIndex, name, rawLine, violations);
        }

        private static void ValidateLocalName(string path, int lineIndex, string name, string rawLine, List<NormViolation> violations)
        {
            if (_keywords.Contains(name))
            {
                return;
            }

            if (_camelCaseRegex.IsMatch(name) == false)
            {
                Add(violations, path, lineIndex, ENormRule.VARIABLE,
                    "La variable \"" + name + "\" ne respecte pas la forme camelCase.", rawLine, name);
            }
        }

        private static void CheckEnumValue(string path, int lineIndex, string line, string rawLine, List<NormViolation> violations)
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith("[") || trimmed == "{" || trimmed == "}")
            {
                return;
            }

            Match match = _enumValueRegex.Match(line);

            if (match.Success == false)
            {
                return;
            }

            string name = match.Groups["name"].Value;

            if (_keywords.Contains(name))
            {
                return;
            }

            if (_upperSnakeRegex.IsMatch(name) == false)
            {
                Add(violations, path, lineIndex, ENormRule.ENUM_VALUE,
                    "La valeur d'enum \"" + name + "\" n'est pas en majuscules.", rawLine, name);
            }
        }

        private static void Add(List<NormViolation> violations, string path, int lineIndex,
                                ENormRule rule, string message, string rawLine, string identifierName)
        {
            violations.Add(new NormViolation(path, lineIndex + 1, rule, message, rawLine.Trim(), identifierName));
        }

        private static string StripComments(string line, ref bool inBlockComment)
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            bool inString = false;
            bool inChar = false;

            for (int i = 0; i < line.Length; i++)
            {
                if (inBlockComment)
                {
                    if (i + 1 < line.Length && line[i] == '*' && line[i + 1] == '/')
                    {
                        inBlockComment = false;
                        i++;
                    }

                    continue;
                }

                if (inString)
                {
                    if (line[i] == '\\')
                    {
                        i++;
                        continue;
                    }

                    if (line[i] == '"')
                    {
                        inString = false;
                        result.Append('"');
                    }

                    continue;
                }

                if (inChar)
                {
                    if (line[i] == '\\')
                    {
                        i++;
                        continue;
                    }

                    if (line[i] == '\'')
                    {
                        inChar = false;
                    }

                    continue;
                }

                if (i + 1 < line.Length && line[i] == '/' && line[i + 1] == '/')
                {
                    break;
                }

                if (i + 1 < line.Length && line[i] == '/' && line[i + 1] == '*')
                {
                    inBlockComment = true;
                    i++;
                    continue;
                }

                if (line[i] == '"')
                {
                    inString = true;
                    result.Append('"');
                    continue;
                }

                if (line[i] == '\'')
                {
                    inChar = true;
                    continue;
                }

                result.Append(line[i]);
            }

            return result.ToString();
        }
    }
}

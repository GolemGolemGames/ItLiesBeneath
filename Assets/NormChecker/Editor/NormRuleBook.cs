using System.Collections.Generic;

namespace NormTools
{
    /// <summary>
    /// Le document de norme, tel qu'il a ete fourni, et la correspondance
    /// entre chaque regle verifiee et sa ligne dans ce document.
    ///
    /// Le plugin ne verifie que la section nommage. Les autres lignes sont
    /// affichees pour reference, mais ne declenchent jamais rien.
    /// </summary>
    public static class NormRuleBook
    {
        private static string[] _documentLines = new string[]
        {
            "NORME",
            "",
            "Class :",
            "CamelCase",
            "",
            "Attributs :",
            "_camelCase",
            "",
            "Variables :",
            "camelCase",
            "",
            "Methodes :",
            "CamelCase()",
            "",
            "Enum :",
            "ECamelCase",
            "",
            "Valeur d'enum :",
            "VALUE",
            "",
            "Interfaces :",
            "ICamelCase",
            "",
            "Abstract Class :",
            "ACamelCase",
            "",
            "INTERDICTION D'UTILISER DES FONCTIONS DE RECHERCHE GLOBALES.",
            "EX : FindObjectOfType()",
            "",
            "INTERDICTION D'UTILISER DES COROUTINES AVANT QU'ON LES AI VU EN COURS.",
            "",
            "INTERDICTION D'UTILISER LE MOT CLE \"STATIC\" SANS AUTORISATIONS.",
            "",
            "TOUS LES ATTRIBUTS DOIVENT ETRE \"PRIVATE\".",
            "TOUS LES NOMS DOIVENT ETRE EN ANGLAIS !",
            "",
            "Toute erreur de norme entrainera des malus aux evaluations."
        };

        private static Dictionary<ENormRule, int> _ruleLines = new Dictionary<ENormRule, int>
        {
            { ENormRule.CLASS, 3 },
            { ENormRule.ATTRIBUTE, 6 },
            { ENormRule.VARIABLE, 9 },
            { ENormRule.METHOD, 12 },
            { ENormRule.ENUM, 15 },
            { ENormRule.ENUM_VALUE, 18 },
            { ENormRule.INTERFACE, 21 },
            { ENormRule.ABSTRACT_CLASS, 24 },
            { ENormRule.ATTRIBUTE_PRIVATE, 33 }
        };

        /// <summary>
        /// Les lignes qui ne sont jamais verifiees par le plugin : affichees
        /// en grise dans la fenetre de norme.
        /// </summary>
        private static HashSet<int> _unwatchedLines = new HashSet<int>
        {
            26, 27, 29, 31, 34
        };

        public static string[] GetDocumentLines()
        {
            return _documentLines;
        }

        /// <summary>
        /// Renvoie l'index de la ligne du document qui correspond a la regle,
        /// ou -1 si la regle n'est pas repertoriee.
        /// </summary>
        public static int GetLineIndexForRule(ENormRule rule)
        {
            if (_ruleLines.ContainsKey(rule) == false)
            {
                return -1;
            }

            return _ruleLines[rule];
        }

        public static bool IsUnwatchedLine(int lineIndex)
        {
            return _unwatchedLines.Contains(lineIndex);
        }

        /// <summary>
        /// Le rappel court affiche a cote de l'ecart releve.
        /// </summary>
        public static string GetShortRule(ENormRule rule)
        {
            switch (rule)
            {
                case ENormRule.CLASS:
                    return "Class : CamelCase";

                case ENormRule.ATTRIBUTE:
                    return "Attributs : _camelCase";

                case ENormRule.ATTRIBUTE_PRIVATE:
                    return "Tous les attributs doivent etre private";

                case ENormRule.VARIABLE:
                    return "Variables : camelCase";

                case ENormRule.METHOD:
                    return "Methodes : CamelCase()";

                case ENormRule.ENUM:
                    return "Enum : ECamelCase";

                case ENormRule.ENUM_VALUE:
                    return "Valeur d'enum : VALUE";

                case ENormRule.INTERFACE:
                    return "Interfaces : ICamelCase";

                case ENormRule.ABSTRACT_CLASS:
                    return "Abstract Class : ACamelCase";

                default:
                    return "";
            }
        }
    }
}

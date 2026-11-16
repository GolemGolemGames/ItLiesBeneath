namespace NormTools
{
    /// <summary>
    /// Un ecart d'ecriture releve dans un fichier source.
    /// Le plugin ne corrige rien : il signale, situe, et renvoie a la regle.
    /// </summary>
    public class NormViolation
    {
        private string _filePath;
        private int _lineNumber;
        private ENormRule _rule;
        private string _message;
        private string _lineContent;
        private string _identifierName;

        public NormViolation(string filePath, int lineNumber, ENormRule rule,
                             string message, string lineContent, string identifierName)
        {
            _filePath = filePath;
            _lineNumber = lineNumber;
            _rule = rule;
            _message = message;
            _lineContent = lineContent;
            _identifierName = identifierName;
        }

        public string GetFilePath()
        {
            return _filePath;
        }

        public int GetLineNumber()
        {
            return _lineNumber;
        }

        public ENormRule GetRule()
        {
            return _rule;
        }

        public string GetMessage()
        {
            return _message;
        }

        public string GetLineContent()
        {
            return _lineContent;
        }

        public string GetIdentifierName()
        {
            return _identifierName;
        }

        /// <summary>
        /// Cle stable utilisee par la liste des ignores.
        /// Basee sur le nom plutot que sur le numero de ligne, pour survivre
        /// aux modifications du fichier.
        /// </summary>
        public string GetIgnoreKey()
        {
            return _filePath + "|" + _rule + "|" + _identifierName;
        }
    }
}

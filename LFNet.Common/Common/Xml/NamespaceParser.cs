using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace LFNet.Common.Xml
{
    /// <summary>
    /// A class to parse all the xml namespaces from an XPathNavigator.
    /// </summary>
    public class NamespaceParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Xml.NamespaceParser" /> class.
        /// </summary>
        public NamespaceParser()
        {
            this.DefaultPrefix = "d";
            this.DefaultScheme = DefaultPrefixScheme.PrefixPlusIndex;
            this.ParseChildren = true;
            this.DefaultNamespaces = new HashSet<string>();
        }

        private string GetDefaultKey(int index)
        {
            int num = 0x61;
            int num2 = 0x7a;
            int num3 = num + index;
            bool flag = this.DefaultScheme == DefaultPrefixScheme.AutomaticLetter;
            bool flag2 = this.DefaultScheme == DefaultPrefixScheme.PrefixAndLetter;
            string defaultPrefix = this.DefaultPrefix;
            do
            {
                bool flag3 = num3 > num2;
                if (!flag3 && flag)
                {
                    defaultPrefix = ((char) num3).ToString();
                }
                else if (!flag3 && flag2)
                {
                    defaultPrefix = this.DefaultPrefix + ((char) num3).ToString();
                }
                else if (index > 0)
                {
                    defaultPrefix = this.DefaultPrefix + index.ToString();
                }
                index++;
                num3++;
            }
            while (this.Namespaces.ContainsKey(defaultPrefix));
            return defaultPrefix;
        }

        /// <summary>
        /// Parses an XML document for its namespaces.
        /// </summary>
        /// <param name="navigator">The navigator.</param>
        public void ParseNamespaces(XPathNavigator navigator)
        {
            if (navigator == null)
            {
                throw new ArgumentNullException("navigator");
            }
            if (this.Namespaces == null)
            {
                this.Namespaces = new Dictionary<string, string>();
            }
            if (string.IsNullOrEmpty(this.DefaultPrefix))
            {
                this.DefaultPrefix = "d";
            }
            this.DefaultNamespaces.Clear();
            navigator.MoveToRoot();
            this.RrecursiveParse(navigator);
            int num = 0;
            foreach (string str in this.DefaultNamespaces)
            {
                string defaultKey = this.GetDefaultKey(num++);
                this.Namespaces.Add(defaultKey, str);
            }
        }

        private void RrecursiveParse(XPathNavigator navigator)
        {
            foreach (KeyValuePair<string, string> pair in navigator.GetNamespacesInScope(XmlNamespaceScope.Local))
            {
                if (string.IsNullOrEmpty(pair.Key))
                {
                    this.DefaultNamespaces.Add(pair.Value);
                }
                else if (!this.Namespaces.ContainsKey(pair.Key))
                {
                    this.Namespaces.Add(pair.Key, pair.Value);
                }
            }
            if ((navigator.HasChildren && (this.ParseChildren || (navigator.NodeType == XPathNodeType.Root))) && navigator.MoveToFirstChild())
            {
                do
                {
                    this.RrecursiveParse(navigator);
                }
                while (navigator.MoveToNext(XPathNodeType.Element));
                navigator.MoveToParent();
            }
        }

        public ICollection<string> DefaultNamespaces { get; private set; }

        /// <summary>
        /// Gets or sets the default prefix.
        /// </summary>
        /// <value>The default prefix.</value>
        public string DefaultPrefix { get; set; }

        /// <summary>
        /// Gets or sets the scheme to use when generating prefix values for default namespaces.
        /// </summary>
        /// <value>The default scheme.</value>
        public DefaultPrefixScheme DefaultScheme { get; set; }

        /// <summary>
        /// Gets or sets the namespaces collection.
        /// </summary>
        /// <value>The namespaces collection.</value>
        public IDictionary<string, string> Namespaces { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to parse child nodes to find namespaces.
        /// </summary>
        /// <value><c>true</c> if parse child nodes; otherwise, to only parse root element, <c>false</c>.</value>
        public bool ParseChildren { get; set; }

        public enum DefaultPrefixScheme
        {
            AutomaticLetter = 1,
            PrefixAndLetter = 3,
            PrefixPlusIndex = 2
        }
    }
}


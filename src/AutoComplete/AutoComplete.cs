using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoComplete
{
    public struct FullName
    {
        public string Name;
        public string Surname;
        public string Patronymic;

        public override string ToString()
        {
            return $"{Surname?.Trim()} {Name?.Trim()} {Patronymic?.Trim()}".Trim();
        }
    }

    public class AutoCompleter
    {   
        /// <summary>
        /// Trie (prefix tree) container for full names
        /// </summary>
        private class Trie
        {
            /// <summary>
            /// Representation of one node (symbol) in Trie
            /// </summary>
            private class Node
            {
                public char Symbol { get; set; }
                public Dictionary<char, Node>? Children { get; set; }
                public bool IsFullName { get; set; }
            }

            private readonly Node _root;

            public Trie()
            {   
                _root = new Node();
            }

            /// <summary>
            /// Adds name to Trie
            /// </summary>
            /// <param name="name">Name</param>
            public void AddName(string name)
            {   
                var current = _root;
                for (int i = 0; i < name.Length; i++)
                {
                    if (current.Children != null && current.Children.TryGetValue(name[i], out var node))
                    {
                        current = node;
                        continue;
                    }

                    if (current.Children == null)
                    {
                        current.Children = new Dictionary<char, Node>();
                    }

                    current.Children.Add(name[i], current = new Node() { Symbol = name[i] });
                }
                current.IsFullName = true;
            }

            /// <summary>
            /// Find all names that starts with prefix
            /// </summary>
            /// <param name="prefix">Prefix</param>
            /// <returns>Collections of names</returns>
            public IEnumerable<string> StartsWith(string prefix)
            {
                var node = GetNode(prefix);
                if (node == null)
                {
                    return Enumerable.Empty<string>();
                }
                return GetNames(new StringBuilder().Append(prefix.AsSpan(0, prefix.Length - 1)), node);
            }

            /// <summary>
            /// Get name from Trie by building it from nodes
            /// </summary>
            /// <param name="builder">String builder</param>
            /// <param name="node">Node</param>
            /// <returns>Name</returns>
            private IEnumerable<string> GetNames(StringBuilder builder, Node node)
            {
                builder.Append(node.Symbol);
                if (node.IsFullName)
                {   
                    yield return builder.ToString();
                }

                if (node.Children == null)
                {
                    builder.Remove(builder.Length - 1, 1);

                    yield break;
                }

                foreach (var childNode in node.Children.Values)
                {
                    foreach (var name in GetNames(builder, childNode))
                    {
                        yield return name;
                    }
                }

                builder.Remove(builder.Length - 1, 1);
            }
            
            /// <summary>
            /// Get endname node from Trie
            /// </summary>
            /// <param name="prefix"></param>
            /// <returns></returns>
            private Node GetNode(string prefix)
            {
                var current = _root;
                for (int i = 0; i < prefix.Length; i++)
                {
                    if (current.Children != null && current.Children.TryGetValue(prefix[i], out var node))
                    {
                        current = node;
                    }
                    else
                    {
                        return null;
                    }
                }
                return current;
            }
        }

        private readonly Trie _fullNamesContainer = new Trie();

        public void AddToSearch(List<FullName> fullNames)
        {
            foreach (var fullName in fullNames)
            {   
                _fullNamesContainer.AddName(fullName.ToString());
            }
        }

        public List<string> Search(string prefix)
        {   
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentNullException(nameof(prefix), "Parameter cant be null or contain only white-spaces");
            }

            if (prefix.Length > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(prefix), "Parameter length cant be more than 100");
            }

            prefix = string.Join(' ', prefix.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            
            return _fullNamesContainer.StartsWith(prefix).ToList();
        }
    }
}

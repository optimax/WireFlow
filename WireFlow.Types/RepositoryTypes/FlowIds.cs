using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WireFlow
{
    /// <summary>
    /// IDs that links name. An element is given one made of its kind and a number (<c>rect-3</c>), a page, layout or part one
    /// made of its name (<c>page-2</c> for "Page 2"). Each is unique in the project; IDs compare ignoring case.
    /// </summary>
    public static class FlowIds
    {
        /// <summary>A set of IDs that compares them ignoring case.</summary>
        public static HashSet<string> NewSet() => new HashSet<string>(StringComparer.OrdinalIgnoreCase);


        /// <summary>The first of <c>kind-1</c>, <c>kind-2</c>, ... not in <paramref name="taken"/>.</summary>
        public static string ForKind(string kind, ICollection<string> taken)
        {
            string prefix = Slug(kind);
            if (prefix.Length == 0)
                prefix = "element";
            for (int n = 1; ; n++)
            {
                string id = $"{prefix}-{n}";
                if (!taken.Contains(id))
                    return id;
            }
        }


        /// <summary>The name in lower case, with a hyphen for each run of other characters than letters and digits, made unique
        /// with <c>-2</c>, <c>-3</c>, ... if <paramref name="taken"/> has it.</summary>
        public static string ForName(string name, ICollection<string> taken)
        {
            string id = Slug(name);
            if (id.Length == 0)
                id = "page";
            if (!taken.Contains(id))
                return id;
            for (int n = 2; ; n++)
                if (!taken.Contains($"{id}-{n}"))
                    return $"{id}-{n}";
        }


        /// <summary>A node and every node under it: a page's layers, a layer's elements, a group's (a stack panel's, a part's) children.</summary>
        public static IEnumerable<FlowBase> Nodes(FlowBase root)
        {
            if (root == null)
                yield break;
            yield return root;
            IEnumerable<FlowBase> children = root switch
            {
                FlowPage page => page.Layers,
                FlowLayer layer => layer.Elements,
                FlowGroup group => group.Children,
                _ => null
            };
            foreach (var child in children ?? Enumerable.Empty<FlowBase>())
                foreach (var node in Nodes(child))
                    yield return node;
        }


        /// <summary>The IDs of a node and the nodes under it that have one.</summary>
        public static IEnumerable<string> IdsIn(FlowBase root) => Nodes(root).Select(n => n.Id).Where(id => !string.IsNullOrWhiteSpace(id));


        private static string Slug(string text)
        {
            var result = new StringBuilder();
            bool gap = false;
            foreach (char c in (text ?? "").Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c))
                {
                    if (gap && result.Length > 0)
                        result.Append('-');
                    result.Append(c);
                    gap = false;
                }
                else
                    gap = true;
            }
            return result.ToString();
        }
    }
}

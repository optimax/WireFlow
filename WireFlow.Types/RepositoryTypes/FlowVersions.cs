using System.Collections.Generic;
using System.Linq;

namespace WireFlow
{
    /// <summary>
    /// The saved versions of a page, layout or part, as the Designer's History pane lists them. A project's file keeps a
    /// document's saved version, the edits saved since as its draft, and, in its archive, each earlier version under the
    /// revision of the project whose save replaced it; so the revisions of its archived versions are those at which the
    /// document was saved anew.
    /// </summary>
    public static class FlowVersions
    {
        /// <summary>The word for the version a document shows, after its title.</summary>
        public const string Current = " (current)";


        /// <summary>
        /// The titles of a document's versions, the latest first: "Draft", when it has edits saved since the last revision;
        /// "Revision N" for each revision of the project at which it was saved anew (the revisions its archived versions
        /// were replaced at), the latest of which is its saved version; and "First version", the one it had before the
        /// first of them. A document without a saved version, as a new one not yet part of a revision, has its draft
        /// alone. The version the document shows, its draft or else its saved version, is marked as current.
        /// </summary>
        public static List<string> Titles(IEnumerable<long> replacedAt, bool hasSaved, bool hasDraft)
        {
            var titles = new List<string>();
            if (hasSaved)
            {
                titles.AddRange((replacedAt ?? Enumerable.Empty<long>()).Distinct().OrderByDescending(r => r).Select(r => $"Revision {r}"));
                titles.Add("First version");
            }
            if (hasDraft)
                titles.Insert(0, "Draft");
            if (titles.Count > 0)
                titles[0] += Current;
            return titles;
        }
    }
}

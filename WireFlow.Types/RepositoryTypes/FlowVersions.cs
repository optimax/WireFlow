using System.Collections.Generic;
using System.Linq;

namespace WireFlow
{
    /// <summary>
    /// A saved version of a page, layout or part, as the Designer's History pane lists it: its title, and where the
    /// project's file holds it.
    /// </summary>
    public class FlowVersion
    {
        /// <summary>"Draft", "Revision N" or "First version", with " (current)" after it for the version the document shows.</summary>
        public string Title { get; set; }

        /// <summary>Whether it is the version the document shows when it has no changes: its draft, or else its saved version.</summary>
        public bool IsCurrent { get; set; }

        /// <summary>Whether it is the draft: the edits saved since the last revision.</summary>
        public bool IsDraft { get; set; }

        /// <summary>
        /// The revision under which the archive holds the version (the revision whose save replaced it); null for the
        /// draft and for the saved version, which the document itself holds.
        /// </summary>
        public long? ArchivedUnder { get; set; }

        public override string ToString() => Title;
    }


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
        /// A document's versions, the latest first: "Draft", when it has edits saved since the last revision; "Revision N"
        /// for each revision of the project at which it was saved anew (the revisions its archived versions were replaced
        /// at), the latest of which is its saved version; and "First version", the one it had before the first of them. A
        /// document without a saved version, as a new one not yet part of a revision, has its draft alone. The version the
        /// document shows, its draft or else its saved version, is marked as current. Each says where the file holds it:
        /// the version saved at a revision lies in the archive under the next revision at which the document was saved,
        /// and the first one under the earliest.
        /// </summary>
        public static List<FlowVersion> List(IEnumerable<long> replacedAt, bool hasSaved, bool hasDraft)
        {
            var versions = new List<FlowVersion>();
            if (hasSaved)
            {
                var revisions = (replacedAt ?? Enumerable.Empty<long>()).Distinct().OrderByDescending(r => r).ToList();
                for (int i = 0; i < revisions.Count; i++)
                    versions.Add(new FlowVersion { Title = $"Revision {revisions[i]}", ArchivedUnder = i == 0 ? (long?)null : revisions[i - 1] });
                versions.Add(new FlowVersion { Title = "First version", ArchivedUnder = revisions.Count == 0 ? (long?)null : revisions[revisions.Count - 1] });
            }
            if (hasDraft)
                versions.Insert(0, new FlowVersion { Title = "Draft", IsDraft = true });
            if (versions.Count > 0)
            {
                versions[0].IsCurrent = true;
                versions[0].Title += Current;
            }
            return versions;
        }


        /// <summary>The titles of a document's versions, the latest first (see <see cref="List"/>).</summary>
        public static List<string> Titles(IEnumerable<long> replacedAt, bool hasSaved, bool hasDraft) =>
            List(replacedAt, hasSaved, hasDraft).Select(v => v.Title).ToList();
    }
}

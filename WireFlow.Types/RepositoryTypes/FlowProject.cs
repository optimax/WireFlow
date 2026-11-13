using System;
using System.Collections.Generic;
using System.Linq;

namespace WireFlow
{
    public class FlowProject: FlowBase
    {
        /// <summary>
        /// The page background of a new project: a gray that differs from the area around the page in every theme (#333333 or #CCCCCC)
        /// without glaring in the dark one.
        /// </summary>
        public const string DEFAULT_BACKGROUND = "#555";

        private long revisionNo;
        private double width;
        private double height;
        private string background;
        private string home;
        private List<string> pageOrder;
        private string navigateBadgeColor;
        private string navigateBadgeTextColor;
        private string linkHighlightColor;
        private double? linkHighlightOpacity;

        public long RevisionNo { get => revisionNo; set => Set(ref revisionNo, value); }
        public double Width { get => width; set => Set(ref width, value); }
        public double Height { get => height; set => Set(ref height, value); }
        public string Background { get => background; set => Set(ref background, value); }
        public string Home { get => home; set => Set(ref home, value); }

        /// <summary>
        /// The names of the pages in their order, the first the project's entry point, or null until they are first ordered. It
        /// may name pages that no longer exist and leave out some that do: <see cref="OrderPages"/> makes the order of the pages.
        /// </summary>
        public List<string> PageOrder { get => pageOrder; set => Set(ref pageOrder, value); }

        /// <summary>
        /// The Designer's Navigate mode colors for this project, #AARRGGBB, as they suit its pages: the badge that shows the mode, its
        /// text, and the highlight laid over links, with the highlight's opacity (0 to 1). Null where the project has none of its own,
        /// as a project made before them: the Designer's own (Tools > Options with no project open), which a new project starts with.
        /// </summary>
        public string NavigateBadgeColor { get => navigateBadgeColor; set => Set(ref navigateBadgeColor, value); }
        public string NavigateBadgeTextColor { get => navigateBadgeTextColor; set => Set(ref navigateBadgeTextColor, value); }
        public string LinkHighlightColor { get => linkHighlightColor; set => Set(ref linkHighlightColor, value); }
        public double? LinkHighlightOpacity { get => linkHighlightOpacity; set => Set(ref linkHighlightOpacity, value); }

        public FlowProject()
        {
            Width = 1024;
            Height = 768;
            Background = DEFAULT_BACKGROUND;
            Home = "index";
        }


        /// <summary>
        /// The pages named, in the project's order: those <see cref="PageOrder"/> names, in its order, then the others in the order
        /// given. Without an order, the home page comes first, so that a project ordered never before keeps its entry point.
        /// Names compare ignoring case.
        /// </summary>
        public List<string> OrderPages(IEnumerable<string> pageNames)
        {
            var names = pageNames.ToList();
            var ordered = new List<string>();
            foreach (string name in PageOrder ?? (string.IsNullOrWhiteSpace(Home) ? new List<string>() : new List<string> { Home }))
            {
                string page = names.FirstOrDefault(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
                if (page != null && !ordered.Contains(page))
                    ordered.Add(page);
            }
            ordered.AddRange(names.Where(n => !ordered.Contains(n)));
            return ordered;
        }
    }
}

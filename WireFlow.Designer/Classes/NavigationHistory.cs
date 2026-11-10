using System;
using System.Collections.Generic;

namespace WireFlow
{
    /// <summary>
    /// The pages visited in Navigate mode, kept as a browser keeps them: Back and Forward move along the list, and a page
    /// visited after going back drops the pages that were ahead. Page names compare ignoring case.
    /// </summary>
    public class NavigationHistory
    {
        private readonly List<string> pages = new List<string>();
        private int index = -1;


        /// <summary>The page shown, or null before the first visit.</summary>
        public string Current => index >= 0 ? pages[index] : null;

        public bool CanGoBack => index > 0;

        public bool CanGoForward => index >= 0 && index < pages.Count - 1;


        /// <summary>Records a page shown; the page shown already is not recorded twice.</summary>
        public void Visit(string page)
        {
            if (string.IsNullOrWhiteSpace(page) || string.Equals(Current, page, StringComparison.OrdinalIgnoreCase))
                return;
            pages.RemoveRange(index + 1, pages.Count - index - 1);
            pages.Add(page);
            index = pages.Count - 1;
        }


        /// <summary>Steps back and gives the page to show, or null when there is none before.</summary>
        public string Back()
        {
            if (!CanGoBack)
                return null;
            index--;
            return pages[index];
        }


        /// <summary>Steps forward and gives the page to show, or null when there is none after.</summary>
        public string Forward()
        {
            if (!CanGoForward)
                return null;
            index++;
            return pages[index];
        }


        public void Clear()
        {
            pages.Clear();
            index = -1;
        }
    }
}

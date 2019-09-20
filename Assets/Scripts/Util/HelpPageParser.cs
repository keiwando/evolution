using System;

namespace Keiwando.Evolution.UI {

    /// Help page file structure
    /// 
    /// Section 1 Title
    /// #-#-#-Text-#-#-#
    /// Section 1 Text
    /// 
    /// #%#%#%Section%#%#%#
    /// Section 2 Title
    /// #-#-#-Text-#-#-#
    /// Section 2 Text
    /// ...
    public static class HelpPageParser {

        private const string SECTION_SEPARATOR = "#%#%#%Section%#%#%#\n";
        private const string TITLE_SEPARATOR = "#-#-#-Text-#-#-#\n";

        private static readonly string[] SECTION_SEPARATOR_ARR = new string[] { SECTION_SEPARATOR };
        private static readonly string[] TITLE_SEPARATOR_ARR = new string[] { TITLE_SEPARATOR };

        public static HelpPage[] Parse(string fileContents) {

            var sections = fileContents.Split(SECTION_SEPARATOR_ARR, System.StringSplitOptions.None);
            var pages = new HelpPage[sections.Length];

            for (int i = 0; i < sections.Length; i++) {
                var section = sections[i].Split(TITLE_SEPARATOR_ARR, System.StringSplitOptions.None);
                pages[i] = new HelpPage () {
                    Title = section[0].Replace("\n", ""),
                    Text = section[1]
                };
            }

            return pages;
        }
    }
}
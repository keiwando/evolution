using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Keiwando.Evolution {

    public struct SearchResult {
        public int[] indices;
        public string[] results;
    }

    public class Search {

        private string[] values;

        public Search(string[] values) {
            this.values = values;
        }

        public SearchResult Find(string query) {
            var resultList = new List<int>();
            var regex = new Regex(string.Format("^.*{0}.*$", query), RegexOptions.IgnoreCase);
            for (int i = 0, length = values.Length; i < length; i++) {
                string comp = values[i];
                if (regex.Matches(comp).Count > 0) {
                    resultList.Add(i);
                }
            }
            int[] indices = resultList.ToArray();
            string[] results = new string[indices.Length];
            for (int i = 0, len = indices.Length; i < len; i++) {
                results[i] = values[indices[i]];
            }
            return new SearchResult {
                indices = indices,
                results = results
            };
        }
    }
}
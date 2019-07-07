using Newtonsoft.Json.Linq;

namespace Keiwando.Evolution {

    public struct EditorSettings {

        public const float MIN_GRID_SIZE = 1f;
        public const float MAX_GRID_SIZE = 3.0f;

        public bool GridEnabled;

        public float GridSize;

        public static readonly EditorSettings Default = new EditorSettings() {
            GridEnabled = false,
            GridSize = 1f
        };

        #region Encode & Decode

        private static class CodingKey {
            public const string GridEnabled = "gridEnabled";
            public const string GridSize = "gridSize";
        }

        public JObject Encode() {
            var json = new JObject();

            json[CodingKey.GridEnabled] = this.GridEnabled;
            json[CodingKey.GridSize] = this.GridSize;
            return json;
        }

        public static EditorSettings Decode(string encoded) {

            if (string.IsNullOrEmpty(encoded)) 
                return Default;

            JObject json = JObject.Parse(encoded);

            bool gridEnabled = json[CodingKey.GridEnabled].ToObject<bool>();
            float gridSize = json[CodingKey.GridSize].ToObject<float>();

            return new EditorSettings() {
                GridEnabled = gridEnabled,
                GridSize = gridSize
            };
        }

        #endregion
    }
}
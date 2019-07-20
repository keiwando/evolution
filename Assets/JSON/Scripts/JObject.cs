using System.Text;
using System.Collections.Generic;

namespace Keiwando.JSON {

    public class JObject: JToken {

        public new static JObject Parse(string encoded) {
            return JSONParser.ParseFirstObject(encoded).Token;
        }

        private Dictionary<string, JToken> values = new Dictionary<string, JToken>();

        public override JToken this[string key] {
            get { return values[key]; }
            set { 
                if (value == null) {
                    values[key] = new JNull();
                } else {
                    values[key] = value; 
                }
            }
        }

        public void Set(string key, float number) {
            this[key] = new JNumber(number);
        }

        public void Set(string key, int number) {
            this[key] = new JNumber(number);
        }

        public void Set(string key, float[] numbers) {
            this[key] = new JArray(numbers);
        }

        public void Set(string key, int[] numbers) {
            this[key] = new JArray(numbers);
        }

        public void Set(string key, JToken[] tokens) {
            this[key] = new JArray(tokens);
        }

        public void Set(string key, bool b) {
            this[key] = new JBoolean(b);
        }

        public void Set(string key, JObject obj) {
            this[key] = obj;
        }

        public bool ContainsKey(string key) {
            return values.ContainsKey(key);
        }

        public bool IsEmpty() {
            return values.Count == 0;
        }

        public JObject ObjectForKey(string key) {
            return this[key] as JObject;
        }

        public override T Decode<T>(IJsonDecoder<T> decoder) {
             return decoder(this);
        }

        internal override void ToString(FormatContext format, StringBuilder builder) {
            builder.Append("{");
            var indented = format.Indented();
            foreach (KeyValuePair<string, JToken> entry in this.values) {
                indented.OptionalNewline(builder);
                builder.Append("\"");
                builder.Append(entry.Key);
                builder.Append("\"");
                indented.OptionalWhitespace(builder);
                builder.Append(":");
                indented.OptionalWhitespace(builder);
                entry.Value.ToString(indented, builder);
                builder.Append(",");
            }
            if (this.values.Count > 0) {
                // Remove the last comma
                builder.Remove(builder.Length - 1, 1);
            }
            format.OptionalNewline(builder);
            builder.Append("}");
        }
    }
}
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Keiwando.JSON {

    public class JArray: JToken {

        private static Regex pattern = new Regex(@"\[(.*)\]");

        private JToken[] elements;

        public JToken this[int i] {
            get { return elements[i]; }
            set {
                if (value == null)
                    elements[i] = new JNull();
                else
                    elements[i] = value;
            }
        }

        public int Length { get { return elements.Length; } }

        public new static JArray Parse(string encoded) {
            return JSONParser.ParseFirstArray(encoded).Token;
        }

        public new static JArray From<T>(List<T> elements) where T: IJsonConvertible {
            var tokens = new JToken[elements.Count];
            for (int i = 0; i < elements.Count; i++) {
                tokens[i] = elements[i].Encode();
            }
            return new JArray(tokens);
        }

        public JArray(IList<float> numbers) {
            this.elements = new JToken[numbers.Count];
            for (int i = 0; i < numbers.Count; i++) {
                this.elements[i] = new JNumber(numbers[i]);
            }
        }

        public JArray(IList<int> numbers) {
            this.elements = new JToken[numbers.Count];
            for (int i = 0; i < numbers.Count; i++) {
                this.elements[i] = new JNumber(numbers[i]);
            }
        }

        public JArray(IList<string> strings) {
            this.elements = new JToken[strings.Count];
            for (int i = 0; i < strings.Count; i++) {
                this.elements[i] = new JString(strings[i]);
            }
        }

        public JArray(IList<IJsonConvertible> elements) {
            this.elements = new JToken[elements.Count];
            for (int i = 0; i < elements.Count; i++) {
                this.elements[i] = elements[i].Encode();
            }
        }

        public JArray(List<JToken> elements) {
            this.elements = elements.ToArray();
        }

        public JArray(string[] strings) {
            this.elements = new JToken[strings.Length];
            for (int i = 0; i < strings.Length; i++) {
                this.elements[i] = new JString(strings[i]);
            }
        }

        public JArray(float[] numbers) {
            this.elements = new JToken[numbers.Length];
            for (int i = 0; i < numbers.Length; i++) {
                this.elements[i] = new JNumber(numbers[i]);
            }
        }

        public JArray(int[] numbers) {
            this.elements = new JToken[numbers.Length];
            for (int i = 0; i < numbers.Length; i++) {
                this.elements[i] = new JNumber(numbers[i]);
            }
        }

        public JArray(JToken[] elements) {
            this.elements = elements;
        }

        internal override void ToString(FormatContext format, StringBuilder builder) {
            builder.Append("[");
            FormatContext indented = format.Indented();

            if (elements.Length > 0)
                indented.OptionalNewline(builder);

            for (int i = 0; i < elements.Length; i++) {
                elements[i].ToString(indented, builder);
                if (i != elements.Length - 1) {
                    builder.Append(",");
                }
                if (i < elements.Length - 1) {
                    indented.OptionalNewline(builder);
                }
            }
            
            if (elements.Length > 0)
                format.OptionalNewline(builder);

            builder.Append("]");
        }

        public override JToken[] ToArray() {
            return this.elements;
        }

        public override List<JToken> ToList() {
            return new List<JToken>(this.elements);
        }

        public override T[] ToArray<T>(IJsonDecoder<T> decode) {

            var result = new T[elements.Length];
            for (int i = 0; i < elements.Length; i++) {
                result[i] = decode(elements[i] as JObject);
            }
            return result;
        }

        public override List<T> ToList<T>(IJsonDecoder<T> decode) {

            var result = new List<T>();
            for (int i = 0; i < elements.Length; i++) {
                result.Add(decode(elements[i] as JObject));
            }
            return result;
        }

        public override int[] ToIntArray() {
            var result = new int[elements.Length];
            for (int i = 0; i < elements.Length; i++) {
                result[i] = elements[i].ToInt();
            }
            return result;
        }

        public override float[] ToFloatArray() {
            var result = new float[elements.Length];
            for (int i = 0; i < elements.Length; i++) {
                result[i] = elements[i].ToFloat();
            }
            return result;
        }

        public override string[] ToStringArray() {
            var result = new string[elements.Length];
            for (int i = 0; i < elements.Length; i++) {
                result[i] = elements[i].ToString();
            }
            return result;
        }
    }
}
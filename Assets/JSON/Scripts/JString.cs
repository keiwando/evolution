using System.Text;

namespace Keiwando.JSON {

    public class JString: JToken {

        private Substring value;

        public JString(string value) {
            this.value = value;
        }

        internal JString(Substring value) {
            this.value = value;
        }

        public static implicit operator JString(string val) {
            return new JString(val);
        }

        public override string ToString() {
            return this.value.ToString();
        }

        internal override void ToString(FormatContext format, StringBuilder builder) {
            builder.Append('"');
            for (int i = 0; i < value.Length; i++) {
                char c = value[i];
                if (c == '"') {
                    builder.Append("\\\"");
                } else {
                    builder.Append(c);
                }
            }
            builder.Append('"');
        }
    }
}
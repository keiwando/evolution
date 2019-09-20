using System.Text;

namespace Keiwando.JSON {

    public class JBoolean: JToken {

        private bool value;

        public JBoolean(bool val) {
            this.value = val;
        }

        public static implicit operator JBoolean(bool val) {
            return new JBoolean(val);
        }

        public override bool ToBool() {
            return this.value;
        }

        internal override void ToString(FormatContext formatting, StringBuilder builder) {
            builder.Append(value ? "true" : "false");
        }
    }
}
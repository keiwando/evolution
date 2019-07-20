using System.Text;

namespace Keiwando.JSON {

    public class JNull: JToken {

        internal override void ToString(FormatContext format, StringBuilder builder) {
            builder.Append("null");
        }

        public override bool IsNull() {
            return true;
        }
    }
}
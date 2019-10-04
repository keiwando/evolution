using System.Text;
using System.Globalization;

namespace Keiwando.JSON {

    public class JNumber: JToken {

        private enum Type {
            Float, Int
        }

        private Type type;

        private float valueF;
        private int valueI;

        public new static JNumber Parse(string encoded) {
            return JSONParser.ParseFirstNumber(encoded).Token;
        }

        public JNumber(float number) {
            this.valueF = number;
            this.valueI = (int)System.Math.Round(number);
            this.type = Type.Float;
        }

        public JNumber(int number) {
            this.valueF = number;
            this.valueI = number;
            this.type = Type.Int;
        }

        public static implicit operator JNumber(float val) {
            return new JNumber(val);
        }

        public static implicit operator JNumber(int val) {
            return new JNumber(val);
        }

        internal override void ToString(FormatContext format, StringBuilder builder) {
            
            if (float.IsInfinity(valueF) || float.IsNaN(valueF)) {
                builder.Append("null");
                return;
            }

            if (this.type == Type.Float)
                builder.Append(valueF.ToString("G9", CultureInfo.CreateSpecificCulture("en-US")));
            else 
                builder.Append(valueI);
        }

        public override int ToInt() {
            return valueI;
        }

        public override float ToFloat() {
            return valueF;
        }
    }
}
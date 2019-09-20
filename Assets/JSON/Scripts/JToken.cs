using System.Text;
using System.Collections.Generic;

namespace Keiwando.JSON {

    public abstract class JToken {

        public virtual JToken this[string key] {
            get { throw new System.InvalidOperationException("This token does not support indexing"); }
            set { throw new System.InvalidOperationException("This token does not support indexing"); }
        }

        public static JToken Parse(string encoded) {
            return JSONParser.ParseFirstToken(encoded).Token;
        }

        public static JToken From<T>(List<T> elements) where T: IJsonConvertible {
            return JArray.From(elements);
        }

        public virtual string ToString(Formatting formatting) {
            var builder = new StringBuilder();
            var format = new FormatContext() {
                Formatting = formatting,
                indentation = 0
            };
            this.ToString(format, builder);
            return builder.ToString();
        }

        public virtual bool IsNull() {
            return false;
        }

        public virtual bool ToBool() {
            throw new System.InvalidCastException("Cannot cast this to a bool!");
        }

        public virtual int ToInt() {
            throw new System.InvalidCastException("Cannot cast this to an int!");
        }

        public virtual float ToFloat() {
            throw new System.InvalidCastException("Cannot cast this to a float!");
        }

        public virtual int[] ToIntArray() {
            throw new System.InvalidCastException("Cannot cast this to an int array!");
        }

        public virtual float[] ToFloatArray() {
            throw new System.InvalidCastException("Cannot cast this to a float array!");
        }

        public virtual string[] ToStringArray() {
            throw new System.InvalidCastException("Cannot cast this to a string array!");
        }

        public virtual JToken[] ToArray() {
            throw new System.InvalidCastException("Cannot cast this to an array!");
        }

        public virtual T[] ToArray<T>(IJsonDecoder<T> decoder) {
            throw new System.InvalidCastException("Cannot cast this to an array!");
        }

        public virtual List<JToken> ToList() {
            throw new System.InvalidCastException("Cannot cast this to a list!");
        }

        public virtual List<T> ToList<T>(IJsonDecoder<T> decoder) {
            throw new System.InvalidCastException("Cannot cast this to a list!");
        }

        public virtual T Decode<T>(IJsonDecoder<T> decoder) {
            throw new System.InvalidCastException("Decode only works on JObject instances!");
        }

        public override string ToString() {
            return this.ToString(Formatting.None);
        }

        internal abstract void ToString(FormatContext format, StringBuilder builder);

        public static implicit operator JToken(string val) {
            return new JString(val);
        }

        public static implicit operator JToken(int val) {
            return new JNumber(val);
        }

        public static implicit operator JToken(float val) {
            return new JNumber(val);
        }

        public static implicit operator JToken(bool val) {
            return new JBoolean(val);
        }

        public static implicit operator JToken(JToken[] val) {
            return new JArray(val);
        }

        public static implicit operator JToken(int[] val) {
            return new JArray(val);
        }

        public static implicit operator JToken(float[] val) {
            return new JArray(val);
        }

        public static implicit operator JToken(string[] val) {
            return new JArray(val);
        } 

        public static implicit operator JToken(List<JToken> val) {
            return new JArray(val);
        }

        public static implicit operator JToken(List<int> val) {
            return new JArray(val);
        }

        public static implicit operator JToken(List<float> val) {
            return new JArray(val);
        }

        public static implicit operator JToken(List<string> val) {
            return new JArray(val);
        } 
    }
}
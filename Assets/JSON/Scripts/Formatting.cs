using System.Text;

namespace Keiwando.JSON {

    public enum Formatting {
        None,
        Pretty
    }

    internal struct FormatContext {
        public Formatting Formatting;
        public int indentation;

        internal void OptionalNewline(StringBuilder builder) {
            if (Formatting == Formatting.Pretty) {
                builder.Append("\n");
                for (int i = 0; i < indentation; i++) {
                    builder.Append(" ");
                }
            }
        }

        internal void OptionalWhitespace(StringBuilder builder) {
            if (Formatting == Formatting.Pretty) {
                builder.Append(" ");
            }
        }

        internal FormatContext Indented() {
            return new FormatContext {
                Formatting = this.Formatting,
                indentation = this.indentation + 2
            };
        }
    }
}
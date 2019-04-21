using System.Text;

public struct MutableString: IMutatable<char> {

    public StringBuilder Builder { get; set; }

    public char this[int index] {
        get { return Builder[index]; }
        set { Builder[index] = value; }
    }

    public int Length { get { return Builder.Length; } }

    public MutableString(StringBuilder builder) {
        this.Builder = builder;
    }

    public MutableString(string str) {
        this.Builder = new StringBuilder(str);
    }

    public char Flipped(char value) {
        if (value == '0') {
            return '1';
        } else {
            return '0';
        }
    }
}
using System;

namespace Keiwando.JSON {

    internal struct Substring {

        private string master;
        public readonly int StartIndex;
        public readonly int Length;

        public int EndIndex {
            get { return StartIndex + Length; }
        }

        public char this[int i] {
            get { return master[i + StartIndex]; }
        }

        public Substring(string master, int start, int length) {
            this.master = master;
            this.StartIndex = Math.Min(Math.Max(start, 0), master.Length - 1);
            this.Length = Math.Min(Math.Max(length, 0), master.Length - StartIndex);
        }

        public Substring SubSubstring(int start) {
            int length = Math.Max(0, Length - start);
            return SubSubstring(start, length);
        }

        public Substring SubSubstring(int start, int length) {
            int safeStart = Math.Min(Math.Max(start, start), EndIndex);
            int safeLength = Math.Min(Math.Max(length, 0), Length - safeStart);
            return new Substring(master, StartIndex + safeStart, safeLength);
        }

        public override string ToString() {
            return master.Substring(StartIndex, Length);
        }

        public static implicit operator Substring(string value) {
            return new Substring(value, 0, value.Length);   
        }
    }
}
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Keiwando.JSON {

    internal class JSONParser {

        internal struct ParseResult<T> where T: JToken {
            public T Token;
            public int SubstringLength;
        }

        public static JToken Parse(string encoded) {
            return JSONParser.ParseFirstToken(encoded).Token;
        }

        public static ParseResult<JToken> ParseFirstToken(Substring encoded) {
            
            JToken token = null;
            int substringLength = 0;
            int i = 0;
            while (i < encoded.Length) {

                char c = encoded[i];
                if (IsWhiteSpace(c)) {
                    i++;
                } else if (c == '{') {
                    // Beginning of JSON
                    var json = ParseFirstObject(encoded.SubSubstring(i));
                    token = json.Token;
                    substringLength = json.SubstringLength;
                    break;
                } else if (c == '"') {
                    var jString = ParseFirstString(encoded.SubSubstring(i));
                    token = jString.Token;
                    substringLength = jString.SubstringLength;
                    break;
                } else if (c == '-' || IsDigit(c)) {
                    var jNumber = ParseFirstNumber(encoded.SubSubstring(i));
                    token = jNumber.Token;
                    substringLength = jNumber.SubstringLength;
                    break;
                } else if (c == '[') {
                    var jArray = ParseFirstArray(encoded.SubSubstring(i));
                    token = jArray.Token;
                    substringLength = jArray.SubstringLength;
                    break;
                } else if (c == 't') {
                    if (encoded[i + 1] == 'r' 
                     && encoded[i + 2] == 'u' 
                     && encoded[i + 3] == 'e') {
                        token = new JBoolean(true);
                        substringLength = 4;
                        break;
                    }
                } else if (c == 'f') {
                    if (encoded[i + 1] == 'a' 
                     && encoded[i + 2] == 'l' 
                     && encoded[i + 3] == 's'
                     && encoded[i + 4] == 'e') {
                        token = new JBoolean(false);
                        substringLength = 5;
                        break;
                    }
                } else if (c == 'n') {
                    if (encoded[i + 1] == 'u' 
                     && encoded[i + 2] == 'l' 
                     && encoded[i + 3] == 'l') {
                        token = new JNull();
                        substringLength = 4;
                        break;
                    }
                } else {
                    throw new System.ArgumentException("Invalid JSON");
                }

                i++;
            }

            return new ParseResult<JToken> {
                Token = token,
                SubstringLength = substringLength
            };
        }

        public static ParseResult<JObject> ParseFirstObject(Substring encoded) {

            int i = LeadingWhiteSpaceCount(encoded);
            if (encoded[i] != '{') {
                throw new System.ArgumentException("Failed to parse JSON Object: " + encoded.ToString());
            }
            i++;
            JObject json = new JObject();
            i += LeadingWhiteSpaceCount(encoded.SubSubstring(i));

            while (encoded[i] != '}') {

                if (i >= encoded.Length) {
                    throw new System.ArgumentException("Unterminated object: " + encoded.ToString());
                }

                i += LeadingWhiteSpaceCount(encoded.SubSubstring(i));

                var keyResult = ParseFirstString(encoded.SubSubstring(i));
                i += keyResult.SubstringLength;
                i += LeadingWhiteSpaceCount(encoded.SubSubstring(i));
                
                if (encoded[i] != ':') {
                    throw new System.ArgumentException("Missing colon when parsing object property: " + encoded.ToString());
                } else {
                    i++;
                }

                i += LeadingWhiteSpaceCount(encoded.SubSubstring(i));
                var valueResult = ParseFirstToken(encoded.SubSubstring(i));
                i += valueResult.SubstringLength;
                i += LeadingWhiteSpaceCount(encoded.SubSubstring(i));

                json[keyResult.Token.ToString()] = valueResult.Token;

                if (encoded[i] != ',') {
                    if (encoded[i] != '}') {
                        throw new System.ArgumentException("Unable to parse JSON object: " + encoded.ToString());
                    }
                } else {
                    i++;
                }
            }

            return new ParseResult<JObject> {
                Token = json,
                SubstringLength = i + 1
            };
        }

        public static ParseResult<JString> ParseFirstString(Substring encoded) {

            int i = LeadingWhiteSpaceCount(encoded);
            if (encoded[i] != '"') {
                throw new System.ArgumentException("No string found!");
            }
            i++;
            int stringStart = i;
            bool inEscape = false;

            while (i < encoded.Length) {
                char c = encoded[i];
                
                if (inEscape) {
                    // Skip the next character
                    inEscape = false;
                } else if (c == '"') {
                    return new ParseResult<JString> {
                        Token = new JString(encoded.SubSubstring(stringStart, i - stringStart)),
                        SubstringLength = i + 1
                    };
                } else if (c == '\\') {
                    inEscape = true;
                }
                
                i++;
            }
            throw new System.ArgumentException("Non-terminated string");
        }

        public static ParseResult<JNumber> ParseFirstNumber(Substring encoded) {
            
            int i = LeadingWhiteSpaceCount(encoded);
            int numberStart = i;

            if (encoded[i] == '-') {
                i++;
            } else if (!IsDigit(encoded[i])) {
                throw new System.ArgumentException("No number found!");
            }
            
            bool inFraction = false;
            bool inExponent = false;
            bool digitRead = false;
            bool fractionDigitRead = false;
            bool exponentDigitRead = false;

            while (i < encoded.Length) {
                char c = encoded[i];
                
                if (IsDigit(c)) {
                    i++;
                    digitRead = true;
                    if (inFraction)
                        fractionDigitRead = true;
                    if (inExponent) 
                        exponentDigitRead = true;
                    continue;
                } 
                
                else if (!inExponent && !inFraction && digitRead && c == '.') {
                    inFraction = true;
                } 
                
                else if (c == 'E' || c == 'e') {
                    inFraction = false;
                    inExponent = true;
                } 

                else if (inExponent && !exponentDigitRead && (c == '+' | c == '-')) {
                    i++;
                    continue;
                }
                
                else {
                    // We read something other than a valid character for a number
                    break;
                }
                
                i++;
            }
            if (!digitRead) {
                throw new System.ArgumentException("No digits found! " + encoded.ToString());
            }
            if (inFraction && !fractionDigitRead) {
                throw new System.ArgumentException("Fraction not terminated " + encoded.ToString());
            }
            if (inExponent && !exponentDigitRead) {
                throw new System.ArgumentException("Exponent not terminated " + encoded.ToString());
            }

            JNumber token = null;
            string numberAsString = encoded.SubSubstring(numberStart, i - numberStart).ToString();
            if (fractionDigitRead || exponentDigitRead) {
                float result;
                var style = System.Globalization.NumberStyles.Float;
                var culture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
                if (!float.TryParse(numberAsString, style, culture, out result)) {
                    throw new System.ArgumentException("Unable to parse number from JSON: " + encoded.ToString());
                }
                token = new JNumber(result);
            } else {
                int result;
                if (!int.TryParse(numberAsString, out result)) {
                    throw new System.ArgumentException("Unable to parse number from JSON: " + encoded.ToString());
                }
                token = new JNumber(result);
            }

            return new ParseResult<JNumber> {
                Token = token,
                SubstringLength = i
            };
        }

        public static ParseResult<JArray> ParseFirstArray(Substring encoded) {

            int i = LeadingWhiteSpaceCount(encoded);
            if (encoded[i] != '[') {
                throw new System.ArgumentException("No array found: " + encoded.ToString());
            }
            i++;
            List<JToken> items = new List<JToken>();
            i += LeadingWhiteSpaceCount(encoded.SubSubstring(i));
        
            while (encoded[i] != ']') {

                if (i >= encoded.Length) {
                    throw new System.ArgumentException("Unterminated array: " + encoded.ToString());
                }

                i += LeadingWhiteSpaceCount(encoded.SubSubstring(i));
                var result = ParseFirstToken(encoded.SubSubstring(i));
                items.Add(result.Token);
                i += result.SubstringLength;
                i += LeadingWhiteSpaceCount(encoded.SubSubstring(i));
                if (encoded[i] != ',') {
                    if (encoded[i] != ']') {
                        throw new System.Exception("Unable to parse array: " + encoded.ToString());
                    } 
                } else {
                    i++;
                }
            }
            
            return new ParseResult<JArray> {
                Token = new JArray(items.ToArray()),
                SubstringLength = i + 1
            };
        }

        private static int LeadingWhiteSpaceCount(Substring s) {
            int count = 0;
            while (IsWhiteSpace(s[count])) {
                count++;
            }
            return count;
        }

        /// <summary>
        /// 0020 = 32   space
        /// 000D = 13   carriage return
        /// 000A = 10   linefeed
        /// 0009 = 9    horizontal tab
        /// </summary>
        private static bool IsWhiteSpace(char c) {
            return c == 9 || c == 10 || c == 13 || c == 32;
        }

        private static bool IsDigit(char c) {
            return c == '0' || IsDigit1to9(c);
        }

        private static bool IsDigit1to9(char c) {
            return c == '1' || c == '2' || c == '3' || c == '4' ||
            c == '5' || c == '6' || c == '7' || c == '8' || c == '9';
        }
    }
}
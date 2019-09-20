namespace Keiwando.JSON {
    
    public delegate T IJsonDecoder<T>(JObject json);
        
    public interface IJsonConvertible {
        JObject Encode();
    }
}


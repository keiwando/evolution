namespace Keiwando {

    public delegate void GestureCallback<T>(T recognizer) where T: IGestureRecognizer<T>;

    public enum GestureRecognizerState {
        Began,
        Changed,
        Ended
    }

    public interface IGestureRecognizer<T> where T: IGestureRecognizer<T> {
        
        event GestureCallback<T> OnGesture;
        GestureRecognizerState State { get; }
    }
}
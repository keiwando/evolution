using System.IO;
using Keiwando.JSON;

namespace Keiwando.Evolution.Scenes {

    public struct CameraControlPoint {

        public readonly float x;
        public readonly float y;

        /// <summary>
        /// The vertical pivot around which the camera zooms relative to
        /// the camera bounds.
        /// 0 = bottom edge, 1 = top edge.
        /// 
        /// When positioning the camera along control points, the pivot
        /// projected into world space should match the interpolated
        /// x and y positions of two successive control points.
        /// </summary>
        public readonly float pivot;

        public CameraControlPoint(float x, float y, float pivot) {
            this.x = x;
            this.y = y;
            this.pivot = pivot;
        }

        #region Encode & Decode

        public void Encode(BinaryWriter writer) {
            long dataLengthOffset = writer.Seek(0, SeekOrigin.Current);
            writer.WriteDummyBlockLength();
            ushort flags = 0;
            writer.Write(flags);
            writer.Write(x);
            writer.Write(y);
            writer.Write(pivot);
            writer.WriteBlockLengthToOffset(dataLengthOffset);
        }

        public static CameraControlPoint Decode(BinaryReader reader) {
            uint dataLength = reader.ReadUInt32();
            long expectedEndByte = reader.BaseStream.Position + (long)dataLength;

            ushort flags = reader.ReadUInt16();
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float pivot = reader.ReadSingle();

            reader.BaseStream.Seek(expectedEndByte, SeekOrigin.Begin);

            return new CameraControlPoint(x, y, pivot);
        }

        public static class CodingKey {
            public const string x = "x";
            public const string y = "y";
            public const string pivot = "pivot";
        }

        public JObject Encode() {

            var json = new JObject();
            json[CodingKey.x] = this.x;
            json[CodingKey.y] = this.y;
            json[CodingKey.pivot] = this.pivot;

            return json;
        }

        public static CameraControlPoint Decode(JObject json) {

            float x = json[CodingKey.x].ToFloat();
            float y = json[CodingKey.y].ToFloat();
            float pivot = json[CodingKey.pivot].ToFloat();

            return new CameraControlPoint(x, y, pivot);
        }

        #endregion
    }
}
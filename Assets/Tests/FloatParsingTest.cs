using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;

public class FloatParsingTest {

    [Test]
    public void TestFloatEncodeDecode() {

        int count = 100000;

        for (int i = 0; i < count; i++) {

            float original = UnityEngine.Random.Range(-10f, 10f);
            string encoded = original.ToString("G9");
            float parsed = float.Parse(encoded);
            Assert.AreEqual(original, parsed);
        }
    }

     private static int FloatToInt32Bits( float f ) {
        return BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
        // return *( (int*)&f );
    }

    private static bool AlmostEqual2sComplement( float a, float b, int maxDeltaBits ) {
        int aInt = FloatToInt32Bits( a );
        if ( aInt < 0 )
            aInt = Int32.MinValue - aInt;

        int bInt = FloatToInt32Bits( b );
        if ( bInt < 0 )
            bInt = Int32.MinValue - bInt;

        int intDiff = Math.Abs( aInt - bInt );
        return intDiff <= ( 1 << maxDeltaBits );
    }
}

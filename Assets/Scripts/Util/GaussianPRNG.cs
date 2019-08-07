using UnityEngine;

namespace Keiwando {

    /// <summary>
    /// Box-Mueller transform for general mean and variance
    /// </summary>
    public class GaussianPRNG {

        private const float EPSILON = float.Epsilon;
        private const float TWO_PI = Mathf.PI * 2f;

        private float mu;
        private float sigma;

        private float z1;
        private bool generate = false;

        public GaussianPRNG(float mu = 0, float sigma = 1) {
            this.mu = mu;
            this.sigma = sigma;
        }

        public float Next() {
            
            generate = !generate;
            if (!generate)
                return z1 * sigma + mu;

            float u1;
            float u2;
            do {
                u1 = Random.Range(0f, 1f);
                u2 = Random.Range(0f, 1f);
            }
            while (u1 <= EPSILON);

            float z0 = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(TWO_PI * u2);
            z1 = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(TWO_PI * u2);
            return z0 * sigma + mu;
        }
    }
}
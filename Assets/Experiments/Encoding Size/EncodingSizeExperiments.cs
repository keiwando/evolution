using System.IO;
using System.Text;
using UnityEngine;

using Keiwando.JSON;

namespace Keiwando.Experiments {

    public class EncodingSizeExperiments: MonoBehaviour {

        void Start() {
            CompareStringVsFloatJSON();
        }

        private void CompareStringVsFloatJSON() {

            float[] weights = new float[10000];
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < weights.Length; i++) {
                float val = UnityEngine.Random.Range(-3f, 3f);
                builder.Append(ConversionUtils.FloatToString(val));
                weights[i] = val;
            }

            JObject floatJSON = new JObject();
            floatJSON["weights"] = new JArray(weights);

            JObject stringJSON = new JObject();
            stringJSON["weights"] = builder.ToString();

            string floatOutputPath = string.Format("/Users/Keiwan/Desktop/{0}_floats.json", weights.Length);
            string stringOutputPath = string.Format("/Users/Keiwan/Desktop/{0}_string.json", weights.Length);

            File.WriteAllText(floatOutputPath, floatJSON.ToString(Formatting.None));
            File.WriteAllText(stringOutputPath, stringJSON.ToString(Formatting.None));
        }
    }
}

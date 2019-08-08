using System;
using System.Collections.Generic;
using Keiwando.JSON;

namespace Keiwando.Evolution {

    [Serializable]
    public struct ChromosomeData: ISelectable<ChromosomeData>, IJsonConvertible {

        public readonly float[] Chromosome;
        public readonly CreatureStats Stats;

        public ChromosomeData(float[] chromosome, CreatureStats stats) {
            this.Chromosome = chromosome;
            this.Stats = stats;
        } 

        #region Comparers

        public class AscendingComparer: IComparer<ChromosomeData> {
            public int Compare(ChromosomeData lhs, ChromosomeData rhs) {
                return lhs.Stats.fitness.CompareTo(rhs.Stats.fitness);
            }
        }

        public class DescendingComparer: IComparer<ChromosomeData> {
            public int Compare(ChromosomeData lhs, ChromosomeData rhs) {
                return rhs.Stats.fitness.CompareTo(lhs.Stats.fitness);
            }
        }

        public float GetFitness() {
            return Stats.fitness;
        }

        public IComparer<ChromosomeData> GetDescendingComparer() {
            return new DescendingComparer();
        }

        public IComparer<ChromosomeData> GetAscendingComparer() {
            return new AscendingComparer();
        }

        #endregion
        #region Encode & Decode

        private static class CodingKey {
            public const string Chromosome = "chromosome";
            public const string CreatureStats = "stats";
        }

        public JObject Encode() {
            JObject json = new JObject();
            json[CodingKey.Chromosome] = this.Chromosome;
            json[CodingKey.CreatureStats] = this.Stats.Encode();
            return json;
        }

        public static ChromosomeData Decode(string encoded) {
            return Decode(JObject.Parse(encoded));
        }

        public static ChromosomeData Decode(JObject json) {

            var chromosome = json[CodingKey.Chromosome].ToFloatArray();
            var statsJSON = json[CodingKey.CreatureStats] as JObject;
            var stats = CreatureStats.Decode(statsJSON);
            return new ChromosomeData(chromosome, stats);
        }

        #endregion
    }
}
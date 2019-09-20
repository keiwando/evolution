using System;
using System.Collections.Generic;
using Keiwando.JSON;

namespace Keiwando.Evolution {

    [Serializable]
    public struct StringChromosomeData: ISelectable<StringChromosomeData>, IJsonConvertible {

        public readonly string Chromosome;
        public readonly CreatureStats Stats;

        public StringChromosomeData(string chromosome, CreatureStats stats) {
            this.Chromosome = chromosome;
            this.Stats = stats;
        } 

        public ChromosomeData ToChromosomeData() {
            return new ChromosomeData(ConversionUtils.BinaryStringToFloatArray(Chromosome), Stats);
        }

        #region Comparers

        public class AscendingComparer: IComparer<StringChromosomeData> {
            public int Compare(StringChromosomeData lhs, StringChromosomeData rhs) {
                return lhs.Stats.fitness.CompareTo(rhs.Stats.fitness);
            }
        }

        public class DescendingComparer: IComparer<StringChromosomeData> {
            public int Compare(StringChromosomeData lhs, StringChromosomeData rhs) {
                return rhs.Stats.fitness.CompareTo(lhs.Stats.fitness);
            }
        }

        public IComparer<StringChromosomeData> GetDescendingComparer() {
            return new DescendingComparer();
        }

        public IComparer<StringChromosomeData> GetAscendingComparer() {
            return new AscendingComparer();
        }

        public float GetFitness() {
            return Stats.fitness;
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

        public static StringChromosomeData Decode(string encoded) {
            return Decode(JObject.Parse(encoded));
        }

        public static StringChromosomeData Decode(JObject json) {

            string chromosome = json[CodingKey.Chromosome].ToString();
            var statsJSON = json[CodingKey.CreatureStats] as JObject;
            var stats = CreatureStats.Decode(statsJSON);
            return new StringChromosomeData(chromosome, stats);
        }

        #endregion
    }
}
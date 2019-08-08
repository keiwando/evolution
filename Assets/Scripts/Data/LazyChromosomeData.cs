using System;
using System.Collections.Generic;

namespace Keiwando.Evolution {

    public struct LazyChromosomeData<T>: ISelectable<LazyChromosomeData<T>> {

        public T Chromosome {
            get {
                if (EqualityComparer<T>.Default.Equals(default(T), cachedChromosome)) {
                    cachedChromosome = this.encodable.ToChromosome();
                }
                return cachedChromosome;
            }
        }
        private T cachedChromosome;

        private readonly IChromosomeEncodable<T> encodable;
        public readonly CreatureStats Stats;

        public LazyChromosomeData(IChromosomeEncodable<T> encodable, CreatureStats stats) {
            this.encodable = encodable;
            this.Stats = stats;
            this.cachedChromosome = default(T);
        } 

        #region Comparers

        public class AscendingComparer: IComparer<LazyChromosomeData<T>> {
            public int Compare(LazyChromosomeData<T> lhs, LazyChromosomeData<T> rhs) {
                return lhs.Stats.fitness.CompareTo(rhs.Stats.fitness);
            }
        }

        public class DescendingComparer: IComparer<LazyChromosomeData<T>> {
            public int Compare(LazyChromosomeData<T> lhs, LazyChromosomeData<T> rhs) {
                return rhs.Stats.fitness.CompareTo(lhs.Stats.fitness);
            }
        }

        public IComparer<LazyChromosomeData<T>> GetDescendingComparer() {
            return new DescendingComparer();
        }

        public IComparer<LazyChromosomeData<T>> GetAscendingComparer() {
            return new AscendingComparer();
        }

        public float GetFitness() {
            return Stats.fitness;
        }

        #endregion
    }
}
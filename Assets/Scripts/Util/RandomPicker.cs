using System.Collections.Generic;

namespace Keiwando {

    public struct WeightedElement<T> {
        public readonly T Element;
        public readonly float Weight;
        public WeightedElement(T element, float Weight) {
            this.Element = element;
            this.Weight = Weight;

        }
    }

    public class RandomPicker<T> {

        private struct CumulativeWeight {
            public T element;
            public float cumulativeWeight;
        }

        private List<CumulativeWeight> cdf;
        private float totalWeight;
        
        public RandomPicker(WeightedElement<T>[] elements = null) {

            this.cdf = new List<CumulativeWeight>();
            float cumulativeWeight = 0;
            if (elements != null) {
                for (int i = 0; i < elements.Length; i++) {
                    var element = elements[i];
                    cumulativeWeight += element.Weight;
                    cdf.Add(new CumulativeWeight() { 
                        element = element.Element,
                        cumulativeWeight = cumulativeWeight
                    });
                }
            }
            totalWeight = cumulativeWeight;
        }

        public T Next() {
            float rnd = UnityEngine.Random.Range(0, totalWeight);
            int count = cdf.Count;
            for (int i = 0; i < count; i++) {
                if (cdf[i].cumulativeWeight > rnd) {
                    return cdf[i].element;
                }
            }
            return count > 0 ? cdf[count - 1].element : default(T);
        }

        public void Add(T element, float weight) {
            totalWeight += weight;
            cdf.Add(new CumulativeWeight() {
                element = element,
                cumulativeWeight = totalWeight
            });
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class Selection {

    public enum Mode {
        FitnessProportional
    }

    private Mode mode;
    private List<ChromosomeData> chromosomes;

    private int[] randomPickingWeights;

    public Selection(Mode mode, List<ChromosomeData> chromosomes) {
        this.mode = mode;
        this.chromosomes = chromosomes;
    }

    private void SetupPickingWeights(Mode mode, int count) {

    }
}
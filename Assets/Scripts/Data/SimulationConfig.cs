

public struct SimulationConfig {

    public CreatureDesign CreatureDesign;
    public SimulationSettings SimulationSettings;
    public NeuralNetworkSettings NeuralNetworkSettings;

    public SimulationConfig(CreatureDesign design, SimulationSettings simulationSettings, NeuralNetworkSettings networkSettings) {
        this.CreatureDesign = design;
        this.SimulationSettings = simulationSettings;
        this.NeuralNetworkSettings = networkSettings;
    }
}
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;
// using Keiwando.JSON;

// namespace Keiwando.Evolution.Test {

//     public class SerializationTests {

//         private struct TestSimulation {
//             internal string name;
//             internal float permittedFitnessDifference;
//         }

//         private readonly TestSimulation[] testSimulations = {
//             new TestSimulation {
//                 name = "FROGGER - Running - Sep 03, 2019 - Gen 30",
//                 permittedFitnessDifference = 0.0007f
//             },
//             new TestSimulation {
//                 name = "FROGGER - Running - Sep 01, 2020 - Gen 31",
//                 permittedFitnessDifference = 0.0005f
//             },
//             new TestSimulation {
//                 name = "Test Jumper - Jumping - Sep 01, 2020 - Gen 36",
//                 permittedFitnessDifference = 0.000001f
//             },
//             new TestSimulation {
//                 name = "Climber - Climbing - Sep 01, 2020 - Gen 21",
//                 permittedFitnessDifference = 0.0025f
//             },
//             new TestSimulation {
//                 name = "SPRING - Obstacle Jump - Sep 01, 2020 - Gen 76",
//                 permittedFitnessDifference = 0.05f
//             }
//         };

//         [UnityTest]
//         [Timeout(10000000)]
//         public IEnumerator CreatureBehaviourIsDeterministic() {

//             SetupSettings();

//             foreach (var testSimulation in testSimulations) {
//                 var testSerialization = Resources.Load("Simulations/" + testSimulation.name) as TextAsset;
//                 var simulationData = SimulationData.Decode(testSerialization.text);

//                 // Simulate the best creature of the last generation multiple times at once.
//                 int populationSize = simulationData.Settings.PopulationSize;
//                 var chromosomeData = simulationData.BestCreatures[simulationData.BestCreatures.Count - 1];
//                 for (int i = 0; i < populationSize; i++) {
//                     simulationData.CurrentChromosomes[i] = chromosomeData.Chromosome;
//                 }

//                 SceneController.LoadSync(SceneController.Scene.Editor);

//                 yield return new WaitForEndOfFrame();
//                 yield return new WaitForEndOfFrame();

//                 var editorObj = GameObject.FindGameObjectWithTag("Editor");
//                 var editor = editorObj.GetComponent<CreatureEditor>();

//                 Evolution.Solution[] evaluatedSolutions = null;

//                 var simulationOptions = new SimulationOptions {
//                     onEvaluatedSolutions = delegate(Evolution.Solution[] solutions) {
//                         evaluatedSolutions = solutions;
//                     }
//                 };

//                 editor.StartSimulation(simulationData, simulationOptions);

//                 yield return new WaitForEndOfFrame();
//                 yield return new WaitForEndOfFrame();

//                 var evolution = MonoBehaviour.FindObjectOfType<Evolution>();
                
//                 while (evaluatedSolutions == null) {
//                     yield return new WaitForEndOfFrame();
//                 }

//                 float minFitness = float.MaxValue;
//                 float maxFitness = -float.MaxValue;
                
//                 // Compare the evaluated solutions to the reference fitness scores
//                 foreach (var solution in evaluatedSolutions) {
//                     minFitness = Math.Min(minFitness, solution.Stats.unclampedFitness);
//                     maxFitness = Math.Max(maxFitness, solution.Stats.unclampedFitness);
//                 }

//                 Assert.AreEqual(minFitness, maxFitness);
//             }
//         }
    
//         [UnityTest]
//         [Timeout(10000000)]
//         public IEnumerator CreatureBehaviourIsUnchanged() {

//             SetupSettings();

//             foreach (var testSimulation in testSimulations) {
//                 var testSerialization = Resources.Load("Simulations/" + testSimulation.name) as TextAsset;
//                 var simulationData = SimulationData.Decode(testSerialization.text);

//                 // Simulate some of the best creatures again and compare the fitness score to the 
//                 // reference from the simulation data
//                 int populationSize = simulationData.Settings.PopulationSize;
//                 Dictionary<string, ChromosomeData> referenceData = new Dictionary<string, ChromosomeData>(populationSize);
//                 for (int i = 0; i < populationSize; i++) {
//                     var chromosomeData = simulationData.BestCreatures[simulationData.BestCreatures.Count - 1 - i];
//                     var chromosomeString = new JArray(chromosomeData.Chromosome).ToString();

//                     if (referenceData.ContainsKey(chromosomeString)) {
//                         Assert.AreEqual(referenceData[chromosomeString].Stats.unclampedFitness, chromosomeData.Stats.unclampedFitness, testSimulation.permittedFitnessDifference);
//                     }
                    
//                     referenceData[chromosomeString] = chromosomeData;
//                     simulationData.CurrentChromosomes[i] = chromosomeData.Chromosome;
//                 }

//                 SceneController.LoadSync(SceneController.Scene.Editor);

//                 yield return new WaitForEndOfFrame();
//                 yield return new WaitForEndOfFrame();

//                 var editorObj = GameObject.FindGameObjectWithTag("Editor");
//                 var editor = editorObj.GetComponent<CreatureEditor>();

//                 Evolution.Solution[] evaluatedSolutions = null;

//                 var simulationOptions = new SimulationOptions {
//                     onEvaluatedSolutions = delegate(Evolution.Solution[] solutions) {
//                         evaluatedSolutions = solutions;
//                     }
//                 };

//                 editor.StartSimulation(simulationData, simulationOptions);

//                 yield return new WaitForEndOfFrame();
//                 yield return new WaitForEndOfFrame();

//                 var evolution = MonoBehaviour.FindObjectOfType<Evolution>();
                
//                 while (evaluatedSolutions == null) {
//                     yield return new WaitForEndOfFrame();
//                 }
                
//                 // Compare the evaluated solutions to the reference fitness scores
//                 foreach (var solution in evaluatedSolutions) {
//                     float[] chromosome = solution.Encodable.ToChromosome();
//                     var chromosomeString = new JArray(chromosome).ToString();
//                     var referenceChromosome = referenceData[chromosomeString];
//                     Assert.AreEqual(solution.Stats.unclampedFitness, referenceChromosome.Stats.unclampedFitness, testSimulation.permittedFitnessDifference);
//                 }
//             }
//         }

//         private void SetupSettings() {
//             Settings.Store = new Settings.DictionaryStore();
//             Settings.AutoSaveEnabled = false;
//             Settings.DidMigrateCreatureSaves = true;
//             Settings.DidMigrateSimulationSaves = true;
//         }
//     }
// }

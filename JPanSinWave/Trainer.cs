﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPanBackprop
{
    public class Trainer
    {
        Network network;
        double momentum = 0.2;
        public Trainer(Network net)
        {
            network = net;
        }

        public void GradientDescent(double[][] inputs, double[][] desiredOutput)
        {
            ClearUpdates();

            for (int i = 0; i < inputs.Length; i++)
            {
                network.Compute(inputs[i]);
                CalculateError(desiredOutput[i]);
                CalculateUpdates(inputs[i], 0.2);
            }

            ApplyUpdates();
        }

        public void ClearUpdates()
        {
            foreach (Layer layer in network.Layers)
            {
                foreach (Neuron neuron in layer.Neurons)
                {
                    for (int i = 0; i < neuron.Weights.Length; i++)
                    {
                        neuron.WeightUpdates[i] = 0;
                        neuron.PrevWeightUpdates[i] = 0;
                        neuron.BiasUpdate = 0;
                        neuron.PrevBiasUpdate = 0;
                    }
                }
            }
        }

        public void CalculateError(double[] desiredOutput)
        {
            Layer outputLayer = network.Layers[network.Layers.Length - 1];
            for (int i = 0; i < outputLayer.Neurons.Length; i++)
            {
                Neuron neuron = outputLayer.Neurons[i];
                double error = (desiredOutput[i] - neuron.Output);
                neuron.PartialDerivative = error * neuron.ActDerivative(neuron.Input);
            }

            for (int i = network.Layers.Length - 2; i >= 0; i--)
            {
                Layer currLayer = network.Layers[i];
                Layer nextLayer = network.Layers[i + 1];

                for (int j = 0; j < currLayer.Neurons.Length; j++)
                {
                    Neuron neuron = currLayer.Neurons[j];
                    double error = 0.0;

                    foreach (Neuron nextNeuron in nextLayer.Neurons)
                    {
                        error += nextNeuron.PartialDerivative * nextNeuron.Weights[j];
                    }

                    neuron.PartialDerivative = error * neuron.ActDerivative(neuron.Input);
                }
            }
        }

        public void CalculateUpdates(double[] input, double learningRate)
        {
            Layer inputLayer = network.Layers[0];
            for (int i = 0; i < inputLayer.Neurons.Length; i++)
            {
                Neuron neuron = inputLayer.Neurons[i];
                for (int j = 0; j < neuron.Weights.Length; j++)
                {
                    neuron.WeightUpdates[j] += learningRate * neuron.PartialDerivative * input[j];
                }
                neuron.BiasUpdate += learningRate * neuron.PartialDerivative;
            }

            for (int i = 1; i < network.Layers.Length; i++)
            {
                Layer currLayer = network.Layers[i];
                Layer prevLayer = network.Layers[i - 1];

                foreach (Neuron neuron in currLayer.Neurons)
                {
                    for (int j = 0; j < prevLayer.Neurons.Length; j++)
                    {
                        neuron.WeightUpdates[j] += learningRate * neuron.PartialDerivative * prevLayer.Neurons[j].Output;
                    }
                    neuron.BiasUpdate += learningRate * neuron.PartialDerivative;
                }
            }
        }

        public void ApplyUpdates()
        {
            foreach (Layer layer in network.Layers)
            {
                foreach (Neuron neuron in layer.Neurons)
                {
                    for (int i = 0; i < neuron.Weights.Length; i++)
                    {
                        double weightChange = neuron.WeightUpdates[i] + (neuron.PrevWeightUpdates[i] * momentum);
                        neuron.Weights[i] += weightChange;
                        neuron.PrevWeightUpdates[i] = neuron.WeightUpdates[i];
                    }

                    double biasChange = neuron.BiasUpdate + (neuron.PrevBiasUpdate * momentum);
                    neuron.Bias += biasChange;
                    neuron.PrevBiasUpdate = biasChange;
                }
            }
        }

        public void SGD(ReadOnlySpan<double[]> inputs, ReadOnlySpan<double[]> desiredOutputs, int batchSize)
        {
            for (int i = 0; i < inputs.Length; i += batchSize)
            {
                GradientDescent(inputs.Slice(i, batchSize).ToArray(), desiredOutputs.Slice(i, batchSize).ToArray());
            }
        }
    }
}



using Fluid_Simulator.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Tests
{
    public class SphTests
    {
        private const int ParticleSize = 10;
        private const float FluidDensity = 1.2f;
        private const float FluidStiffness = 4f;

        private readonly List<Particle> _particles = new();
        private readonly SpatialHashing _spatialHashing = new(2 * ParticleSize);

        public SphTests()
        {
            // Generate 10x10 Particles in a Uniform Grid
            var start = Vector2.Zero;
            var end = new Vector2(ParticleSize * 10);
            for (float x = start.X; x < end.X; x += ParticleSize)
            {
                for (float y = start.Y; y < end.Y; y += ParticleSize)
                {
                    var particle = new Particle(new Vector2(x, y), ParticleSize, FluidDensity, false);
                    _particles.Add(particle);
                    _spatialHashing.InsertObject(particle);
                }
            }
        }

        public void KernelIdealSamplingTest()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf � 75
            var neighbors = new List<Particle>();
            foreach (var particle in _particles)
            {
                neighbors.Clear();
                _spatialHashing.InRadius(particle.Position, 2 * ParticleSize, ref neighbors);
                if (neighbors.Count < 13) continue;
                var kernelSum = 0f;
                foreach (var neighbor in neighbors)
                    kernelSum += SphFluidSolver.Kernel(particle.Position, neighbor.Position, ParticleSize);
                Assert.AreEqual(kernelSum, 1 / MathF.Pow(ParticleSize, 2), 0.001);
            }
        }

        public void KernelComputationTest()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf � 75

            var kernelSum = SphFluidSolver.Kernel(Vector2.Zero, Vector2.Zero, ParticleSize);
            Assert.AreEqual(kernelSum, 4 * SphFluidSolver.KernelAlpha(ParticleSize), 0.001);

            kernelSum = SphFluidSolver.Kernel(Vector2.Zero, new Vector2(ParticleSize, 0), ParticleSize);
            Assert.AreEqual(kernelSum, SphFluidSolver.KernelAlpha(ParticleSize), 0.001);

            kernelSum = SphFluidSolver.Kernel(Vector2.Zero, new Vector2(ParticleSize * 2, 0), ParticleSize);
            Assert.AreEqual(kernelSum, 0);
        }

        public void KernelDerivativeIdealSamplingTest()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf � 73
            var neighbors = new List<Particle>();
            foreach (var particle in _particles)
            {
                neighbors.Clear();
                _spatialHashing.InRadius(particle.Position, 2 * ParticleSize, ref neighbors);
                if (neighbors.Count < 13) continue;

                Vector2 kernelDerivative = Vector2.Zero;
                foreach (var neighbor in neighbors)
                    kernelDerivative += SphFluidSolver.KernelDerivative(particle.Position, neighbor.Position, ParticleSize);
                Assert.AreEqual(kernelDerivative.X, 0, 0.01);
                Assert.AreEqual(kernelDerivative.Y, 0, 0.01);
            }
        }

        public void KernelDerivativeComputationTest()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf � 79
            var kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, Vector2.Zero, ParticleSize);
            Assert.AreEqual(0, kernelDerivative.Length());
 
            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(ParticleSize, 0), ParticleSize);
            Assert.AreEqual(3 * SphFluidSolver.KernelAlpha(ParticleSize) / ParticleSize, kernelDerivative.X, 0.001);
            Assert.AreEqual(kernelDerivative.Y, 0);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(-ParticleSize, 0), ParticleSize);
            Assert.AreEqual(-(3 * SphFluidSolver.KernelAlpha(ParticleSize) / ParticleSize), kernelDerivative.X, 0.001);
            Assert.AreEqual(kernelDerivative.Y, 0);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(0, ParticleSize), ParticleSize);
            Assert.AreEqual(kernelDerivative.X, 0);
            Assert.AreEqual(3 * SphFluidSolver.KernelAlpha(ParticleSize) / ParticleSize, kernelDerivative.Y, 0.001);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(ParticleSize), ParticleSize);
            Assert.AreEqual(kernelDerivative.X, kernelDerivative.Y);
            Assert.AreEqual(kernelDerivative.Y, -5.147 / (14 * MathF.PI * MathF.Pow(ParticleSize, 3) * MathF.Sqrt(2)), 0.001f);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(-ParticleSize, ParticleSize), ParticleSize);
            Assert.AreEqual(-kernelDerivative.X, kernelDerivative.Y);
            Assert.AreEqual(kernelDerivative.Y, 5.147 / (14 * MathF.PI * MathF.Pow(ParticleSize, 3) * MathF.Sqrt(2)), 0.001f);
        }

        public void LocalDensityIdealSamplingTest()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf � 76

            var neighbors = new List<Particle>();
            foreach (var particle in _particles)
            {
                neighbors.Clear();
                _spatialHashing.InRadius(particle.Position, 2 * ParticleSize, ref neighbors);
                if (neighbors.Count < 13) continue;

                var localDensity = SphFluidSolver.ComputeLocalDensity(ParticleSize, particle, neighbors);
                Assert.AreEqual(localDensity, FluidDensity, 0.001);
            }
        }

        public void LocalStiffnesIdealSamplingTest()
        {
            var neighbors = new List<Particle>();
            foreach (var particle in _particles)
            {
                neighbors.Clear();
                _spatialHashing.InRadius(particle.Position, 2 * ParticleSize, ref neighbors);
                if (neighbors.Count < 13) continue;

                var localDensity = SphFluidSolver.ComputeLocalDensity(ParticleSize, particle, neighbors);
                Assert.AreEqual(localDensity, FluidDensity, 0.001);
                var localPressure = SphFluidSolver.ComputeLocalPressure(FluidStiffness, FluidDensity, localDensity);
                Assert.AreEqual(localPressure, 0, 0.001);
            }
        }
    }
}

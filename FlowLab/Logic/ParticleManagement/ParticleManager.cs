﻿// ParticleManager.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine;
using FlowLab.Logic.SphComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowLab.Logic.ParticleManagement
{
    internal class ParticleManager
    {
        public double SimulationTime { get; private set; }
        private readonly List<Particle> _particles;
        private readonly List<Particle> _fluidParticles;
        private readonly List<Particle> _boundaryParticles;
        private readonly SpatialHashing _spatialHashing;
        public readonly float ParticleDiameter;
        public readonly float FluidDensity;
        private Effect _effect;

        public ParticleManager(int particleDiameter, float fluidDensity)
        {
            _particles = new();
            _fluidParticles = new();
            _boundaryParticles = new();
            _spatialHashing = new(particleDiameter * 2);
            ParticleDiameter = particleDiameter;
            FluidDensity = fluidDensity;
        }

        public void LoadContent(ContentManager content)
            => _effect = content.Load<Effect>("FilledCircle");

        public void AddPolygon(Polygon polygon)
        {
            var width = polygon.Right * ParticleDiameter;
            var height = polygon.Bottom * ParticleDiameter;
            var position = new Vector2(-width / 2, -height / 2);


            var vertex = polygon.Vertices.First();
            var offsetCircle = new CircleF(Vector2.Zero, ParticleDiameter);
            for (int i = 1; i <= polygon.Vertices.Length; i++)
            {
                var nextVertex = i == polygon.Vertices.Length ? polygon.Vertices.First() : polygon.Vertices[i];
                var stepDirection = Vector2.Subtract(nextVertex, vertex).NormalizedCopy();
                var particlePosition = vertex * ParticleDiameter;

                for (int _ = 0; _ < Vector2.Distance(nextVertex, vertex); _++)
                {
                    offsetCircle.Position = particlePosition;
                    AddNewParticle(particlePosition + position, true);
                    particlePosition += stepDirection * ParticleDiameter;
                }

                vertex = nextVertex;
            }
        }

        public void Clear()
        {
            foreach (var particle in _particles.Where(particle => !particle.IsBoundary).ToList())
                RemoveParticle(particle);
        }

        public void ClearAll()
        {
            _particles.Clear();
            _fluidParticles.Clear();
            _boundaryParticles.Clear();
            _spatialHashing.Clear();
        }

        public void RemoveParticle(Particle particle)
        {
            _particles.Remove(particle);
            if (particle.IsBoundary)
                _boundaryParticles.Remove(particle);
            else
                _fluidParticles.Remove(particle);
            _spatialHashing.RemoveObject(particle);
        }

        public void AddNewParticle(Vector2 position, bool isBoundary = false)
        {
            var particle = new Particle(position, ParticleDiameter, FluidDensity, isBoundary);
            _particles.Add(particle);
            if (isBoundary)
                _boundaryParticles.Add(particle);
            else
                _fluidParticles.Add(particle);
            _spatialHashing.InsertObject(particle);
        }

        public int Count => _particles.Where(p => !p.IsBoundary).Count();

        public float RelativeDensityError => _fluidParticles.Count <= 0 ? 0 : float.Abs(_fluidParticles.Average(p => p.DensityError));

        public float CflCondition => _fluidParticles.Count == 0 ? 0 : _fluidParticles.Max(p => p.Cfl);

        public void Update(float fluidStiffness, float fluidViscosity, float gravitation, float timeSteps, bool collectData)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // SPHSolver.IISPH(_particles, _spatialHashing, ParticleDiameter, FluidDensity, FluidDensity, gravitation, timeSteps);
            SPHSolver.SESPH(_particles, _spatialHashing, ParticleDiameter, FluidDensity, fluidStiffness, fluidViscosity, gravitation, timeSteps);
            watch.Stop();
            SimulationTime = watch.Elapsed.TotalMilliseconds;
        }
         
        public void DrawParticles(SpriteBatch spriteBatch, Matrix transformationMatrix, Texture2D particleTexture, Color boundaryColor)
        {
            spriteBatch.Begin(transformMatrix: transformationMatrix, effect: _effect, blendState: BlendState.AlphaBlend);
            foreach (var particle in _particles)
            {
                var position = particle.Position;
                Color color = !particle.IsBoundary ? Color.Blue : boundaryColor;

                spriteBatch.Draw(particleTexture, position, null, color, 0, new Vector2(particleTexture.Width * .5f), ParticleDiameter / particleTexture.Width, SpriteEffects.None, 0);
            } 
            spriteBatch.End();
        }
    }
}

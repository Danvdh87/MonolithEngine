﻿using System;
using System.Collections.Generic;
using System.Text;
using MonolithEngine.Entities;
using MonolithEngine.Entities.Interfaces;
using MonolithEngine.Global;
using MonolithEngine.Source.Camera2D;
using MonolithEngine.Source.GridCollision;
using Microsoft.Xna.Framework;

namespace MonolithEngine.Entities
{
    public class LayerManager
    {
        private List<Layer> parallaxLayers = new List<Layer>();

        List<List<Layer>> allLayers = new List<List<Layer>>();

        public Layer EntityLayer;

        private List<Layer> foregroundLayers = new List<Layer>();

        private List<Layer> backgroundLayers = new List<Layer>();

        public Camera Camera;

        private static readonly LayerManager instance = new LayerManager();

        private LayerManager()
        {

        }
        static LayerManager()
        {
        }

        public void InitLayers()
        {
            if (EntityLayer != null)
            {
                throw new Exception("Root already initialized!");
            }
            EntityLayer = new Layer(Camera, 10);

            allLayers.Add(parallaxLayers);
            allLayers.Add(backgroundLayers);
            allLayers.Add(
                new List<Layer>()
                    {
                        EntityLayer,
                    }
                );
            allLayers.Add(foregroundLayers);
        }

        public static LayerManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void Destroy()
        {
            foreach (List<Layer> layers in allLayers)
            {
                foreach (Layer l in layers)
                {
                    l.Destroy();
                }
            }
        }

        public void DrawAll(GameTime gameTime)
        {
            foreach (List<Layer> layers in allLayers)
            {
                foreach (Layer l in layers)
                {
                    l.DrawAll(gameTime);
                }
            }
        }

        public void UpdateAll()
        {
            foreach (List<Layer> layers in allLayers)
            {
                foreach (Layer l in layers)
                {
                    l.UpdateAll();
                }
            }
        }

        public void FixedUpdateAll()
        {
            foreach (List<Layer> layers in allLayers)
            {
                foreach (Layer l in layers)
                {
                    l.FixedUpdateAll();
                }
            }
        }

        public Layer CreateForegroundLayer(int priority = 0)
        {
            Layer l = new Layer(Camera, priority, false);
            AddLayer(foregroundLayers, l);
            return l;
        }

        public Layer CreateBackgroundLayer(int priority = 0)
        {
            Layer l = new Layer(Camera, priority, false);
            AddLayer(backgroundLayers, l);
            return l;
        }

        public Layer CreateParallaxLayer(int priority = 0, float scrollSpeedMultiplier = 1, bool lockY = false)
        {
            Layer l = new Layer(Camera, priority, false, scrollSpeedMultiplier, lockY);
            AddLayer(parallaxLayers, l);
            return l;
        }

        private void AddLayer(List<Layer> layer, Layer newLayer)
        {
            layer.Add(newLayer);
            layer.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }
    }
}

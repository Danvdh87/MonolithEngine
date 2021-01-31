﻿using GameEngine2D.Engine.Source.Entities;
using GameEngine2D.Engine.Source.Graphics;
using GameEngine2D.Engine.Source.Level;
using GameEngine2D.Engine.Source.Util;
using GameEngine2D.Entities;
using GameEngine2D.Global;
using GameEngine2D.Source.Camera2D;
using GameEngine2D.Source.Layer;
using GameEngine2D.Source.Level;
using GameEngine2D.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using ForestPlatformerExample.Source.Hero;
using ForestPlatformerExample.Source.Environment;
using ForestPlatformerExample.Source.Items;
using ForestPlatformerExample.Source.Enemies;
using GameEngine2D.Engine.Source.Physics.Collision;
using System.Linq;
using GameEngine2D.Source.Util;

namespace TestExample
{
    public class Test : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Camera Camera;

        private SpriteFont font;
        private FrameCounter frameCounter;

        private HeroTest hero;

        public Test()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Config.GRAVITY_ON = false;
            Config.GRAVITY_FORCE = 12f;
            Config.ZOOM = 2f;
            Config.CHARACTER_SPEED = 2f;
            Config.JUMP_FORCE = 7f;
            Config.INCREASING_GRAVITY = true;


            //Config.RES_W = 3840;
            //Config.RES_W = 2160;
            //Config.FULLSCREEN = true;

            //Config.GRID = 64;

            Config.FPS = 0;
            if (Config.FPS == 0)
            {
                // uncapped framerate
                graphics.SynchronizeWithVerticalRetrace = false;
                IsFixedTimeStep = false;
            }
            else
            {
                //Config.FPS = 2000;
                IsFixedTimeStep = true;//false;
                graphics.SynchronizeWithVerticalRetrace = false;
                TargetElapsedTime = TimeSpan.FromSeconds(1d / Config.FPS); //60);
            }
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            SpriteUtil.Content = Content;
            SpriteUtil.GraphicsDeviceManager = graphics;
            Layer.GraphicsDeviceManager = graphics;
            TileGroup.GraphicsDevice = graphics.GraphicsDevice;
            //font = Content.Load<SpriteFont>("DefaultFont");
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            graphics.PreferredBackBufferWidth = Config.RES_W;
            graphics.PreferredBackBufferHeight = Config.RES_H;
            graphics.IsFullScreen = Config.FULLSCREEN;
            graphics.ApplyChanges();
            Camera = new Camera(graphics);
            LayerManager.Instance.Camera = Camera;
            LayerManager.Instance.InitLayers();

            font = Content.Load<SpriteFont>("DefaultFont");

            LoadLevel();

            hero = new HeroTest(font);
            hero.SetSprite(SpriteUtil.CreateRectangle(16, Color.Blue));

            EntityTest e = new EntityTest();
            Camera.TrackTarget(hero, true);
            //TODO: use this.Content to load your game content here

            frameCounter = new FrameCounter();

            Logger.Log("Object count: " + GameObject.GetObjectCount());
        }

        class HeroTest : Hero
        {

            private Texture2D red = SpriteUtil.CreateRectangle(16, Color.Red);
            private Texture2D blue = SpriteUtil.CreateRectangle(16, Color.Blue);
            public HeroTest(SpriteFont font) : base(new Vector2(18 * Config.GRID, 31 * Config.GRID), font)
            {
                DEBUG_SHOW_PIVOT = true;
                DEBUG_SHOW_CIRCLE_COLLIDER = true;
                GridCollisionCheckDirections = new HashSet<Direction>(Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList());
                CircleCollider = new CircleCollider(this, 30);
                //DrawOffset = new Vector2(-8, -8);
            }

            float angle;
            bool colliding = false;
            protected override void OnCircleCollisionStart(Entity otherCollider, float intersection)
            {
                colliding = true;
                SetSprite(red);
                angle = (float)(MathUtil.RadFromVectors(Position, otherCollider.Position) * 180 / Math.PI);
                if (angle <= 135 && angle >= 45)
                {
                    //Logger.Log("Angle: " + angle);
                }
            }

            protected override void OnCircleCollisionEnd(Entity otherCollider)
            {
                SetSprite(blue);
                colliding = false;
            }

            Texture2D collPivot = SpriteUtil.CreateCircle(5, Color.Black);
            public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
            {
                base.Draw(spriteBatch, gameTime);
                spriteBatch.Draw(collPivot, CircleCollider.Position, Color.White);
                if (colliding)
                    spriteBatch.DrawString(font, "Angle: " + angle, Position, Color.Black);

            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
            }
        }

        class EntityTest: Entity
        {
            public EntityTest() : base(LayerManager.Instance.EntityLayer, null, new Vector2(22 * Config.GRID, 33 * Config.GRID), SpriteUtil.CreateRectangle(16, Color.Green))
            {
                CircleCollider = new CircleCollider(this, 16);
                ColliderOnGrid = true;
                DEBUG_SHOW_PIVOT = true;
                DEBUG_SHOW_CIRCLE_COLLIDER = true;
                //DrawOffset = new Vector2(-8, -8);
            }

            Texture2D collPivot = SpriteUtil.CreateCircle(5, Color.Black);
            public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
            {
                base.Draw(spriteBatch, gameTime);
                spriteBatch.Draw(collPivot, CircleCollider.Position, Color.White);
            }
        }

        private void LoadLevel()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            //gameTime = new GameTime(gameTime.TotalGameTime / 5, gameTime.ElapsedGameTime / 5);
            // TODO: Add your update logic here
            Timer.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);
            LayerManager.Instance.UpdateAll(gameTime);
            Camera.update(gameTime);
            Camera.postUpdate(gameTime);

            base.Update(gameTime);
        }

        private float lastPrint = 0;
        string fps = "";
        protected override void Draw(GameTime gameTime)
        {
            //gameTime = new GameTime(gameTime.TotalGameTime / 5, gameTime.ElapsedGameTime / 5);
            GraphicsDevice.Clear(Color.White);

            lastPrint += gameTime.ElapsedGameTime.Milliseconds;
            LayerManager.Instance.DrawAll(gameTime);

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            frameCounter.Update(deltaTime);
            
            if (lastPrint > 10)
            {
                fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);
                lastPrint = 0;
            }

            spriteBatch.Begin();
            spriteBatch.DrawString(font, fps, new Vector2(1, 1), Color.Red);
            spriteBatch.End();


            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
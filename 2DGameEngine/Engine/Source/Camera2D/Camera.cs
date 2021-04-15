﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonolithEngine.Engine.Source.Global;
using MonolithEngine.Entities;
using MonolithEngine.Global;
using MonolithEngine.Source.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonolithEngine.Engine.Source.Camera2D
{
    public class Camera
    {
        private const float MinZoom = 0.01f;

        public Viewport _viewport;
        private Vector2 _origin;

        private Vector2 _position;
        private float _zoom = 1f;
        private Rectangle? _limits;

        public static Entity target;
        private Vector2 targetPosition = Vector2.Zero;
        private Vector2 targetTracingOffset = Vector2.Zero;
        private float targetCameraDistance;
        private float angle;
        private float friction = 0.89f;

        private float shakePower = 1.5f;
        private float shakeStarted = 0f;
        private float shakeDuration = 0f;
        private bool easedStop;

        private bool SCROLL = true;

        private bool shake = false;

        private float elapsedTime;

        private Vector2 direction;

        private Matrix uiTransofrmMatrix;

        private float scrollSpeedModifier;

        private GraphicsDeviceManager graphicsDeviceManager;

        private bool above = false;

        public Camera(GraphicsDeviceManager graphicsDeviceManager, bool above)
        {
            this.graphicsDeviceManager = graphicsDeviceManager;
            Position = Vector2.Zero;
            direction = Vector2.Zero;
            Zoom = Config.SCALE;
            this.above = above;
            ResolutionUpdated();
        }

        public void ResolutionUpdated()
        {

            _viewport = graphicsDeviceManager.GraphicsDevice.Viewport;
            ///_viewport.Height /= 2;
            _viewport.Width /= 2;
            if (!above)
            {
                //_viewport.Y += _viewport.Height;
                _viewport.X += _viewport.Width;
            }
            //graphicsDeviceManager.GraphicsDevice.Viewport = _viewport;
            _origin = new Vector2(_viewport.Width / 2.0f, _viewport.Height / 2.0f);
            Zoom = Config.SCALE;
            if (target != null)
            {
                TrackTarget(target, true, targetTracingOffset);
            }
            uiTransofrmMatrix = Matrix.Identity * Matrix.CreateScale(Config.SCALE, Config.SCALE, 1);
        }

        public void Shake(float power = 5, float duration = 300, bool easeOut = true)
        {
            shakePower = power;
            shakeDuration = duration;
            shake = true;
            easedStop = easeOut;
        }

        public void Update()
        {
            if (!SCROLL)
            {
                return;
            }

            elapsedTime = (float)Globals.ElapsedTime / Config.CAMERA_TIME_MULTIPLIER;
            // Follow target entity
            if (target != null)
            {
                targetPosition = target.Transform.Position + targetTracingOffset - new Vector2(_viewport.Width / 2.0f, _viewport.Height / 2.0f);

                targetCameraDistance = Vector2.Distance(Position, targetPosition);
                if (targetCameraDistance >= Config.CAMERA_DEADZONE)
                {
                    angle = MathUtil.RadFromVectors(Position, targetPosition);
                    direction.X += (float)Math.Cos(angle) * (targetCameraDistance - Config.CAMERA_DEADZONE) * Config.CAMERA_FOLLOW_DELAY * elapsedTime;
                    direction.Y += (float)Math.Sin(angle) * (targetCameraDistance - Config.CAMERA_DEADZONE) * Config.CAMERA_FOLLOW_DELAY * elapsedTime;
                }
            }

            Position += direction * elapsedTime;

            direction *= new Vector2((float)Math.Pow(friction, elapsedTime), (float)Math.Pow(friction, elapsedTime));

            PostUpdate();
        }

        private void PostUpdate()
        {
            // Shakes
            if (shake)
            {
                shakeStarted += Globals.ElapsedTime;
                float power = shakePower;
                if (easedStop)
                {
                    float alpha = shakeStarted / shakeDuration;
                    power = MathHelper.Lerp(shakePower, 0, alpha);
                }
                Position += new Vector2((float)(Math.Cos(Globals.GameTime.TotalGameTime.TotalMilliseconds * 1.1) * power), (float)(Math.Sin(0.3 + Globals.GameTime.TotalGameTime.TotalMilliseconds * 1.7) * power));
                /*position.X += (float)(Math.Cos(Globals.GameTime.TotalGameTime.TotalMilliseconds * 1.1) * power);
				position.Y += (float)(Math.Sin(0.3 + Globals.GameTime.TotalGameTime.TotalMilliseconds * 1.7) * power);*/

                if (shakeStarted > shakeDuration)
                {
                    shake = false;
                    shakeStarted = 0f;
                }
            }
        }

        public void TrackTarget(Entity e, bool immediate, Vector2 tracingOffset = new Vector2())
        {
            targetTracingOffset = tracingOffset;
            target = e;
            if (immediate)
            {
                Recenter();
            }
        }

        public void StopTracking()
        {
            target = null;
        }

        public void Recenter()
        {
            if (target != null)
            {
                Position = target.Transform.Position + targetTracingOffset;
            }
        }

        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                ValidatePosition();
            }
        }

        public float Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                _zoom = MathHelper.Max(value, MinZoom);
                ValidateZoom();
                ValidatePosition();
            }
        }

        public Rectangle? Limits
        {
            set
            {
                _limits = value;
                ValidateZoom();
                ValidatePosition();
            }
        }

        public Matrix ViewMatrix
        {
            get
            {
                return Matrix.CreateTranslation(new Vector3(-new Vector2(_position.X * scrollSpeedModifier, _position.Y), 0f)) *
                       Matrix.CreateTranslation(new Vector3(-_origin, 0f)) *
                       Matrix.CreateScale(_zoom, _zoom, 1f) *
                       Matrix.CreateTranslation(new Vector3(_origin, 0f));
            }
        }

        private void ValidatePosition()
        {
            if (_limits.HasValue)
            {
                Vector2 cameraWorldMin = Vector2.Transform(Vector2.Zero, Matrix.Invert(ViewMatrix));
                Vector2 cameraSize = new Vector2(_viewport.Width, _viewport.Height) / _zoom;
                Vector2 limitWorldMin = new Vector2(_limits.Value.Left, _limits.Value.Top);
                Vector2 limitWorldMax = new Vector2(_limits.Value.Right, _limits.Value.Bottom);
                Vector2 positionOffset = _position - cameraWorldMin;
                _position = Vector2.Clamp(cameraWorldMin, limitWorldMin, limitWorldMax - cameraSize) + positionOffset;
            }
        }

        private void ValidateZoom()
        {
            if (_limits.HasValue)
            {
                float minZoomX = (float)_viewport.Width / _limits.Value.Width;
                float minZoomY = (float)_viewport.Height / _limits.Value.Height;
                _zoom = MathHelper.Max(_zoom, MathHelper.Max(minZoomX, minZoomY));
            }
        }

        public Matrix GetUITransformMatrix()
        {
            return uiTransofrmMatrix;
        }

        public Matrix GetTransformMatrix(float scrollSpeedModifier = 1f, bool lockY = false)
        {
            this.scrollSpeedModifier = scrollSpeedModifier;
            return ViewMatrix;
        }
    }
}

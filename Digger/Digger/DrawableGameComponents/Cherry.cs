using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Digger
{
    public class Cherry : Entity, ICollidable
    {
        const string CHERRY_TEXTURE_NAME = "CHERRY";
        public const int CHERRY_DRAW_ORDER = 2;
        public const int DEFAULT_CHERRY_DURATION = 5;  // Cherries will last 5 seconds by default

        public const double SFX_DURATION = 0.5;

        public const string SFX_EATEN = "eatCherry";
        private static SoundEffectInstance cherryEatenSFX;
        private double cherryEatenSFXTimer;

        // What percentage the Cherry's BoundingRectangle will be with respect to containing cell's rectangle 
        public const float CELL_RECTANGLE_PERCENTAGE = 0.60f;

        public Rectangle CollisionBox => BoundingRectangle;

        public int allowedDuration { get; private set; }  // How long the cherry will last
        private double lifeTimer;

        public Cherry(Game game, Rectangle cellBoundingRectangle, int duration = DEFAULT_CHERRY_DURATION)
            : base(game, cellBoundingRectangle, CELL_RECTANGLE_PERCENTAGE)
        {
            DrawOrder = CHERRY_DRAW_ORDER;
            cherryEatenSFXTimer = 0;
            allowedDuration = duration;
            lifeTimer = 0;
        }

        protected override void LoadContent()
        {
            // Set texture
            texture = Game.Content.Load<Texture2D>(CHERRY_TEXTURE_NAME);

            if (cherryEatenSFX == null)
                cherryEatenSFX = Game.Content.Load<SoundEffect>(SFX_EATEN).CreateInstance();

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            if (lifeTimer >= allowedDuration)
            {
                // Time to disappear
                Game.Components.Remove(this); 
                lifeTimer = 0;
            }
            else
            {
                lifeTimer += gameTime.ElapsedGameTime.TotalSeconds;
            }

            //cherryEatenSFXTimer += gameTime.ElapsedGameTime.TotalSeconds;
            //if (cherryEatenSFXTimer >= SFX_DURATION)
            //{
            //    cherryEatenSFXTimer = 0;
            //    cherryEatenSFX.Stop();
            //}

            base.Update(gameTime);
        }

        public void HandleCollision()
        {
            cherryEatenSFX.Play();

            Game.Components.Remove(this); // Cherry has been "eaten"
        }
    }
}

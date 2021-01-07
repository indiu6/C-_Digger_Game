using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Digger
{
    public class Emerald : Entity, ICollidable
    {
        public const string EMERALD_TEXTURE_NAME = "EMERALD";
        public const int EMERALD_DRAW_ORDER = 2;

        // What percentage the Emerald's BoundingRectangle will be with respect to containing cell's rectangle 
        public const float CELL_RECTANGLE_PERCENTAGE = 0.50f;

        public Rectangle CollisionBox => BoundingRectangle;

        public Emerald(Game game, Rectangle cellBoundingRectangle) 
            : base(game, cellBoundingRectangle, CELL_RECTANGLE_PERCENTAGE)
        {
            DrawOrder = EMERALD_DRAW_ORDER;
        }

        protected override void LoadContent()
        {
            // Set texture
            texture = Game.Content.Load<Texture2D>(EMERALD_TEXTURE_NAME);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public void HandleCollision()
        {
            Game.Components.Remove(this); // Emerald has been "eaten"
        }
    }
}

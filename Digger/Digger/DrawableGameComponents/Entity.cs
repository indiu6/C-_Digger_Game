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
    /// <summary>
    /// Parent class of all drawable components in the game. Inherits from DrawableComponent
    /// Contains the most basic fields of a drawable object
    /// </summary>
    public abstract class Entity : DrawableGameComponent
    {
        protected Vector2 position;
        protected float size;
        protected Texture2D texture;

        public Rectangle BoundingRectangle;  // Bounding rectangle for the Digger

        public int startCoordinateX { get; protected set; }  // Graphic X coordinate of the Entity's bounding rectangle
        public int startCoordinateY { get; protected set; }  // Graphic Y coordinate of the Entity's bounding rectangle
        public SpriteEffects spriteEffects  { get; protected set; } // Field to control the sprite effects

        protected Grid grid { get => Game.Services.GetService<Grid>(); }  // A shortcut to access the Game grid

        public int CurrentRow { get; protected set; }  // Current row of Entity
        public int CurrentCol { get; protected set; }  // Current column of Entity

        public Entity(Game game) : base(game) { }

        /// <summary>
        /// Constructor that takes the rectangle of containing cell as a parameter plus a size percentage and generates
        /// Bounding rectangle for the Entity considering the same center of the rectangle parameter but with width and left that
        /// are a percentage of it.
        /// This constructor is useful when child classes want to define their BoundingRectangle right away when they are created and
        /// already know what size % of the containing cell they want it to be
        /// </summary>
        /// <param name="game">the game class</param>
        /// <param name="cellRectangle">containing cell bounding rectangle</param>
        /// <param name="sizePercentage">desired percetange of containing cell bounding rectangle</param>
        public Entity(Game game, Rectangle cellRectangle, float sizePercentage = 1.0f) : base(game)
        {
            // Create bounding rectangle having the same center as its containing cellRectangle and considering a size percentage of it
            sizePercentage = (sizePercentage <= 0 || sizePercentage > 1) ? 1.0f : sizePercentage;
            int entityWidth = (int)(cellRectangle.Width * sizePercentage);
            int entityHeight = (int)(cellRectangle.Height * sizePercentage);

            startCoordinateX = cellRectangle.X + (cellRectangle.Width - entityWidth) / 2;
            startCoordinateY = cellRectangle.Y + (cellRectangle.Height - entityHeight) / 2;

            BoundingRectangle = new Rectangle(startCoordinateX, startCoordinateY, entityWidth, entityHeight);
            UpdateCurrentRowAndCol();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateCurrentRowAndCol();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sb = Game.Services.GetService<SpriteBatch>();
            sb.Begin();
            sb.Draw(texture,
                    BoundingRectangle,
                    null, // Rectangle? sourceRectangle -> set if only drawing from a sprite sheet
                    Color.White,
                    0.0f, // rotation
                    Vector2.Zero, // origin of the rotation
                    spriteEffects, // Entity can flip by this
                    0f); // layer depth
            sb.End();

            base.Draw(gameTime);
        }

        // Updates the current row and column in grid where entity is located
        protected void UpdateCurrentRowAndCol()
        {
            // Obtain current location in grid
            CellLocation currentLocation = grid.GetRectangleCenterGridPosition(BoundingRectangle);

            // Update current row and column of entity
            CurrentRow = currentLocation.Row;
            CurrentCol = currentLocation.Col;
        }
    }
}

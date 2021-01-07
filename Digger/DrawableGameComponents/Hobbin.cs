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
    public class Hobbin : Enemy
    {
        const string TEXTURE_NAME = "VRHOB2";
        const int DEFAULT_HOBBIN_SPEED = 4;
        const int DEFAULT_HOBBIN_LIVES = 1;
        const DirectionType DEFAULT_HOBBIN_HORIZONTAL_DIRECTION = DirectionType.Left;
        const DirectionType DEFAULT_HOBBIN_VERTICAL_DIRECTION = DirectionType.None;
        const int HOBBIN_DRAWORDER = 10;
        const int LOCATION_OFFSET = 5;

        public Hobbin(Game game, CellLocation startLocation,
                        DirectionType horizontalDirection = DEFAULT_HOBBIN_HORIZONTAL_DIRECTION,
                        DirectionType verticalDirection = DEFAULT_HOBBIN_VERTICAL_DIRECTION)
            : base(game,   // The game object
                   MovementCondition.MovingRandomly,   // Hobbin will move randomly
                   true,   // Hobbin CAN dig
                   DEFAULT_HOBBIN_LIVES) // default number of lives for the Digger
        {
            // Set the start screen coordinates based on the specified startLocation
            grid.GetCellCoordinates(startLocation.Row, startLocation.Col, out int coordX, out int coordY);
            startCoordinateX = coordX >= 0 ? coordX + LOCATION_OFFSET : grid.OriginX + LOCATION_OFFSET;
            startCoordinateY = coordY >= 0 ? coordY + LOCATION_OFFSET : grid.OriginY + LOCATION_OFFSET;

            // Set direction properties
            SetHorizontalDirection(horizontalDirection);
            SetVerticalDirection(VerticalDirection);
            SetLastMovingDirection(DirectionType.Left); // This is the key one for Hobbin

            UpdateCurrentRowAndCol();

            // Set other drawing variables
            Speed = DEFAULT_HOBBIN_SPEED;
            spriteEffects = SpriteEffects.FlipHorizontally;
            DrawOrder = HOBBIN_DRAWORDER;
        }

        protected override void LoadContent()
        {
            texture = Game.Content.Load<Texture2D>(TEXTURE_NAME);
            BoundingRectangle = new Rectangle(startCoordinateX, startCoordinateY,
                                    grid.CellWidth - 2 * LOCATION_OFFSET, grid.CellHeight - 2 * LOCATION_OFFSET);
            UpdateCurrentRowAndCol();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            spriteEffects = (LastMovingDirection == DirectionType.Left)? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            base.Update(gameTime);  // Uses Creature parent class Update method to react to keyboard
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);  // Uses Entity parent class Draw method
        }


        // Overriden method for handling collision
        public override void HandleCollision()
        {
            debugger.SetMessage("Hobbin was hit!");
            Game.Components.Remove(this);
        }
    }
}


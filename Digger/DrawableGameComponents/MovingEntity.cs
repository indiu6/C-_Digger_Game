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
    /// Enumerator type DirectionType, used to indicate in which direction the moving entity is currently moving
    /// </summary>
    public enum DirectionType
    {
        None = 0,   // The moving entity is still
        Up,         // Going up
        Down,       // Going down
        Left,       // Going to the left
        Right       // Going to the right
    }

    /// <summary>
    /// Moving entity in the game. Inherits from Entity. Anything that moves will inherit from this class
    /// </summary>
    public abstract class MovingEntity : Entity
    {
        const int DEFAULT_SPEED = 5;  // Default speed

        public int Speed { get; protected set; }  // Speed of the moving entity
        public DirectionType HorizontalDirection { get; protected set; }  // In what direction the moving entity is horizontally heading to
        public DirectionType VerticalDirection { get; protected set; }  // In what direction the moving entity is vertically heading to
        public DirectionType LastMovingDirection { get; protected set; }  // What is the direction of the last time the MovingEntity was heading to

        // Constructor with default values
        public MovingEntity(Game game) : this(game, DEFAULT_SPEED, DirectionType.None, DirectionType.None) { }

        /// <summary>
        /// Constructor which sets the class properties
        /// </summary>
        /// <param name="game">Game object. This parameter is passed to the base class</param>
        /// <param name="speed">Speed for the moving entity</param>
        /// <param name="horizontalDirection">horizontal direction of the moving entity</param>
        /// <param name="verticalDirection">vertical direction of the moving entity</param>
        public MovingEntity(Game game, int speed, DirectionType horizontalDirection, DirectionType verticalDirection) : base(game)
        {
            SetSpeed(speed);
            SetHorizontalDirection(horizontalDirection);
            SetVerticalDirection(verticalDirection);
            SetLastMovingDirection(DirectionType.None);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Sets the speed for the MovingEntity object. Validates that parameter is greater than zero
        /// </summary>
        /// <param name="speed">speed value to be set</param>
        public void SetSpeed(int speed)
        {
            Speed = speed > 0 ? speed : DEFAULT_SPEED;
        }

        /// <summary>
        /// Sets value of horizontal direction parameter to HorizintalDirection property. 
        /// Performs a validation first to ensure that value is correct
        /// </summary>
        /// <param name="direction">value to be set</param>
        /// <returns>true if set was successful and false otherwise (the value of parameter was invalid)</returns>
        public bool SetHorizontalDirection(DirectionType direction)
        {
            if (Enum.IsDefined(typeof(DirectionType), direction))
            {
                HorizontalDirection = direction;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets value of vertical direction parameter to VerticalDirection property. 
        /// Performs a validation first to ensure that value is correct
        /// </summary>
        /// <param name="direction">value to be set</param>
        /// <returns>true if set was successful and false otherwise (the value of parameter was invalid)</returns>
        public bool SetVerticalDirection(DirectionType direction)
        {
            if (Enum.IsDefined(typeof(DirectionType), direction))
            {
                VerticalDirection = direction;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets value of direction parameter to LastMovingDirection property
        /// Performs a validation first to ensure that value is correct
        /// </summary>
        /// <param name="direction">value to be set</param>
        /// <returns>true if set was successful and false otherwise (the value of parameter was invalid)</returns>
        public bool SetLastMovingDirection(DirectionType direction)
        {
            if (Enum.IsDefined(typeof(DirectionType), direction))
            {
                LastMovingDirection = direction;
                return true;
            }

            return false;
        }
    }
}

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
    public class Enemy : Creature
    {
        public Enemy(Game game) : base(game)
        {
        }

        /// <summary>
        /// Constructor which sets the class properties
        /// </summary>
        /// <param name="game">Game object. This parameter is passed to the base class</param>
        /// <param name="movementCondition">MovementCondition enum to indicate how the Enemy will move</param>
        /// <param name="canDig">boolean to indicate whether the creature can dig</param>
        /// <param name="numberOfLives">number of lives</param>
        public Enemy(Game game, MovementCondition movementCondition, bool canDig, int numberOfLives) 
            : base(game, movementCondition, canDig, numberOfLives)
        {
        }
    }
}

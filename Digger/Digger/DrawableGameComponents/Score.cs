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
    public class Score : DrawableGameComponent
    {
        SpriteFont font;
        public int scoreValue { get; private set; }
        Vector2 position;
        const int LOCATION_OFFSET = 40;

        string livesAndScoreText => $"Lives left:  {Game.Services.GetService<Digger>().NumberOfLives}                                      Score: "
                                    + scoreValue.ToString("000000");

        public Score(Game game) : base(game)
        {
            scoreValue = 0;

            if (Game.Services.GetService<Score>() != null)
            {
                Game.Services.RemoveService(typeof(Score));
            }
            Game.Services.AddService(this);

            position = new Vector2(LOCATION_OFFSET, 0);

        }

        protected override void LoadContent()
        {
            font = Game.Content.Load<SpriteFont>("hudFont");

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sb = Game.Services.GetService<SpriteBatch>();
            sb.Begin();
            sb.DrawString(font, livesAndScoreText, position, Color.Green);
            sb.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Adds given score to game score
        /// Must be a positive value
        /// </summary>
        /// <param name="value">integer greater than zero</param>
        public void AddScore(int value)
        {
            if (value > 0)
            {
                scoreValue += value;
            }
        }

        /// <summary>
        /// Resets game score to zero
        /// </summary>
        public void ResetScore()
        {
            scoreValue = 0;
        }
    }
}

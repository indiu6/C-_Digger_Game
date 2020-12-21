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
    public class DebugInfo : DrawableGameComponent
    {
        SpriteFont font;
        Vector2 position;

        StringBuilder builder;
        private string debugMessage;

        KeyboardState prevKS;
        bool showDebug;

        public DebugInfo(Game game) : base(game)
        {
            DrawOrder = int.MaxValue;  // To ensure it will be drawn on the top ALWAYS

            builder = new StringBuilder();
            position = Vector2.Zero;
            prevKS = Keyboard.GetState();
            showDebug = false;
        }

        protected override void LoadContent()
        {
            font = Game.Content.Load<SpriteFont>("Arial20");
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.D) && prevKS.IsKeyUp(Keys.D))
            {
                showDebug = !showDebug;
            }
            prevKS = ks;

            if (showDebug)
            {
                builder.Clear();
                builder.AppendLine(debugMessage);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (showDebug)
            {
                SpriteBatch sb = Game.Services.GetService<SpriteBatch>();
                sb.Begin();
                sb.DrawString(font, builder.ToString(), position, Color.Red);
                sb.End();
                base.Draw(gameTime);
            }
        }

        public void ClearMessage()
        {
            builder.Clear();
            debugMessage = builder.ToString();
        }

        public void SetMessage(string addingString)
        {
            builder.Clear();
            builder.Append(addingString);
            debugMessage = builder.ToString();
        }

        public void AppendToMessage(string addingString)
        {
            builder.Append(addingString);
            debugMessage = builder.ToString();
        }

        public void AppendLineToMessage(string addingString)
        {
            builder.AppendLine(addingString);
            debugMessage = builder.ToString();
        }
    }
}

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
    class AboutComponent : DrawableGameComponent
    {
        SpriteFont regularFont;
        Color regularColor = Color.White;
        Vector2 textPosition;
        Vector2 diggerPosition;
        Vector2 nobbinPosition;

        Texture2D coolDigger;
        Texture2D coolNobbin;

        const string ABOUT_TEXT = "Created by Renzo and Sean";

        public AboutComponent(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            textPosition = new Vector2(300, 660);
            diggerPosition = new Vector2(20, 20);
            nobbinPosition = new Vector2(660, 150);

            base.Initialize();
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();

            spriteBatch.Begin();

            spriteBatch.DrawString(regularFont, ABOUT_TEXT, textPosition, regularColor);
            spriteBatch.Draw(coolDigger, diggerPosition, Color.White);
            spriteBatch.Draw(coolNobbin, nobbinPosition, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            regularFont = Game.Content.Load<SpriteFont>("regularFont");
            coolDigger = Game.Content.Load<Texture2D>("coolDig1");
            coolNobbin = Game.Content.Load<Texture2D>("coolDig2");

            base.LoadContent();
        }
    }
}

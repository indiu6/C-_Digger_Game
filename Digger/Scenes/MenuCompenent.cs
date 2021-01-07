using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Digger
{
    public enum MenuSelection
    {
        StartGame,
        Help,
        About,
        Quit
    }

    public class MenuComponent : DrawableGameComponent
    {
        SpriteFont regularFont;
        SpriteFont highlightFont;

        private List<string> menuItems;
        private int selectedIndex;
        private Vector2 startingPosition;
        private Vector2 bannerPosition;

        private Color regularColor = Color.White;
        private Color hilightColor = Color.WhiteSmoke;

        private KeyboardState prevKS;

        Texture2D diggerBanner;

        public MenuComponent(Game game) : base(game)
        {
            menuItems = new List<string>
            {
                "Start Game",
                "Help",
                "About",
                "Quit"
            };
            prevKS = Keyboard.GetState();
        }

        public override void Initialize()
        {
            // starting position of the menu items - but you can decise to put it elsewhere
            startingPosition = new Vector2(180, 180);

            // starting position of banner image
            bannerPosition = new Vector2(100, 20);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // load the fonts we will be using for this menu
            regularFont = Game.Content.Load<SpriteFont>("regularFont");
            highlightFont = Game.Content.Load<SpriteFont>("hilightFont");
            diggerBanner = Game.Content.Load<Texture2D>("diggerBanner");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            menuItems[0] = (((DiggerGame)Game).GameInProgress) ? "Resume Game" : "Start Game";

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Down) && prevKS.IsKeyUp(Keys.Down))
            {
                selectedIndex++;

                // if we're now out of bounds in our menu
                // items, reset to first item at index 0
                if (selectedIndex == menuItems.Count)
                {
                    selectedIndex = 0;
                }
            }

            if (ks.IsKeyDown(Keys.Up) && prevKS.IsKeyUp(Keys.Up))
            {
                selectedIndex--;

                // if we're now out of bounds at -1, 
                // move us to the last menu item index                
                if (selectedIndex == -1)
                {
                    selectedIndex = menuItems.Count - 1;
                }
            }
            else if (ks.IsKeyDown(Keys.Enter))
            {
                SwitchScenes();
            }
            prevKS = ks;

            base.Update(gameTime);
        }

        private void SwitchScenes()
        {
            ((DiggerGame)Game).HideAllScenes();

            switch ((MenuSelection)selectedIndex)
            {
                case MenuSelection.StartGame:
                    Game.Services.GetService<ActionScene>().StartOrResumeGame();
                    break;

                case MenuSelection.Help:
                    Game.Services.GetService<HelpScene>().Show();
                    break;

                case MenuSelection.Quit:
                    Game.Exit();
                    break;

                case MenuSelection.About:
                    Game.Services.GetService<AboutScene>().Show();
                    break;

                    //default:
                    //    // for now there is nothing handling the other options
                    //    // we will simply show this screen again
                    //    Game.Services.GetService<StartScene>().Show();
                    //    break;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sb = Game.Services.GetService<SpriteBatch>();

            Vector2 nextPosition = startingPosition;

            sb.Begin();

            sb.Draw(diggerBanner, bannerPosition, Color.White);

            for (int i = 0; i < menuItems.Count; i++)
            {
                SpriteFont activeFont = regularFont;
                Color activeColor = regularColor;

                // if the selection is the item we are drawing
                // made it a the special font and colour
                if (selectedIndex == i)
                {
                    activeFont = highlightFont;
                    activeColor = hilightColor;
                }

                sb.DrawString(activeFont, menuItems[i], nextPosition, activeColor);

                // update the position of next string
                nextPosition.Y += regularFont.LineSpacing;
            }

            sb.End();

            base.Draw(gameTime);
        }
    }
}

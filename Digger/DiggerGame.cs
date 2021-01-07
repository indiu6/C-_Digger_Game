using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Digger
{
    /// <summary>
    /// The main class of the Digger Game.
    /// </summary>
    public class DiggerGame : Game
    {
        const int DEFAULT_SCREEN_WIDTH = 1372;
        const int DEFAULT_SCREEN_HEIGHT = 768;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public bool GameInProgress => Services.GetService<ActionScene>().CurrentGameState == GameState.GameOn;

        public DiggerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Set the screen size according to configuration in resources
            graphics.PreferredBackBufferWidth = int.TryParse(Resource.screenWidth, out int width) ? width : DEFAULT_SCREEN_WIDTH;
            graphics.PreferredBackBufferHeight = int.TryParse(Resource.screenHeight, out int height) ? height : DEFAULT_SCREEN_HEIGHT;           
            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Comment this part if not debugging
            DebugInfo debugger = new DebugInfo(this);
            Components.Add(debugger);  
            Services.AddService(debugger);

            // Add Scenes
            StartScene menuScene = new StartScene(this);
            Components.Add(menuScene);
            Services.AddService(menuScene);
            
            ActionScene actionScene = new ActionScene(this);
            Components.Add(actionScene);
            Services.AddService(actionScene);

            HelpScene helpScene = new HelpScene(this);
            Components.Add(helpScene);
            Services.AddService(helpScene);

            AboutScene aboutScene = new AboutScene(this);
            Components.Add(aboutScene);
            Services.AddService(aboutScene);

            base.Initialize();

            // hide all then show our first scene
            // this has to be done after the initialize methods are called
            // on all our components 
            HideAllScenes();
            menuScene.Show();
        }

        /// <summary>
        /// Get all scenes currently in our game, and hide/disable them
        /// </summary>
        public void HideAllScenes()
        {
            foreach (GameScene scene in Components.OfType<GameScene>())
            {
                scene.Hide();
            }           
            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Services.AddService<SpriteBatch>(spriteBatch); // Adding spriteBatch to Services so that other classes can use it
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            // PENDING: Control transition of levels here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}

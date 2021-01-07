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
    public class EnemyManager : GameComponent
    {
        private LevelDesign levelDesign;
        private bool timeToCreateEnemy;  // Flag that will indicate it is time to create a Nobbin
        private bool stopCreatingEnemies; // Used when Digger is dead and there is no longer need to create enemies
        private bool timeToCreateCherry;  // Flag that will indicate it is time to create a Cherry
        private bool timeToDeleteCherry;  // Flag that will indicate it is time to delete a Cherry

        private int totalNobbinsCreated;  // How many Nobbins have been created so far
        private int totalHobbinsCreated;  // How many Hobbins have been created so far
        private int totalCherriesCreated;  // How many Cherries have been created so far
        
        double nobbinCreationTimer;  // Timer to wait for a new Nobbin creation
        double hobbinCreationTimer;  // Timer to wait for a new Hobbin creation
        double cherryCreationTimer; // Timer to wait for a new Cherry creation

        ActionScene parent;

        // Getter for quick access to the game grid
        private Grid grid { get => Game.Services.GetService<Grid>(); }

        public EnemyManager(Game game, LevelDesign levelDesign, ActionScene parent) : base(game)
        {
            if (levelDesign == null)
            {
                throw new ArgumentException("A level design cannot be null when creating Enemy Manager");
            }

            this.levelDesign = levelDesign;
            totalNobbinsCreated = 0;   // No Nobbin created in the beginning
            totalHobbinsCreated = 0;   // No Hobbin created in the beginning
            timeToCreateEnemy = true; // Initially, a Nobbin can be created without waiting 
            stopCreatingEnemies = false;  // Green light to create enemies when due time

            timeToCreateCherry = false; // Initially, a Cherry can't be created  
            timeToDeleteCherry = false; // Initially, a Cherry can't be deleted  
            totalCherriesCreated = 0; // No Cherry created in the beginning

            Enabled = false;
            this.parent = parent;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (!stopCreatingEnemies)
            {
                UpdateNobbinCreation(gameTime);
                UpdateHobbinCreation(gameTime);
                if (totalHobbinsCreated > 0 || totalHobbinsCreated > 0)
                    CheckForEnemiesLeft();
                UpdateCherryCreation(gameTime);
            }

            base.Update(gameTime);
        }
        public void UpdateNobbinCreation(GameTime gameTime)
        {
            // Check if the maximum number of Nobbins has already been created, in which case just return
            if (totalNobbinsCreated >= levelDesign.NobbinCount)
                return;

            // Check if there are less Nobbins on screen than the maximum simultaneous Nobbins allowed in level 
            if (Game.Components.OfType<Nobbin>().Count() < levelDesign.MaxSimultaneousNobbins)
            {
                if (timeToCreateEnemy)
                {
                    // Create a new Nobbin
                    Nobbin nobbin = new Nobbin(Game, levelDesign.EnemiesStartLocation);
                    parent.AddComponent(nobbin);
                    totalNobbinsCreated++;

                    // Set the flag off and the timer to wait for a new creation
                    timeToCreateEnemy = false;
                    nobbinCreationTimer = 0;
                }
                else if (nobbinCreationTimer >= levelDesign.NobbinGenerationFrequency)
                {
                    timeToCreateEnemy = true;
                    nobbinCreationTimer = 0;
                }
                else
                {
                    // Update timer while waiting to create a Nobbin
                    nobbinCreationTimer += gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }

        public void UpdateHobbinCreation(GameTime gameTime)
        {
            // Check if the maximum number of Hobbins has already been created, in which case just return
            if (totalHobbinsCreated >= levelDesign.HobbinCount)
                return;

            // Check if there are less Hobbins on screen than the maximum simultaneous Hobbins allowed in level 
            if (Game.Components.OfType<Hobbin>().Count() < levelDesign.MaxSimultaneousHobbins)
            {
                if (timeToCreateEnemy)
                {
                    // Create a new Hobbin
                    Hobbin hobbin = new Hobbin(Game, levelDesign.EnemiesStartLocation);
                    parent.AddComponent(hobbin);
                    totalHobbinsCreated++;

                    // Set the flag off and the timer to wait for a new creation
                    timeToCreateEnemy = false;
                    hobbinCreationTimer = 0;
                }
                else if (hobbinCreationTimer >= levelDesign.HobbinGenerationFrequency)
                {
                    timeToCreateEnemy = true;
                    hobbinCreationTimer = 0;
                }
                else
                {
                    // Update timer while waiting to create a Hobbin
                    hobbinCreationTimer += gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }

        public void UpdateCherryCreation(GameTime gameTime)
        {
            // Check if the maximum number of Cherries has already been created, in which case just return
            if (totalCherriesCreated >= levelDesign.CherryCount)
                return;

            // Check if there are less Cherries on screen than the maximum simultaneous Cherries allowed in level 
            if (Game.Components.OfType<Cherry>().Count() < levelDesign.MaxSimultaneousCherries)
            {
                if (timeToCreateCherry)
                {
                    // Create a new Cherry
                    GridCell gridCell = grid.GetCellByGridCoordinates(levelDesign.EnemiesStartLocation.Row,
                                        levelDesign.EnemiesStartLocation.Col);
                    //gridCell.SetContentType(CellContentType.Cherry);
                    if (gridCell != null)
                    {
                        Cherry cherry = new Cherry(Game, gridCell.BoundingRectangle, levelDesign.CherryDuration);
                        parent.AddComponent(cherry);
                        totalCherriesCreated++;

                        // Set the flag off and the timer to wait for a new creation
                        timeToCreateCherry = false;
                        cherryCreationTimer = 0;
                    }
                }
                else if (cherryCreationTimer >= levelDesign.CherryGenerationFrequency)
                {
                    timeToCreateCherry = true;
                    cherryCreationTimer = 0;
                }
                else
                {
                    // Update timer while waiting to create a Cherry
                    cherryCreationTimer += gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }

        private void CheckForEnemiesLeft()
        {
            // Check if no Enemies remain on screen, in which case, the level is deemed completed
            if (Game.Components.OfType<Enemy>().Count() == 0)
            {
                parent.LevelCompleted();  // Notify parent
            }
        }

        /// <summary>
        /// Stops all Enemies so that they can no longer move but remain visible
        /// Also sets the flag to stop creating enemies to true
        /// Used by ActionScene every time Digges gets killed while funeral music is played
        /// </summary>
        public void StopAllEnemies()
        {
            stopCreatingEnemies = true;
            foreach (Enemy enemy in Game.Components.OfType<Enemy>())
            {
                enemy.Enabled = false;
            }
        }

        public void RestartLevel()
        {
            // Remove all existing enemies
            for (int i = 0; i < Game.Components.OfType<Enemy>().Count(); i++)
            {
                Enemy enemy = Game.Components.OfType<Enemy>().ElementAt(i);
                Game.Components.Remove(enemy);
                i--;
            }

            // Reset counter of enemies created
            totalHobbinsCreated = 0;
            totalNobbinsCreated = 0;

            stopCreatingEnemies = false;
        }
    }
}

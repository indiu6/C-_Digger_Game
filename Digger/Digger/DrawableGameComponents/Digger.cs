using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Digger
{
    public class Digger : Creature
    {
        // Textures for Digger images
        public const string TEXTURE_DIGGER = "CLDIG2";
        public const string TEXTURE_UP_NAME = "digup";
        public const string TEXTURE_DOWN_NAME = "digdown";
        public const string TEXTURE_DIGGER_HIT = "CDDIE";
        public const string TEXTURE_DIGGER_DEAD1 = "CGRAVE1";
        public const string TEXTURE_DIGGER_DEAD2 = "CGRAVE2";
        public const string TEXTURE_DIGGER_DEAD3 = "CGRAVE3";
        public const string TEXTURE_DIGGER_DEAD4 = "CGRAVE4";
        public const string TEXTURE_DIGGER_DEAD5 = "CGRAVE5";

        public const double FRAME_DURATION = 0.6;  // For Digger animation when it's dead

        public const string SFX_GET_EMERALD = "getItem";
        public const string SFX_DIGGER_KILLED = "diggerHitByEnemy";

        public const int DEFAULT_DIGGER_SPEED = 4;
        public const int DEFAULT_DIGGER_LIVES = 3;
        public const int DIGGER_DRAWORDER = 10;
        public const int RECTANGLE_OFFSET = 5;  // Offset to create BoundingRectangle on each side with regard to containing cell
        public const int DIGGER_CENTERED_OFFSET = 10;  // Offset to consider for determining if it's centered with respect to containing cell
        public const int FIRING_WAIT_PERIOD = 8;  // How many seconds Digger will be able to fire after last time it did
        public const int BULLET_OFFSET = 10;  // Offset to be used when setting bullet start position

        public const int POINTS_EMERALD = 25; // Points for Digger "eating" an Emerald
        public const int POINTS_GOLD = 500; // Points for Digger "eating" Gold
        public const int POINTS_ENEMY_SHOT = 250;  // Points for Digger killing an Enemy by shooting it
        public const int POINTS_CHERRY = 750;  // Points for Digger "eating" an Cherry

        private static SoundEffect getEmeraldSFX;  // Sound effect to be played when an emerald is "eaten"
        private static SoundEffect diggerKilledSFX;  // Sound effect to be played when Digger is hit by an Enemy

        private KeyboardState prevKs;  // To store the previous keyboard stroke
        private bool canFire;   // If Digger is allowed to fire at the moment (depends on FIRING_TIMER since last time it fired)
        private double firingTimer;
        private double deathWaitingTimer;  // Timer to wait while death sound effects and music are played
        private bool waitingForSongToFinish;  // Flag to indicate whether Digger is waiting for a song to finish (like in Funeral)

        ActionScene parent;  // The scene that created this grid
        
        // Frame variables
        private int currentFrame;
        private double frameTimer;

        private List<Texture2D> deadTextures; 


        public Digger(Game game, CellLocation startLocation, ActionScene parent) 
            : base(game,   // The game object
                   MovementCondition.MovingByKeyboard,   // Digger is alive
                   true,   // Digger can dig
                   DEFAULT_DIGGER_LIVES) // default number of lives for the Digger
        {
            this.parent = parent; // Set the parent game scene

            // Set the start screen coordinates based on the specified startLocation
            grid.GetCellCoordinates(startLocation.Row, startLocation.Col, out int coordX, out int coordY);
            startCoordinateX = coordX >= 0 ? coordX + RECTANGLE_OFFSET : grid.OriginX + RECTANGLE_OFFSET;
            startCoordinateY = coordY >= 0 ? coordY + RECTANGLE_OFFSET : grid.OriginY + RECTANGLE_OFFSET;
            centeredOffset = DIGGER_CENTERED_OFFSET;
            canFire = true;
            firingTimer = 0;
            deathWaitingTimer = 0;
            waitingForSongToFinish = false;

            // Set other drawing variables
            Speed = DEFAULT_DIGGER_SPEED;
            spriteEffects = SpriteEffects.None;
            DrawOrder = DIGGER_DRAWORDER;
            currentFrame = 0;
            frameTimer = 0;
        }

        protected override void LoadContent()
        {
            texture = Game.Content.Load<Texture2D>(TEXTURE_DIGGER);
            BoundingRectangle = new Rectangle(startCoordinateX, startCoordinateY, 
                                    grid.CellWidth - 2*RECTANGLE_OFFSET, grid.CellHeight - 2*RECTANGLE_OFFSET);

            if (getEmeraldSFX == null)
                getEmeraldSFX = Game.Content.Load<SoundEffect>(SFX_GET_EMERALD);

            if (diggerKilledSFX == null)
                diggerKilledSFX = Game.Content.Load<SoundEffect>(SFX_DIGGER_KILLED);

            deadTextures = new List<Texture2D> { Game.Content.Load<Texture2D>(TEXTURE_DIGGER_DEAD1),
                                                 Game.Content.Load<Texture2D>(TEXTURE_DIGGER_DEAD2),
                                                 Game.Content.Load<Texture2D>(TEXTURE_DIGGER_DEAD3),
                                                 Game.Content.Load<Texture2D>(TEXTURE_DIGGER_DEAD4),
                                                 Game.Content.Load<Texture2D>(TEXTURE_DIGGER_DEAD5) };

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (MovementCondition == MovementCondition.MovingByKeyboard)
            {
                CheckForCollisions();

                KeyboardState ks = Keyboard.GetState();

                CheckFiring(gameTime, ks);

                if (ks.IsKeyDown(Keys.Up))
                {
                    if (grid.IsHorizontallyCentered(BoundingRectangle.Center.X))
                    {
                        texture = Game.Content.Load<Texture2D>(TEXTURE_UP_NAME);
                    }
                }
                else if (ks.IsKeyDown(Keys.Down))
                {
                    if (grid.IsHorizontallyCentered(BoundingRectangle.Center.X))
                    {
                        texture = Game.Content.Load<Texture2D>(TEXTURE_DOWN_NAME);
                    }
                }

                if (ks.IsKeyDown(Keys.Left))
                {
                    texture = Game.Content.Load<Texture2D>(TEXTURE_DIGGER);
                }
                else if (ks.IsKeyDown(Keys.Right))
                {
                    texture = Game.Content.Load<Texture2D>(TEXTURE_DIGGER);
                }

                prevKs = ks; // Update last key pressed
            }

            if (MovementCondition == MovementCondition.Dead)
            {
                frameTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (frameTimer >= FRAME_DURATION)
                {
                    frameTimer = 0;
                    currentFrame++;
                }

                if (currentFrame >= deadTextures.Count)
                {
                    currentFrame--;  // Return to last frame
                }
                texture = deadTextures[currentFrame];
            }

            base.Update(gameTime);  // Uses Creature parent class Update method to react to keyboard
        }

        /// <summary>
        /// Controls timer for Digger's ability to shoot and checks if it is trying to fire
        /// If Digger is firing and it can fire, a Bullet is created on the same direction Digger is heading to
        /// </summary>
        /// <param name="gameTime">Update of game elapsed time</param>
        /// <param name="ks">Keyboard state</param>
        private void CheckFiring(GameTime gameTime, KeyboardState ks)
        {
            if (!canFire)
            {
                firingTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (firingTimer >= FIRING_WAIT_PERIOD)
                {
                    canFire = true;
                    debugger.SetMessage("Ready to fire");
                }
            }
            if (ks.IsKeyDown(Keys.Space) && prevKs.IsKeyUp(Keys.Space))
            {
                // User hit space to fire. Check if Digger is allowed to fire or check if the necessary time has passed
                if (canFire)
                {
                    FireBullet();
                    debugger.SetMessage("Digger fired!");
                    canFire = false;  // Digger will need to wait the FIRING_WAIT_PERIOD number of seconds to be able to fire again
                    firingTimer = 0;  // Set the timer to be able to fire again
                }
                else
                {
                    debugger.SetMessage("Digger can't fire yet");
                }
            }
        }

        /// <summary>
        /// Digger "fires" by creating a bullet in the direction it is heading to
        /// </summary>
        private void FireBullet()
        {
            Bullet bullet = null;

            switch (LastMovingDirection)
            {
                case DirectionType.Up:
                    bullet = new Bullet(Game, BoundingRectangle.Center.X - Bullet.BULLET_SIDE_SIZE/2, 
                                            BoundingRectangle.Top - Bullet.BULLET_SIDE_SIZE/2);
                    bullet.SetLastMovingDirection(DirectionType.Up);
                    break;
                case DirectionType.Down:
                    bullet = new Bullet(Game, BoundingRectangle.Center.X - Bullet.BULLET_SIDE_SIZE / 2, 
                                            BoundingRectangle.Bottom - BULLET_OFFSET);
                    bullet.SetLastMovingDirection(DirectionType.Down);
                    break;
                case DirectionType.Left:
                    bullet = new Bullet(Game, BoundingRectangle.Left-Bullet.BULLET_SIDE_SIZE+ BULLET_OFFSET, 
                                            BoundingRectangle.Center.Y - Bullet.BULLET_SIDE_SIZE/2);
                    bullet.SetLastMovingDirection(DirectionType.Left);
                    break;
                case DirectionType.Right:
                    bullet = new Bullet(Game, BoundingRectangle.Right- BULLET_OFFSET, 
                                            BoundingRectangle.Center.Y - Bullet.BULLET_SIDE_SIZE/2);
                    bullet.SetLastMovingDirection(DirectionType.Right);
                    break;
            }

            // Once created, the bullet is added as a component of the scene
            if (bullet != null)
                Game.Services.GetService<ActionScene>().AddComponent(bullet);
        }

        public override void Draw(GameTime gameTime)  
        {
            base.Draw(gameTime);  // Uses Entity parent class Draw method
        }

        private void CheckForCollisions()
        {
            CheckForEmeraldCollision();
            CheckForGoldBagCollision();
            CheckForEnemyCollision();
            CheckForCherryCollision();
        }

        private void CheckForEmeraldCollision()
        {
            // Check if Digger is "eating an emerald"
            for (int i = 0; i < Game.Components.OfType<Emerald>().Count(); i++)
            {
                Emerald emerald = Game.Components.OfType<Emerald>().ElementAt(i);

                if (this.CollisionBox.Intersects(emerald.CollisionBox))
                {
                    emerald.HandleCollision();
                    Game.Services.GetService<Score>().AddScore(POINTS_EMERALD);
                    getEmeraldSFX.Play(0.4f, 1, 0);
                    i--; // When the collision is handled by the Emerald, it will be removed from the components so the list will reduce
                }
            }

            // Check if no Emeralds remain, in which case, the level is completed
            if (Game.Components.OfType<Emerald>().Count() == 0)
            {
                parent.LevelCompleted();
            }
        }

        private void CheckForGoldBagCollision()
        {
            // Check if Digger is "eating gold"
            for (int i = 0; i < Game.Components.OfType<GoldBag>().Count(); i++)
            {
                GoldBag goldBag = Game.Components.OfType<GoldBag>().ElementAt(i);

                if (this.CollisionBox.Intersects(goldBag.CollisionBox))
                {
                    goldBag.HandleCollision();
                    if (goldBag.State == GoldBagState.Gold)
                    {
                        Game.Services.GetService<Score>().AddScore(POINTS_GOLD);
                        i--; // When the collision is handled by GoldBag in Gold state, it will be removed from the components 
                    }
                }
            }
        }

        private void CheckForEnemyCollision()
        {
            for (int i = 0; i < Game.Components.OfType<Enemy>().Count(); i++)
            {
                Enemy enemy = Game.Components.OfType<Enemy>().ElementAt(i);

                if (this.CollisionBox.Intersects(enemy.CollisionBox))
                {
                    enemy.HandleCollision();
                    this.HandleCollision();
                    break; // Digger will die, just leave this loop
                }
            }
        }

        private void CheckForCherryCollision()
        {
            // Check if Digger is "eating an cherry"
            for (int i = 0; i < Game.Components.OfType<Cherry>().Count(); i++)
            {
                Cherry cherry = Game.Components.OfType<Cherry>().ElementAt(i);

                if (this.CollisionBox.Intersects(cherry.CollisionBox))
                {
                    cherry.HandleCollision();
                    Game.Services.GetService<Score>().AddScore(POINTS_CHERRY);
                    getEmeraldSFX.Play(0.4f, 1, 0);
                    i--; // When the collision is handled by the Cherry, it will be removed from the components so the list will reduce
                }
            }
        }

        // Overriden method for handling collision
        public override void HandleCollision()
        {
            parent.LevelFailed();  // Let parent know that it has been killed
        }

        /// <summary>
        /// Allows Digger to execute actions related to its victory, like staying still.
        /// </summary>
        /// <returns>true if finished and false otherwise</returns>
        public bool ExecuteVictory(GameTime gameTime)
        {
            MovementCondition = MovementCondition.Still;  // Digger will stop moving but continues enabled
            return true; 
        }

        /// <summary>
        /// Allows Digger to execute actions related to its death, like staying still, playing death sound effect and reducing number of lives.
        /// This is a staged asynchronous method which will be invoked more than once due to having to wait while music is playing,
        /// therefore, it returns a boolean to indicate if it has finished or still needs to be invoked at least once again
        /// </summary>
        /// <returns>true if finished and false otherwise</returns>
        public bool ExecuteDeath(GameTime gameTime)
        {
            int duration = diggerKilledSFX.Duration.Seconds;

            if (waitingForSongToFinish)
            {
                deathWaitingTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (deathWaitingTimer >= duration)
                {
                    waitingForSongToFinish = false;
                    texture = Game.Content.Load<Texture2D>(TEXTURE_DIGGER_DEAD1);
                    MovementCondition = MovementCondition.Dead;
                    return true; // ExecuteDeath is finished
                }
                else
                    return false; // Still waiting for song to finish
                
            }
            else
            {
                spriteEffects = SpriteEffects.None;
                texture = Game.Content.Load<Texture2D>(TEXTURE_DIGGER_HIT);
                MovementCondition = MovementCondition.Still;  // Digger will stop moving but continues enabled
                NumberOfLives--; // One life is lost
                deathWaitingTimer = 0;
                diggerKilledSFX.Play();
                waitingForSongToFinish = true;
                return false; // Waiting for song to finish
            }
        }

        /// <summary>
        /// Digger will start a new attempt in the current level
        /// </summary>
        public void GetReadyToStartLevel()
        {
            // Set texture and position to original values for current level
            texture = Game.Content.Load<Texture2D>(TEXTURE_DIGGER);
            BoundingRectangle = new Rectangle(startCoordinateX, startCoordinateY,
                                    grid.CellWidth - 2 * RECTANGLE_OFFSET, grid.CellHeight - 2 * RECTANGLE_OFFSET);

            // Enable Digger to move again
            MovementCondition = MovementCondition.MovingByKeyboard;

            currentFrame = 0;
        }

        /// <summary>
        /// Resets number of lives to default value
        /// </summary>
        public void ResetNumberOfLives()
        {
            NumberOfLives = DEFAULT_DIGGER_LIVES;
        }
    }
}

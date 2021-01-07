using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Digger
{
    /// <summary>
    /// Enumerator type BulletState, used to store the current status of Bullet
    /// </summary>
    public enum BulletState
    {
        Fire,
        Explosion
    }

    public class Bullet : MovingEntity, ICollidable
    {
        public const double FRAME_DURATION = 0.1;
        public const int BULLET_SIDE_SIZE = 30;  // Size of each side of Bullet's bounding rectangle (square in fact)
        public const int BULLET_DRAWORDER = 15;
        public const int BULLET_SPEED = 8;

        public const string BULLET_FIRE1_TEXTURE_NAME = "VFIRE1";      // Texture for fire status - 1
        public const string BULLET_FIRE2_TEXTURE_NAME = "VFIRE2";      // Texture for fire status - 2
        public const string BULLET_FIRE3_TEXTURE_NAME = "VFIRE3";      // Texture for fire status - 3
        public const string BULLET_EXPL1_TEXTURE_NAME = "VEXP1";      // Texture for explosion status - 1
        public const string BULLET_EXPL2_TEXTURE_NAME = "VEXP2";      // Texture for explosion status - 1
        public const string BULLET_EXPL3_TEXTURE_NAME = "VEXP3";      // Texture for explosion status - 1

        // Sound effects
        public const string SFX_FIRING = "firingLong";
        public const string SFX_EXPLOSION = "enemyHitByBullet";

        static Dictionary<BulletState, List<Texture2D>> sourceFrames;
        BulletState state;  // Current state of bullet

        private static SoundEffectInstance fireSFX;   // Sound effect to be played when a bullet is fired
        private static SoundEffectInstance explosionSFX;  // Sound effect to be played when enemy is hit by bullet
        bool firingSFXPlayed;

        // Frame variables
        int currentFrame;
        double frameTimer;

        public Rectangle CollisionBox => BoundingRectangle;

        public Bullet(Game game, int coordX, int coordY) : base(game)
        {
            state = BulletState.Fire;
            currentFrame = 0;
            spriteEffects = SpriteEffects.None;
            DrawOrder = BULLET_DRAWORDER;
            Speed = BULLET_SPEED;
            frameTimer = 0;
            firingSFXPlayed = false;

            // Set the coordinates
            startCoordinateX = coordX;
            startCoordinateY = coordY;

            startCoordinateX = MathHelper.Clamp(startCoordinateX, grid.OriginX, grid.OriginX + grid.Width - BULLET_SIDE_SIZE);
            startCoordinateY = MathHelper.Clamp(startCoordinateY, grid.OriginY, grid.OriginY + grid.Height - BULLET_SIDE_SIZE);

            BoundingRectangle = new Rectangle(startCoordinateX, startCoordinateY, BULLET_SIDE_SIZE, BULLET_SIDE_SIZE);
        }

        protected override void LoadContent()
        {
            if (sourceFrames == null)
            {
                sourceFrames = new Dictionary<BulletState, List<Texture2D>>();

                // Load the dictionary with the corresponding textures
                sourceFrames.Add(BulletState.Fire, new List<Texture2D> { Game.Content.Load<Texture2D>(BULLET_FIRE1_TEXTURE_NAME),
                                                                     Game.Content.Load<Texture2D>(BULLET_FIRE2_TEXTURE_NAME),
                                                                     Game.Content.Load<Texture2D>(BULLET_FIRE3_TEXTURE_NAME) });

                sourceFrames.Add(BulletState.Explosion, new List<Texture2D> { Game.Content.Load<Texture2D>(BULLET_EXPL1_TEXTURE_NAME),
                                                                          Game.Content.Load<Texture2D>(BULLET_EXPL2_TEXTURE_NAME),
                                                                          Game.Content.Load<Texture2D>(BULLET_EXPL3_TEXTURE_NAME) });
            }

            // Set the initial texture
            texture = sourceFrames[BulletState.Fire][0];

            // Load sound effects
            if (fireSFX == null)
                fireSFX = Game.Content.Load<SoundEffect>(SFX_FIRING).CreateInstance();
            
            if (explosionSFX == null)
                explosionSFX = Game.Content.Load<SoundEffect>(SFX_EXPLOSION).CreateInstance();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (!firingSFXPlayed)
            {
                // Play the sound effect if it hasn't been played
                fireSFX.Play();
                firingSFXPlayed = true;
            }

            // Update frame and texture accordingly
            UpdateFrame(gameTime);
            texture = sourceFrames[state][currentFrame];

            UpdateMovement();

            CheckForCollision();

            base.Update(gameTime);
        }

        private void UpdateFrame(GameTime gameTime)
        {
            frameTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (frameTimer >= FRAME_DURATION)
            {
                frameTimer = 0;
                currentFrame++;
            }

            if (currentFrame >= sourceFrames[state].Count)
            {
                currentFrame = 0;

                // Check if state is Explosion. That means that the Bullet already hit an enemy and therefore
                // it must be removed from game components after all frames were drawn
                if (state == BulletState.Explosion)
                {
                    Game.Components.Remove(this);
                }
            }
        }

        private void UpdateMovement()
        {
            if (state == BulletState.Fire)
            {
                switch (LastMovingDirection)
                {
                    case DirectionType.Up:
                        BoundingRectangle.Y -= Speed;
                        break;
                    case DirectionType.Down:
                        BoundingRectangle.Y += Speed;
                        break;
                    case DirectionType.Left:
                        BoundingRectangle.X -= Speed;
                        break;
                    case DirectionType.Right:
                        BoundingRectangle.X += Speed;
                        break;
                    default:
                        break;
                }

                // Check if it has left the grid without hitting any enemy
                if (BoundingRectangle.X < grid.OriginX || BoundingRectangle.X > grid.OriginX + grid.Width
                    || BoundingRectangle.Y < grid.OriginY || BoundingRectangle.Y > grid.OriginY + grid.Height)
                {
                    fireSFX.Stop();
                    Game.Components.Remove(this); // Remove itself from game components
                }
            }
        }

        public void CheckForCollision()
        {
            // Check if Bullet hits an Enemy
            for (int i = 0; i < Game.Components.OfType<Enemy>().Count(); i++)
            {
                Enemy enemy = Game.Components.OfType<Enemy>().ElementAt(i);

                if (this.CollisionBox.Intersects(enemy.CollisionBox))
                {
                    enemy.HandleCollision();
                    this.HandleCollision();
                    break; // No more need to loop as bullet will also be removed
                }
            }
        }

        /// <summary>
        /// Method executed after it hit a Digger's Enemy. It updates score, plays sound effect and removes itself from Game components
        /// </summary>
        public void HandleCollision()
        {
            fireSFX.Stop();
            explosionSFX.Play();
            Game.Services.GetService<Score>().AddScore(Digger.POINTS_ENEMY_SHOT);

            // state will change to Explosion to show animation of it. After that, it must be removed from the game components
            state = BulletState.Explosion;
            currentFrame = 0;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}

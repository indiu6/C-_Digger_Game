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
    /// Enumerator type GoldBagState, used to indicate what the current movement status of goldbag is
    /// </summary>
    public enum GoldBagState
    {
        Still,
        Shaking,
        Falling,
        Gold
    }

    public class GoldBag : Entity, ICollidable
    {
        public const double FRAME_DURATION = 0.2;

        public const string GOLDBAG_STILL_TEXTURE_NAME = "VSBAG";       // Texture for when goldbag is still
        public const string GOLDBAG_LEFT_TEXTURE_NAME = "VLBAG";        // Texture for when goldbag is shaking to the left
        public const string GOLDBAG_RIGHT_TEXTURE_NAME = "VRBAG";       // Texture for when goldbag is shaking to the right
        public const string GOLDBAG_FALLING_TEXTURE_NAME = "VFBAG";     // Texture for when goldbag is falling
        public const string GOLDBAG_GOLD1_TEXTURE_NAME = "VGOLD1";      // Texture for when goldbag became gold (1)
        public const string GOLDBAG_GOLD2_TEXTURE_NAME = "VGOLD2";      // Texture for when goldbag became gold (2)
        public const string GOLDBAG_GOLD3_TEXTURE_NAME = "VGOLD3";      // Texture for when goldbag became gold (3)

        // What percentage the Emerald's BoundingRectangle will be with respect to containing cell's rectangle 
        public const float CELL_RECTANGLE_PERCENTAGE = 0.70f;
        public const int HEIGHT_DELTA = 8;  // Bounding rectangle height delta for adjustment

        public const int GOLDBAG_DRAWORDER = 2;
        public const int SHAKING_DURATION = 2;  // Goldbag will shake during 2 seconds when it detects cell below it has been dug
        public const int FALLING_SPEED = 8;   // Speed when goldbag is falling

        // Sound effects
        public const string SFX_SHAKING = "goldbagShaking";
        public const string SFX_FALLING = "goldbagFallingLong";
        public const string SFX_FELL = "enemyHitByBullet";
        public const string SFX_EATEN = "eatMoney";

        private static SoundEffectInstance goldShakingSFX;   // Sound effect to be played when gold bag is shaking
        private static SoundEffectInstance goldFallingSFX;   // Sound effect to be played when gold bag is falling
        private static SoundEffectInstance goldFellSFX;     // Sound effect to be played when gold bag just fell
        private static SoundEffectInstance goldEatenSFX;   // Sound effect to be played when gold has been eaten

        // Dictionary for CellContentType textures
        Dictionary<GoldBagState, List<Texture2D>> sourceFrames;

        public GoldBagState State { get; private set; } // Current state of gold bag

        protected DebugInfo debugger { get => Game.Services.GetService<DebugInfo>(); }  // A shortcut to access the Game debugger

        public int Speed { get; private set; }  // Speed of the falling bag

        public Rectangle CollisionBox => BoundingRectangle;

        // Frame variables
        private int currentFrame;
        private double frameTimer;

        private double shakingTimer;  // Timer for the duration of Shaking State
        private bool stopOnCurrentCell; // Flag to be used when falling. Will indicate if GoldBag must stop falling on current cell

        public GoldBag(Game game, Rectangle cellBoundingRectangle)
            : base(game, cellBoundingRectangle, CELL_RECTANGLE_PERCENTAGE)
        {
            DrawOrder = GOLDBAG_DRAWORDER;
            State = GoldBagState.Still;

            currentFrame = 0;
            spriteEffects = SpriteEffects.None;
            Speed = FALLING_SPEED;
            frameTimer = 0;
            stopOnCurrentCell = false;

            // Adjust bounding rectangle with the bottom of the containing cell
            BoundingRectangle.Height += (cellBoundingRectangle.Height - BoundingRectangle.Height) / 2 + HEIGHT_DELTA;
            UpdateCurrentRowAndCol();
        }

        protected override void LoadContent()
        {
            // add content textures to dictionary 
            if (sourceFrames == null)
            {
                sourceFrames = new Dictionary<GoldBagState, List<Texture2D>>();
                // Load the dictionary with the corresponding textures
                sourceFrames.Add(GoldBagState.Still, new List<Texture2D> { Game.Content.Load<Texture2D>(GOLDBAG_STILL_TEXTURE_NAME) });

                sourceFrames.Add(GoldBagState.Shaking, new List<Texture2D> { Game.Content.Load<Texture2D>(GOLDBAG_LEFT_TEXTURE_NAME),
                                                                             Game.Content.Load<Texture2D>(GOLDBAG_RIGHT_TEXTURE_NAME) });

                sourceFrames.Add(GoldBagState.Falling, new List<Texture2D> { Game.Content.Load<Texture2D>(GOLDBAG_FALLING_TEXTURE_NAME) });

                sourceFrames.Add(GoldBagState.Gold, new List<Texture2D> { Game.Content.Load<Texture2D>(GOLDBAG_GOLD1_TEXTURE_NAME),
                                                                          Game.Content.Load<Texture2D>(GOLDBAG_GOLD2_TEXTURE_NAME),
                                                                          Game.Content.Load<Texture2D>(GOLDBAG_GOLD3_TEXTURE_NAME) });
            }

            // Set the initial texture
            texture = sourceFrames[GoldBagState.Still][0];

            // Load sound effects
            if (goldShakingSFX == null)
                goldShakingSFX = Game.Content.Load<SoundEffect>(SFX_SHAKING).CreateInstance();

            if (goldFallingSFX == null)
                goldFallingSFX = Game.Content.Load<SoundEffect>(SFX_FALLING).CreateInstance();

            if (goldFellSFX == null)
                goldFellSFX = Game.Content.Load<SoundEffect>(SFX_FELL).CreateInstance();

            if (goldEatenSFX == null)
                goldEatenSFX = Game.Content.Load<SoundEffect>(SFX_EATEN).CreateInstance();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            // Update frame and texture accordingly
            UpdateFrame(gameTime);
            texture = sourceFrames[State][currentFrame];

            UpdateState(gameTime);

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

            if (currentFrame >= sourceFrames[State].Count)
            {
                if (State != GoldBagState.Gold)
                    currentFrame = 0;  // Only reset current frame if not in the State of Gold
                else
                    currentFrame--;  // Return to last frame
            }
        }

        private void UpdateState(GameTime gameTime)
        {
            GridCell cellBelow = null; // Cell below the still or falling gold bag

            switch (State)
            {
                case GoldBagState.Still:
                    // Obtain cell below and check if it has been dug
                    cellBelow = grid.GetCellByGridCoordinates(CurrentRow + 1, CurrentCol);
                    if (cellBelow != null)
                    {
                        if ((cellBelow.DugStatus & CellDugStatus.DugFully) == CellDugStatus.DugFully
                            || (cellBelow.DugStatus & CellDugStatus.DugHorizontal) == CellDugStatus.DugHorizontal
                            || (cellBelow.DugStatus & CellDugStatus.DugVertical) == CellDugStatus.DugVertical
                            || (cellBelow.DugStatus & CellDugStatus.DugRight) == CellDugStatus.DugRight
                            || (cellBelow.DugStatus & CellDugStatus.DugLeft) == CellDugStatus.DugLeft
                            || (cellBelow.DugStatus & CellDugStatus.DugUp) == CellDugStatus.DugUp)
                        {
                            // Cell below has been dug. Start timer for shaking and change State
                            debugger.SetMessage("Dug cell below goldbag detected!");
                            goldShakingSFX.IsLooped = true;
                            goldShakingSFX.Play();
                            State = GoldBagState.Shaking;
                            currentFrame = 0;
                            shakingTimer = 0;
                        }
                    }
                    break;
                case GoldBagState.Shaking:
                    if (shakingTimer >= SHAKING_DURATION)
                    {
                        debugger.SetMessage("Goldbag will now fall");
                        
                        // Before falling, obtain current cell and set it as empty content - not to have it marked as blocking content anymore
                        GridCell currentCell = grid.GetCellByGridCoordinates(CurrentRow, CurrentCol);
                        currentCell.SetContentType(CellContentType.Nothing);

                        State = GoldBagState.Falling;
                        goldShakingSFX.Stop();
                        goldFallingSFX.Play();
                        currentFrame = 0;
                    }
                    else
                        shakingTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    break;
                case GoldBagState.Falling:
                    if (stopOnCurrentCell)
                    {
                        // Obtain current cell
                        GridCell currentCell = grid.GetCellByGridCoordinates(CurrentRow, CurrentCol);

                        // Goldbug must stop at the bottom of the current cell
                        if (BoundingRectangle.Bottom > currentCell.BoundingRectangle.Bottom - Speed)
                        {
                            // It's already on the bottom. Stop and change State
                            State = GoldBagState.Gold;
                            goldFallingSFX.Stop();
                            goldFellSFX.Play();
                            currentFrame = 0;
                            stopOnCurrentCell = false;
                        }

                    }
                    else
                    {
                        // Check if it has reached the bottom of the open path
                        // Obtain cell below and check if it has the way open
                        cellBelow = grid.GetCellByGridCoordinates(CurrentRow + 1, CurrentCol);
                        if (cellBelow == null)
                        {
                            // Falling gold bag is at the bottom of the grid
                            stopOnCurrentCell = true;
                        }
                        else
                        {
                            if ((cellBelow.DugStatus & CellDugStatus.DugFully) != CellDugStatus.DugFully
                                && (cellBelow.DugStatus & CellDugStatus.DugVertical) != CellDugStatus.DugVertical
                                && (cellBelow.DugStatus & CellDugStatus.DugHorizontal) != CellDugStatus.DugHorizontal
                                && (cellBelow.DugStatus & CellDugStatus.DugUp) != CellDugStatus.DugUp)
                            {
                                // Cell below is not dug enough for bag to continue falling. Stop on this cell
                                stopOnCurrentCell = true;
                            }
                        }
                    }
                    break;
                case GoldBagState.Gold:
                    break;
                default:
                    break;
            }
        }

        private void UpdateMovement()
        {
            if (State == GoldBagState.Falling)
            {
                BoundingRectangle.Y += Speed;
                BoundingRectangle.Y = MathHelper.Clamp(BoundingRectangle.Y, grid.OriginY, grid.OriginY + grid.Height - BoundingRectangle.Height);
            }
        }

        public void CheckForCollision()
        {

        }

        public void HandleCollision()
        {
            if (State == GoldBagState.Gold)
            {
                goldEatenSFX.Play();
                Game.Components.Remove(this); // Gold has been "eaten"
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}

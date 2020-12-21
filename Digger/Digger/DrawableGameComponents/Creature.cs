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
    /// <summary>
    /// Enumerator type MovementCondition, used to indicate what type of movement a Creature has
    /// </summary>
    public enum MovementCondition
    {
        Still = 0,              // Creature cannot move - useful when a Creature has been hit and will die
        MovingByKeyboard,       // Creature will move manually by following keyboard - example: Digger
        MovingWithRestrictions, // Creature will move automatically and along dug cells only - example: Nobbin
        MovingRandomly,         // Creature will move automatically over the whole grid (dug or not dug cells) - example: Hobbin
        Falling,                // Creature can no longer move freely and is following a falling path
        Dead                    // Creature will no longer move - it's dead
    }

    /// <summary>
    /// Creature of the game. Abstract class which inherits from MovingEntity
    /// </summary>
    public abstract class Creature : MovingEntity, ICollidable
    {
        const int DEFAULT_CENTERED_OFFSET = 5;  // Default offset to consider for determining if it's centered with respect to containing cell

        protected int centeredOffset; // Offset to consider for determining if it's centered with respect to containing cell
                                      // It's set with DEFAULT_CENTERED_OFFSET value in constructor and each instance can modify it

        public MovementCondition MovementCondition { get; protected set; }  // What movement condition the Creature has

        public bool CanDig { get; protected set; }   // If the creature can dig - default: false
        public int NumberOfLives { get; protected set; }  // How many lives the creature has - default is obtained from Resource.defaultNumberOfLives

        private static int defaultNumberOfLives;  // Default number of lives for all the creatures (static field) 
                                                  // Value is obtained from Resource file

        protected DebugInfo debugger { get => Game.Services.GetService<DebugInfo>(); }  // A shortcut to access the Game debugger

        // Getters for quick access of grid rows and columns of BoundingRectangle sides and center
        protected int currentLeftSideCol => grid.GetRectangleLeftSideColumn(BoundingRectangle);
        protected int currentRightSideCol => grid.GetRectangleRightSideColumn(BoundingRectangle);
        protected int currentTopSideRow => grid.GetRectangleTopSideRow(BoundingRectangle);
        protected int currentBottomSideRow => grid.GetRectangleBottomSideRow(BoundingRectangle);
        protected int currentCenterRow => grid.FindRow(BoundingRectangle.Center.Y);
        protected int currentCenterCol => grid.FindColumn(BoundingRectangle.Center.X);

        // Getters for quick access to some other useful boolean methods from the Grid
        protected bool isHorizontallyCentered => grid.IsHorizontallyCentered(BoundingRectangle.Center.X, centeredOffset);
        protected bool isVerticallyCentered => grid.IsVerticallyCentered(BoundingRectangle.Center.Y, centeredOffset);

        public Rectangle CollisionBox => BoundingRectangle; // This could be reset by the child classes if they want a different collision box

        static Random random;  // Used to randomly determine the way a creature will follow when does not follow user input

        // Fields to control movement of Creature which doesn't follow user input 
        private GridCell previousGridCell;  // Previous cell of creature
        protected bool forcedToKeepDirection;   // If creature must keep its same direction 

        // fields useful for Creatures which can dig and do not follow Keyboard input
        private int cellsToGoInSameDirection; // How many cells creature must go in the same direction - will be set randomly
        private CellLocation targetCellLocation;  // Where the creature is targeting to go


        // Constructor with default values
        public Creature(Game game) : this(game, MovementCondition.Still, false, 0) { }  // 0 to use the default number of lives

        /// <summary>
        /// Constructor for Creature which sets the basic class properties
        /// </summary>
        /// <param name="game">Game object. This parameter is passed to the base class</param>
        /// <param name="movementCondition">MovementCondition enum to indicate how the creature will move</param>
        /// <param name="canDig">boolean to indicate whether the creature can dig</param>
        /// <param name="numberOfLives">number of lives. If 0, the defaul number of lives will be used</param>
        public Creature(Game game, MovementCondition movementCondition, bool canDig, int numberOfLives) : base(game)
        {
            MovementCondition = movementCondition;
            CanDig = canDig;
            centeredOffset = DEFAULT_CENTERED_OFFSET;

            // Initialization of static field
            if (defaultNumberOfLives <= 0)
            {
                defaultNumberOfLives = int.TryParse(Resource.defaultNumberOfLives, out int value) ? value : 1;
            }

            NumberOfLives = numberOfLives > 0 ? numberOfLives : defaultNumberOfLives;

            cellsToGoInSameDirection = 0;
            forcedToKeepDirection = false;

            if (random == null)
                random = new Random();
        }

        public override void Update(GameTime gameTime)
        {
            // Copy BoundingRectangle to save current screen position before it is updated
            Rectangle currentRectangle = new Rectangle(BoundingRectangle.X, BoundingRectangle.Y,
                                                        BoundingRectangle.Width, BoundingRectangle.Height);

            // Grid cell corresponding to creature's current position
            GridCell currentCell = grid.GetCellByGridCoordinates(currentCenterRow, currentCenterCol);

            switch (MovementCondition)
            {
                case MovementCondition.Still:
                    break;
                case MovementCondition.MovingByKeyboard:
                    UpdateUserInput();  // A Digger will use this method
                    break;
                case MovementCondition.MovingWithRestrictions:
                    UpdateRestrictedPath();  // A Nobbin will use this method
                    break;
                case MovementCondition.MovingRandomly:
                    UpdateRandomPath();  // A Hobbin will use this method
                    break;
                case MovementCondition.Falling:
                    break;
                case MovementCondition.Dead:
                    break;
                default:
                    break;
            }

            if (CanDig)
            {
                // At this point, position of creature has already been updated by the previous methods. Therefore, 
                // currentRectangle and currentCell are now related to the Creature's previous position 
                UpdateDigging(currentRectangle, currentCell); // arguments of the Creature's previous position (see previous comment)
            }

            base.Update(gameTime);
        }

        protected void UpdateUserInput()
        {
            SetVerticalDirection(DirectionType.None);
            SetHorizontalDirection(DirectionType.None);

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Up))
            {
                if (grid.IsHorizontallyCentered(BoundingRectangle.Center.X))
                {
                    SetVerticalDirection(DirectionType.Up);
                    BoundingRectangle.Y -= Speed;
                }
                SetLastMovingDirection(DirectionType.Up);
            }
            else if (ks.IsKeyDown(Keys.Down))
            {
                if (grid.IsHorizontallyCentered(BoundingRectangle.Center.X))
                {
                    SetVerticalDirection(DirectionType.Down);
                    BoundingRectangle.Y += Speed;
                }
                SetLastMovingDirection(DirectionType.Down);
            }

            if (ks.IsKeyDown(Keys.Left))
            {
                if (grid.IsVerticallyCentered(BoundingRectangle.Center.Y))
                {
                    SetHorizontalDirection(DirectionType.Left);
                    BoundingRectangle.X -= Speed;
                }
                SetLastMovingDirection(DirectionType.Left);
                spriteEffects = SpriteEffects.None;
            }
            else if (ks.IsKeyDown(Keys.Right))
            {
                if (grid.IsVerticallyCentered(BoundingRectangle.Center.Y))
                {
                    SetHorizontalDirection(DirectionType.Right);
                    BoundingRectangle.X += Speed;
                }
                SetLastMovingDirection(DirectionType.Right);
                spriteEffects = SpriteEffects.FlipHorizontally;
            }

            BoundingRectangle.X = MathHelper.Clamp(BoundingRectangle.X, grid.OriginX, grid.OriginX + grid.Width - BoundingRectangle.Width);
            BoundingRectangle.Y = MathHelper.Clamp(BoundingRectangle.Y, grid.OriginY, grid.OriginY + grid.Height - BoundingRectangle.Height);
        }

        protected void UpdateDigging(Rectangle previousRectangle, GridCell previousCell)
        {
            GridCell newCell;  // Will hold the grid cell corresponding to the new Creature's position

            int previousLeftSideCol = grid.GetRectangleLeftSideColumn(previousRectangle);
            int previousRightSideCol = grid.GetRectangleRightSideColumn(previousRectangle);
            int previousTopSideRow = grid.GetRectangleTopSideRow(previousRectangle);
            int previousBottomSideRow = grid.GetRectangleBottomSideRow(previousRectangle);

            // If Creature is not following user input, Vertical/Horizontal direction is updated based on LastMovingDirection
            if (MovementCondition != MovementCondition.MovingByKeyboard)
            {
                switch (LastMovingDirection)
                {
                    case DirectionType.Up:
                    case DirectionType.Down:
                        SetVerticalDirection(LastMovingDirection);
                        break;
                    case DirectionType.Left:
                    case DirectionType.Right:
                        SetHorizontalDirection(LastMovingDirection);
                        break;
                    default:
                        break;
                }
            }

            // Check if a new GridCell has been entered and if it does not contain a blocking object
            switch (VerticalDirection)
            {
                case DirectionType.Up:
                    int newTopSideRow = grid.GetRectangleTopSideRow(BoundingRectangle);
                    newCell = grid.GetCellByGridCoordinates(newTopSideRow, currentCenterCol);
                    if (newTopSideRow != previousTopSideRow) // New cell has been accessed
                    {
                        if (newCell.ContainsBlockingObject)
                            BoundingRectangle.Y += Speed;  // Creature can't pass
                        else
                        {
                            previousCell.AddDugStatus(CellDugStatus.DugUp);  // The current cell has been dugged up
                            newCell.AddDugStatus(CellDugStatus.DugDown); // Creature is going up so the new cell has been dug down
                        }
                    }
                    else
                    {
                        // Still in the same cell. Check if top half of the cell has been reached
                        if (BoundingRectangle.Top <= newCell.BoundingRectangle.Center.Y)
                        {
                            newCell.AddDugStatus(CellDugStatus.DugVertical); // Cell has been fully dug vertically
                        }
                    }
                    break;
                case DirectionType.Down:
                    int newBottomSideRow = grid.GetRectangleBottomSideRow(BoundingRectangle);
                    newCell = grid.GetCellByGridCoordinates(newBottomSideRow, currentCenterCol);
                    if (newBottomSideRow != previousBottomSideRow) // New cell has been accessed
                    {
                        if (newCell.ContainsBlockingObject)
                            BoundingRectangle.Y -= Speed;  // Creature can't pass
                        else
                        {
                            previousCell.AddDugStatus(CellDugStatus.DugDown);  // The current cell has been dugged down
                            newCell.AddDugStatus(CellDugStatus.DugUp); // Creature is going down so the new cell has been dug up
                        }
                    }
                    else
                    {
                        // Still in the same cell. Check if bottom half of the cell has been reached
                        if (BoundingRectangle.Bottom >= newCell.BoundingRectangle.Center.Y)
                        {
                            newCell.AddDugStatus(CellDugStatus.DugVertical); // Cell has been fully dug vertically
                        }
                    }
                    break;
                default:
                    break;
            }
            switch (HorizontalDirection)
            {
                case DirectionType.Right:
                    int newRightSideCol = grid.GetRectangleRightSideColumn(BoundingRectangle);
                    newCell = grid.GetCellByGridCoordinates(currentCenterRow, newRightSideCol);
                    if (newRightSideCol != previousRightSideCol) // New cell has been accessed
                    {
                        if (newCell.ContainsBlockingObject)
                            BoundingRectangle.X -= Speed;  // Creature can't pass
                        else
                        {
                            previousCell.AddDugStatus(CellDugStatus.DugRight);  // The current cell has been dugged right
                            newCell.AddDugStatus(CellDugStatus.DugLeft); // Creature is going right so the new cell has been dug left
                        }
                    }
                    else
                    {
                        // Still in the same cell. Check if right half of the cell has been reached
                        if (BoundingRectangle.Right >= newCell.BoundingRectangle.Center.Y)
                        {
                            newCell.AddDugStatus(CellDugStatus.DugHorizontal); // Cell has been fully dug horizontally
                        }
                    }
                    break;
                case DirectionType.Left:
                    int newLeftSideCol = grid.GetRectangleLeftSideColumn(BoundingRectangle);
                    newCell = grid.GetCellByGridCoordinates(currentCenterRow, newLeftSideCol);
                    if (newLeftSideCol != previousLeftSideCol) // New cell has been accessed
                    {
                        if (newCell.ContainsBlockingObject)
                            BoundingRectangle.X += Speed;  // Creature can't pass
                        else
                        {
                            previousCell.AddDugStatus(CellDugStatus.DugLeft);  // The current cell has been dugged left
                            newCell.AddDugStatus(CellDugStatus.DugRight); // Creature is going left so the new cell has been dug right
                        }
                    }
                    else
                    {
                        // Still in the same cell. Check if left half of the cell has been reached
                        if (BoundingRectangle.Left <= newCell.BoundingRectangle.Center.Y)
                        {
                            newCell.AddDugStatus(CellDugStatus.DugHorizontal); // Cell has been fully dug horizontally
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        protected void UpdateRandomPath()
        {
            // Check if creature must set a new path
            if (cellsToGoInSameDirection <= 0)
            {
                // Determine how many cells are on the way based on how many non blocking cells exist following LastMovingDirection
                int maxCellsInDefinedDirection = grid.CountNonBlockingCells(currentCenterRow, currentCenterCol, LastMovingDirection);

                if (maxCellsInDefinedDirection > 0)
                {
                    // Set a random number of cells to go between 1 and maxCellsInDefinedDirection (including)
                    cellsToGoInSameDirection = random.Next(1, maxCellsInDefinedDirection + 1);
                    // Update target cell based on how many cells Creature will go
                    switch (LastMovingDirection)
                    {
                        case DirectionType.Up:
                            targetCellLocation = new CellLocation(CurrentRow - cellsToGoInSameDirection, CurrentCol);
                            break;
                        case DirectionType.Down:
                            targetCellLocation = new CellLocation(CurrentRow + cellsToGoInSameDirection, CurrentCol);
                            break;
                        case DirectionType.Left:
                            targetCellLocation = new CellLocation(CurrentRow, CurrentCol - cellsToGoInSameDirection);
                            break;
                        case DirectionType.Right:
                            targetCellLocation = new CellLocation(CurrentRow, CurrentCol + cellsToGoInSameDirection);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // Chaange direction

                    List<DirectionType> possibleDirections = new List<DirectionType>()
                    {
                        DirectionType.Left, DirectionType.Right, DirectionType.Up, DirectionType.Down
                    };
                    possibleDirections.Remove(LastMovingDirection);
                    LastMovingDirection = PickRandomDirection(possibleDirections);
                    return;
                }
            }

            // Save row and column of cell where Creature is currently in
            int previousRow = CurrentRow;
            int previousCol = CurrentCol;

            // Update position based on LastMovingDirection, adjust it to grid boundaries and check if a new cell has been reached
            switch (LastMovingDirection)
            {
                case DirectionType.Left:
                    BoundingRectangle.X -= Speed;
                    break;
                case DirectionType.Right:
                    BoundingRectangle.X += Speed;
                    break;
                case DirectionType.Up:
                    BoundingRectangle.Y -= Speed;
                    break;
                case DirectionType.Down:
                    BoundingRectangle.Y += Speed;
                    break;
                default:
                    break;
            }
            BoundingRectangle.X = MathHelper.Clamp(BoundingRectangle.X, grid.OriginX, grid.OriginX + grid.Width - BoundingRectangle.Width);
            BoundingRectangle.Y = MathHelper.Clamp(BoundingRectangle.Y, grid.OriginY, grid.OriginY + grid.Height - BoundingRectangle.Height);

            // Check if Creature has reached target cell
            if (CurrentRow == targetCellLocation.Row && CurrentCol == targetCellLocation.Col)
            {
                debugger.SetMessage("Nobbin arrived!");
                
                // Obtain the grid cell the Creature is currently in
                GridCell currentGridCell = grid.GetCellByScreenCoordinates(BoundingRectangle.Center.X, BoundingRectangle.Center.Y);

                List<DirectionType> possibleDirections = new List<DirectionType>();

                // Keep going until Creature has reached or passed the middle of current cell and then pick a new direction
                switch (LastMovingDirection)
                {
                    case DirectionType.Left:
                        // See if its center already passed the vertical center of its current grid cell (heading left)
                        if (BoundingRectangle.Center.X <= currentGridCell.BoundingRectangle.Center.X)
                        {
                            //possibleDirections.Add(DirectionType.Right);
                            possibleDirections.Add(DirectionType.Up);
                            possibleDirections.Add(DirectionType.Down);
                            LastMovingDirection = PickRandomDirection(possibleDirections);
                            cellsToGoInSameDirection = 0;
                        }
                        break;
                    case DirectionType.Right:
                        // See if its center already passed the vertical center of its current grid cell (heading right)
                        if (BoundingRectangle.Center.X >= currentGridCell.BoundingRectangle.Center.X)
                        {
                            //possibleDirections.Add(DirectionType.Left);
                            possibleDirections.Add(DirectionType.Up);
                            possibleDirections.Add(DirectionType.Down);
                            LastMovingDirection = PickRandomDirection(possibleDirections);
                            cellsToGoInSameDirection = 0;
                        }
                        break;
                    case DirectionType.Up:
                        // See if its center already passed the horizontal center of its current grid cell (heading Up)
                        if (BoundingRectangle.Center.Y <= currentGridCell.BoundingRectangle.Center.Y)
                        {
                            possibleDirections.Add(DirectionType.Left);
                            possibleDirections.Add(DirectionType.Right);
                            //possibleDirections.Add(DirectionType.Down);
                            LastMovingDirection = PickRandomDirection(possibleDirections);
                            cellsToGoInSameDirection = 0;
                        }
                        break;
                    case DirectionType.Down:
                        // See if its center already passed the horizontal center of its current grid cell (heading down)
                        if (BoundingRectangle.Center.Y >= currentGridCell.BoundingRectangle.Center.Y)
                        {
                            possibleDirections.Add(DirectionType.Left);
                            possibleDirections.Add(DirectionType.Right);
                            //possibleDirections.Add(DirectionType.Up);
                            LastMovingDirection = PickRandomDirection(possibleDirections);
                            cellsToGoInSameDirection = 0;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        protected void UpdateRestrictedPath()
        {
            // Obtain the grid cell the Creature is currently in
            GridCell currentGridCell = grid.GetCellByScreenCoordinates(BoundingRectangle.Center.X, BoundingRectangle.Center.Y);

            switch (HorizontalDirection)
            {
                case DirectionType.Left:
                    // See if its center already passed the vertical center of its current grid cell (heading left)
                    if (BoundingRectangle.Center.X <= currentGridCell.BoundingRectangle.Center.X)
                    {
                        List<DirectionType> possibleDirections = new List<DirectionType>();
                        if (CanGoLeft())
                            possibleDirections.Add(DirectionType.Left);
                        if (CanGoUp())
                            possibleDirections.Add(DirectionType.Up);
                        if (CanGoDown())
                            possibleDirections.Add(DirectionType.Down);

                        DirectionType directionToFollow;

                        if (possibleDirections.Count == 0)
                        {
                            // The Creature cannot go to any direction and it's forced to return
                            SetHorizontalDirection(DirectionType.Right);
                            forcedToKeepDirection = false; // No need for this flag as there is only one possible path
                        }
                        else
                        {
                            // If there is only one possible direction, get it directly. If more than one, choose it randomly
                            directionToFollow = possibleDirections.Count == 1 ? possibleDirections.First() : PickRandomDirection(possibleDirections);
                            if (directionToFollow == DirectionType.Left)
                                BoundingRectangle.X -= Speed;  // Just keep going
                            else
                            {
                                // Creature will turn
                                SetVerticalDirection(directionToFollow);
                                SetHorizontalDirection(DirectionType.None);
                                previousGridCell = currentGridCell; // Save the current cell as previous in order to control
                                forcedToKeepDirection = true; // Creature must keep its new direction until it reaches a new cell
                            }
                        }
                    }
                    else
                        BoundingRectangle.X -= Speed;  // Creature still has way to go within its current cell
                    break;
                case DirectionType.Right:
                    // See if its center already passed the vertical center of its current grid cell (heading right)
                    if (BoundingRectangle.Center.X >= currentGridCell.BoundingRectangle.Center.X)
                    {
                        List<DirectionType> possibleDirections = new List<DirectionType>();
                        if (CanGoRight())
                            possibleDirections.Add(DirectionType.Right);
                        if (CanGoUp())
                            possibleDirections.Add(DirectionType.Up);
                        if (CanGoDown())
                            possibleDirections.Add(DirectionType.Down);

                        DirectionType directionToFollow;

                        if (possibleDirections.Count == 0)
                        {
                            // The Creature cannot go to any direction and it's forced to return
                            SetHorizontalDirection(DirectionType.Left);
                            forcedToKeepDirection = false; // No need for this flag as there is only one possible path
                        }
                        else
                        {
                            // If there is only one possible direction, get it directly. If more than one, choose it randomly
                            directionToFollow = possibleDirections.Count == 1 ? possibleDirections.First() : PickRandomDirection(possibleDirections);
                            if (directionToFollow == DirectionType.Right)
                                BoundingRectangle.X += Speed;  // Just keep going
                            else
                            {
                                // Creature will turn
                                SetVerticalDirection(directionToFollow);
                                SetHorizontalDirection(DirectionType.None);
                                previousGridCell = currentGridCell; // Save the current cell as previous in order to control
                                forcedToKeepDirection = true; // Creature must keep its new direction until it reaches a new cell
                            }
                        }
                    }
                    else
                        BoundingRectangle.X += Speed;  // Creature still has way to go within its current cell
                    break;
                default:
                    break;
            }

            switch (VerticalDirection)
            {
                case DirectionType.Up:
                    // See if its center already passed the horizontal center of its current grid cell (heading Up)
                    if (BoundingRectangle.Center.Y <= currentGridCell.BoundingRectangle.Center.Y)
                    {
                        List<DirectionType> possibleDirections = new List<DirectionType>();
                        if (CanGoUp())
                            possibleDirections.Add(DirectionType.Up);
                        if (CanGoLeft())
                            possibleDirections.Add(DirectionType.Left);
                        if (CanGoRight())
                            possibleDirections.Add(DirectionType.Right);

                        DirectionType directionToFollow;

                        if (possibleDirections.Count == 0)
                        {
                            // The Creature cannot go to any direction and it's forced to return
                            SetVerticalDirection(DirectionType.Down);
                            forcedToKeepDirection = false; // No need for this flag as there is only one possible path
                        }
                        else
                        {
                            // If there is only one possible direction, get it directly. If more than one, choose it randomly
                            directionToFollow = possibleDirections.Count == 1 ? possibleDirections.First() : PickRandomDirection(possibleDirections);
                            if (directionToFollow == DirectionType.Up)
                                BoundingRectangle.Y -= Speed;  // Just keep going
                            else
                            {
                                // Creature will turn
                                SetHorizontalDirection(directionToFollow);
                                SetVerticalDirection(DirectionType.None);
                                previousGridCell = currentGridCell; // Save the current cell as previous in order to control
                                forcedToKeepDirection = true; // Creature must keep its new direction until it reaches a new cell
                            }
                        }
                    }
                    else
                        BoundingRectangle.Y -= Speed;  // Creature still has way to go within its current cell
                    break;
                case DirectionType.Down:
                    // See if its center already passed the horizontal center of its current grid cell (heading down)
                    if (BoundingRectangle.Center.Y >= currentGridCell.BoundingRectangle.Center.Y)
                    {
                        List<DirectionType> possibleDirections = new List<DirectionType>();
                        if (CanGoDown())
                            possibleDirections.Add(DirectionType.Down);
                        if (CanGoLeft())
                            possibleDirections.Add(DirectionType.Left);
                        if (CanGoRight())
                            possibleDirections.Add(DirectionType.Right);

                        DirectionType directionToFollow;

                        if (possibleDirections.Count == 0)
                        {
                            // The Creature cannot go to any direction and it's forced to return
                            SetVerticalDirection(DirectionType.Up);
                            forcedToKeepDirection = false; // No need for this flag as there is only one possible path
                        }
                        else
                        {
                            // If there is only one possible direction, get it directly. If more than one, choose it randomly
                            directionToFollow = possibleDirections.Count == 1 ? possibleDirections.First() : PickRandomDirection(possibleDirections);
                            if (directionToFollow == DirectionType.Down)
                                BoundingRectangle.Y += Speed;  // Just keep going
                            else
                            {
                                // Creature will turn
                                SetHorizontalDirection(directionToFollow);
                                SetVerticalDirection(DirectionType.None);
                                previousGridCell = currentGridCell; // Save the current cell as previous in order to control
                                forcedToKeepDirection = true; // Creature must keep its new direction until it reaches a new cell
                            }
                        }
                    }
                    else
                        BoundingRectangle.Y += Speed;  // Creature still has way to go within its current cell
                    break;
                default:
                    break;
            }

            BoundingRectangle.X = MathHelper.Clamp(BoundingRectangle.X, grid.OriginX, grid.OriginX + grid.Width - BoundingRectangle.Width);
            BoundingRectangle.Y = MathHelper.Clamp(BoundingRectangle.Y, grid.OriginY, grid.OriginY + grid.Height - BoundingRectangle.Height);

            // Check if new cell has been reached
            if (currentGridCell != grid.GetCellByScreenCoordinates(BoundingRectangle.Center.X, BoundingRectangle.Center.Y))
            {
                previousGridCell = currentGridCell;
                forcedToKeepDirection = false; // No need to keep current direction
            }

            return;
        }

        /// <summary>
        /// Verifies if creature can go left based on (1) its digging capacity, (2) if it's at the edge of grid, 
        /// (3) the dug status of the current and desired cells
        /// </summary>
        /// <returns>true if Creature can in fact go left and false otherwise</returns>
        public bool CanGoLeft()
        {
            if (CanDig)
                return true;  // If Creature can dig, it can always go

            // If creature must keep its current direction, Left is allowed only if it is its current direction
            if (forcedToKeepDirection && HorizontalDirection != DirectionType.Left)
                return false;

            // Obtain the grid cell the Crature is currently in
            GridCell currentGridCell = grid.GetCellByScreenCoordinates(BoundingRectangle.Center.X, BoundingRectangle.Center.Y);

            // Obtain the grid cell the creature wants to go - the next one to the left
            GridCell newGridCell = grid.GetCellByGridCoordinates(currentCenterRow, currentCenterCol - 1);

            if (newGridCell == null)
                return false; // Creature is at the edge of the grid and cannot go further

            // Return if current cell is dug left and if next cell is dug right (so that Creature can go)
            return (currentGridCell.DugStatus & CellDugStatus.DugLeft) == CellDugStatus.DugLeft
                                    && (newGridCell.DugStatus & CellDugStatus.DugRight) == CellDugStatus.DugRight;
        }

        /// <summary>
        /// Verifies if creature can go right based on (1) its digging capacity, (2) if it's at the edge of grid, 
        /// (3) the dug status of the current and desired cells
        /// </summary>
        /// <returns>true if Creature can in fact go right and false otherwise</returns>
        public bool CanGoRight()
        {
            if (CanDig)
                return true;  // If Creature can dig, it can always go

            // If creature must keep its current direction, Right is allowed only if it is its current direction
            if (forcedToKeepDirection && HorizontalDirection != DirectionType.Right)
                return false;

            // Obtain the grid cell the Crature is currently in
            GridCell currentGridCell = grid.GetCellByScreenCoordinates(BoundingRectangle.Center.X, BoundingRectangle.Center.Y);

            // Obtain the grid cell the creature wants to go - the next one to the left
            GridCell newGridCell = grid.GetCellByGridCoordinates(currentCenterRow, currentCenterCol + 1);

            if (newGridCell == null)
                return false; // Creature is at the edge of the grid and cannot go further

            // Return if current cell is dug right and if next cell is dug left (so that Creature can go)
            return (currentGridCell.DugStatus & CellDugStatus.DugRight) == CellDugStatus.DugRight
                                    && (newGridCell.DugStatus & CellDugStatus.DugLeft) == CellDugStatus.DugLeft;
        }

        /// <summary>
        /// Verifies if creature can go up based on (1) its digging capacity, (2) if it's at the edge of grid, 
        /// (3) the dug status of the current and desired cells
        /// </summary>
        /// <returns>true if Creature can in fact go up and false otherwise</returns>
        public bool CanGoUp()
        {
            if (CanDig)
                return true;  // If Creature can dig, it can always go

            // If creature must keep its current direction, Up is allowed only if it is its current direction
            if (forcedToKeepDirection && VerticalDirection != DirectionType.Up)
                return false;

            // Obtain the grid cell the Crature is currently in
            GridCell currentGridCell = grid.GetCellByScreenCoordinates(BoundingRectangle.Center.X, BoundingRectangle.Center.Y);

            // Obtain the grid cell the creature wants to go - the next one up
            GridCell newGridCell = grid.GetCellByGridCoordinates(currentCenterRow - 1, currentCenterCol);

            if (newGridCell == null)
                return false; // Creature is at the edge of the grid and cannot go further

            // Return if current cell is dug up and if next cell is dug down (so that Creature can go)
            return (currentGridCell.DugStatus & CellDugStatus.DugUp) == CellDugStatus.DugUp
                                    && (newGridCell.DugStatus & CellDugStatus.DugDown) == CellDugStatus.DugDown;
        }

        /// <summary>
        /// Verifies if creature can go down based on (1) its digging capacity, (2) if it's at the edge of grid, 
        /// (3) the dug status of the current and desired cells
        /// </summary>
        /// <returns>true if Creature can in fact go down and false otherwise</returns>
        public bool CanGoDown()
        {
            if (CanDig)
                return true;  // If Creature can dig, it can always go

            // If creature must keep its current direction, Down is allowed only if it is its current direction
            if (forcedToKeepDirection && VerticalDirection != DirectionType.Down)
                return false;

            // Obtain the grid cell the Crature is currently in
            GridCell currentGridCell = grid.GetCellByScreenCoordinates(BoundingRectangle.Center.X, BoundingRectangle.Center.Y);

            // Obtain the grid cell the creature wants to go - the next one down
            GridCell newGridCell = grid.GetCellByGridCoordinates(currentCenterRow + 1, currentCenterCol);

            if (newGridCell == null)
                return false; // Creature is at the edge of the grid and cannot go further

            // Return if current cell is dug down and if next cell is dug up (so that Creature can go)
            return (currentGridCell.DugStatus & CellDugStatus.DugDown) == CellDugStatus.DugDown
                                    && (newGridCell.DugStatus & CellDugStatus.DugUp) == CellDugStatus.DugUp;
        }


        /// <summary>
        /// Gets a list of possible values of DirectionType and returns one of them randomly
        /// If the parameter list is null, it returns a random value of DirectionType
        /// </summary>
        /// <param name="possibleDirections">List of DirectionType values to choose randomly from</param>
        /// <returns>DirectionType value chosen randomly</returns>
        private DirectionType PickRandomDirection(List<DirectionType> possibleDirections = null)
        {
            if (possibleDirections == null)
            {
                // Pick a random direction among all possible values
                int numElements = Enum.GetValues(typeof(DirectionType)).Length;
                return (DirectionType)random.Next(1, numElements+1);  // start with 1 to exclude "None"
            }

            // Return on of the list direction values randomly
            return possibleDirections[random.Next(0, possibleDirections.Count)];
        }

        public virtual void HandleCollision()
        {
            // To be implemented by each child class
        }
    }
}

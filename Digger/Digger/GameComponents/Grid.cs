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
    public class Grid : GameComponent
    {
        // Maximum and minimum number of rows and columns allowed
        public const int MAXIMUM_NUMBER_OF_GRID_ROWS = 10;
        public const int MAXIMUM_NUMBER_OF_GRID_COLUMNS = 15;
        public const int MINIMUM_NUMBER_OF_GRID_ROWS = 5;
        public const int MINIMUM_NUMBER_OF_GRID_COLUMNS = 8;

        const int MARGIN_TOP = 60;
        const int MARGIN_BOTTOM = 20;
        const int MARGIN_SIDES = 40;

        const int DEFAULT_OFFSET = 6;   // How many pixels on each side of the center will be considered by default
                                        // for the HorizontallyCentered and VerticallyCentered methods

        public int CellWidth { get; private set; }  // Width of the cell (or cell)
        public int CellHeight { get; private set; } // Height of the cell (or cell)

        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public int OriginX { get; private set; }  // X coordinate of grid origin on screen (0 plus margin)
        public int OriginY { get; private set; }  // Y coordinate of grid origin on screen (0 plus margin)

        public int Width { get => Cols * CellWidth; }   // Width of the grid
        public int Height { get => Rows * CellHeight; }  // Height of the grid

        private LevelDesign gridDesign;  // Design of the grid 

        public GridCell[,] GridCell; // The cells of the grid

        GameScene parent;  // The scene that created this grid

        public Grid(Game game, LevelDesign levelDesign, GameScene parent) : base(game)
        {
            if (levelDesign == null)
            {
                throw new ArgumentException("A level design cannot be null when creating Grid");
            }

            gridDesign = levelDesign;  // Assign the parameter as the design of the grid

            // Set the number of rows and columns from the level design but validating value is within the allowed range
            Rows = gridDesign.NumberOfRows < MINIMUM_NUMBER_OF_GRID_ROWS ? MINIMUM_NUMBER_OF_GRID_ROWS :
                    (gridDesign.NumberOfRows > MAXIMUM_NUMBER_OF_GRID_ROWS) ? MAXIMUM_NUMBER_OF_GRID_ROWS : gridDesign.NumberOfRows;
            
            Cols = gridDesign.NumberOfColumns < MINIMUM_NUMBER_OF_GRID_COLUMNS ? MINIMUM_NUMBER_OF_GRID_COLUMNS :
                    (gridDesign.NumberOfColumns > MAXIMUM_NUMBER_OF_GRID_COLUMNS) ? MAXIMUM_NUMBER_OF_GRID_COLUMNS : gridDesign.NumberOfColumns;

            // Set the origin screen coordinates of the grid considering the defined margins
            OriginX = MARGIN_SIDES;
            OriginY = MARGIN_TOP;

            // Determine the widht and height of the grid cells
            CellWidth = (Game.GraphicsDevice.Viewport.Width - MARGIN_SIDES * 2) / Cols;
            CellHeight = (Game.GraphicsDevice.Viewport.Height - MARGIN_TOP - MARGIN_BOTTOM) / Rows;

            Enabled = false;
            this.parent = parent;
        }

        public override void Initialize()
        {
            GridCell = new GridCell[Rows, Cols];  // Create array of cells for the grid

            // Initialize all cells as being Intact (no dug) and containing nothing and add them to the game components
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    GetCellCoordinates(row, col, out int coordX, out int coordY);
                    GridCell[row, col] = new GridCell(Game, coordX, coordY, CellDugStatus.Intact, CellContentType.Nothing, this);
                    GridCell[row, col].Enabled = false;
                    GridCell[row, col].Visible = false;
                    parent.AddComponent(GridCell[row, col]);
                }
            }

            // Set the cells according to the definition of gridDesign
            foreach (GridCellDefinition cellDefinition in gridDesign.GridCellDefinitions)
            {
                GridCell gridCell = GridCell[cellDefinition.Location.Row, cellDefinition.Location.Col];
                gridCell.SetDugStatus(cellDefinition.DugStatus);
                gridCell.SetContentType(cellDefinition.ContentType);
            }
        }

        /// <summary>
        /// Method which checks if the given coordinate X is horizontally centered with respect to the grid cell it is in.
        /// It considers an offset given by the OFFSET constant.
        /// This method is intended to help a drawable object know if it can allow for movement within the vertical line of its current grid cell
        /// Only objects that are "horizontally centered" with respect to their grid cell can move vertically
        /// </summary>
        /// <param name="coordinateX">X coordiate to be evaluated</param>
        /// <returns>true if the coordinate is horizontally centered and false otherwise</returns>
        public bool IsHorizontallyCentered(int coordinateX, int offset = DEFAULT_OFFSET)
        {
            int currentColumn = FindColumn(coordinateX);

            if (currentColumn < 0) // coordinateX is invalid
            {
                return false;
            }

            double currentColumnCenterX = OriginX + (currentColumn + 0.5) * CellWidth; // Central X coordinate of currentColumn

            // Check if coordinateX is around columnCenterX within the OFFSET and return accordingly
            return coordinateX >= currentColumnCenterX - offset && coordinateX <= currentColumnCenterX + offset;
        }

        /// <summary>
        /// Determines which column of the grid contains a given X coordinate
        /// </summary>
        /// <param name="coordinateX">X coordinate to use for determining current column</param>
        /// <returns>column number, starting with 0 for the first column, or -1 if coordinateX is not valid</returns>
        public int FindColumn(int coordinateX)
        {
            if (coordinateX < OriginX || coordinateX > Game.GraphicsDevice.Viewport.Width - MARGIN_SIDES)
            {
                return -1; // coordinateX is invalid
            }

            int column;
            for (column = 0; column < Cols; column++)
            {
                if (coordinateX <= OriginX + (column + 1) * CellWidth)
                {
                    return column;
                }
            }

            return -1; // This part of the code should not be reached based on the initial validation
        }


        /// <summary>
        /// Method which checks if the given coordinate Y is vertically centered with respect to the grid cell it is in.
        /// It considers an offset given by the OFFSET constant
        /// This method is intended to help a drawable object know if it can allow for movement within the horizontal line of its current grid cell
        /// Only objects that are "vertically centered" with respect to their grid cell can move horizontally
        /// </summary>
        /// <param name="coordinateX">X coordiate to be evaluated</param>
        /// <returns>true if the coordinate is horizontally centered and false otherwise</returns>
        public bool IsVerticallyCentered(int coordinateY, int offset = DEFAULT_OFFSET)
        {
            int currentRow = FindRow(coordinateY);

            if (currentRow < 0) // coordinateY is invalid
            {
                return false;
            }

            double currentRowCenterY = OriginY + (currentRow + 0.5) * CellHeight; // Y coordinate of currentRow center

            // Check if coordinateY is around columnCenterY within the OFFSET and return accordingly
            return coordinateY >= currentRowCenterY - offset && coordinateY <= currentRowCenterY + offset;
        }

        /// <summary>
        /// Determines which row of the grid contains a given Y coordinate
        /// </summary>
        /// <param name="coordinateY">Y coordinate to use for determining current row</param>
        /// <returns>row number, starting with 0 for the first row, or -1 if coordinateY is not valid</returns>
        public int FindRow(int coordinateY)
        {
            if (coordinateY < OriginY || coordinateY > Game.GraphicsDevice.Viewport.Height - MARGIN_BOTTOM)
            {
                return -1; // coordinateY is invalid
            }

            int row;
            for (row = 0; row < Rows; row++)
            {
                if (coordinateY <= OriginY + (row + 1) * CellHeight)
                {
                    return row;
                }
            }

            return -1; // This part of the code should not be reached based on the initial validation
        }

        /// <summary>
        /// Adds a component into the list of components of parent scene 
        /// </summary>
        /// <param name="componentToAdd">component to be added</param>
        public void AddCellContentToScene(GameComponent componentToAdd)
        {
            parent.AddComponent(componentToAdd);
        }

        /// <summary>
        /// Returns the upper left coordinates of the grid cell corresponding to the row and col parameters
        /// The coordinates are returned in the coordX and coordY parameters which are passed by reference
        /// </summary>
        /// <param name="row">row of cell for which Y coordinate is requested</param>
        /// <param name="col">col of cell for which X coordinate is requested</param>
        /// <param name="coordX">reference parameter to store the resulting X coordinate, or -1 if the col parameter is invalid</param>
        /// <param name="coordY">reference parameter to store the resulting Y coordinate, or -1 if the row parameter is invalid</param>
        public void GetCellCoordinates(int row, int col, out int coordX, out int coordY)
        {
            coordX = (col >= 0 && col < Cols) ? OriginX + col * CellWidth : -1;
            coordY = (row >= 0 && row < Rows) ? OriginY + row * CellHeight : -1;
        }

        /// <summary>
        /// Returns the grid cell which corresponds to the grid coordinates specified by the parameters
        /// </summary>
        /// <param name="row">row of desired cell</param>
        /// <param name="col">column of desired cell</param>
        /// <returns></returns>
        public GridCell GetCellByGridCoordinates(int row, int col)
        {
            return (row >= 0 && row < Rows && col >= 0 && col < Cols) ? GridCell[row, col] : null;
        }

        /// <summary>
        /// Returns the grid cell which corresponds to the screen coordinates specified by the parameters
        /// </summary>
        /// <param name="x">x coordinate of desired cell</param>    
        /// <param name="y">y coordinate of desired cell</param>
        /// <returns>the cell if found or null otherwise</returns>
        public GridCell GetCellByScreenCoordinates(int x, int y)
        {
            int row = FindRow(y);
            int col = FindColumn(x);

            return GetCellByGridCoordinates(row, col);
        }

        /// <summary>
        /// The following are 4 utility methods which obtain:
        /// - The grid column corresponding to the right side of a rectangle (GetRectangleRightSideColumn)
        /// - The grid column corresponding to the left side of a rectangle (GetRectangleLeftSideColumn)
        /// - The grid row corresponding to the top side of a rectangle (GetRectangleTopSideRow)
        /// - The grid row corresponding to the bottom side of a rectangle (GetBottomSideRow)
        /// </summary>
        /// <param name="rectangle">rectangle which specified side is used to obtain the desired row or column</param>
        /// <returns>the desired row or column of the grid</returns>
        public int GetRectangleRightSideColumn (Rectangle rectangle) => FindColumn(rectangle.Right);
        public int GetRectangleLeftSideColumn(Rectangle rectangle) => FindColumn(rectangle.Left);
        public int GetRectangleTopSideRow(Rectangle rectangle) => FindRow(rectangle.Top);
        public int GetRectangleBottomSideRow(Rectangle rectangle) => FindRow(rectangle.Bottom);

        /// <summary>
        /// Utility method which obtains the grid row and column of the center of a rectangle
        /// </summary>
        /// <param name="rectangle">rectangle which center is used to obtain the row and column</param>
        /// <returns>CellLocation struct containing the grid row and column corresponding to the center of the rectangle</returns>
        public CellLocation GetRectangleCenterGridPosition(Rectangle rectangle)
        {
            int row = FindRow(rectangle.Center.Y);
            int col = FindColumn(rectangle.Center.X);

            if (row < 0 || col < 0)
                return new CellLocation(0, 0);

            return new CellLocation(row, col);
        }

        /// <summary>
        /// Utility method which returns how many non-blocking cells are in the way of cell in (row, col) following the specified direction.
        /// A blocking cell is the one which does not contain a blocking object
        /// The count stops when a blocking cell is found or when the edge of the grid is reached
        /// </summary>
        /// <param name="row">row of base cell to start count</param>
        /// <param name="col">column of base cell to start count</param>
        /// <param name="direction">desired direction for the count</param>
        /// <returns>the number of the non blocking cells that exist in the specified direction</returns>
        public int CountNonBlockingCells (int row, int col, DirectionType direction)
        {
            if (direction == DirectionType.None)
                return 0;  // No direction outside of the (row,col) cell was specified

            int cellCount = 0;
            int newRow = row;
            int newCol = col;
            bool exitLoop = false;

            do
            {
                // Obtain the cell defined by (row, col) and if it's not a valid cell definition, return 0
                GridCell gridCell = GetCellByGridCoordinates(row, col);
                if (gridCell == null)
                    exitLoop = true;
                else
                {
                    // Set newRow and newCol according to the specified direction
                    switch (direction)
                    {
                        case DirectionType.Up:
                            newRow--;
                            break;
                        case DirectionType.Down:
                            newRow++;
                            break;
                        case DirectionType.Left:
                            newCol--;
                            break;
                        case DirectionType.Right:
                            newCol++;
                            break;
                        default:
                            break;
                    }
                    // Get the new grid cell based on the new coordinates (the grid performs the necessary validations)
                    gridCell = GetCellByGridCoordinates(newRow, newCol);

                    // Check if there is a new cell to go and if it does not contain a blocking object
                    if (gridCell == null)
                        exitLoop = true;
                    else
                    {
                        if (gridCell.ContainsBlockingObject)
                            exitLoop = true;
                        else
                            cellCount++;
                    }
                }

            } while (!exitLoop);

            return cellCount;
        }
    }
}

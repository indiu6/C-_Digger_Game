using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;

namespace Digger
{
    /// <summary>
    /// struct type CellLocation, used to define the location of a given cell in the grid, with row and column 
    /// </summary>
    public struct CellLocation
    {
        public int Row { get; }
        public int Col { get; }

        public CellLocation(int row, int col)
        {
            if (row < 0 || col < 0)
            {
                throw new ArgumentException("row and col must be great than or equal to 0");
            }
            this.Row = row;
            this.Col = col;
        }

        public override string ToString()
        {
            return $"{Row}, {Col}";
        }
    }

    /// <summary>
    /// struct type GridCellDefinition, used to store the complete definition of a grid cell, including its location, 
    /// its "dug status" (if it's intact or has been dug from any direction), and what it contains
    /// </summary>
    public struct GridCellDefinition
    {
        public CellLocation Location { get; }   // Location in the grid in terms of row and column
        public CellDugStatus DugStatus { get; }   // If the cell is intact or has been dug from any direction 
        public CellContentType ContentType { get;  }  // What object the cell contains

        // If only a location is provided, by default the cell will be intact and containing nothing
        public GridCellDefinition(CellLocation location) : this(location, CellDugStatus.Intact, CellContentType.Nothing) { }

        // Constructor which allows to specify all three properties for the struct
        public GridCellDefinition(CellLocation location, CellDugStatus dugStatus, CellContentType contentType)
        {
            Location = location;
            DugStatus = dugStatus;
            ContentType = contentType;
        }
    }

    /// <summary>
    /// LevelDesign class. Represents a "template" of the configuration of a level for the Digger game.
    /// There should be an instance of this class per each level of the game
    /// Each instance will store information specific to its own level:
    /// - Level number
    /// - Number of rows and columns
    /// - Start location of the Digger
    /// - Definition of the grid cells for its corresponding level (position, content, and which "dug status"
    /// </summary>
    public class LevelDesign : GameComponent
    {
        public const int DEFAULT_NOBBIN_COUNT = 4;
        public const int DEFAULT_HOBBIN_COUNT = 1;

        static string[] levelDesignFileNames = { "Level01.txt", "Level02.txt" };
        public int LevelNumber { get; private set; }   // Level number

        public int NumberOfRows { get; private set; }   // How many rows the grid of this level has
        public int NumberOfColumns { get; private set; }   // How many columns the grid of this level has

        public CellLocation DiggerStartLocation { get; private set; }   // Digger's start position in the grid
        public CellLocation EnemiesStartLocation { get; private set; }   // Enemies' start position in the grid

        public int NobbinCount { get; private set; }  // How many Nobbins in total will be generated in this level
        public int MaxSimultaneousNobbins { get; private set; }  // Up to how many Nobbins can be on the grid simultaneously 
        public int NobbinGenerationFrequency { get; private set; } // How often (in seconds) Nobbins will be generated 
        public int HobbinCount { get; private set; }  // How many Hobbins in total will be generated in this level
        public int MaxSimultaneousHobbins { get; private set; }  // Up to how many Hobbins can be on the grid simultaneously 
        public int HobbinGenerationFrequency { get; private set; } // How often (in seconds) Hobbins will be generated 

        public int CherryCount { get; private set; }  // How many Cherry in total will be generated in this level
        public int MaxSimultaneousCherries { get; private set; }  // Up to how many Cherry can be on the grid simultaneously
        public int CherryGenerationFrequency { get; private set; } // How often (in seconds) Cherry will be generated 
        public int CherryDuration { get; private set; } // How long (in seconds) Cherry will last

        public List<GridCellDefinition> GridCellDefinitions { get; private set; }   // The definition of the grid cells for this level. 
                                    // It's not necessary to have all the cells in this list. The game will initialize all cells by default 
                                    // as being Intact and containing nothing, and will apply these definitions on top of that. 
                                    // Therefore, only cells that do contain something or which has a 'dug status' different from Intact 
                                    // need to be explicitly defined and included in this list

        public bool SuccessfullyLoaded { get; private set; }   // If this level was loaded successfully and therefore it is usable

        public LevelDesign(Game game, int levelNumber) : base(game)
        {
            LevelNumber = levelNumber;
            SuccessfullyLoaded = LoadFromFile();
        }

        private bool LoadFromFile()
        {
            if (LevelNumber > levelDesignFileNames.Length)
            {
                return false;
            }

            // PENDING: to implement the loading from level configuration file. 
            // For now, some fixed values are used in the methods below to set the level design
            if (LevelNumber == 1)
            {
                LoadLevel1();
            }
            else
            {
                LoadLevel2();
            }

            return true;
        }

        private void LoadLevel1()
        {
            NumberOfRows = Grid.MAXIMUM_NUMBER_OF_GRID_ROWS;
            NumberOfColumns = Grid.MAXIMUM_NUMBER_OF_GRID_COLUMNS;

            DiggerStartLocation = new CellLocation(9, 7);
            EnemiesStartLocation = new CellLocation(0, 14);

            // Nobbin configuration
            NobbinCount = 8;  // 8 Nobbins in total will be generated in this level
            MaxSimultaneousNobbins = 4;   // Up to 4 Nobbins can be on the grid simultaneously 
            NobbinGenerationFrequency = 3;  // Nobbins will be generated every 3 seconds

            // Hobbin configuration
            HobbinCount = 0;  // 0 Hobbin in total will be generated in this level
            MaxSimultaneousHobbins = 0;   // Up to 1 Hobbin can be on the grid simultaneously 
            HobbinGenerationFrequency = 100;  // No Hobbins will be generated 

            // Cherry configuration
            CherryCount = 4;   // 3 Cherry in total will be generated in this level
            MaxSimultaneousCherries = 1;  // Up to 1 Cherry can be on the grid simultaneously 
            CherryGenerationFrequency = 45;  // Cherry will be generated every 45 seconds
            CherryDuration = 8; // Cherry will last 8 seconds

            GridCellDefinitions = new List<GridCellDefinition>()
            {
                new GridCellDefinition(new CellLocation(0, 0), CellDugStatus.DugDown, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(1, 0), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(2, 0), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(3, 0), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(4, 0), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(5, 0), CellDugStatus.DugRightUp, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(5, 1), CellDugStatus.DugLeftDown, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(6, 1), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(7, 1), CellDugStatus.DugRightUp, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(7, 2), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(7, 3), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(7, 4), CellDugStatus.DugLeftDown, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(8, 4), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 4), CellDugStatus.DugRightUp, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 5), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 6), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 7), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 8), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 9), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 10), CellDugStatus.DugLeftUp, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(8, 10), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(7, 10), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(6, 10), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(5, 10), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(4, 10), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(3, 10), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(2, 10), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(1, 10), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 10), CellDugStatus.DugRightDown, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 11), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 12), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 13), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 14), CellDugStatus.DugLeft, CellContentType.Nothing),

                // Emeralds
                new GridCellDefinition(new CellLocation(1, 3), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(1, 4), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(1, 7), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(2, 3), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(2, 4), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(2, 7), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 3), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 4), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 7), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 12), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 13), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 14), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(4, 3), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(4, 4), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(4, 7), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(4, 12), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(4, 13), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(4, 14), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(5, 3), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(5, 4), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(5, 7), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(5, 12), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(5, 13), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(5, 14), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(8, 0), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(8, 14), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(9, 0), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(9, 1), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(9, 13), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(9, 14), CellDugStatus.Intact, CellContentType.Emerald),

                // Gold bags
                new GridCellDefinition(new CellLocation(0, 4), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(1, 12), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(2, 1), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(3, 5), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(3, 8), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(6, 6), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(6, 8), CellDugStatus.Intact, CellContentType.GoldBag)
            };
        }

        private void LoadLevel2()
        {
            NumberOfRows = Grid.MAXIMUM_NUMBER_OF_GRID_ROWS;
            NumberOfColumns = Grid.MAXIMUM_NUMBER_OF_GRID_COLUMNS;

            DiggerStartLocation = new CellLocation(9, 7);
            EnemiesStartLocation = new CellLocation(0, 14);

            // Nobbin configuration
            NobbinCount = 8;  // 8 Nobbins in total will be generated in this level
            MaxSimultaneousNobbins = 4;   // Up to 4 Nobbins can be on the grid simultaneously 
            NobbinGenerationFrequency = 3;  // Nobbins will be generated every 3 seconds

            // Hobbin configuration
            HobbinCount = 1;  // 1 Hobbin in total will be generated in this level
            MaxSimultaneousHobbins = 1;   // Up to 1 Hobbin can be on the grid simultaneously 
            HobbinGenerationFrequency = 40;  // Hobbins will be generated every 40 seconds

            // Cherry configuration
            CherryCount = 2;   // 1 Cherry in total will be generated in this level
            MaxSimultaneousCherries = 1;  // Up to 1 Cherry can be on the grid simultaneously 
            CherryGenerationFrequency = 60;  // Cherry will be generated every 60 seconds
            CherryDuration = 5; // Cherry will last 30 seconds

            GridCellDefinitions = new List<GridCellDefinition>()
            {
                new GridCellDefinition(new CellLocation(0, 0), CellDugStatus.DugRight, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 1), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 2), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 3), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 4), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 5), CellDugStatus.DugLeftDown, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(1, 5), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(2, 5), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(3, 5), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(4, 5), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(5, 5), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(6, 5), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(7, 5), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(8, 5), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 5), CellDugStatus.DugRightUp, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 6), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 7), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 8), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 9), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(9, 10), CellDugStatus.DugLeftUp, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(8, 10), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(7, 10), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(6, 10), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(5, 10), CellDugStatus.DugRightDown, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(5, 11), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(5, 12), CellDugStatus.DugHorizontal, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(5, 13), CellDugStatus.DugLeftUp, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(4, 13), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(3, 13), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(2, 13), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(1, 13), CellDugStatus.DugVertical, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 13), CellDugStatus.DugRightDown, CellContentType.Nothing),
                new GridCellDefinition(new CellLocation(0, 14), CellDugStatus.DugLeft, CellContentType.Nothing),    

                // Emeralds
                new GridCellDefinition(new CellLocation(1, 1), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(2, 1), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 1), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(4, 1), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(5, 1), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(6, 1), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(1, 2), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(2, 2), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 2), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(4, 2), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(5, 2), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(6, 2), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(4, 0), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(5, 0), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(4, 3), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(5, 3), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(8, 0), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(9, 0), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(9, 1), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(2, 7), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(2, 8), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(2, 9), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(2, 10), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(2, 11), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 7), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 8), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 9), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 10), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(3, 11), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(6, 7), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(6, 8), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(7, 7), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(7, 8), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(8, 7), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(8, 8), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(7, 6), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(7, 9), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(7, 12), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(7, 13), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(8, 12), CellDugStatus.Intact, CellContentType.Emerald),
                new GridCellDefinition(new CellLocation(8, 13), CellDugStatus.Intact, CellContentType.Emerald),

                // Gold bags
                new GridCellDefinition(new CellLocation(3, 0), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(3, 3), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(7, 1), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(7, 2), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(0, 8), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(0, 10), CellDugStatus.Intact, CellContentType.GoldBag),
                new GridCellDefinition(new CellLocation(5, 7), CellDugStatus.Intact, CellContentType.GoldBag),
            };
        }
    }
}

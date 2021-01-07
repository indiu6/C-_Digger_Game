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
    /// Enumerator type CellDugStatus, used to indicate how a cell has been dug so far
    /// </summary>
    [Flags]
    public enum CellDugStatus
    {
        // Basic values
        Intact = 0,
        DugLeft = 1,
        DugRight = 2,
        DugUp = 4,
        DugDown = 8,

        // Derived values
        DugHorizontal = DugLeft | DugRight,
        DugVertical = DugUp | DugDown,
        DugFully = DugHorizontal | DugVertical,
        DugHorizontalUp = DugHorizontal | DugUp,
        DugHorizontalDown = DugHorizontal | DugDown,
        DugVerticalLeft = DugVertical | DugLeft,
        DugVerticalRight = DugVertical | DugRight,
        DugLeftUp = DugLeft | DugUp,
        DugLeftDown = DugLeft | DugDown,
        DugRightUp = DugRight | DugUp,
        DugRightDown = DugRight | DugDown
    }

    /// <summary>
    /// Enumerator type CellContentType, used to indicate what the current content of the cell is
    /// </summary>
    public enum CellContentType
    {
        Nothing = 0,
        Cherry,
        Emerald,
        GoldBag,
        Gold
    }

    public class GridCell : DrawableGameComponent
    {
        const string INTACT_TEXTURE_NAME = "TileEmpty";
        const string DUG_UP_TEXTURE_NAME = "CellDugUp";
        const string DUG_DOWN_TEXTURE_NAME = "CellDugDown";
        const string DUG_RIGHT_TEXTURE_NAME = "CellDugRight";
        const string DUG_LEFT_TEXTURE_NAME = "CellDugLeft";
        const string DUG_HORIZONTAL_TEXTURE_NAME = "CellDugHorizontal";
        const string DUG_HORIZONTAL_UP_TEXTURE_NAME = "CellDugHorizontalUp";
        const string DUG_HORIZONTAL_DOWN_TEXTURE_NAME = "CellDugHorizontalDown";
        const string DUG_VERTICALLY_TEXTURE_NAME = "CellDugVertically";
        const string DUG_VERTICALLY_RIGHT_TEXTURE_NAME = "CellDugVerticallyRight";
        const string DUG_VERTICALLY_LEFT_TEXTURE_NAME = "CellDugVerticallyLeft";
        const string DUG_FULLY_TEXTURE_NAME = "CellDugFully";
        const string DUG_LEFT_UP_TEXTURE_NAME = "CellDugLeftUp";
        const string DUG_LEFT_DOWN_TEXTURE_NAME = "CellDugLeftDown";
        const string DUG_RIGHT_UP_TEXTURE_NAME = "CellDugRightUp";
        const string DUG_RIGHT_DOWN_TEXTURE_NAME = "CellDugRightDown";

        const int CELL_DRAWORDER = 1; // Cell will be on the background so it will be the first thing to be drawn

        //private Texture2D tileTexture;
        public Rectangle BoundingRectangle { get; private set; }

        public CellDugStatus DugStatus { get; private set; }  // What the current dug status of the cell is
        public CellContentType ContentType { get; private set; }  // What the cell currently contains

        // If the content of the cell is something that blocks the passage of creatures
        public bool ContainsBlockingObject { get; private set; } 

        // Dictionary for CellDugStatus textures
        private static Dictionary<CellDugStatus, Texture2D> cellTextures;

        // Parent grid
        private Grid grid;

        private Cherry cherry;

        /// <summary>
        /// Constructor of a grid cell. Some basic parameters are set as specified
        /// </summary>
        /// <param name="game">The DiggerGame object</param>
        /// <param name="coordX">Screen X coordinate for the cell</param>
        /// <param name="coordY">Screen Y coordinate for the cell</param>
        /// <param name="dugStatus">Dug status for the cell</param>
        /// <param name="contentType">Type of content that the cell will have</param>
        /// <param name="parent">Grid object which is creating this GridCell</param>
        /// <param name="isBlockingObject">Whether the content is blocking. Optional parameter</param>
        public GridCell(Game game, int coordX, int coordY, 
                        CellDugStatus dugStatus, CellContentType contentType, 
                        Grid parent, bool isBlockingObject = false) 
            : base(game)
        {
            this.grid = parent;

            DrawOrder = CELL_DRAWORDER; 

            BoundingRectangle = new Rectangle(coordX, coordY, grid.CellWidth, grid.CellHeight);

            ContainsBlockingObject = false; // False until something that is blocking is set as content
            SetDugStatus(dugStatus);
            SetContentType(contentType, isBlockingObject);
        }

        protected override void LoadContent()
        {
            //tileTexture = Game.Content.Load<Texture2D>(INTACT_TEXTURE_NAME); // PENDING: Texture needs to be set according to DugStatus

            // add textures to dictionary 
            if (cellTextures == null)
            {
                cellTextures = new Dictionary<CellDugStatus, Texture2D>();
                cellTextures.Add(CellDugStatus.Intact, Game.Content.Load<Texture2D>(INTACT_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugUp, Game.Content.Load<Texture2D>(DUG_UP_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugDown, Game.Content.Load<Texture2D>(DUG_DOWN_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugRight, Game.Content.Load<Texture2D>(DUG_RIGHT_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugLeft, Game.Content.Load<Texture2D>(DUG_LEFT_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugHorizontal, Game.Content.Load<Texture2D>(DUG_HORIZONTAL_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugHorizontalUp, Game.Content.Load<Texture2D>(DUG_HORIZONTAL_UP_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugHorizontalDown, Game.Content.Load<Texture2D>(DUG_HORIZONTAL_DOWN_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugVertical, Game.Content.Load<Texture2D>(DUG_VERTICALLY_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugVerticalRight, Game.Content.Load<Texture2D>(DUG_VERTICALLY_RIGHT_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugVerticalLeft, Game.Content.Load<Texture2D>(DUG_VERTICALLY_LEFT_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugFully, Game.Content.Load<Texture2D>(DUG_FULLY_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugLeftUp, Game.Content.Load<Texture2D>(DUG_LEFT_UP_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugLeftDown, Game.Content.Load<Texture2D>(DUG_LEFT_DOWN_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugRightUp, Game.Content.Load<Texture2D>(DUG_RIGHT_UP_TEXTURE_NAME));
                cellTextures.Add(CellDugStatus.DugRightDown, Game.Content.Load<Texture2D>(DUG_RIGHT_DOWN_TEXTURE_NAME));
            }            

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {            
            base.Update(gameTime);
        }        
                
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sb = Game.Services.GetService<SpriteBatch>();
            sb.Begin();

            sb.Draw(cellTextures[DugStatus], BoundingRectangle, Color.White);

            sb.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Sets value of dugStatus parameter to DugStatus property. Performs a validation first to ensure that value is correct
        /// </summary>
        /// <param name="dugStatus">value to be set</param>
        /// <returns>true if set was successful and false otherwise (the value of parameter was invalid)</returns>
        public bool SetDugStatus (CellDugStatus dugStatus)
        {
            if (Enum.IsDefined( typeof(CellDugStatus),dugStatus))
            {
                DugStatus = dugStatus;  // Assigns the parameter value
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds the value of dugStatus parameter to DugStatus propery. Performs a validation first to ensure that value is correct
        /// </summary>
        /// <param name="dugStatus">value to be set</param>
        /// <returns>true if set was successful and false otherwise (the value of parameter was invalid)</returns>
        public bool AddDugStatus(CellDugStatus dugStatus)
        {
            if (Enum.IsDefined(typeof(CellDugStatus), dugStatus))
            {
                DugStatus |= dugStatus;  // Adds the parameter value to the existing status
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets value of contentType parameter to ContentType property. Performs a validation first to ensure that value is correct
        /// </summary>
        /// <param name="contentType">value to be set</param>
        /// <param name="isBlockingObject">potional flag to indicate whether the content to be set is blocking</param>
        /// <returns>true if set was successful and false otherwise (the value of parameter was invalid)</returns>
        public bool SetContentType(CellContentType contentType, bool isBlockingObject=false)
        {
            if (Enum.IsDefined(typeof(CellContentType), contentType))
            {
                if (ContentType != contentType) // Check if the content type being assigned not already set
                {
                    ContentType = contentType;
                    // If an object is being set, create it
                    switch (ContentType)
                    {
                        case CellContentType.Cherry:
                            cherry = new Cherry(Game, BoundingRectangle);
                            grid.AddCellContentToScene(cherry);
                            break;
                        case CellContentType.Emerald:
                            Emerald emerald = new Emerald(Game, BoundingRectangle);
                            grid.AddCellContentToScene(emerald);
                            break;
                        case CellContentType.GoldBag:
                            GoldBag goldBag = new GoldBag(Game, BoundingRectangle);
                            isBlockingObject = true; // Gold bags are blocking objects
                            grid.AddCellContentToScene(goldBag);
                            break;
                        case CellContentType.Gold:
                            break;
                        case CellContentType.Nothing:
                            //Game.Components.Remove(cherry);
                            break;
                        default:
                            break;
                    }
                }
                ContainsBlockingObject = (ContentType == CellContentType.Nothing) ? false : isBlockingObject;
                return true;
            }

            return false;
        }
    }
}

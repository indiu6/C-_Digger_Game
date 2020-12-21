Version History

v0.2.1
- Digger class:
	- Added animation for Digger when it's dead

V0.2.0
- ActionScene class:
	- Fixed an issue with Digger as game component when new game was started after losing
- Bullet class:
	- Changed sourceFrames to static and initialized only if it is null
- Cherry class:
	- Added duration in constuctor and set timer in Update for the duration
- EnemyManager class:
	- Changed CherryCreationAndDeletion to only creation

V0.1.9
- LevelDesign class:
	- added missing grid design of level 2
	- added variables of Cherry configuration	
- Cherry class:
	- implemented similar with Emerald class
- EnemyManager class:
	- added 2 cherry timers, UpdateCherryCreationDeletion()
- Digger class:
	- added CheckForCherryCollision()
	- added cost POINTS_CHERRY = 750 (points)
- GridCell class:
	- added switch case of Cherry in SetContentType()

v0.1.8
ActionScene class:
	- Implemented logic for handling game states using Enum and proceeding according to each state
Digger class:
	- Added parent field to reference its parent game scene and call its methods
	- Modified constructor to set this new field
	- Added method ExecuteDeath
 	- Added other methods
DiggerGame class:
	- Changed GameInProgress property to return if ActionScene current state is GameOn
	- Remove all methods related to game state handling (also handled by ActionScene)
MenuComponent class:
	- Changed references to GameInProgress
Bullet class:
	- Removed the check for no more enemies and moved this to the EnemyManager
EnemyManager class:
	- Changed type of parent from GameScene to ActionScene
	- Implemented CheckForEnemiesLeft method which notifies parent when there are no more enemies
	- Added method StopAllEnemies, called by ActionScene when Digger is killed
	- Added field stopCreatingEnemies 
GridCell class:
	- Added grid as constructor parameter and assigned it in grid property
Grid class:
	- Added this when calling gridcell constructor
Score class:
	- Added method to reset score

v0.1.7
- Creature class:
	- Added property MovementCondition and deleted flags FollowsKeyboard and IsAlive
	- Changed Update method to use MovementCondition instead of flags
	- Updated constructor accordingly
- Digger class:
	- Updated constructor and call to Creature base constructor to use MovementCondition
- Enemy, Hobbin, Nobbin classes:
	- Updated constructors and calls to base constructor to use MovementCondition
- Goldbag class:
	- Set current cell as empty (and not blocking content) when goldbag will start to fall
- GridCell class
	- Corrected order of setting ContainsBlockingObject property and return in SetContentType()
	- added class-level var. Cherry cherry

v0.1.6
- Bullet class:
	- Added sound effects for both states
- GoldBag class:
	- Added sound effects for all states
- Creature class:
	- Added logic to prevent Creature from entering a cell that has a blocking content
	- Added enum MovementCondition to indicate what type of movement the creature has
- Score class:
	- Added one more digit to score display and moved it a bit closer to right side

v0.1.5
- GoldBag class:
	- Implemented its animation and the fact that it falls down and turns into gold when cell below is digged
	- Implemented collision with Digger when it has become gold and is "eaten"
- Digger class:
	- Implemented collision with GoldBag and score points when it "eats" the gold

v0.1.4
- Merged v0.1.3a + v0.1.3b

v0.1.3a
- Score class:
	- Renamed scoreText to livesAndScoreText and modified the string to also show lives left
- LevelDesign class:
	- Updated LoadFromFile() to include Hobbin
	- Added properties for Hobbin definition
- Enemy manager class:
	- Implemented creation of Hobbins according to definition in Level design
- Hobbin class:
	- Implemented it
- Creature class:
	- Renamed method UpdateRandomPath to UpdateRestrictedPath (to be used for Nobbins)
	- Created new method UpdateRandomPath (to be used by Hobbins)
	- Modified the logic order in Update
	- Renamed field keepDirectionUntilNewCell to forcedToKeepDirection
- Grid class:
	- Implemented CountNonBlockingCells method
- GridCell class:
	- Added property ContainsBlockingObject and initialized as false in constructor
	- Added optional parameter isBlockingObject to SetContentType to indicate if content is a blocking object
	- Modified constructor and SetContentType method to add isBlockingObject as optional parameter

v0.1.3b
- Added new classes (AboutScene, AboutComponent), changed "Credit" to "About"
- AboutComponent class:
	- Added our names as text
	- Added 2 cool images
- MenuComponent class
	- Added new banner image
- Edited 'helpImage' in HelpComponent class to show the required key presses clicks to play
- ActionScene class:
	- LoadFromFile(): added level 2 grid design
	- LevelCompleted(): added DestroyCurrentLevel(), CheckForEscape()

v0.1.2
- Main update: Digger now shoots, bullet has animation, Enemy gets killed and Digger scores
- Grid class:
	- Renamed const OFFSET to DEFAULT_OFFSET
	- Added optional parameter offset to IsHorizontallyCentered and IsVerticallyCentered  
- Digger class:
	- Renamed const LOCATION_OFFSET to RECTANGLE_OFFSET
	- Added const DIGGER_CENTERED_OFFSET to have its own offset for "Is H/V Centered"
	- Added variables to implement firing and firing timer 
	- Added CheckFire() and FireBullet() methods
- Bullet class:
	- Was implemented
- Creature class:
	- Added constant DEFAULT_CENTERED_OFFSET to be used as default with IsHorizontallyCentered and IsVerticallyCentered 
	- Added property centeredOffset which set iw with DEFAULT_CENTERED_OFFSET value in constructor
	- Deleted commented code (previous version of a method) 
	- Implemented ICollidable. Defined CollisionBox and set HandleCollision as virtual
- MovingEntity class:
	- Added property LastMovingDirection (useful for Digger to shoot bullet even when stopped)
- Emerald class, GoldBag class:
	- Added const for draw order

v0.1.1
- Emerald class:
	- Set it as ICollidable and implemented collision with Digger
	- Removed commented code from v0.1.0
- Digger class:
	- Set it as ICollidable and implemented check for collision with Emerald
	- Updated score
- GameScene class:
	- Set sceneComponent list as protected
- ActionScene class:
	- Created method LevelCompleted. PENDING: Play victory song
- DiggerGame class:
	- Created method SetLevelCompleted which finalizes the level successfully
		// PENDING: Code the transition to a new level
	- Created method SetLevelFailed which finalizes the level unsuccessfully
		// PENDING: Code highscores and return to main menu
- Score class:
	- Changed the formatting of score to be displayed with leading zeroes


v0.1.0
- DiggerGame class:
	- Added flag GameInProgress to track if there is currently a game in progress
	- Added a setter for this flag (PENDING: make some validations before setting)
- MenuComponent class:
	- Added possibility of switching from "Start Game" to "Resume Game" when game is paused
	- Informs Game when game has started so that GameInProgress flag is set
- Emerald class:
	- Added cell bounding rectangle parameter in constructor
	- Commented out code related to dictionart for texture - left only one texture
- Grid class:
	- Added public method AddCellContentToScene
	- Added validation in method GetRectangleCenterGridPosition
- GridCell class:
	- Enhanced SetContentType method to create the object for the new content that is being set
- Entity class:
	- Added a new constructor which takes the BoundingRectangle of containing grid cell plus a size % of it
	- Added CurrentRow and CurrentCol properties
	- Implemented update of CurrentRow and CurrentCol in Update method



v0.0.9
- Added new 6 Scene classes in Scenes folder
- DiggerGame class:
	- Moved LoadLevels(), some class-level variables to ActionScene class
	- Commented out 'IsKeyDown(Keys.Escape)' in Update() to go back menu screen, not closing app
- Grid class:
	- Added class-level 'GameScene parent' var. and used in constructor as 3rd parameter 'GameScene parent' 
	- Added '.Enabled' property
	- Added parent.AddComponent(GridCell[row, col]) in Initialize()
- EnemyManager class:
	- Added class-level 'GameScene parent' var. and used in constructor as 3rd parameter 'GameScene parent' 
	- Added '.Enabled' property
	- Added parent.AddComponent(nobbin) in Update()
- Added helpImage in HelpScene
- Added 4 Sound files
	- Sound Effect 'funeral.mp3': sound when digger dies
	- Sound Effect 'level.mp3': when game moves to next level?
	- Ambient Sound 'popcorn.mp3': background sound
	- Sound Effect 'williamtell.mp3': when digger gets cherry?
	- Sound Effect 'getItem.wave' : when digger gets item
- ActionScene class:
	- Added MediaPlayer.State in Update()
- MenuComponent class:
	- Added Song backgroundMusic in LoadContent()
	- Added MediaPlayer.Resume() or .Play() in SwitchScenes()
- Added new Score class (same code with Aneta's, just changed font color and size)
- Digger class:
	- Added 2 new textures(digup and digdown) - now we can see all 4, different digger textures
	- Updated textures in Update() based on ks.IsKeyDown()
PENDING: Emerald class: fix emerald is not showing on grid

v0.0.8
- Creature class:
	- created methods CanGoLeft(), canGoRight(), canGoUp() and canGoDown()
	- Implemented creature movement when not following keyboard
	- Improved code of random movement by creature not following keyboard
- Nobbin class:
	- Now has movement based on Creature class
- EnemyManager class:
	- Dynamically generates nobbins following maximum number of simulatenous nobbins
	- Generates nobbins until a level pre-defined count (just like in the real game)
	- Uses a level pre-defined time interval for nobbin generation frequency

v0.0.7
- Main update: Digger makes its own path
- Added Arial20 font to project
- Reused Debugger class from Week11 and added new methods
- Moving Entity class: 
	- Implemented Update() to set Direction propery based on key presed
	- Corrected from private to public in SetSpeed and SetDirection methods
	- Replaced Direction property by HorizontalDirection and VerticalDirection
- Grid class: 
	- created GetCellByScreenCoordinates method
	- Added more utility methods
- GridCell class:
	- created method AddDugStatus which is used by the Digger
- DiggerGame class:
	- added creation of debugger and inclusion as a service
	- added creation of EnemyManager and inclusion as a service
- Creature class:
	- added property FollowsUserInput to indicate if the creature can be guided by keyboard
	- constructors updated accordingly
	- moved most of Digger's Update() method to Creature base class
- Entity class:
	- moved Digger's Draw() method to Entity class
- Nobbin class: Implementation of class - pending to add movement
- Enemy Manager: Implementation of class - pending to Implement the dynamic generation of enemies

v0.0.6
- add Dictionary for CellDugStatus textures 
- GridCell class: LoadContent() - added textures to dictionary
- GridCell class: Draw() - changed for using dictionary
- added cells in LevelDesign class: like original's level 1 design 

v0.0.5
- Removed getters and setters of Entity class properties and set them all as protected fields (used lowercase)
- Renamed Image property of Entity class to texture and removed texture from Digger class
- Renamed enum Direction to DirectionType in MovingEntity, added "None" value to it and moved it out of the class (at the namespace level)
- Added a Direction property in MovingEntity class.
- Implemented Constructors of MovingEntity class, one of them for setting default values
- Added setter methods in MovingEntity class
- Changed from set to protected set all properties of Creature class and set them to Public
- Implemented Constructors of Creature class, one of them for setting default values
- Changed the invocation of the base constructor from the Digger constructor in order to set base class properties

v0.0.4
- Added LoadLevels method to the DiggerGame class
- Added resource file Resource.resx
- Defined const values for default screen witdh and height but still read from resources
- Implemented LevelDesign class
- PENDING: implement LoadFromFile() method in LevelDesign class
- Created structs CellLocation and GridCellDefinition in LevelDesign.cs
- Modified DiggerGames.cs to create the Grid passing the first level in constructor
- Added constants in Grid class for minimum and maximum numbers of rows and columns
- Added derived values in CellDugStatus enum in GridCell.cs
- Added SetDugStatus and SetContentType methods to GridCell
- Changed Digger class to make it start on the grid cell specified by the LevelDesign

v0.0.3
- Added flip effect(horizontally) to Digger class

v0.0.2 
- Changed all references to "square" by "cell", including GetSquareCoordinates() to GetCellCoordinates, and properties SquareWidth, SquareHeight of Gird to CellWidth and CellHeight
- Modified the enum in GridCell class, renamed it to CellDugStatus and set it as [Flags]
- Created new enum CellContentType in GridCell class
- Added some constants to Digger class
- Implemented GridCell class - still pending update of tile according to CellDugStatus
- Removed GameBackground class
- Some comments added

v0.0.1 - Very initial version, obtained from the combination of SampleGame1 + the addition of classes but everything created from scratch again to get rid of the old namespace (SampleGame..) and use Digger Instead. Changes:
- Tile graphic file was renamed to "TileEmpty"
- Specific directories were created in Resources for Tiles and for Creatures graphics.


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.Linq;

namespace Digger
{
    /// <summary>
    /// Enumerator type GameState, used to indicate what stage the game is currently in
    /// </summary>
    public enum GameState
    {
        Initializing,       // Indicates that ActionScene has just been created and needs to load the levels
        ReadyToStartLevel,  // Indicates that levels are loaded and ActionScene is waiting for menu to inform to start a new level
        StartNewLevel,      // Indicates that a new level must be started
        GameOn,             // Indicates that a game is in progress
        LevelCompleted,     // Indicates that a level is completed and needs to check if there is a new one
        LevelFailed,        // Indicates that Digger was killed in current level
        GameOverSuccess,    // Indicates that game was finished completely (all levels successfully) and control must be returned to menu
        GameOverFail,       // Indicates that game was finished by Digger running out of all its lives and control must be returned to menu
        GameAbort           // Used when something abnormal occurs (should not happen but just in case) and control must be returned to menu
    }

    /// <summary>
    /// Enumerator type ProcessStage, used to perform asynchronous methods divided in stages
    /// </summary>
    public enum ProcessStage
    {
        Stage1 = 1,
        Stage2,
        Stage3,
        Stage4,
        Stage5
    }

    public class ActionScene : GameScene
    {
        public const string BACKGROUND_MUSIC = "popcorn";
        public const string FUNERAL_MUSIC = "funeral";
        public const string VICTORY_MUSIC = "levelComplete";
        const float AMBIENT_SOUND_VOLUME = 0.3f; // 0 == Silent 1 == Full Volume

        private int numberOfLevels; // How many levels the game will handle - value is read from resource file == 2
        private List<LevelDesign> levels;  // List of all levels loaded into the game
        LevelDesign currentLevel;   // What the current playing level is
        private Grid currentGrid;  // Grid of current level

        public GameState CurrentGameState { get; private set; }  // Will indicate what state the game is currently in

        private Song backgroundMusic;  // Background music for the game
        private SoundEffect funeralMusicSFX;  // Sound effect to be played when Digger has died
        private SoundEffect victoryMusicSFX;  // Sound effect to be played when Digger has completed a level

        private bool musicPlaying;  // If funeral or victory music is currently playing - must wait during it
        private double waitingTimer;  // Timer to wait while funeral music is playing

        private ProcessStage stage;
        private ProcessStage clearStage;  // Used for clearing method

        public ActionScene(Game game) : base(game)
        {
            // Get number of levels from resources
            numberOfLevels = int.TryParse(Resource.numberOfLevels, out int levels) ? levels : 1;
            CurrentGameState = GameState.Initializing;
            stage = ProcessStage.Stage1;
            clearStage = ProcessStage.Stage1;
            musicPlaying = false;
            waitingTimer = 0;
        }

        /// <summary>
        /// Loads the levels defined for the game and sets CurrentGameState based on the result of loading
        /// </summary>
        public override void Initialize()
        {
            LoadLevels();
            CurrentGameState = numberOfLevels > 0 ? GameState.ReadyToStartLevel : GameState.GameAbort;

            backgroundMusic = Game.Content.Load<Song>(BACKGROUND_MUSIC);
            funeralMusicSFX = Game.Content.Load<SoundEffect>(FUNERAL_MUSIC);
            victoryMusicSFX = Game.Content.Load<SoundEffect>(VICTORY_MUSIC);

            MediaPlayer.Volume = AMBIENT_SOUND_VOLUME;
            MediaPlayer.IsRepeating = true; // Background music will loop

            base.Initialize();
        }

        /// <summary>
        /// Creates the list of levels defined for the game and adds them to the list of successfully loaded levels
        /// </summary>
        private void LoadLevels()
        {
            levels = new List<LevelDesign>();

            for (int level = 1; level <= numberOfLevels; level++)
            {
                LevelDesign levelDesign = new LevelDesign(Game, level);
                if (levelDesign.SuccessfullyLoaded)
                {
                    levels.Add(new LevelDesign(Game, level));
                }
            }

            numberOfLevels = levels.Count; // Synchronizing  with the number of levels effectively loaded
        }

        /// <summary>
        /// Handles each GameState and proceeds accordingly. Transitions to a new state when appropriate
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            switch (CurrentGameState)
            {
                case GameState.StartNewLevel:
                    StartNewLevel();
                    break;
                case GameState.GameOn:
                    CheckForEscape();
                    break;
                case GameState.LevelCompleted:
                    PerformDiggersVictory(gameTime);
                    break;
                case GameState.LevelFailed:
                    PerformDiggersDeath(gameTime);
                    break;
                case GameState.GameOverSuccess:
                    ClearGameAndReload();
                    break;
                case GameState.GameOverFail:
                    ClearGameAndReload();
                    break;
                case GameState.GameAbort:
                    // This part of the code should not be reached
                    break;
                default:
                    break;
            }

            base.Update(gameTime);
        }

        private void ClearGameAndReload()
        {
            switch (stage)
            {
                case ProcessStage.Stage1:
                    // Reset score, Digger's lives and remove all components
                    Score score = Game.Components.OfType<Score>().FirstOrDefault();
                    if (score != null)
                    {
                        score.ResetScore();
                        score.Visible = false;
                    }

                    Digger digger = Game.Components.OfType<Digger>().FirstOrDefault();
                    if (digger != null)
                    {
                        digger.ResetNumberOfLives();
                        digger.Visible = false;
                    }

                    for (int i = 0; i < sceneComponents.Count(); i++)
                    {
                        GameComponent component = sceneComponents[i];
                        Game.Components.Remove(component);
                    }
                    sceneComponents.Clear();
                    stage = ProcessStage.Stage2;
                    break;

                case ProcessStage.Stage2:
                    // Remove EnemyManager from Game services
                    Game.Services.RemoveService(typeof(EnemyManager));
                    stage = ProcessStage.Stage3; // ExecuteDeath() finished
                    break;

                case ProcessStage.Stage3:
                    // Load game again
                    LoadLevels();
                    CurrentGameState = numberOfLevels > 0 ? GameState.ReadyToStartLevel : GameState.GameAbort;
                    Game.Services.GetService<StartScene>().Show();
                    stage = ProcessStage.Stage1;  // Reset for future use
                    break;

                default:
                    break;
            }
        }

        private bool ClearForNewLevel()
        {
            switch (clearStage)
            {
                case ProcessStage.Stage1:
                    // Remove all components except Digger
                    for (int i = 0; i < sceneComponents.Count(); i++)
                    {
                        GameComponent component = sceneComponents[i];
                        // Keep Digger and Score as components
                        if (!(component is Digger) && !(component is Score))
                            Game.Components.Remove(component);
                    }
                    sceneComponents.Clear();
                    clearStage = ProcessStage.Stage2;
                    break;

                case ProcessStage.Stage2:
                    // Remove EnemyManager from Game services
                    Game.Services.RemoveService(typeof(EnemyManager));
                    clearStage = ProcessStage.Stage3; // ExecuteDeath() finished
                    break;

                case ProcessStage.Stage3:
                    clearStage = ProcessStage.Stage1;  // Reset for future use
                    return true; // All taks finished

                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// If game is ready to start, sets its status to effectively start a new level
        /// Used by the MenuComponent to notify this Action Scene that game must be started
        /// </summary>
        public void StartOrResumeGame()
        {
            if (CurrentGameState == GameState.ReadyToStartLevel)
            {
                CurrentGameState = GameState.StartNewLevel;
                Enabled = true;
            }
            else
            {
                // Game is to be resumed
                Show();
            }

            // Play (or resume playing) background music
            if (MediaPlayer.State == MediaState.Paused)
                MediaPlayer.Resume();
            else
                MediaPlayer.Play(backgroundMusic);
        }

        private void StartNewLevel()
        {
            if (levels == null) // This should not occur!
                CurrentGameState = GameState.GameAbort;
            else
            {
                if (levels.Count > 0)
                {
                    // A new level must be started
                    currentLevel = levels.ElementAt(0); // Always take the first level on the list

                    // Create the game elements for the new level

                    currentGrid = new Grid(Game, currentLevel, this);
                    // Remove current Grid service and enter new one
                    Game.Services.RemoveService(typeof(Grid));
                    Game.Services.AddService(currentGrid);
                    AddComponent(currentGrid);

                    // Only create digger if it does not exist already (it could come from previous levels - points and lives should be kept)
                    Digger digger = Game.Services.GetService<Digger>();
                    if (digger == null)
                    {
                        digger = new Digger(Game, currentLevel.DiggerStartLocation, this);
                        AddComponent(digger);
                        Game.Services.AddService(digger);
                    }
                    else
                    {
                        digger.GetReadyToStartLevel();
                        if (Game.Components.OfType<Digger>().FirstOrDefault() == null)
                            AddComponent(digger);
                        digger.Enabled = true;
                        digger.Visible = true;
                    }

                    EnemyManager enemyManager = new EnemyManager(Game, currentLevel, this);
                    AddComponent(enemyManager);
                    Game.Services.AddService(enemyManager);

                    // Only create a new score if it is not a component of game already - there can be one from previous leves
                    Score score = Game.Components.OfType<Score>().FirstOrDefault();
                    if (score == null)
                        AddComponent(new Score(Game));
                    else
                        score.Visible = true;

                    Show();  // Show components
                    levels.RemoveAt(0); // Remove level from list of levels
                    CurrentGameState = GameState.GameOn; // Game is now in progress
                }
                else
                {
                    // No more levels to load. Game was completed successfully
                    CurrentGameState = GameState.GameOverSuccess;
                }
            }
        }

        private void CheckForEscape()
        {
            KeyboardState ks = Keyboard.GetState();

            // handle the escape key for this scene
            if (ks.IsKeyDown(Keys.Escape))
            {
                ((DiggerGame)Game).HideAllScenes();

                if (MediaPlayer.State == MediaState.Playing)
                    MediaPlayer.Pause();

                Game.Services.GetService<StartScene>().Show();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        private void PerformDiggersVictory(GameTime gameTime)
        {
            switch (stage)
            {
                case ProcessStage.Stage1:
                    // Stop background music
                    if (MediaPlayer.State == MediaState.Playing)
                        MediaPlayer.Stop();

                    Game.Services.GetService<EnemyManager>().StopAllEnemies(); // Stop all enemies
                    stage = ProcessStage.Stage2;
                    break;

                case ProcessStage.Stage2:
                    // Have Digger perform its victory method and move to Stage3 only when finished
                    if (Game.Services.GetService<Digger>().ExecuteVictory(gameTime))
                        stage = ProcessStage.Stage3; // ExecuteDeath() finished
                    break;

                case ProcessStage.Stage3:
                    // Play victory music
                    if (musicPlaying)
                    {
                        waitingTimer += gameTime.ElapsedGameTime.TotalSeconds;
                        if (waitingTimer >= victoryMusicSFX.Duration.Seconds + 0.2) // 0.2 is to give a little more time after played
                        {
                            // Victory music finished playing
                            musicPlaying = false;
                            stage = ProcessStage.Stage4;
                        }
                    }
                    else
                    {
                        waitingTimer = 0;
                        victoryMusicSFX.Play();
                        musicPlaying = true;
                    }
                    break;

                case ProcessStage.Stage4:
                    // Some game components must be cleared to allow a new level to start
                    if (ClearForNewLevel())
                    {
                        CurrentGameState = GameState.StartNewLevel;
                        MediaPlayer.Play(backgroundMusic);
                        stage = ProcessStage.Stage1; // Ready for a new use
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Method to be executed when Digger has died
        /// </summary>
        private void PerformDiggersDeath(GameTime gameTime)
        {
            switch (stage)
            {
                case ProcessStage.Stage1:
                    // Stop background music
                    if (MediaPlayer.State == MediaState.Playing)
                        MediaPlayer.Stop();

                    Game.Services.GetService<EnemyManager>().StopAllEnemies(); // Stop all enemies
                    stage = ProcessStage.Stage2;
                    break;

                case ProcessStage.Stage2:
                    // Have Digger perform its death method and move to Stage3 only when finished
                    if (Game.Services.GetService<Digger>().ExecuteDeath(gameTime))
                        stage = ProcessStage.Stage3; // ExecuteDeath() finished
                    break;

                case ProcessStage.Stage3:
                    // Play funeral music
                    if (musicPlaying)
                    {
                        waitingTimer += gameTime.ElapsedGameTime.TotalSeconds;
                        if (waitingTimer >= funeralMusicSFX.Duration.Seconds + 1) // 1 is to give a little more time after played
                        {
                            // Funeral music finished playing
                            musicPlaying = false;
                            stage = ProcessStage.Stage4;
                        }
                    }
                    else
                    {
                        waitingTimer = 0;
                        funeralMusicSFX.Play();
                        musicPlaying = true;
                    }
                    break;

                case ProcessStage.Stage4:
                    // Decide next state based on Digger's remaining number of lives
                    if (Game.Services.GetService<Digger>().NumberOfLives > 0)
                    {
                        RestartWithNewLife();
                        CurrentGameState = GameState.GameOn;
                    }
                    else
                    {
                        CurrentGameState = GameState.GameOverFail;
                    }
                    stage = ProcessStage.Stage1; // Ready for a new use
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Method to be used when Digger just died and will resume the level with a new life
        /// It "wakes up" Digger to start a new attempt and also the Enemy Manager to start generating enemies again
        /// </summary>
        private void RestartWithNewLife()
        {
            Game.Services.GetService<Digger>().GetReadyToStartLevel();
            Game.Services.GetService<EnemyManager>().RestartLevel();
            MediaPlayer.Play(backgroundMusic);
        }

        /// <summary>
        /// If game is in progress, sets its status to LevelCompleted
        /// Used by the Digger to notify this Action Scene that it completed a level successfully
        /// </summary>
        public void LevelCompleted()
        {
            if (CurrentGameState == GameState.GameOn)
                CurrentGameState = GameState.LevelCompleted;
        }

        /// <summary>
        /// If game is in progress, sets its status to LevelFailed
        /// Used by the Digger to notify this Action Scene that it was killed
        /// </summary>
        public void LevelFailed()
        {
            if (CurrentGameState == GameState.GameOn)
                CurrentGameState = GameState.LevelFailed;
        }
    }
}

using Raylib_cs;
using System.Numerics;
using StellarSurvivors.Core;

using StellarSurvivors.States;

namespace StellarSurvivors.States
{
    public class MainMenuState : IGameState
    {
        private Game _world;
        private Rectangle _playButtonRect;
        private Rectangle _quitButtonRect;

        private int _screenWidth;
        private int _screenHeight;
        
        // --- NEW FIELDS ---
        // This is our new "source of truth" for what's selected.
        // 0 = Play, 1 = Quit
        private int _selectedButtonIndex = 0;
        private int _buttonCount = 2;

        private string _title = "Stellar Survivors";
        private int _titleFontSize = 40;
        private int _buttonFontSize = 20;

        private Color _btnNormalColor = Color.DarkGray;
        private Color _btnHoverColor = Color.LightGray; // This is now "Selected" color
        private Color _btnTextColor = Color.Black;

        public MainMenuState(Game world, int screenWidth, int screenHeight)
        {
            _world = world;
            _screenWidth =  screenWidth;
            _screenHeight = screenHeight;
        }

        public void Enter()
        {
            float btnWidth = 200;
            float btnHeight = 50;
            float btnSpacing = 20;
            
            float playBtnX = (_screenWidth - btnWidth) / 2.0f;
            float playBtnY = _screenHeight / 2.0f - btnHeight - (btnSpacing / 2.0f);

            float quitBtnX = playBtnX;
            float quitBtnY = _screenHeight / 2.0f + (btnSpacing / 2.0f);

            _playButtonRect = new Rectangle(playBtnX, playBtnY, btnWidth, btnHeight);
            _quitButtonRect = new Rectangle(quitBtnX, quitBtnY, btnWidth, btnHeight);
            
            // Explicitly set our starting selection
            _selectedButtonIndex = 0;
        }

        public void Exit()
        {
            // Nothing to clean up!
        }

        public void Update(StateManager stateManager, float dt)
        {
            Vector2 mousePos = Raylib.GetMousePosition();

            // --- 1. HANDLE KEYBOARD NAVIGATION ---
            // We use IsKeyPressed to only fire *once*
            if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S))
            {
                _selectedButtonIndex++;
                if (_selectedButtonIndex >= _buttonCount)
                {
                    _selectedButtonIndex = 0; // Wrap around to the top
                }
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W))
            {
                _selectedButtonIndex--;
                if (_selectedButtonIndex < 0)
                {
                    _selectedButtonIndex = _buttonCount - 1; // Wrap around to the bottom
                }
            }

            // --- 2. HANDLE MOUSE NAVIGATION ---
            // The mouse *overrides* the keyboard selection
            if (Raylib.CheckCollisionPointRec(mousePos, _playButtonRect))
            {
                _selectedButtonIndex = 0;
            }
            else if (Raylib.CheckCollisionPointRec(mousePos, _quitButtonRect))
            {
                _selectedButtonIndex = 1;
            }

            // --- 3. HANDLE ACTIONS (Enter Key OR Mouse Click) ---
            
            // Check for mouse click (this is from your original code)
            bool playClicked = Raylib.CheckCollisionPointRec(mousePos, _playButtonRect) && 
                               Raylib.IsMouseButtonPressed(MouseButton.Left);
                               
            bool quitClicked = Raylib.CheckCollisionPointRec(mousePos, _quitButtonRect) && 
                               Raylib.IsMouseButtonPressed(MouseButton.Left);
            
            // Check for Enter key
            bool enterPressed = Raylib.IsKeyPressed(KeyboardKey.Enter);

            // --- Perform action based on *either* input ---
            
            // Action for "Play" (Index 0)
            if (playClicked || (_selectedButtonIndex == 0 && enterPressed))
            {
                stateManager.ChangeState(new PlayingState(_world, _screenWidth, _screenHeight), _world);
            }

            // Action for "Quit" (Index 1)
            if (quitClicked || (_selectedButtonIndex == 1 && enterPressed))
            {
                Raylib.CloseWindow();
            }
        }

        public void Draw()
        {
            // --- Draw Title ---
            int titleWidth = Raylib.MeasureText(_title, _titleFontSize);
            Raylib.DrawText(_title, (Raylib.GetScreenWidth() - titleWidth) / 2, 
                (int)_playButtonRect.Y - 100, _titleFontSize, Color.White);

            // --- 4. DRAW BUTTONS (Based on the "Source of Truth") ---
            
            // --- Draw Play Button ---
            // We NO LONGER check for hover here. We just check our index.
            bool isPlaySelected = _selectedButtonIndex == 0;
            Raylib.DrawRectangleRec(_playButtonRect, isPlaySelected ? _btnHoverColor : _btnNormalColor);
            
            int playTextWidth = Raylib.MeasureText("Play", _buttonFontSize);
            Raylib.DrawText("Play", 
                (int)(_playButtonRect.X + (_playButtonRect.Width - playTextWidth) / 2), 
                (int)(_playButtonRect.Y + (_playButtonRect.Height - _buttonFontSize) / 2), 
                _buttonFontSize, _btnTextColor);
            
            // --- Draw Quit Button ---
            bool isQuitSelected = _selectedButtonIndex == 1;
            Raylib.DrawRectangleRec(_quitButtonRect, isQuitSelected ? _btnHoverColor : _btnNormalColor);
            
            int quitTextWidth = Raylib.MeasureText("Quit", _buttonFontSize);
            Raylib.DrawText("Quit", 
                (int)(_quitButtonRect.X + (_quitButtonRect.Width - quitTextWidth) / 2), 
                (int)(_quitButtonRect.Y + (_quitButtonRect.Height - _buttonFontSize) / 2), 
                _buttonFontSize, _btnTextColor);
        }
    }
}
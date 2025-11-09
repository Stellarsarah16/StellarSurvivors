namespace StellarSurvivors.Core;
using StellarSurvivors.UI;
using Raylib_cs;
using System.Numerics;
using StellarSurvivors.Entities;

public class UIManager
{
    private int _screenWidth;
    private int _screenHeight;
    
    private bool _isSidebarOpen = false;
    private const int _sidebarWidth = 200;
    private const int _topPanelHeight = 60;
    
    private Button _buyMinerButton;
    private Rectangle _topPanelBounds;
    private Rectangle _sidebarBounds;
    private Rectangle _mainContentBounds;
    private Boolean _mouseOverUI;
    
    private Game _world;

    public UIManager(Game world, int screenWidth, int screenHeight)
    {
        
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _world = world;
        
        //Subscribe to Events
        _world.EventManager.Subscribe<ToggleSidebarEvent>(OnToggleSidebar);
        _world.EventManager.Subscribe<ManualGoldClickEvent>(OnManualGoldClick);
        
        _topPanelBounds = new Rectangle(0, 0, _screenWidth, _topPanelHeight);
        _sidebarBounds = new Rectangle(
            _screenWidth - _sidebarWidth, 
            _topPanelHeight, 
            _sidebarWidth, 
            _screenHeight - _topPanelHeight
        );
        _mainContentBounds = new Rectangle(
            0, 
            _topPanelHeight, 
            _screenWidth - (_isSidebarOpen ? _sidebarWidth : 0), 
            _screenHeight - _topPanelHeight
        );
        
        // --- UI Element Creation ---
            
        // 1. Toggle Button for Sidebar
        // Note the Action: () => { ... } is a "lambda function".
        // We pass a function directly into the button's constructor.
        Action toggleSidebarAction = () => 
        { 
            _isSidebarOpen = !_isSidebarOpen; 
            // Update main content bounds when sidebar toggles
            _mainContentBounds.Width = _screenWidth - (_isSidebarOpen ? _sidebarWidth : 0);
        };

        // 2. Buy Miner Button (inside the sidebar)
        Action buyMinerAction = () =>
        {
            // When clicked, add a new generator to our list
            world.Generators.Add(new BasicMiner(0.5));
        };
        _buyMinerButton = new Button(
            new Rectangle(_screenWidth - 190, _topPanelHeight + 10, 180, 30),
            "Buy Miner (0.5/s)",
            buyMinerAction
        );
    }

    public void Update()
    {
        // Mouse Controls
        Vector2 mousePosition = Raylib.GetMousePosition();
        _mouseOverUI = Raylib.CheckCollisionPointRec(mousePosition, _topPanelBounds) ||
                       (_isSidebarOpen && Raylib.CheckCollisionPointRec(mousePosition, 
                           _sidebarBounds));
        
        if (_isSidebarOpen)
        {
            _buyMinerButton.Update(mousePosition);
        }

    }

    public void Draw()
    {
        // A. Draw Panels
        Raylib.DrawRectangleRec(_topPanelBounds, Raylib.ColorAlpha(Color.Black, 0.7f));
        Raylib.DrawRectangleLinesEx(_topPanelBounds, 1, Color.DarkGray);

        if (_isSidebarOpen)
        {
            Raylib.DrawRectangleRec(_sidebarBounds, Raylib.ColorAlpha(Color.LightGray, 0.5f));
            Raylib.DrawRectangleLinesEx(_sidebarBounds, 1, Color.DarkGray);
        }

        // B. Draw Buttons
        if (_isSidebarOpen)
        {
            _buyMinerButton.Draw();
        }
        
        // 6. Draw UI (outside camera)
        string goldText = $"Gold: {_world.GameData.CurrentGold:F1}"; // "F1" = 1 decimal place
        Raylib.DrawText(goldText, _screenWidth - 150, 10, 20, Color.Red);
        int playerId = _world.EntityManager.Players.First();
        
        var podFuel = _world.EntityManager.Fuels[playerId];
        string podFuelText = $"Fuel: {podFuel.CurrentFuel:F1}";
        Raylib.DrawText(podFuelText, _screenWidth - 150, 40, 20, Color.White);
        Raylib.DrawFPS(10, 40);

        
        if (_world.EntityManager.Pods.Count > 0)
        {
            
            if (_world.EntityManager.Transforms.ContainsKey(playerId))
            {
                var playerTransform = _world.EntityManager.Transforms[playerId];
                string coordsText = $"x: {_world.Camera.Offset.X:F0}, y: {_world.Camera.Offset.Y:F0}";
                Raylib.DrawText(coordsText, 10, 10, 20, Color.White);
            }
        }
    }
    
    public void OnToggleSidebar(ToggleSidebarEvent e){
        _isSidebarOpen = !_isSidebarOpen; 
        _mainContentBounds.Width = _screenWidth - (_isSidebarOpen ? _sidebarWidth : 0);
    }

    public void OnManualGoldClick(ManualGoldClickEvent e)
    {
        if (!_mouseOverUI)
        {
            _world.GameData.CurrentGold += 1;
        }
    }
    
    public Rectangle getSidebarBounds()
    {
        return _sidebarBounds;
    }

    public Rectangle getTopPanelBounds()
    {
        return _topPanelBounds;
    }

    public Boolean isSidebarOpen()
    {
        return _isSidebarOpen;
    }
}
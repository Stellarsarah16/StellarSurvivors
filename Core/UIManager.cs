using Raylib_cs;
using System.Numerics;
using StellarSurvivors.Core; // For Game
using StellarSurvivors.Entities; // For EntityManager
using StellarSurvivors.Components; // For InventoryComponent
using System.Linq; // For .Any() and .First()

namespace StellarSurvivors.UI
{
    public class UIManager
    {
        private EntityManager _entityManager;
        private Game _world;

        // Panel Layout
        private bool _isSidebarOpen = true;
        private const int _sidebarWidth = 260;
        private const int _topPanelHeight = 60;
        private int _screenWidth;
        private int _screenHeight;
        
        private Rectangle _topPanelBounds;
        private Rectangle _sidebarBounds;
        private bool _mouseOverUI;
        
        private const float INTERACT_RANGE_SQ = 100 * 100;  // for mothership

        // Drawing State
        private int _yOffset; // For drawing text line by line

        public UIManager(Game world, int screenWidth, int screenHeight)
        {
            _world = world;
            _entityManager = world.EntityManager;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            
            //Subscribe to Events
            _world.EventManager.Subscribe<ToggleSidebarEvent>(OnToggleSidebar);
            
            _topPanelBounds = new Rectangle(0, 0, screenWidth, _topPanelHeight);
            _sidebarBounds = new Rectangle(
                screenWidth - _sidebarWidth, 
                _topPanelHeight, 
                _sidebarWidth, 
                screenHeight - _topPanelHeight
            );
        }

        public void Update()
        {
            // Mouse Controls
            Vector2 mousePosition = Raylib.GetMousePosition();
            _mouseOverUI = Raylib.CheckCollisionPointRec(mousePosition, _topPanelBounds) ||
                           (_isSidebarOpen && Raylib.CheckCollisionPointRec(mousePosition, _sidebarBounds));
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

            // B. Draw Content
            DrawTopPanelContent();
            
            if (_isSidebarOpen)
            {
                DrawSidebarContent();
            }
        }

        /// <summary>
        /// Draws the content for the (always visible) top panel.
        /// </summary>
        private void DrawTopPanelContent()
        {
            Raylib.DrawFPS(30, 10);
            
            // Get player and draw coordinates
            if (_entityManager.ControlledEntityId != -1)
            {
                int playerId = _entityManager.ControlledEntityId;
                if (_entityManager.Transforms.ContainsKey(playerId))
                {
                    var playerTransform = _entityManager.Transforms[playerId];
                    string coordsText = $"({playerTransform.Position.X/16:F0}, {playerTransform.Position.Y/16:F0})";
                    Raylib.DrawText(coordsText, 30, 35, 20, Color.White);
                }
                // --- THIS IS THE RE-ADDED LOGIC ---
                // 2. Draw Fuel
                if (_entityManager.Fuels.TryGetValue(playerId, out var fuelComp))
                {
                    string podFuelText = $"Fuel: {fuelComp.CurrentFuel:F1}";
                    // Drawn at the top right
                    Raylib.DrawText(podFuelText, _screenWidth - 150, 10, 20, Color.White);
                }
            }
        }
        
        private void DrawSidebarContent()
        {
            _yOffset = (int)_sidebarBounds.Y + 10; // Reset text position

            // 1. Get singleton entity IDs
            int spacemanId = _entityManager.SpacemanId;
            int podId = _entityManager.PodId;
            int mothershipId = _entityManager.MothershipId;

            // 2. Draw Inventories and Fuel
            if (spacemanId != -1)
                DrawEntityInfo("Spaceman", spacemanId);

            if (podId != -1)
                DrawEntityInfo("Pod", podId);
        
            if (mothershipId != -1)
                DrawEntityInfo("Mothership", mothershipId);

            if (_entityManager.SpacemanId != -1)
                spacemanId = _entityManager.SpacemanId;
            
            if (_entityManager.PodId != -1)
                podId = _entityManager.PodId;
            
            if (_entityManager.MothershipId != -1) // Assuming you have this
                mothershipId = _entityManager.MothershipId;

            // 3. Draw Inventories
            if (spacemanId != -1) DrawInventory("Spaceman", spacemanId);
            if (podId != -1) DrawInventory("Pod", podId);
            if (mothershipId != -1) DrawInventory("Mothership", mothershipId);
            
            // Draw Tool Selection
            if (spacemanId != -1 && _entityManager.Tools.TryGetValue(spacemanId, out var toolComp))
            {
                DrawSidebarText("Tools", 20, Color.White);

                // Loop through all available tools in the Toolset
                foreach (string toolName in toolComp.Toolset.Keys)
                {
                    // (This assumes you have a simple button-drawing function)
                    // (You'll need to implement a 'DrawButton' helper)

                    // Check if this tool is the currently equipped one
                    bool isEquipped = (toolComp.CurrentTool == toolComp.Toolset[toolName]);
                    Color buttonColor = isEquipped ? Color.Green : Color.Gray;

                    // Draw a simple text button
                    Rectangle buttonRect =
                        new Rectangle(_sidebarBounds.X + 10, _yOffset, _sidebarWidth - 20, 25);
                    Raylib.DrawRectangleRec(buttonRect, buttonColor);
                    Raylib.DrawText(toolName, (int)buttonRect.X + 5, (int)buttonRect.Y + 5, 18, Color.Black);

                    // Check for click and publish the event
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
                        Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), buttonRect))
                    {
                        _world.EventManager.Publish(new EquipToolEvent
                        {
                            EntityId = spacemanId,
                            ToolName = toolName
                        });
                    }

                    _yOffset += 30; // Move down for the next button
                }
            }
            
            if (_entityManager.ControlledEntityId == podId && mothershipId != -1)
            {
                var podPos = _entityManager.Transforms[podId].Position;
                var shipPos = _entityManager.Transforms[mothershipId].Position;

                if (Vector3.DistanceSquared(podPos, shipPos) < INTERACT_RANGE_SQ) 
                {
                    _yOffset += 10; 
                    DrawSidebarText("Mothership Docked", 18, Color.Green);
        
                    Vector2 mousePos = Raylib.GetMousePosition();

                    // --- Refine Button ---
                    Rectangle refineButton = new Rectangle(_sidebarBounds.X + 10, _yOffset, _sidebarWidth - 20, 25);
                    bool refineHover = Raylib.CheckCollisionPointRec(mousePos, refineButton);
                    Raylib.DrawRectangleRec(refineButton, refineHover ? Color.LightGray : Color.DarkGray);
                    Raylib.DrawText("Refine Coal [Q]", (int)refineButton.X + 5, (int)refineButton.Y + 5, 18, Color.Black);
        
                    if (refineHover && Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        _world.EventManager.Publish(new RefineFuelEvent());
                    }
                    _yOffset += 30; // Move down for the next button

                    // --- Recharge Button ---
                    Rectangle rechargeButton = new Rectangle(_sidebarBounds.X + 10, _yOffset, _sidebarWidth - 20, 25);
                    bool rechargeHover = Raylib.CheckCollisionPointRec(mousePos, rechargeButton);
                    Raylib.DrawRectangleRec(rechargeButton, rechargeHover ? Color.LightGray : Color.DarkGray);
                    Raylib.DrawText("Recharge Pod [R]", (int)rechargeButton.X + 5, (int)rechargeButton.Y + 5, 18, Color.Black);

                    if (rechargeHover && Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        _world.EventManager.Publish(new RechargePodEvent());
                    }
                    _yOffset += 30;
                }
            }
        }

        

        private void DrawInventory(string label, int entityId)
        {
            if (_entityManager.Inventories.TryGetValue(entityId, out var inventory))
            {
                // Draw Title
                DrawSidebarText($"{label} Inventory", 20, Color.White);

                // Draw Capacity
                string capacityText = $"({inventory.GetCurrentLoad()} / {inventory.MaxCapacity})";
                DrawSidebarText(capacityText, 16, Color.Gray);

                // Draw Contents
                if (inventory.Contents.Count == 0)
                {
                    DrawSidebarText("- Empty -", 16, Color.DarkGray);
                }
                else
                {
                    foreach (var item in inventory.Contents)
                    {
                        DrawSidebarText($"- {item.Key}: {item.Value}", 16, Color.LightGray);
                    }
                }
                
                _yOffset += 10; // Add padding
            }
        }

        private void DrawSidebarText(string text, int fontSize, Color color)
        {
            Raylib.DrawText(text, (int)_sidebarBounds.X + 10, _yOffset, fontSize, color);
            _yOffset += (fontSize + 4);
        }
        
        public void OnToggleSidebar(ToggleSidebarEvent e)
        {
            _isSidebarOpen = !_isSidebarOpen;
        }
        
        // --- Getters for other systems (like InputSystem) ---
        public Rectangle getSidebarBounds() { return _sidebarBounds; }
        public Rectangle getTopPanelBounds() { return _topPanelBounds; }
        public bool isMouseOverUI() { return _mouseOverUI; }
        public bool isSidebarOpen() { return _isSidebarOpen; }
        
        
        private void DrawEntityInfo(string label, int entityId)
        {
            // --- 1. Draw Inventory (if it has one) ---
            if (_entityManager.Inventories.TryGetValue(entityId, out var inventory))
            {
                DrawSidebarText($"{label} Inventory", 20, Color.White);
                string capacityText = $"({inventory.GetCurrentLoad()} / {inventory.MaxCapacity})";
                DrawSidebarText(capacityText, 16, Color.Gray);

                if (inventory.Contents.Count == 0)
                {
                    DrawSidebarText("- Empty -", 16, Color.DarkGray);
                }
                else
                {
                    foreach (var item in inventory.Contents)
                    {
                        DrawSidebarText($"- {item.Key}: {item.Value}", 16, Color.LightGray);
                    }
                }
                _yOffset += 5; // Small padding
            }

            // --- 2. Draw Fuel (if it has one) ---
            if (_entityManager.Fuels.TryGetValue(entityId, out var fuel))
            {
                DrawSidebarText($"{label} Fuel", 20, Color.Orange);
                string fuelText = $"{fuel.CurrentFuel:F1} / {fuel.FuelCapacity:F1}";
                DrawSidebarText(fuelText, 16, Color.White);
            
                // --- Optional: Draw a simple fuel bar ---
                float fuelPercent = 0;
                if (fuel.FuelCapacity > 0) // Avoid divide-by-zero
                {
                    fuelPercent = fuel.CurrentFuel / fuel.FuelCapacity;
                }
                Rectangle barBg = new Rectangle(_sidebarBounds.X + 10, _yOffset, _sidebarWidth - 20, 10);
                Rectangle barFg = new Rectangle(_sidebarBounds.X + 10, _yOffset, (_sidebarWidth - 20) * fuelPercent, 10);
            
                Raylib.DrawRectangleRec(barBg, Color.DarkGray);
                Raylib.DrawRectangleRec(barFg, Color.Green);
                _yOffset += 14; // Height of bar + padding
            }
        
            _yOffset += 10; // Padding between entities
        }
    }
}
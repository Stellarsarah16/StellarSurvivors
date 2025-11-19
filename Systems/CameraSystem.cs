using Raylib_cs;
using System.Numerics;
using StellarSurvivors.Core;

namespace StellarSurvivors.Systems;

public class CameraSystem
{
    // We can make the zoom speed a constant.
    private const float ZOOM_SPEED = 5.0f; // Higher is faster
    private const float MOVE_SPEED = 8.0f;
    
    private EntityManager _entityManager;
    public Camera2D Camera { get; set; }

    public CameraSystem(EntityManager entityManager, int screenWidth, int screenHeight)
    {
        _entityManager = entityManager;
        Vector2 screenCenterOffset = new Vector2(screenWidth / 2.0f, screenHeight / 2.0f);
        Camera = new Camera2D()
        {
            Target = new System.Numerics.Vector2(0, 0),
            Offset = screenCenterOffset,
            Rotation = 0f,
            Zoom = 1.0f // Start at default 1.0f zoom
        };
    }

    public void Update(float deltaTime)
    {
        // 1. Get the "State" (Who is controlled?)
        int controlledId = _entityManager.ControlledEntityId;
        
        // --- JOB 1: Get the Target Position (The "Position" Blueprint) ---
        // This is the "synergy" with the TransformComponent system.
        Vector2 targetPosition = this.Camera.Target;
        Vector2 offset = this.Camera.Offset;
        if (controlledId != -1)
        {
            // THIS IS THE LINE YOU FOUND! It's the fix.
            if (_entityManager.Transforms.TryGetValue(controlledId, out var transform))
            {
                targetPosition = new Vector2(transform.Position.X, transform.Position.Y);
                offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
            }
        }
        
        // --- JOB 2: Get the Target Zoom (The "Zoom" Blueprint) ---
        // This is the "synergy" with the CameraFocusComponent system.
        float targetZoom = 1.0f; 
        if (controlledId != -1)
        {
            if (_entityManager.CameraFocus.TryGetValue(controlledId, out var focus))
            {
                targetZoom = focus.TargetZoom;
            }
        }
        
        
        // --- 3. "ACTOR" (Update the Camera State) ---
        // We create a temporary copy to modify (because Camera2D is a struct)
        var tempCamera = this.Camera;

        // Apply Job 1
        tempCamera.Target = Raymath.Vector2Lerp(tempCamera.Target, targetPosition, deltaTime * MOVE_SPEED);
        tempCamera.Offset =  offset;

        // Apply Job 2
        tempCamera.Zoom = Raymath.Lerp(tempCamera.Zoom, targetZoom, deltaTime * ZOOM_SPEED);
        
        // Write the modified copy back to the "Single Source of Truth"
        this.Camera = tempCamera;
    }
}
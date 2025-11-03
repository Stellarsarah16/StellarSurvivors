namespace StellarSurvivors.UI;
using System.Numerics;
using System;
using Raylib_cs;

public class Button
{
    public Rectangle Bounds { get; set; }
    public string Text { get; set; }

    // This is the "polymorphic" part. 
    // We can assign *any* function (with no parameters)
    // to be executed when this button is clicked.
    private readonly Action OnClick;

    private bool isHovered = false;
    private int fontSize = 20;

    public Button(Rectangle bounds, string text, Action onClick)
    {
        Bounds = bounds;
        Text = text;
        OnClick = onClick;
    }

    /// <summary>
    /// Checks for mouse interaction. Call this every frame.
    /// </summary>
    public void Update(Vector2 mousePosition)
    {
        // Check if mouse is over the button
        isHovered = Raylib.CheckCollisionPointRec(mousePosition, Bounds);

        if (isHovered && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            // If clicked, invoke the stored action
            OnClick?.Invoke();
        }
    }

    /// <summary>
    /// Draws the button to the screen. Call this inside the Draw loop.
    /// </summary>
    public void Draw()
    {
        // Draw button background
        // Change color on hover for visual feedback
        Color bgColor = isHovered ? Color.LightGray : Color.White;
        Raylib.DrawRectangleRec(Bounds, bgColor);

        // Draw button border
        Raylib.DrawRectangleLinesEx(Bounds, 2, Color.DarkGray);

        // Draw text centered inside the button
        int textWidth = Raylib.MeasureText(Text, fontSize);
        int textX = (int)(Bounds.X + (Bounds.Width - textWidth) / 2);
        int textY = (int)(Bounds.Y + (Bounds.Height - fontSize) / 2);
            
        Raylib.DrawText(Text, textX, textY, fontSize, Color.Black);
    }
}
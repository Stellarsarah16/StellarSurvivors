using System.Drawing;

namespace StellarSurvivors.Components;

public struct MotherShipComponent
{
    public Rectangle RechargeZone;

    public MotherShipComponent(int x, int y, int width, int height)
    {
        RechargeZone = new Rectangle(x, y, width, height);
    }
}
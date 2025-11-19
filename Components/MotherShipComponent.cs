using System.Drawing;
using System.Security.Cryptography.X509Certificates;

namespace StellarSurvivors.Components;

public struct MotherShipComponent
{
    public Rectangle RechargeZone;
    public float RechargeRate;

    public MotherShipComponent(float rechargeRate, int x, int y, int width, int height)
    {
        RechargeRate = rechargeRate;
        RechargeZone = new Rectangle(x, y, width, height);
    }
}
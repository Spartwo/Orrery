// Class containing physical constants relevant to runtime calculations

namespace StellarGenHelpers
{
    public static class PhysicalConstants
    {
        public const double GRAV = 6.6720e-08; // Gravitational Constant (dyne-cm^2/g^2)
        public const double GRAVACC = 980.67; // Earth's Gravitational Acceleration (cm/sec^2)
        public const double SOLARMASS = 1.99e33; // Solar Mass (g)
        public const double SOLARRADIUS = 6.96e10; // Solar Radius (cm)
        public const double SOLARLUM = 3.90e33; // Solar Luminosity (erg/sec)
        public const double SOLARFLUX = 6.41e10; // Solar Flux (erg/cm^2-sec)
        public const double AU = 1.50e13; // Astronomical Unit (cm)
        public const double LIGHTSPEED = 2.9979e10; // Speed of Light (cm/sec)
    }
}
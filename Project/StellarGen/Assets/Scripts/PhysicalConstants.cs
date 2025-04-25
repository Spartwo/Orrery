// Class containing physical constants relevant to runtime calculations

namespace StellarGenHelpers
{
    public static class PhysicalConstants 
    {
        public const double GRAV = 6.6720e-08; // Gravitational Constant (dyne-cm^2/g^2)
        public const double GRAV_ACC = 980.67; // Earth's Gravitational Acceleration (cm/sec^2)
        public const double JOV_MASS = 1.898e21; // Jupiter Mass (Metric Kiloton)
        public const double E_MASS = 5.972e18; // Earth Mass (Metric Kiloton)
        public const int SOLAR_TEMPERATURE = 5780; // Kelvin
        public const double SOLAR_MASS = 1.989e24; // Solar Mass (Metric Kiloton)
        public const double SOLAR_RADIUS = 6.96e10; // Solar Radius (cm)
        public const double SOLAR_LUM = 3.90e33; // Solar Luminosity (erg/sec)
        public const double SOLAR_FLUX = 6.41e10; // Solar Flux (erg/cm^2-sec)
        public const double AU = 1.50e13; // Astronomical Unit (cm)
        public const double LIGHTSPEED = 2.9979e10; // Speed of Light (cm/sec)
        public const decimal AU_TO_METERS = 149597870700m; //1 AU = 149,597,870.7 km, converted to meters
        public const float TAU = 6.28318530718f;
    }
}
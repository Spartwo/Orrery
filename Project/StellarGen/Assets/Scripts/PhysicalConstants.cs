// Class containing physical constants relevant to runtime calculations

using System;

namespace StellarGenHelpers
{
    public static class PhysicalConstants 
    {
        public const double GRAV = 6.6720e-11;                          // Gravitational Constant (dyne-cm^2/g^2)
        public const double GRAV_ACC = 980.67;                          // Earth's Gravitational Acceleration (cm/sec^2)
        public const short ICE_DENSITY = 900;                           // Ice Density (kg/m^3)
        public const short ROCK_DENSITY = 3000;                         // Rock Density (kg/m^3) averaged
        public const short METAL_DENSITY = 7800;                        // Metal Density (kg/m^3)
        public const float ICE_ROCHE_C = 1.6f;                          // Roche Coefficient, higher less holdy-togethery
        public const float ROCK_ROCHE_C = 1.4f;                         // Roche Coefficient, higher less holdy-togethery
        public const float METAL_ROCHE_C = 1.2f;                        // Roche Coefficient, higher less holdy-togethery
        public const double EARTH_MASS = 5.972e18;                      // Earth Mass (Metric Kiloton)
        public const double EARTH_RADIUS = 6378000;                     // Earth Radius (km)
        public const double JOV_MASS = 1.898e21;                        // Jupiter Mass (Metric Kiloton)
        public const double SOLAR_MASS = 1.989e24;                      // Solar Mass (Metric Kiloton)
        public const short SOLAR_TEMPERATURE = 5780;                    // Kelvin
        public const double SOLAR_RADIUS = 6.96e10;                     // Solar Radius (cm)
        public const double SOLAR_LUM = 3.90e33;                        // Solar Luminosity (erg/sec)
        public const double SOLAR_FLUX = 6.41e10;                       // Solar Flux (erg/cm^2-sec)
        public const decimal STELLAR_DISK_MASS = 4.967e21m;             // Stellar Disk Mass Estimation (Metric Kiloton)
        public const short SUBLIMATION_TEMPERATURE = 1500;              // Sublimation Temperature of anything useful (K)
        public const float ATMOSPHERE_TEMPERATURE_EXPONENT = 0.2f;      // For atmosphere size calculation
        public const double AU = 1.50e13;                               // Astronomical Unit (cm)
        public const double LIGHTSPEED = 2.9979e10;                     // Speed of Light (cm/sec)
        public const decimal AU_TO_METERS = 149597870700m;              //1 AU = 149,597,870.7 km, converted to meters
        public const double TAU = Math.PI*2;
        public const int PASCAL_ATM = 101325;                           // 1 atm 
        public const int PASCAL_BAR = 100000;                           // 1 bar
        public const double GAS_CONSTANT_R = 8.314;                     // J/(mol�K) Idealised molar constant
        public const double KUIPER_DENSITY = 53680898876405;            // Kuiper Belt Density KT/Cubic AU
        public const float SOLAR_METALICITY = 0.0054f;                  // ~5.4% heavy elements
    }
}
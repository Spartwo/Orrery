using System;
using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;
using Models;
using StellarGenHelpers;

namespace SystemGen
{
    public class PlanetGen : BodyGen
    {
        public override BodyProperties Generate(int seedValue)
        {
            seedValue = RandomUtils.TweakSeed(seedValue);

            return new PlanetProperties();
        }
        public void GenerateChildren()
        {
            base.GenerateChildren(new List<BodyGen>());
        }


        private void Generate(string ParentBody, string ParentBodyName, int BodyNumber, float MeanEccentricity, float MaxInclination, float OrbitScale)
        {

            //call the orbit generation method and add to the saved print data
            //BodyOrbitArray.Add(SetOrbit(OrbitalPositions[i] * OrbitScale, MeanEccentricity, MaxInclination));
            //remove SMA index from the array
            //OrbitalPositions.RemoveAt(i);
        }


        /// <summary>
        /// Generates a stellar mass based on probability values along a curve
        /// {{0, 0.1}, {0.16, 0.2}, {0.47, 0.5}, {0.7, 0.75}, {0.9, 1.5}, {0.96, 3.5}, {1, 5}}
        /// </summary>
        /// <param name="seedValue">The numerical seed for this </param>
        /// <returns>A stellar mass in sols</returns>
        private void GeneratePlanetaryComposition()
        {
            /* // Instantiate random number generator 
             Random rand = new Random(seedValue);

             // Get a random percentile
             float graphX = (float)rand.NextDouble();
             // Convert it to stellar masses between 0.1 and 5 using the formula
             float graphY = (0.1f + ((-0.374495f * graphX) / (-1.073858f + graphX)));

             Logger.Log(GetType().Name, ("Sols Mass: " + graphY);
             return graphY;


             //print body data to file
             for (int i = 0; i < BodyCount; i++)
             {
                 string PlanetaryData = "\nBODY_" + (i + 1) + " {"
                 + BodyDataArray[i]
                 + "\n"
                 + BodyOrbitArray[i]
                 + "\n\tCOMPOSITION {"
                 //+ BodyCompArray[i]
                 + "\n\t}"
                 + "\n\tATMOSPHERE {"
                 //+ BodyAtmoArray[i]
                 + "\n\t}\n}\n";
                 File.AppendAllText(SystemFileName, PlanetaryData);
             }*/
        }
    }

    // Sidereal properties specific to celestial bodies
    [Serializable]
    public class SiderealProperties
    {
        private double siderealDayLength; // Sidereal Day Length (in hours)
        private float axialTilt; // Sidereal Longitude (degrees)

        // Constructor
        public SiderealProperties(double SiderealDayLength, float AxialTilt)
        {
            this.SiderealDayLength = Math.Max(siderealDayLength, 0.001);  // Sidereal day cannot be zero or negative
            this.axialTilt = axialTilt;
        }

        #region Getter and Setters
        public double SiderealDayLength
        {
            get => siderealDayLength;
            set => siderealDayLength = Math.Max(value, 0.001);  // Ensures sidereal day is not too small
        }

        public float AxialTilt
        {
            get => axialTilt;
            set => axialTilt = value;
        }

        #endregion
    }
}
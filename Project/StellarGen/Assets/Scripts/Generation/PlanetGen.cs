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
        public PlanetProperties Generate(int seedValue, StarProperties parent)
        {
            if (seedValue == 0)
            {
                // If no seed is provided then pick one at random
                seedValue = RandomUtils.RandomInt(0, int.MaxValue);
            }
            else
            {
                // If it is provided then adjust its value to avoid intersections
                seedValue = RandomUtils.TweakSeed(seedValue);
            }

            Logger.Log("System Generation", $"Planet Seed: {seedValue}");

            return new PlanetProperties();
        }

        /// <summary>
        /// Base method to generate planets, moons, etc
        /// </summary>
        /// <param name="children">The elements being passed downwards from the inherited classes</param>
        public List<BodyProperties> GenerateChildren(StarProperties planet)
        {
            base.GenerateChildren((BodyProperties)planet);

            List<BodyProperties> childBodies = new List<BodyProperties>();
            return childBodies;
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
        
        /// <summary>
        /// Calculates the atmosphere of a planet based on its properties and the star's characteristics.
        /// </summary>
        private void CalculateAtmosphere()
        {

        }
    }

    
}
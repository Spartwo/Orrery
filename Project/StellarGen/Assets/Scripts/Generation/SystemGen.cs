using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class SystemGen : MonoBehaviour
{
    //declare prefab bodies to instantiate
    [SerializeField]  GameObject AsteroidPrefab;
    [SerializeField]  GameObject CometPrefab;
    [SerializeField]  GameObject RingPrefab;
    [SerializeField]  GameObject BodyPrefab;
    [SerializeField]  GameObject UnaryPrefab;
    [SerializeField]  GameObject BinaryPrefab;
    [SerializeField]  GameObject TrinayPrefab;
    //declare the animation curve for star mass generation
    public AnimationCurve StellarMassProbabilityCurve;
    //declare seed input from UI and
    private string SeedInput;
    protected string SystemFileName;

    //declare system age as 0 and work from thhre
    public float SystemAge;
    protected float[] StarLifespans;
    private string[] StarDataArray;
    private string[] StarDataAgedArray;
    private string[] StarOrbitArray;

    //viable planetary variables
    
    int BodyCount;
    private List<float> OrbitalPositions;
    private List<string>  BodyDataArray;
    private List<string>  BodyOrbitArray;
    
    //system loaddata
    protected List<string> SystemDataArray;

    void Start()
    {
        //create folder on initialisation incase of total deletion
        Directory.CreateDirectory(Application.streamingAssetsPath + "/Star_Systems/");

        SetSystemName();
    }

    public void SaveSystemFile()
    {   
        //function to save system file post-generation
        //create the system file in the extant directory from Start()
        SystemFileName = Application.streamingAssetsPath 
            + "/Star_Systems/" 
            + (GameObject.Find("Seed_Input").GetComponent<TMP_InputField>().text) 
            + ".system";

        //overwrite values
        // File.WriteAllText(SystemFileName, "UsableSeed = " + UsableSeed);
        // File.AppendAllText(SystemFileName, InitGenConfig);
    }

    public void LoadSystemFile()
    {       
        StartCoroutine(LoadSystemCoroutine());
    }

    IEnumerator LoadSystemCoroutine()
    {
        GameObject Focus = GameObject.Find("Camera_Focus");
        //reset camera parentage
        Focus.GetComponent<CameraMovement>().Parent = this.transform;
        while(Focus.transform.parent != this.transform)
        {
            Debug.Log("No camera parent set yet");
            yield return null;
        }

        //delete all previous bodies
        GameObject[] DeleteBodies =  GameObject.FindGameObjectsWithTag("Root_Body");
 
        for(var i = 0 ; i < DeleteBodies.Length ; i ++) 
        {
            Destroy(DeleteBodies[i]);
        }
        //clear any previous data
        SystemDataArray = new List<string>();

        string SystemName = GameObject.Find("Seed_Input").GetComponent<TMP_InputField>().text;
        //create the system file in the extant directory from Start()
        Debug.Log("System Name" + SystemName);
        SystemFileName = Application.streamingAssetsPath 
            + "/Star_Systems/" 
            + SystemName 
            + ".system";
        
        //read all save data to a list
        SystemDataArray = File.ReadAllLines(SystemFileName).ToList();
        //get star system age from the file
        SystemAge = float.Parse(ReturnFileValue(3));

        //declare the object input for future use
        GameObject obj;
        yield return new WaitForEndOfFrame();
        //instantiate the correct stellar arrangement
        switch (int.Parse(ReturnFileValue(1)))
        {
            case 1:
                Debug.Log("Unary System");
                obj = Instantiate(UnaryPrefab);
                obj.transform.SetParent(this.gameObject.transform, false);
                obj.name = "UNARY_BARYCENTRE";
                break;
            case 2:
                Debug.Log("Binary System");
                obj = Instantiate(BinaryPrefab);
                obj.transform.SetParent(this.gameObject.transform, false);
                obj.name = "BINARY_BARYCENTRE";
                break;
            case 3:
                Debug.Log("Trinary System");
                obj = Instantiate(TrinayPrefab);
                obj.transform.SetParent(this.gameObject.transform, false);
                obj.name = "TRINARY_BARYCENTRE";
                break;
            default:
                Debug.Log("Rogue Planet");
                break;
        }

        //for each planet
        for (int i = 0; i < int.Parse(ReturnFileValue(2)) ; i++) // or count if it is a list
        {
            //Instantiate bodies and pass their unique ID and the system file to their script
            obj = Instantiate(BodyPrefab);
            obj.transform.SetParent(this.gameObject.transform, false);
            //set the starmanager unique identifier
            obj.GetComponent<BodyManager>().BodySearchTerm = "BODY_" + (i+1);
            //set the starmanager input filename
            obj.GetComponent<BodyManager>().SystemFileName = SystemName;
        }
    
        //once generated, call the camera to update its list of objects
        Focus.GetComponent<CameraMovement>().UpdateBodyList();
    }

    //return value from a certain line of systemdata
    string ReturnFileValue(int index)
    {
        int FilePoint = SystemDataArray[index].IndexOf("=");
        return(SystemDataArray[index].Substring(FilePoint+2));
    }
    public void SetSystemName()
    {
        //generate if no custom provided
        //import list from text file
        string NameGenerationFilePath = Application.streamingAssetsPath + "/Localisation/System_Names.txt";
        List<string> StarNameArray = File.ReadAllLines(NameGenerationFilePath).ToList();

        //choose position in new array
        int RandNamePosition = Random.Range(1,(StarNameArray.Count)); 
        int RandNumber = Random.Range(1,999);
        //retrieve name from position
        string RandName = StarNameArray[RandNamePosition];
        //apply to name
        string NameInput = RandName + "-" + RandNumber;
        GameObject.Find("Seed_Input").GetComponent<TMP_InputField>().text = NameInput;
    }
    public void GenerateSystemFile()
    {
        SeedInput = GameObject.Find("Seed_Input").GetComponent<TMP_InputField>().text;
        //generate system seed
        if (SeedInput == "") 
        {
            SetSystemName();
        }

        //code for converting provided characters into a usable seed
        int UsableSeed = SeedInput.GetHashCode();
        //set appropriate file name to the name value
        SystemFileName = Application.streamingAssetsPath 
            + "/Star_Systems/" 
            + SeedInput 
            + ".system";
        
        //set system seed to the converted value
        Random.InitState(UsableSeed); 

        //initialise the data arrays and reset the values
        StarDataArray = new string[3];
        StarDataAgedArray = new string[3];
        StarOrbitArray = new string[3];
        BodyCount = 0;


        int StarDictator = Random.Range(1, 1000);
        int StarCount;
        //assign number of stars
        if (StarDictator > 50) {
            StarCount = 1;
            //set single star distance at centre
            float ABDistance = 0;
            
            StarOrbitArray[0] = 
              "\n\t\tSMA = " + 0
            + "\n\t\tEccentricity = " + 0
            + "\n\t\tLongitude = " + 0
            + "\n\t\tInclination = " + 0
            + "\n\t\tPeriArgument = " + 0;

            if (StarDictator > 480) {
                StarCount = 2;
                //generate binary seperation
                ABDistance = Random.Range(1F, 10000F)/100;
                //Eccentricity capacity increases with distance
                float ABEccentricity = Random.Range(0f, (ABDistance*0.005f));

                if (StarDictator > 960) {
                    StarCount = 3;
                    //reduce binary seperation
                    ABDistance = Mathf.Min(ABDistance/10, 0.01f);
                    //generate p-type seperation of body 3
                    float CDistance = Random.Range(ABDistance*30,3000F)/10;
                    float CEccentricity = Random.Range(0f, 0.5f);
                    
                    StarOrbitArray[2] = SetOrbit(CDistance, CEccentricity, 360);
                }

                StarOrbitArray[0] = 
                 "\n\t\tSMA = " + ABDistance/2
                + "\n\t\tEccentricity = " + ABEccentricity
                + "\n\t\tLongitude = " + 0
                + "\n\t\tInclination = " + 0
                + "\n\t\tPeriArgument = " + 0;
                
                StarOrbitArray[1] = 
                 "\n\t\tSMA = " + ABDistance/2
                + "\n\t\tEccentricity = " + ABEccentricity
                + "\n\t\tLongitude = " + 180
                + "\n\t\tInclination = " + 0
                + "\n\t\tPeriArgument = " + 0;
            }
        } else {
            StarCount = 0;
            //rogue planet with no parent star
            //PlanetGen(0,10,0);
        }
        
        //reset star ages to current time maximum
        StarLifespans = new float[] {13.5F, 13.5F, 13.5F, 13.5F};
        //reset the body arrays
        BodyDataArray = new List<string>();
        BodyOrbitArray = new List<string>();

        //call the generation method for each star
        for (int i = 0; i < StarCount; i++)
        {
            //calls generation method
            StarLifespans[i] = StarGen(i+1);
        }
        //Gets a reasonable current age between universe age and too young to support planets for all stars
        SystemAge = Random.Range(0.3f, StarLifespans.Min());
        Debug.Log("System Age" + SystemAge);

        
        //call the secondary generation method for each star
        for (int i = 0; i < StarCount; i++)
        {
            //calls generation method section 2
            StarGen2(i+1);
        }



        
        //create the system file with hypothetical values
        File.WriteAllText(SystemFileName,   "System Name = " + SeedInput + "\n" 
                                        +   "Stars = " + StarCount + "\n" 
                                        +   "Planets = " + BodyCount + "\n"
                                        +   "SystemAge = " + SystemAge + "\n");

        //print star data to file
        for (int i = 0; i < StarCount; i++)
        {
            string StellarData = "\nSTAR_" + (i+1) + " {"
            + StarDataArray[i] 
            + StarDataAgedArray[i] 
            + "\n\tORBIT {"
            + StarOrbitArray[i]
            + "\n\t}\n}\n";
            File.AppendAllText(SystemFileName, StellarData);
        }

        //print body data to file
        for (int i = 0; i < BodyCount; i++)
        {
            string PlanetaryData = "\nBODY_" + (i+1) + " {"
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
        }

        StartCoroutine(LoadSystemCoroutine());
    }

    float StarGen(int StarNumber)
    {
        //Generate Stellar mass using an animation curve
        float MassDictator = Random.Range(0.5f, 5f);
        float Mass = StellarMassProbabilityCurve.Evaluate(MassDictator);
        //Diameter at formation
        float Diameter = Mathf.Pow(Mass, 0.7f);
        //get general luminosity from size and mass based temperature
        float Luminosity = Mathf.Pow(Diameter, 2) * Mathf.Pow((Mathf.Pow(Mass, 0.5f)), 4);
        //estimate stellar lifespan from diameter divided by luminosity
        float Lifespan = (Diameter / Luminosity) * 10;


        

        string StarSuffix;
        if(StarNumber == 1) {
            StarSuffix="A";
        } else if(StarNumber == 2) {
            StarSuffix="B";
        } else {
            StarSuffix="C";
        }
        //prep star 
        StarDataArray[StarNumber-1] = 
              "\n\tName = " + SeedInput + "-" + StarSuffix
            + "\n\tMass = " + Mass; 



        //generate number of planets
        int PlanetCount = (int)(Mathf.Max((Mathf.Pow(Mass,0.3f)*Random.Range(1,10)),1));
        BodyCount += PlanetCount;

        //estimated distance of the heliopause
        float SOIEdge = Mathf.Sqrt(Luminosity)*75;
        //set inner edge as closest bearable temperature limit
        float SOIInner = Mathf.Pow((3f*Mass) / (9f*3.14f*5.51f),0.33f);
        //get starting orbital standards
        float InnerOrbit = Random.Range(SOIInner, SOIInner*5);
        float PlanetarySpacing = Random.Range((0.06f)*SOIEdge, 10);
        //get orbit placing values
        float MeanEccentricity = 0.01f + Mathf.Pow(PlanetCount, -0.3f)/10;
        float MaxInclination = (15f-PlanetCount)/2;

        OrbitalPositions = new List<float>();
        for (int i = 0; i < PlanetCount + Mathf.Round(PlanetCount/5f); i++)
        {
            //add the list of orbital limits to the arraylist
            OrbitalPositions.Add(InnerOrbit + (PlanetarySpacing * Mathf.Pow(2, i)));
        }
        
        //scale orbits down if extending beyond the SOI edge
        float OrbitScale = Mathf.Min(SOIEdge/OrbitalPositions[OrbitalPositions.Count-1], 1f); 

        for (int i = 0; i < PlanetCount; i++)
        {
            PlanetGen(
                ("STAR_" + StarNumber), 
                (SeedInput + "-" + StarSuffix), 
                i, 
                MeanEccentricity, 
                MaxInclination,
                OrbitScale);
        }

        
        //return value for comparison
        return Lifespan;
    }

    void StarGen2(int StarNumber) 
    {

      /*  
        
        //Calculate Hill Sphere in AU
        float BHillSphere = ABDistance*(1-ABEccentricity)*(BMass/3*AMass)^0.4


     

        //calculate mean eccentricity
        float MeanEccentricity = mathf.Max((PlanetCount^(-0.15)-0.65), 0.02);
        //for each planet calculate the available mass
        for (int x = 0; x < PlanetCount; x++) 
        {
            //Pass parent body and orbital limitations
            PlanetGen(i+1, HillRadius, SOIEdge);

        }
        InitGenConfig = InitGenConfig + "STAR_" + StarNumber + "\n{\n\tObject = " + StarNumber + "\n\tStellar Mass = " + Mass + "\n\tChild Bodies = " + PlanetCount + "\n\tHelioPause Distance (AU) = " + SOIEdge + "\n\tRoche Limit (AU) = " + RocheLimit + "\n}\n";  
    */
    //Calculate the rotation rate and stellar activity now knowing the age


         //temp values
        float RotationRate = 10f;
        float AxialTilt = 0.5f;
        float ActivityIndex = 1f;

        //prep star 
        StarDataAgedArray[StarNumber-1] = 
              "\n\tRotationRate = " + RotationRate
            + "\n\tAxialTilt = " + AxialTilt
            + "\n\tActivityIndex = " + ActivityIndex; 
    }

    private void PlanetGen(string ParentBody, string ParentBodyName, int BodyNumber, float MeanEccentricity, float MaxInclination, float OrbitScale) {
        


        float Mass = 1;
        float RotationRate = 24;
        float AxialTilt = 0;
        
        BodyDataArray.Add( 
              "\n\tName = " + ParentBodyName + (BodyNumber+1)
            + "\n\tMass = " + Mass 
            + "\n\tParentBody = " + ParentBody 
            + "\n\tRotationRate = " + RotationRate
            + "\n\tAxialTilt = " + AxialTilt); 

        //get a random orbital position
        int i = Random.Range(0, OrbitalPositions.Count);
        //call the orbit generation method and add to the saved print data
        BodyOrbitArray.Add(SetOrbit(OrbitalPositions[i]*OrbitScale, MeanEccentricity, MaxInclination));
        //remove SMA index from the array
        OrbitalPositions.RemoveAt(i);
    }

    private string SetOrbit(float SMAInput, float MeanEccentricity, float MaxInclination)
    {
        //Jiggle the orbit a little within a permissable range
        SMAInput += Random.Range(-0.03f, 0.03f)*SMAInput;
        float SMA = Mathf.Max(SMAInput, 0.00001f); 
        //Generate Eccentricity with a basis in the mean solar eccentricity
        //Mean is generated by planetcount and isn't a tally of the actual mean
        float Eccentricity = Random.Range(0.001f, MeanEccentricity*2);
        //Generate the angle where the orbit goes from below the equator to above it
        float LongitudeOfAscending = Random.Range(0f, 359f);
        //Generate the inclination of the body in a given range
        float Inclination = Random.Range(0f, MaxInclination);
        //Generate how far in the inclined disk the body will set it's periapse
        float PeriArgument = Random.Range(0f, 359f);

        string OrbitValues = "\tORBIT {"
        + "\n\t\tSMA = " + SMA
        + "\n\t\tEccentricity = " + Eccentricity
        + "\n\t\tLongitude = " + LongitudeOfAscending
        + "\n\t\tInclination = " + Inclination
        + "\n\t\tPeriArgument = " + PeriArgument
        + "\n\t\tColourR = " + Random.Range(0f, 255f)
        + "\n\t\tColourG = " + Random.Range(0f, 255f)
        + "\n\t\tColourB = " + Random.Range(0f, 255f) + "\n\t}";

        return OrbitValues;
    }

}
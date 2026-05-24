using System.Collections.Generic;
using UnityEngine;
using Sylvan.Data.Csv;
using System.Linq;

public class StarDataPrototype : MonoBehaviour
{
    public TextAsset database;
    public Material starMaterial;
    public Mesh starMesh;
    public ParticleSystem systemPrefab;

    private ParticleSystem starParticleSystem;
    private List<Star> stars;

    public float exposure = 10f;
    public float starScreenSize = 0.0006f;

    public struct Star
    {
        public Vector3 position;
        public float magnitude;
        public string name;

        public Star(float ra, float dec, float distance, float magnitude)
        {
            position = EquatorialToCartesian(ra, dec, distance);
            this.magnitude = magnitude;
            name = "";
        }
    }

    void Start()
    {
        var tr = new System.IO.StringReader(database.text);

        var options = new CsvDataReaderOptions();
        options.Quote = '|';

        CsvDataReader dr = CsvDataReader.Create(tr, options);

        int starCount = 0;
        stars = new List<Star>();
        float scale = transform.localScale.x;

        while (dr.Read())
        {
            Star star = new Star
            {
                position = EquatorialToCartesian(dr.GetFloat(7), dr.GetFloat(8), dr.GetFloat(9)) * scale,
                magnitude = dr.GetFloat(13),
                name = dr.GetString(6)
            };

            stars.Add(star);
            starCount++;
        }

        dr.Dispose();

        var filtered = stars.OrderBy(s => s.magnitude).Take(Mathf.RoundToInt(stars.Count * 0.2f)).ToList();
        stars = filtered;

        CreateParticleSystem();
        UpdateParticleSystem();
    }

    private static Vector3 EquatorialToCartesian(float ra, float dec, float distance)
    {
        float x = distance * Mathf.Cos(Mathf.Deg2Rad * dec) * Mathf.Cos(Mathf.Deg2Rad * (ra * 15));
        float y = distance * Mathf.Cos(Mathf.Deg2Rad * dec) * Mathf.Sin(Mathf.Deg2Rad * (ra * 15));
        float z = distance * Mathf.Sin(Mathf.Deg2Rad * dec);

        return new Vector3(x, z, y);
    }

    private void CreateParticleSystem()
    {
        starParticleSystem = Instantiate(systemPrefab, Vector3.zero, Quaternion.identity);
        starParticleSystem.transform.SetParent(transform, false);
        starParticleSystem.transform.localPosition = Vector3.zero;

        var renderer = starParticleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.material = starMaterial;
        renderer.maxParticleSize = starScreenSize;
        renderer.minParticleSize = starScreenSize;

        var main = starParticleSystem.main;
        starParticleSystem.Pause();
        main.maxParticles = stars.Count;
        starParticleSystem.Emit(stars.Count);
        starParticleSystem.Pause();
    }

    private void UpdateParticleSystem()
    {
        var renderer = starParticleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.material = starMaterial;
        renderer.maxParticleSize = starScreenSize;
        renderer.minParticleSize = starScreenSize;

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[starParticleSystem.main.maxParticles];
        starParticleSystem.GetParticles(particles);

        for (int i = 0; i < stars.Count; i++)
        {
            particles[i].position = stars[i].position;
            particles[i].startSize = 0.1f;

            Color colour = Color.white * StarMagnitudeToBrightness(stars[i].magnitude) * exposure;
            colour.a = 1f;

            particles[i].startColor = colour;
        }

        starParticleSystem.SetParticles(particles, stars.Count);
    }

    private float StarMagnitudeToBrightness(float magnitude)
    {
        // Magnitude is reverse logarithmic, so we need to invert it and make it linear.
        return Mathf.Pow(2.512f, -magnitude);
    }
}

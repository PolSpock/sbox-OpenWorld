using System;
using Sandbox;
using Sandbox.Utility;

public sealed class NoiseCreator : Component
{
	public float[,] Luminance { get; private set; }
	[Property] public Texture texture { get; private set; }
	[Property] public SpriteRenderer spriteRenderer { get; set; }
	[Property] public Model model { get; set; }
	[Property] public ModelRenderer modelRenderer { get; set; }
	[Property] public Material material { get; set; }
	[Property] public int mapHeight { get; set; }
	[Property] public int mapWidth { get; set; }
	[Property] public float noiseScale { get; set; }
	public Vector3[] vertices;
	protected override void OnAwake()
{
    // Define the size of the grid
    int gridSizeX = 25;
    int gridSizeY = 25;

    // Define the size of each chunk
    int chunkSizeX = mapWidth;
    int chunkSizeY = mapHeight;

    // Create a grid of chunks
    for (int y = 0; y < gridSizeY; y++)
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            // Generate the noise map for this chunk
            var noise = CreateNoise(chunkSizeX, chunkSizeY, noiseScale, x * chunkSizeX, y * chunkSizeY);

            // Create the chunk
            Vector3 chunkPos = new Vector3(x * chunkSizeX, y * chunkSizeY, 0);
            CreateChunk(chunkPos, noise);
        }
    }
}

public void CreateChunk(Vector3 pos, float[,] noiseValues)
{
    // Create a texture and mesh for the chunk
    texture = noiseTexture(noiseValues, mapWidth, mapHeight);
    spriteRenderer.Texture = texture;
    Luminance = noiseValues;
    string chunkName = "Chunk" + pos.x + pos.y;
    CreateMesh(pos, material, chunkName);
}
	protected override void OnUpdate()
	{
		CreateNoiseTexture();
		spriteRenderer.Texture = texture;
	}
	public float[,] CreateNoise(int width, int height, float scale, int offsetX, int offsetY)
{
    float[,] noiseMap = new float[width, height];

    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            float sampleX = (x + offsetX) / scale;
            float sampleY = (y + offsetY) / scale;

            float noise = Noise.Perlin(sampleX, sampleY);

            noiseMap[x, y] = noise;
        }
    }

    return noiseMap; // return noiseMap instead of Luminance
}

	public Texture noiseTexture(float[,] noiseValue, int width, int height)
	{

		List<Byte> bytes = new List<Byte>();

		for (int y = 0; y < width; y++)
		{
			for (int x = 0; x < height; x++)
			{
  				float lum = noiseValue[x, y] * 255f;
                lum = lum.CeilToInt().Clamp(0, 255);

                byte val = byte.Parse(lum.ToString());
				bytes.Add(val);
				bytes.Add(val);
				bytes.Add(val);
				bytes.Add(255);
			}
		}

		var texture = Texture.Create(height, width).WithFormat(ImageFormat.RGBA8888).WithData(bytes.ToArray()).Finish();

		return texture;
	}
	public void CreateNoiseTexture()
{   
    int octavas = 4;
    float persistence = 0.5f;
    float lacunarity = 2;
    int offsetX = 0; // Define the offsetX
    int offsetY = 0; // Define the offsetY
    var noise = CreateNoise(mapWidth, mapHeight, noiseScale, offsetX, offsetY); // Use the offsets
    texture = noiseTexture(noise, mapWidth, mapHeight);
}

	public void CreateMesh(Vector3 pos, Material chunkMaterial, string name)
{
    Mesh mesh = new Mesh();
    VertexBuffer vertexBuffer = new VertexBuffer();
    for (int y = 0; y < mapHeight; y++)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            // Get the luminance values for the four corners of the quad
            float lum1 = Luminance[x, y] * 100;
            float lum2 = Luminance[x + 1, y] * 100;
            float lum3 = Luminance[x, y + 1] * 100;
            float lum4 = Luninance[x + 1, y + 1] * 100;

            // Create the four vertices of the quad
            Vertex v1 = new Vertex(RoundVector3(new Vector3(x, y, lum1)), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));
            Vertex v2 = new Vertex(RoundVector3(new Vector3(x + 1, y, lum2)), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));
            Vertex v3 = new Vertex(RoundVector3(new Vector3(x, y + 1, lum3)), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));
            Vertex v4 = new Vertex(RoundVector3(new Vector3(x + 1, y + 1, lum4)), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));

            // Add two triangles to form the quad
            vertexBuffer.AddTriangle(v1, v2, v3);
            vertexBuffer.AddTriangle(v2, v4, v3);
        }
    }

    mesh.CreateBuffers(vertexBuffer);
    mesh.Material = chunkMaterial;
    ModelBuilder builder = new ModelBuilder();
    builder.AddMesh(mesh);
    model = builder.Create();
    var newGameObject = new GameObject();
    newGameObject.Components.Create<ModelRenderer>();
    var modelRenderer = newGameObject.Components.Get<ModelRenderer>();
    modelRenderer.Model = model;
    newGameObject.Name = name;
    newGameObject.Transform.Position = pos;
}
private Vector3 RoundVector3(Vector3 vector)
{
    return new Vector3(MathF.Round(vector.x, 4), MathF.Round(vector.y, 4), MathF.Round(vector.z, 4));
}
}

// ------------------------------------------------------------
// Hex Grid Constants
// ------------------------------------------------------------

// Orientation modes
static const int HEX_ORIENT_POINTY = 0;
static const int HEX_ORIENT_FLAT   = 1;

// Axial neighbor offsets
static const int2 AXIAL_NEIGHBORS[6] = {
    int2(+1,  0),
    int2(+1, -1),
    int2( 0, -1),
    int2(-1,  0),
    int2(-1, +1),
    int2( 0, +1)
};

// Math constants
static const float SQRT3        = 1.7320508;
static const float HALF_SQRT3   = 0.8660254; // sqrt(3)/2
static const float ONE_THIRD    = 0.3333333;
static const float TWO_THIRDS   = 0.6666666;
static const float SQRT3_OVER_3 = 0.5773502; // sqrt(3)/3

// Edge normals for pointy-top hexes
static const float2 HEX_NORMALS_POINTY[6] = {
    float2( 1.0,        0.0),        // 0:  EAST        (0°)
    float2( 0.5,        HALF_SQRT3), // 1:  NORTHEAST   (60°)
    float2(-0.5,        HALF_SQRT3), // 2:  NORTHWEST   (120°)
    float2(-1.0,        0.0),        // 3:  WEST        (180°)
    float2(-0.5,       -HALF_SQRT3), // 4:  SOUTHWEST   (240°)
    float2( 0.5,       -HALF_SQRT3)  // 5:  SOUTHEAST   (300°)
};

// Edge normals for flat-top hexes
static const float2 HEX_NORMALS_FLAT[6] = {
    float2( HALF_SQRT3,  0.5),  // 0:  30°   (E/NE)
    float2( 0.0,         1.0),  // 1:  90°   (N)
    float2(-HALF_SQRT3,  0.5),  // 2: 150°   (W/NW)
    float2(-HALF_SQRT3, -0.5),  // 3: 210°   (W/SW)
    float2( 0.0,        -1.0),  // 4: 270°   (S)
    float2( HALF_SQRT3, -0.5)   // 5: 330°   (E/SE)
};

// ------------------------------------------------------------
// Hex Math Utilities
// ------------------------------------------------------------

float2 WorldToAxial(float2 worldPos, float2 gridOrigin, float HexRadius, int orientation)
{
    float2 p = worldPos - gridOrigin;
    float invRadius = 1.0 / HexRadius;

    if (orientation == HEX_ORIENT_POINTY)
    {
        float q = (SQRT3_OVER_3 * p.x - ONE_THIRD * p.y) * invRadius;
        float r = (TWO_THIRDS * p.y) * invRadius;
        return float2(q, r);
    }
    else
    {
        float q = (TWO_THIRDS * p.x) * invRadius;
        float r = (-ONE_THIRD * p.x + SQRT3_OVER_3 * p.y) * invRadius;
        return float2(q, r);
    }
}

float3 AxialToCube(float2 axial)
{
    float x = axial.x;
    float z = axial.y;
    float y = -x - z;
    return float3(x, y, z);
}

int3 CubeRound(float3 cube)
{
    float rx = round(cube.x);
    float ry = round(cube.y);
    float rz = round(cube.z);

    float dx = abs(rx - cube.x);
    float dy = abs(ry - cube.y);
    float dz = abs(rz - cube.z);

    if (dx > dy && dx > dz)
        rx = -ry - rz;
    else if (dy > dz)
        ry = -rx - rz;
    else
        rz = -rx - ry;

    return int3(rx, ry, rz);
}

float2 CubeToLocalPos(float3 cube, int3 roundedCube, float HexRadius, int orientation)
{
    float3 d = cube - float3(roundedCube);

    if (orientation == HEX_ORIENT_POINTY)
    {
        float x = HexRadius * (SQRT3 * (d.x + 0.5 * d.z));
        float y = HexRadius * (1.5 * d.z);
        return float2(x, y);
    }
    else
    {
        float x = HexRadius * (1.5 * d.x);
        float y = HexRadius * (SQRT3 * (d.z + 0.5 * d.x));
        return float2(x, y);
    }
}

void GetHexNormals(int orientation, out float2 normals[6])
{
    if (orientation == HEX_ORIENT_POINTY)
    {
        [unroll] for (int i = 0; i < 6; i++)
            normals[i] = HEX_NORMALS_POINTY[i];
    }
    else
    {
        [unroll] for (int i = 0; i < 6; i++)
            normals[i] = HEX_NORMALS_FLAT[i];
    }
}

void ComputeEdgeDistances(float2 localPos, float2 normals[6], float HexRadius, out float distances[6])
{
    float apothem = HALF_SQRT3 * HexRadius;
    [unroll] for (int i = 0; i < 6; i++)
        distances[i] = apothem - dot(normals[i], localPos);
}

float2 ComputeNeighborNormal(int i, int orientation)
{
    float2 axialDir = float2(AXIAL_NEIGHBORS[i].x, AXIAL_NEIGHBORS[i].y);
    
    float3 cubeDir = float3(axialDir.x, -axialDir.x - axialDir.y, axialDir.y);

    float2 localDir;
    if (orientation == HEX_ORIENT_POINTY)
    {
        localDir = float2(
            SQRT3 * (cubeDir.x + 0.5 * cubeDir.z),
            1.5 * cubeDir.z
        );
    }
    else
    {
        localDir = float2(
            1.5 * cubeDir.x,
            SQRT3 * (cubeDir.z + 0.5 * cubeDir.x)
        );
    }

    return normalize(localDir);
}

// ------------------------------------------------------------
// Sampling Helpers
// ------------------------------------------------------------

int SampleCellState(int2 axial, UnityTexture2D dataTex, UnitySamplerState samp, float2 texCenter)
{
    int2 uv = axial + int2(texCenter);
    float value = dataTex.Load(int3(uv, 0)).r;
    return (value > 0.5) ? 1 : 0;
}

void SampleNeighbors(int2 axial, UnityTexture2D dataTex, UnitySamplerState samp, float2 texCenter, out int neighbors[6])
{
    [unroll] for (int i = 0; i < 6; i++)
    {
        int2 uv = axial + AXIAL_NEIGHBORS[i] + int2(texCenter);
        float value = dataTex.Load(int3(uv, 0)).r;
        neighbors[i] = (value > 0.5) ? 1 : 0;
    }
}

// ------------------------------------------------------------
// Border Mask Utilities
// ------------------------------------------------------------

float InsetMask(float distance, float thickness)
{
    return saturate(1.0 - distance / thickness);
}

float CenteredMask(float distance, float halfWidth)
{
    return saturate(1.0 - abs(distance) / halfWidth);
}

float CombineMasks(float a, float b)
{
    return max(a, b);
}

float CombineEdgeMasks(float distances[6], float thickness)
{
    float m = 0.0;
    [unroll] for (int i = 0; i < 6; i++)
        m = max(m, InsetMask(distances[i], thickness));
    return m;
}

int3 RoundedCube(float2 worldPos, float2 gridOrigin, float hexRadius, int roundedOrientation)
{
    float2 axial = WorldToAxial(worldPos, gridOrigin, hexRadius, roundedOrientation);
    float3 cube = AxialToCube(axial);
    return CubeRound(cube);
}

// ------------------------------------------------------------
// Main Hexagon Shader Logic
// ------------------------------------------------------------

void GetHexState_float(
    float2 worldPos, 
    float2 gridOrigin, 
    float hexRadius, 
    float orientation, 
    UnityTexture2D dataTex, 
    UnitySamplerState dataSamp, 
    float2 texCenter, 
    out float state)
{
    int roundedOrientation = round(orientation);
    int3 roundedCube = RoundedCube(worldPos, gridOrigin, hexRadius, roundedOrientation);
    int2 axialInt = int2(roundedCube.x, roundedCube.z);

    state = SampleCellState(axialInt, dataTex, dataSamp, texCenter);
}

void GetHexInactiveFill_float(
    float2 worldPos,
    float2 gridOrigin,
    float hexRadius,
    float orientation,
    float hexState,
    out float fill)
{
    if (hexState > 0.5)
    {
        fill = 0.0;
        return;
    }

    int roundedOrientation = (int)round(orientation);
    int3 roundedCube = RoundedCube(worldPos, gridOrigin, hexRadius, roundedOrientation);

    float2 axial = WorldToAxial(worldPos, gridOrigin, hexRadius, roundedOrientation);
    float3 cube = AxialToCube(axial);
    float2 localPos = CubeToLocalPos(cube, roundedCube, hexRadius, roundedOrientation);

    float2 normals[6];
    GetHexNormals(roundedOrientation, normals);

    float distances[6];
    ComputeEdgeDistances(localPos, normals, hexRadius, distances);

    float inside = 1.0;
    [unroll] for (int i = 0; i < 6; i++)
        inside = min(inside, step(0.0, distances[i]));

    fill = inside;
}

void GetHexOutline_float(
    float2 worldPos,
    float2 gridOrigin,
    float hexRadius,
    float orientation,
    float centerState,
    float thickness,
    UnityTexture2D dataTex,
    UnitySamplerState dataSamp,
    float2 texCenter,
    out float outlineMask)
{
    if (centerState < 0.5)
    {
        outlineMask = 0.0;
        return;
    }

    int roundedOrientation = (int)round(orientation);
    int3 roundedCube = RoundedCube(worldPos, gridOrigin, hexRadius, roundedOrientation);
    int2 axialInt    = int2(roundedCube.x, roundedCube.z);

    float2 axial = WorldToAxial(worldPos, gridOrigin, hexRadius, roundedOrientation);
    float3 cube  = AxialToCube(axial);
    float2 localPos = CubeToLocalPos(cube, roundedCube, hexRadius, roundedOrientation);
    float apothem = HALF_SQRT3 * hexRadius;

    int neighbors[6];
    SampleNeighbors(axialInt, dataTex, dataSamp, texCenter, neighbors);

    float mask = 0.0;
    [unroll] for (int i = 0; i < 6; i++)
    {
        if (neighbors[i] == 0)
        {
            float2 normal = ComputeNeighborNormal(i, roundedOrientation);
            float distance = apothem - dot(normal, localPos);
            float edgeMask = InsetMask(distance, thickness);

            mask = max(mask, edgeMask);
        }
    }

    outlineMask = mask;
}

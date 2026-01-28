#ifndef HEX_PROJECTION_HLSL
#define HEX_PROJECTION_HLSL

static const float SQRT3 = 1.73205080757;
static const float ONE_THIRD = 1.0 / 3.0;
static const float TWO_THIRDS = 2.0 / 3.0;
static const float SQRT3_OVER_3 = SQRT3 / 3.0; // sqrt(3) / 3

static const float2 neighbors[6] = {
    float2(0, -1),
    float2(1, -1),
    float2(1, 0),
    float2(0, 1),
    float2(-1, 1),
    float2(-1, 0)
};

// Converts Cartesian coordinates to axial hex coordinates
void ToHex_float(float2 worldPos, float radius, float isPointy, out float2 hex)
{
    if (isPointy > 0.5)
    {
        float q = (SQRT3_OVER_3 * worldPos.x - ONE_THIRD * worldPos.y) / radius;
        float r = (TWO_THIRDS * worldPos.y) / radius;
        hex = float2(q, r);
    }
    else
    {
        float q = (TWO_THIRDS * worldPos.x) / radius;
        float r = (SQRT3_OVER_3 * worldPos.y - ONE_THIRD * worldPos.x) / radius;
        hex = float2(q, r);
    }
}

// Converts axial hex coordinates to Cartesian coordinates
void FromHex_float(float2 hex, float radius, float isPointy, out float2 worldPos)
{
    float2 result;
    if (isPointy > 0.5)
    {
        result.x = radius * SQRT3 * (hex.x + hex.y * 0.5);
        result.y = radius * 1.5 * hex.y;
    }
    else
    {
        result.x = radius * 1.5 * hex.x;
        result.y = radius * SQRT3 * (hex.y + hex.x * 0.5);
    }
    worldPos = result;
}

// Rounds axial hex coordinates to the nearest hex
void RoundAxial_float(float2 hex, out float2 roundedHex)
{
    float x = hex.x;
    float z = hex.y;
    float y = -x - z;

    float rx = round(x);
    float ry = round(y);
    float rz = round(z);

    float dx = abs(rx - x);
    float dy = abs(ry - y);
    float dz = abs(rz - z);

    if (dx > dy && dx > dz) rx = -ry - rz;
    else if (dy > dz)       ry = -rx - rz;
    else                    rz = -rx - ry;
    
    roundedHex = float2(rx, rz);
}

// Computes the distance from the center of a hex to a point in local hex coordinates
void HexDistance_float(float2 local, float radius, float isPointy, out float distance)
{
    float2 v = abs(local);
    float d;

    if (isPointy < 0.5) d = max(v.y, v.x * 0.86602540 + v.y * 0.5);
    else                d = max(v.x, v.y * 0.86602540 + v.x * 0.5);

    distance = d / radius;
}

// Converts axial hex coordinates to UV coordinates in the range [0, 1]
void AxialToUV_float(float2 axial, float2 gridSize, out float2 uv)
{
    float u = (axial.x + gridSize.x * 0.5) / gridSize.x;
    float v = (axial.y + gridSize.y * 0.5) / gridSize.y;
    uv = float2(u, v);
}

// Samples hex visibility from a texture (stub implementation)
void SampleHexVisibility_float(UnityTexture2D dataTex, UnitySamplerState ss, float2 roundedHex, float2 gridSize, out float visibility)
{
    float u = (roundedHex.x / gridSize.x) + 0.5;
    float v = (roundedHex.y / gridSize.y) + 0.5;

    float4 data = dataTex.SampleLevel(ss, float2(u, v), 0);
    float visible = data.r;

    visibility = visible;
}

#endif // HEX_PROJECTION_HLSL
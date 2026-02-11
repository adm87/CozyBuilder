# HEX GRID SHADER — SYSTEM DESIGN SPECIFICATION
Unity URP — Shader Graph + HLSL Helpers  
Author: Aaron  
Purpose: Deterministic, contributor‑proof hex grid outline rendering with neighbor‑aware logic.

---

## 1. Overview

This document defines the architecture for a Unity URP shader that renders **hexagonal grid outlines** using Shader Graph + HLSL helper functions.

The shader supports:

- Pointy‑top or flat‑top orientation (runtime switch)
- World‑space grid positioning
- Uniform cell size
- R8_UNORM data texture (1 texel = 1 cell)
- Neighbor‑aware border logic (inset vs centered)
- Deterministic, invariant‑safe behavior

The system is structured in explicit layers to ensure clarity, maintainability, and correctness.

## 2. Architectural Layers

---

### 2.1 Coordinate System Layer

Converts world space into a stable indexing space (axial coordinates).

#### 2.1.1 World → Axial Conversion

Orientation determines the basis vectors used to convert world positions into axial coordinates:

- **Pointy‑top orientation** uses one basis
- **Flat‑top orientation** uses another

Public API:

WorldToAxial(worldPos, origin, cellSize, orientation)


This is the only orientation‑dependent part of the indexing pipeline.

#### 2.1.2 Axial → Cube Coordinates

Cube coordinates (x, y, z) provide:

- Stable rounding
- Stable neighbor offsets
- Stable local‑position math

#### 2.1.3 Cube Rounding

Cube coordinates are rounded to the nearest hex center.

#### 2.1.4 Local Hex Position

localPos = cube - cubeRounded


This produces a 2D position inside the hex cell, centered at (0,0), used for border distance tests.

---

### 2.2 Data Texture Layer (Orientation‑Agnostic)

The R8_UNORM texture maps directly to axial coordinates:

texel (q, r) → hex cell (q, r)


#### 2.2.1 SampleCellState

Returns 0 (OFF) or 1 (ON).

#### 2.2.2 SampleNeighbors

Axial neighbors are universal and orientation‑agnostic:

(+1,  0)
(+1, -1)
( 0, -1)
(-1,  0)
(-1, +1)
( 0, +1)

These offsets never change.

## 2.3 Border Geometry Layer

Defines the shape of a hex cell and how border distances are computed.

---

### 2.3.1 Edge Normals

Hexes have 6 edges → 6 outward normals.

Normals depend on orientation:

- **Pointy‑top:** edges at 0°, 60°, 120°, 180°, 240°, 300°
- **Flat‑top:** edges at 30°, 90°, 150°, 210°, 270°, 330°

---

### 2.3.2 Distance to Edge

For each edge:

distanceToEdge = edgeDistance - dot(localPos, edgeNormal)


---

### 2.3.3 Inset Border Mask

If neighbor is OFF:

- Border is fully inside the cell
- Thickness is a 0–1 fraction of cell size

---

### 2.3.4 Centered Border Mask

If neighbor is ON:

- Border is centered on the shared edge
- Half thickness in each cell

## 2.4 Neighbor‑Aware Border Logic Layer

For each of the 6 edges:

if self OFF     → no border
if neighbor OFF → inset border
if neighbor ON  → centered border

Masks from all 6 edges are OR’d together.

This produces:

- Clean outlines around isolated cells
- Seamless shared borders between adjacent active cells
- No bleeding outside the cell bounds

## 2.5 Shader Graph Layer

Shader Graph orchestrates the system using Custom Function nodes.

---

### 2.5.1 Inputs

- `_CellSize` (float)
- `_Thickness` (0–1, fraction of cell size)
- `_GridOrigin` (float2)
- `_Orientation` (int: 0 = pointy, 1 = flat)
- `_DataTex` (R8_UNORM)
- `_LineColor`
- `_BackgroundColor`

---

### 2.5.2 Custom Function Nodes

- `WorldToAxial`
- `AxialToCube`
- `CubeRound`
- `ComputeLocalHexPos`
- `SampleCellState`
- `SampleNeighbors`
- `HexBorderMaskWithNeighbors`

This node outputs a **0–1 mask** representing the outline intensity.

---

### 2.5.3 Final Color

finalColor = lerp(_BackgroundColor, _LineColor, mask)

Where:

- `mask = 0` → background color  
- `mask = 1` → full line color  
- intermediate values allow anti‑aliasing or soft edges (added later in polish)


# 3. Build Plan (Implementation Roadmap)

---

### Step 1 — World→Axial conversion (both orientations)

- Define basis vectors for pointy‑top and flat‑top
- Implement axial mapping
- Implement cube conversion + rounding
- Implement local hex position computation

---

### Step 2 — Inset hex borders (no data texture)

- Compute distance to 6 edges using localPos and edge normals
- OR masks from all edges
- Visualize geometry to validate shape and thickness

---

### Step 3 — Integrate data texture

- Sample ON/OFF state from R8_UNORM texture in axial space
- If OFF → mask = 0 (no border)
- If ON → use inset border mask

---

### Step 4 — Neighbor sampling

- Sample 6 axial neighbors using fixed offsets
- Optionally visualize neighbor states for debugging

---

### Step 5 — Neighbor‑aware border shifting

- For each edge:
  - If neighbor OFF → inset border
  - If neighbor ON → centered border on shared edge
- Combine all 6 edge masks into final border mask

---

### Step 6 — Polish

- Anti‑aliasing
- Color blending
- Optional: fill, hover, selection, etc.

# 4. Invariants (System Guarantees)

- Axial coordinates are the canonical indexing space.
- Neighbor offsets are constant and orientation‑agnostic.
- Orientation affects only:
  - world→axial mapping
  - edge normals
  - local hex geometry
- Data texture is always sampled in axial space.
- Border logic is always per‑edge and deterministic.
- No border ever bleeds outside the cell.

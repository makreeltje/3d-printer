import lib3mf
import numpy as np


def compute_volume(vertices, triangles):
    volume = 0.0
    for tri in triangles:
        # Assuming tri has methods to get vertex indices (e.g., GetV1(), etc.)
        idx0 = tri.GetV1()
        idx1 = tri.GetV2()
        idx2 = tri.GetV3()
        v0 = np.array([vertices[idx0].GetX(), vertices[idx0].GetY(), vertices[idx0].GetZ()])
        v1 = np.array([vertices[idx1].GetX(), vertices[idx1].GetY(), vertices[idx1].GetZ()])
        v2 = np.array([vertices[idx2].GetX(), vertices[idx2].GetY(), vertices[idx2].GetZ()])
        tetra_volume = np.dot(v0, np.cross(v1, v2)) / 6.0
        volume += tetra_volume
    return abs(volume)


def main():
    model = lib3mf.Model()

    try:
        # Load your 3MF file
        model.ReadFromFile("parts.3mf")
    except Exception as e:
        print(f"Error reading 3MF file: {e}")
        return

    # This part may vary: explore the model object to locate your build items.
    try:
        build = model.GetBuild()
    except Exception as e:
        print(f"Could not retrieve build items: {e}")
        return

    # If build is iterable or has methods to get individual items, use them here.
    # For demonstration, let's assume you can get the first item:
    try:
        first_item = build.GetItem(0)
        mesh = first_item.GetMesh()  # Adjust according to the actual API.
    except Exception as e:
        print(f"Error retrieving mesh: {e}")
        return

    # Get vertices and triangles from the mesh (API may differ)
    try:
        vertices = mesh.GetVertices()  # For example, might return a list of vertex objects.
        triangles = mesh.GetTriangles()  # Similarly, a list of triangle objects.
    except Exception as e:
        print(f"Error retrieving geometry data: {e}")
        return

    print(f"Found {len(vertices)} vertices and {len(triangles)} triangles.")

    volume = compute_volume(vertices, triangles)
    print(f"Computed Volume: {volume:.2f} cubic units")

    # Continue with filament estimation, etc.
    # Example for filament usage estimation:
    infill_percentage = 15
    adjusted_volume = volume * (infill_percentage / 100.0)
    density = 1.24  # g/cm^3 for PLA
    weight = adjusted_volume * density
    cost_per_gram = 0.05
    cost = weight * cost_per_gram

    print(f"Adjusted Volume: {adjusted_volume:.2f} cm^3")
    print(f"Estimated Filament Weight: {weight:.2f} grams")
    print(f"Estimated Cost: ${cost:.2f}")


if __name__ == '__main__':
    main()
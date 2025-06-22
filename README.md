# Squares API

## What It Does
This API helps manage 2D points and automatically finds all possible **squares** made from those points.

## Features
- Import a list of 2D points
- Add a single point
- Delete a point by its coordinates
- View all squares formed from stored points

## Tech Used
- .NET 8 (Web API)
- Entity Framework Core (with SQLite)
- Swagger (for API documentation)
- xUnit (for testing)
- Logging (Microsoft.Extensions.Logging)
- Containerization-ready (optional: Docker)

## How to Run
1. Clone the repository
2. Run the project:
   ```bash
   dotnet run
3. Open Swagger UI in browser:
https://localhost:7038/swagger/index.html

## Example Input - POST /api/points/import
[
  { "coordinateX": -1, "coordinateY": 1 },
  { "coordinateX": 1, "coordinateY": 1 },
  { "coordinateX": 1, "coordinateY": -1 },
  { "coordinateX": -1, "coordinateY": -1 }
]

## Output - GET /api/squares
[
  {
    "square": [
      { "coordinateX": -1, "coordinateY": 1 },
      { "coordinateX": 1, "coordinateY": 1 },
      { "coordinateX": 1, "coordinateY": -1 },
      { "coordinateX": -1, "coordinateY": -1 }
    ]
  }
]

## Testing
Run tests with: dotnet test

## Notes
* Squares are detected only if all 4 corners exist in the database.

* Performance is optimized using HashSet for fast lookups.

* Duplicate squares are filtered using coordinate-based signatures.

* Database is seeded with example points so you can try detecting squares immediately.

## Time spent
8+ hours


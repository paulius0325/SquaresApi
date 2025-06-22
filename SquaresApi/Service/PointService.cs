using Microsoft.EntityFrameworkCore;
using SquaresApi.Dtos.PointDtos;
using SquaresApi.Dtos.Square;
using SquaresApi.Models;
using SquaresApi.Repository;
using SquaresApi.ServiceResponder;

namespace SquaresApi.Service
{
    public class PointService : IPointService
    {
        private readonly IPointRepository _pointRepository;
        private readonly ILogger<PointService> _logger;

        public PointService(IPointRepository pointRepository, ILogger<PointService> logger)
        {
            _pointRepository = pointRepository;
            _logger = logger;
        }

        // Imports a list of points into the database
        public async Task<ServiceResponse<List<PointDto>>> ImportListOfPointsAsync(List<ImportPointDto> importPoints)
        {
            // Check for duplicates before adding
            var existingPoints = await _pointRepository.GetPointsByCoordinatesAsync(
                importPoints.Select(p => (p.CoordinateX, p.CoordinateY)).ToList());

            if (existingPoints.Any())
            {
                var firstDuplicate = existingPoints.First();

                _logger.LogWarning("Duplicate point detected during import: ({X},{Y})",
                    firstDuplicate.CoordinateX, firstDuplicate.CoordinateY);

                return new ServiceResponse<List<PointDto>>
                {
                    Success = false,
                    Error = $"Point ({firstDuplicate.CoordinateX}, {firstDuplicate.CoordinateY}) already exists."
                };
            }

            //Manual mapping used here for simplicity. In a bigger solution, AutoMapper would be preferred.
            var pointEntities = importPoints.Select(dto => new Point
            {
                CoordinateX = dto.CoordinateX,
                CoordinateY = dto.CoordinateY,
            }).ToList();
 
                await _pointRepository.AddPointsAsync(pointEntities);

            _logger.LogInformation("Successfully imported {Count} new points.", pointEntities.Count);

            var result = pointEntities.Select(p => new PointDto
                {
                    PointId = p.PointId,
                    CoordinateX = p.CoordinateX,
                    CoordinateY = p.CoordinateY,
                }).ToList();

                return new ServiceResponse<List<PointDto>>
                {
                    Data = result,
                    Success = true,
                    Message = "Points imported Successfully"
                };
        }

        // Adds a single point to the database
        public async Task<ServiceResponse<PointDto>> AddPointAsync(ImportPointDto newPoint)
        {
            var pointEntitie = new Point
            {
                CoordinateX = newPoint.CoordinateX,
                CoordinateY = newPoint.CoordinateY,
            };

            try
            { 
                var addedPoint = await _pointRepository.AddPointAsync(pointEntitie);

                _logger.LogInformation("Added point ({X},{Y})", newPoint.CoordinateX, newPoint.CoordinateY);

                var result = new PointDto
                {
                    PointId = addedPoint.PointId,
                    CoordinateX = addedPoint.CoordinateX,
                    CoordinateY = addedPoint.CoordinateY,
                };

                return new ServiceResponse<PointDto>
                {
                    Data = result,
                    Success = true,
                    Message = "Single point was added to the list"
                };
            }
            catch (DbUpdateException )
            {
                _logger.LogWarning("Attempted to add duplicate point ({X},{Y})", newPoint.CoordinateX, newPoint.CoordinateY);

                return new ServiceResponse<PointDto>
                {
                    Success = false,
                    Error = $"Point ({newPoint.CoordinateX}, {newPoint.CoordinateY}) already exists."
                };
            }
        }

        // Deletes a point by its ID
        public async Task<ServiceResponse<bool>> DeletePointByCoordinatesAsync(int x, int y)
        {
            var result = await _pointRepository.DeletePointByCoordinatesAsync(x, y);

            if (!result)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    Success = false,
                    Error = $"Point with Id ({x},{y}) does not exist."
                };
            }

            return new ServiceResponse<bool>
            {
                Data = true,
                Success = true,
                Message = "Point has been deleted successfully"
            };
        }

        // Retrieves all points and returns a list of squares found among them
        public async Task<ServiceResponse<List<SquareDto>>> GetSquaresAsync()
        {
            var points = await _pointRepository.GetAllPointsAsync();
            var squares = FindSquares(points);

            return new ServiceResponse<List<SquareDto>> { Data = squares };
        }

        // Helper Method for: Brute-force O(n²) with HashSet and duplicate filtering
        private List<SquareDto> FindSquares(List<Point> points)
        {
            
            var pointSet = new HashSet<string>(points.Select(p => $"{p.CoordinateX},{p.CoordinateY}"));

           
            var result = new HashSet<string>();

            var squares = new List<SquareDto>();
            int n = points.Count;

            // Map from coordinate string to PointDto for reuse
            var pointMap = points.ToDictionary(
                p => $"{p.CoordinateX},{p.CoordinateY}",
                p => new PointDto
                {
                    PointId = p.PointId,
                    CoordinateX = p.CoordinateX,
                    CoordinateY = p.CoordinateY
                });

            // Iterate over all unique point pairs
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    // Trying both clockwise and counter-clockwise orientations
                    var square1 = TryFormSquare(points[i], points[j], pointSet, pointMap, clockwise: true);
                    var square2 = TryFormSquare(points[i], points[j], pointSet, pointMap, clockwise: false);

                    foreach (var square in new[] { square1, square2 })
                    {
                      
                        if (square == null || square.Square!.DistinctBy(p => (p.CoordinateX, p.CoordinateY)).Count() != 4)
                            continue;

                        // Normalize square representation for duplicate filtering
                        var key = string.Join("|", square.Square!
                            .OrderBy(p => p.CoordinateX)
                            .ThenBy(p => p.CoordinateY)
                            .Select(p => $"{p.CoordinateX},{p.CoordinateY}"));

                        if (!result.Contains(key))
                        {
                            result.Add(key);
                            squares.Add(square);
                        }
                    }
                }
            }

            _logger.LogInformation("Detected {Count} unique squares from {PointCount} points.", squares.Count, points.Count);

            return squares;
        }

        // Completes a square from two given points by calculating the other two corners.
        private SquareDto? TryFormSquare(Point a, Point b, HashSet<string> pointSet, Dictionary<string, PointDto> pointMap, bool clockwise)
        {
            int dx = b.CoordinateX - a.CoordinateX;
            int dy = b.CoordinateY - a.CoordinateY;

            if (dx == 0 && dy == 0)
                return null;

            // Coordinates of the other two corners
            int cx, cy, dx2, dy2;

            if (clockwise)
            {
                // Rotate 90° clockwise
                cx = b.CoordinateX - dy;
                cy = b.CoordinateY + dx;

                dx2 = a.CoordinateX - dy;
                dy2 = a.CoordinateY + dx;
            }
            else
            {
                // Rotate 90° counter-clockwise
                cx = b.CoordinateX + dy;
                cy = b.CoordinateY - dx;

                dx2 = a.CoordinateX + dy;
                dy2 = a.CoordinateY - dx;
            }

            // Check if both additional corners exist in the point set
            if (pointSet.Contains($"{cx},{cy}") && pointSet.Contains($"{dx2},{dy2}"))
            {
                pointMap.TryGetValue($"{a.CoordinateX},{a.CoordinateY}", out var pointA);
                pointMap.TryGetValue($"{b.CoordinateX},{b.CoordinateY}", out var pointB);
                pointMap.TryGetValue($"{cx},{cy}", out var pointC);
                pointMap.TryGetValue($"{dx2},{dy2}", out var pointD);

                // Return the square if all points exist
                if (pointA != null && pointB != null && pointC != null && pointD != null)
                {
                    return new SquareDto
                    {
                        Square = new List<PointDto> { pointA, pointB, pointC, pointD }
                    };
                }
            }

            return null;
        }

    }
}

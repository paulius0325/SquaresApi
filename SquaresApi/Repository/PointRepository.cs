using Microsoft.EntityFrameworkCore;
using SquaresApi.Data;
using SquaresApi.Dtos.PointDtos;
using SquaresApi.Dtos.Square;
using SquaresApi.Models;
using SquaresApi.ServiceResponder;

namespace SquaresApi.Repository
{
    public class PointRepository : IPointRepository
    {
        private readonly PointsContext _pointsContext;

        public PointRepository(PointsContext pointsContext)
        {
            _pointsContext = pointsContext;
        }

        public async Task<List<Point>> GetPointsByCoordinatesAsync(List<(int CoordinateX, int CoordinateY)> coordinates)
        {
            var distinctCoords = coordinates.Distinct().ToList();

            // Extract X and Y into separate lists
            var xValues = distinctCoords.Select(c => c.CoordinateX).ToList();
            var yValues = distinctCoords.Select(c => c.CoordinateY).ToList();

            // Bring all points that *might* match to memory
            var allPoints = await _pointsContext.Points
                .Where(p => xValues.Contains(p.CoordinateX) && yValues.Contains(p.CoordinateY))
                .ToListAsync();

            // Final filter in memory
            var matchingPoints = allPoints
                .Where(p => distinctCoords.Contains((p.CoordinateX, p.CoordinateY)))
                .ToList();

            return matchingPoints;
        }

        //Adds single 2D point to the database 
        public async Task<Point> AddPointAsync(Point point)
        {
            _pointsContext.Points.Add(point);
            await _pointsContext.SaveChangesAsync();
            return point;
        }

        //Adds list of 2D points to the database
        public async Task<List<Point>> AddPointsAsync(List<Point> points)
        {
            await _pointsContext.Points.AddRangeAsync(points);
            await _pointsContext.SaveChangesAsync();
            return points;
        }

        // Deletes single 2D point by coordinates from database
        public async Task<bool> DeletePointByCoordinatesAsync(int x, int y)
        {
            var point = await _pointsContext.Points
                .FirstOrDefaultAsync(p => p.CoordinateX == x && p.CoordinateY == y);

            if (point == null)
                return false;

            _pointsContext.Points.Remove(point);
            await _pointsContext.SaveChangesAsync();
            return true;
        }

        // Retrieves all stored points for square calculation
        public async Task<List<Point>> GetAllPointsAsync()
        {
            return await _pointsContext.Points.ToListAsync();
        }
    }
}

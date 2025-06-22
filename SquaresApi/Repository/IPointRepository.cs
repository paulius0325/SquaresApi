using SquaresApi.Dtos.PointDtos;
using SquaresApi.Models;

namespace SquaresApi.Repository
{
    public interface IPointRepository
    {
        Task<List<Point>> GetAllPointsAsync();
        Task<List<Point>> AddPointsAsync(List<Point> points);
        Task<Point> AddPointAsync(Point point);
        Task<bool> DeletePointByCoordinatesAsync(int x, int y);
        Task<List<Point>> GetPointsByCoordinatesAsync(List<(int CoordinateX, int CoordinateY)> coordinates);
    }
}

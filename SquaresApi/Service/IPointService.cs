using SquaresApi.Dtos.PointDtos;
using SquaresApi.Dtos.Square;
using SquaresApi.ServiceResponder;

namespace SquaresApi.Service
{
    public interface IPointService
    {
        Task<ServiceResponse<List<PointDto>>> ImportListOfPointsAsync(List<ImportPointDto> importPoints);
        Task<ServiceResponse<PointDto>> AddPointAsync(ImportPointDto newPoint);
        Task<ServiceResponse<bool>> DeletePointByCoordinatesAsync(int x, int y);
        Task<ServiceResponse<List<SquareDto>>> GetSquaresAsync();
    }
}

using Microsoft.Extensions.Logging;
using Moq;
using SquaresApi.Models;
using SquaresApi.Repository;
using SquaresApi.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingSquaresApi.Services
{
    public class PointServiceTest
    {
        [Fact]
        public async Task GetSquaresAsync_CompletesUnder5Seconds()
        {
            // Arrange
            var mockRepo = new Mock<IPointRepository>();
            var mockLogger = new Mock<ILogger<PointService>>();

            // Simulate 100 points forming multiple squares
            var points = new List<Point>();
            for (int i = 0; i < 10; i++) // grid: 10x10
            {
                for (int j = 0; j < 10; j++)
                {
                    points.Add(new Point { CoordinateX = i, CoordinateY = j });
                }
            }

            mockRepo.Setup(r => r.GetAllPointsAsync()).ReturnsAsync(points);
            var service = new PointService(mockRepo.Object, mockLogger.Object);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await service.GetSquaresAsync();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Execution took too long: {stopwatch.ElapsedMilliseconds}ms");
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }
    }
}

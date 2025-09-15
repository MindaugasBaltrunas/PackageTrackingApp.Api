using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Domain.Interfaces;
using PackageTrackingApp.Service.Dtos;
using PackageTrackingApp.Service.Interfaces;
using PackageTrackingApp.Service.Services;

namespace Tests.Servics
{
    public class BaseServiceTests
    {
        private readonly Mock<IBaseRepository<BaseEntity>> repositoryMock;
        private readonly Mock<IValidator<BaseEntity>> validatorMock;
        private readonly Mock<IResultFactory> resultFactoryMock;
        private readonly Fixture fixture;
        private readonly BaseService<BaseEntity> service;

        public BaseServiceTests()
        {
            repositoryMock = new Mock<IBaseRepository<BaseEntity>>();
            validatorMock = new Mock<IValidator<BaseEntity>>();
            resultFactoryMock = new Mock<IResultFactory>();
            fixture = new Fixture();

            service = new BaseService<BaseEntity>(
                repositoryMock.Object,
                validatorMock.Object,
                resultFactoryMock.Object);
        }

        [Fact]
        public void Constructor_WhenRepositoryIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BaseService<BaseEntity>(null!, validatorMock.Object, resultFactoryMock.Object));
        }

        [Fact]
        public void Constructor_WhenValidatorIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BaseService<BaseEntity>(repositoryMock.Object, null!, resultFactoryMock.Object));
        }

        [Fact]
        public void Constructor_WhenResultFactoryIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BaseService<BaseEntity>(repositoryMock.Object, validatorMock.Object, null!));
        }

        [Fact]
        public async Task AddEntityAsync_WhenValidationFails_ReturnsFailureResult()
        {
            // Arrange
            var entity = fixture.Create<BaseEntity>();
            var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Property1", "Error message 1"),
            new ValidationFailure("Property2", "Error message 2")
        };
            var validationResult = new ValidationResult(validationErrors);
            var expectedFailureResult = Mock.Of<Result<BaseEntity>>();

            validatorMock.Setup(v => v.ValidateAsync(entity, default))
                         .ReturnsAsync(validationResult);

            resultFactoryMock.Setup(f => f.CreateFailure<BaseEntity>(
                                 It.Is<List<string>>(errors =>
                                     errors.Contains("Error message 1") &&
                                     errors.Contains("Error message 2"))))
                             .Returns(expectedFailureResult);

            // Act
            var result = await service.AddEntityAsync(entity);

            // Assert
            Assert.Equal(expectedFailureResult, result);
            validatorMock.Verify(v => v.ValidateAsync(entity, default), Times.Once);
            repositoryMock.Verify(r => r.AddAsync(It.IsAny<BaseEntity>()), Times.Never);
            resultFactoryMock.Verify(f => f.CreateSuccess(It.IsAny<BaseEntity>()), Times.Never);
        }

        [Fact]
        public async Task AddEntityAsync_WhenValidationPassesAndAddSucceeds_ReturnsSuccessResult()
        {
            // Arrange
            var entity = fixture.Create<BaseEntity>();
            var addedEntity = fixture.Create<BaseEntity>();
            var validationResult = new ValidationResult(); 
            var expectedSuccessResult = Mock.Of<Result<BaseEntity>>();

            validatorMock.Setup(v => v.ValidateAsync(entity, default))
                         .ReturnsAsync(validationResult);

            repositoryMock.Setup(r => r.AddAsync(entity))
                          .ReturnsAsync(addedEntity);

            resultFactoryMock.Setup(f => f.CreateSuccess(addedEntity))
                             .Returns(expectedSuccessResult);

            // Act
            var result = await service.AddEntityAsync(entity);

            // Assert
            Assert.Equal(expectedSuccessResult, result);
            validatorMock.Verify(v => v.ValidateAsync(entity, default), Times.Once);
            repositoryMock.Verify(r => r.AddAsync(entity), Times.Once);
            resultFactoryMock.Verify(f => f.CreateSuccess(addedEntity), Times.Once);
            resultFactoryMock.Verify(f => f.CreateFailure<BaseEntity>(It.IsAny<List<string>>()), Times.Never);
        }

        [Fact]
        public async Task AddEntityAsync_WhenExceptionOccurs_ReturnsFailureResult()
        {
            // Arrange
            var entity = fixture.Create<BaseEntity>();
            var validationResult = new ValidationResult(); // Valid result
            var exceptionMessage = "Database connection failed";
            var exception = new Exception(exceptionMessage);
            var expectedFailureResult = Mock.Of<Result<BaseEntity>>();

            validatorMock.Setup(v => v.ValidateAsync(entity, default))
                         .ReturnsAsync(validationResult);

            repositoryMock.Setup(r => r.AddAsync(entity))
                          .ThrowsAsync(exception);

            resultFactoryMock.Setup(f => f.CreateFailure<BaseEntity>(
                                 It.Is<List<string>>(errors => errors.Contains(exceptionMessage))))
                             .Returns(expectedFailureResult);

            // Act
            var result = await service.AddEntityAsync(entity);

            // Assert
            Assert.Equal(expectedFailureResult, result);
            validatorMock.Verify(v => v.ValidateAsync(entity, default), Times.Once);
            repositoryMock.Verify(r => r.AddAsync(entity), Times.Once);
            resultFactoryMock.Verify(f => f.CreateFailure<BaseEntity>(
                It.Is<List<string>>(errors => errors.Contains(exceptionMessage))), Times.Once);
            resultFactoryMock.Verify(f => f.CreateSuccess(It.IsAny<BaseEntity>()), Times.Never);
        }
    }
}

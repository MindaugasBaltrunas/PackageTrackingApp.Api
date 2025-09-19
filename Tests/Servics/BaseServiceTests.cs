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
     
    }
}

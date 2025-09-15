using AutoFixture;
using AutoMapper;
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
    public class PackageServiceTests
    {
        private readonly Mock<IPackageRepository> packageRepoMock;
        private readonly Mock<IValidator<PackageRequest>> validatorMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IResultFactory> resultFactoryMock;
        private readonly Mock<IBaseRepository<Sender>> senderRepoMock;
        private readonly Mock<IBaseRepository<Recipient>> recipientRepoMock;
        private readonly Mock<IBaseRepository<PackageStatusHistory>> packageStatusHistoryRepoMock;
        private readonly Mock<IValidStatusTransition> validStatusTransitionMock;
        private readonly Fixture fixture;
        private readonly PackageService service;

        public PackageServiceTests()
        {
            packageRepoMock = new Mock<IPackageRepository>();
            validatorMock = new Mock<IValidator<PackageRequest>>();
            mapperMock = new Mock<IMapper>();
            resultFactoryMock = new Mock<IResultFactory>();
            senderRepoMock = new Mock<IBaseRepository<Sender>>();
            recipientRepoMock = new Mock<IBaseRepository<Recipient>>();
            packageStatusHistoryRepoMock = new Mock<IBaseRepository<PackageStatusHistory>>();
            validStatusTransitionMock = new Mock<IValidStatusTransition>();

            fixture = new Fixture();

            // Handle circular references
            fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            service = new PackageService(
                packageRepoMock.Object,
                validatorMock.Object,
                mapperMock.Object,
                resultFactoryMock.Object,
                senderRepoMock.Object,
                recipientRepoMock.Object,
                packageStatusHistoryRepoMock.Object,
                validStatusTransitionMock.Object
            );
        }
        //AddPackageAsync

        [Fact]
        public async Task AddPackageAsync_WhenValid_PassesPackageWithCreatedAtAndStatusToRepository()
        {
            // Arrange
            var request = fixture.Create<PackageRequest>();
            var sender = fixture.Create<Sender>();
            var recipient = fixture.Create<Recipient>();
            var response = fixture.Create<PackageResponse>();
            var package = fixture.Create<Package>();

            validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            senderRepoMock.Setup(s => s.GetByIdAsync(request.SenderId)).ReturnsAsync(sender);
            recipientRepoMock.Setup(r => r.GetByIdAsync(request.RecipientId)).ReturnsAsync(recipient);

            package.SenderId = request.SenderId;
            package.RecipientId = request.RecipientId;
            package.CurrentStatus = PackageStatus.Created;

            mapperMock.Setup(m => m.Map<Package>(request)).Returns(package);
            mapperMock.Setup(m => m.Map<PackageResponse>(package)).Returns(response);

            Package? capturedPackage = null;
            packageRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Package>()))
                .Callback<Package>(p => capturedPackage = p)
                .ReturnsAsync(package);

            resultFactoryMock
                .Setup(f => f.CreateSuccess(response))
                .Returns(Mock.Of<Result<PackageResponse>>());

            // Act
            var result = await service.AddPackageAsync(request);

            // Assert
            packageRepoMock.Verify(r => r.AddAsync(It.IsAny<Package>()), Times.Once);
            Assert.NotNull(capturedPackage);
            Assert.Equal(request.SenderId, capturedPackage!.SenderId);
            Assert.Equal(request.RecipientId, capturedPackage.RecipientId);
            Assert.Equal(PackageStatus.Created, capturedPackage.CurrentStatus);

            var diff = DateTime.UtcNow - capturedPackage.CreatedAt;
            Assert.True(diff < TimeSpan.FromSeconds(2), $"CreatedAt was not recent: {capturedPackage.CreatedAt}");

            mapperMock.Verify(m => m.Map<Package>(request), Times.Once);
            mapperMock.Verify(m => m.Map<PackageResponse>(package), Times.Once);
            validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
            resultFactoryMock.Verify(f => f.CreateSuccess(response), Times.Once);
        }

        [Fact]
        public async Task AddPackageAsync_WhenValidationReturnsMultipleErrors_CallsCreateFailureWithAllErrors()
        {
            // Arrange
            var request = fixture.Create<PackageRequest>();
            var failures = new List<ValidationFailure>
            {
                new("Field1", "Error 1"),
                new("Field2", "Error 2")
            };

            validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            resultFactoryMock
                .Setup(f => f.CreateFailure<PackageResponse>(It.IsAny<List<string>>()))
                .Returns(Mock.Of<Result<PackageResponse>>());

            // Act
            var result = await service.AddPackageAsync(request);

            // Assert
            validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
            resultFactoryMock.Verify(f => f.CreateFailure<PackageResponse>(
                It.Is<List<string>>(l => l.Contains("Error 1") && l.Contains("Error 2"))), Times.Once);
            packageRepoMock.Verify(r => r.AddAsync(It.IsAny<Package>()), Times.Never);
        }

        // GetAllPackagesAsync()
       
        [Fact]
        public async Task GetAllPackagesAsync_WhenRepositoryReturnsNull_ReturnsFailureResult()
        {
            // Arrange
            packageRepoMock.Setup(r => r.GetAllAsync())
                  .Returns(() => Task.FromResult<List<Package>>(null!));

            // Act
            var result = await service.GetAllPackagesAsync();

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("packages not found", result.ErrorMessage);
            Assert.Null(result.Data);
            mapperMock.Verify(m => m.Map<List<PackageResponse>>(It.IsAny<List<Package>>()), Times.Never);
        }

        [Fact]
        public async Task GetAllPackagesAsync_WhenRepositoryReturnsEmptyList_ReturnsFailureResult()
        {
            // Arrange
            packageRepoMock.Setup(r => r.GetAllAsync())
                           .ReturnsAsync(new List<Package>());

            // Act
            var result = await service.GetAllPackagesAsync();

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("packages not found", result.ErrorMessage);
            Assert.Null(result.Data);
            mapperMock.Verify(m => m.Map<List<PackageResponse>>(It.IsAny<List<Package>>()), Times.Never);
        }

        [Fact]
        public async Task GetAllPackagesAsync_WhenRepositoryReturnsPackages_ReturnsSuccessResult()
        {
            // Arrange
            var packages = new List<Package>
            {
                fixture.Create<Package>(),
                fixture.Create<Package>()
            };
            var expectedResponseList = new List<PackageResponse>
            {
                fixture.Create<PackageResponse>(),
                fixture.Create<PackageResponse>()
            };

            packageRepoMock.Setup(r => r.GetAllAsync())
                           .ReturnsAsync(packages);

            mapperMock.Setup(m => m.Map<List<PackageResponse>>(packages))
                      .Returns(expectedResponseList);

            // Act
            var result = await service.GetAllPackagesAsync();

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(expectedResponseList, result.Data);
            Assert.Equal(expectedResponseList.Count, result.Data!.Count);

            mapperMock.Verify(m => m.Map<List<PackageResponse>>(packages), Times.Once);
        }

        [Fact]
        public async Task GetAllPackagesAsync_WhenRepositoryReturnsPackages_MapsCorrectlyAndReturnsSuccess()
        {
            // Arrange
            var package1 = fixture.Create<Package>();
            var package2 = fixture.Create<Package>();
            var packages = new List<Package> { package1, package2 };

            var response1 = fixture.Create<PackageResponse>();
            var response2 = fixture.Create<PackageResponse>();
            var expectedResponses = new List<PackageResponse> { response1, response2 };

            packageRepoMock.Setup(r => r.GetAllAsync())
                           .ReturnsAsync(packages);

            mapperMock.Setup(m => m.Map<List<PackageResponse>>(packages))
                      .Returns(expectedResponses);

            // Act
            var result = await service.GetAllPackagesAsync();

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(response1, result.Data);
            Assert.Contains(response2, result.Data);

            packageRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            mapperMock.Verify(m => m.Map<List<PackageResponse>>(packages), Times.Once);
        }

        [Fact]
        public async Task GetAllPackagesAsync_WhenRepositoryReturnsListWithOneItem_ReturnsSuccessResult()
        {
            // Arrange
            var package = fixture.Create<Package>();
            var packages = new List<Package> { package };
            var expectedResponse = fixture.Create<PackageResponse>();
            var expectedResponses = new List<PackageResponse> { expectedResponse };

            packageRepoMock.Setup(r => r.GetAllAsync())
                           .ReturnsAsync(packages);

            mapperMock.Setup(m => m.Map<List<PackageResponse>>(packages))
                      .Returns(expectedResponses);

            // Act
            var result = await service.GetAllPackagesAsync();

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(expectedResponse, result.Data.First());
        }

        // GetPackageByIdAsync(string packageId)

        [Fact]
        public async Task GetPackageByIdAsync_WhenPackageIdIsInvalid_ReturnsFailureResult()
        {
            // Arrange
            var invalidPackageId = "invalid-guid";

            // Act
            var result = await service.GetPackageByIdAsync(invalidPackageId);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("Invalid packageId", result.ErrorMessage);
            Assert.Null(result.Data);
            packageRepoMock.Verify(r => r.GetAsync(It.IsAny<Guid>()), Times.Never);
            mapperMock.Verify(m => m.Map<PackageResponse>(It.IsAny<Package>()), Times.Never);
        }

        [Fact]
        public async Task GetPackageByIdAsync_WhenValidIdButPackageNotFound_ReturnsFailureResult()
        {
            // Arrange
            var validGuid = Guid.NewGuid();
            var validPackageId = validGuid.ToString();

            packageRepoMock.Setup(r => r.GetAsync(validGuid))
                           .ReturnsAsync((Package)null!);

            // Act
            var result = await service.GetPackageByIdAsync(validPackageId);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("package not found", result.ErrorMessage);
            Assert.Null(result.Data);
            packageRepoMock.Verify(r => r.GetAsync(validGuid), Times.Once);
            mapperMock.Verify(m => m.Map<PackageResponse>(It.IsAny<Package>()), Times.Never);
        }

        [Fact]
        public async Task GetPackageByIdAsync_WhenValidIdAndPackageExists_ReturnsSuccessResult()
        {
            // Arrange
            var validGuid = Guid.NewGuid();
            var validPackageId = validGuid.ToString();
            var package = fixture.Create<Package>();
            var expectedResponse = fixture.Create<PackageResponse>();

            packageRepoMock.Setup(r => r.GetAsync(validGuid))
                           .ReturnsAsync(package);

            mapperMock.Setup(m => m.Map<PackageResponse>(package))
                      .Returns(expectedResponse);

            // Act
            var result = await service.GetPackageByIdAsync(validPackageId);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedResponse, result.Data);
            packageRepoMock.Verify(r => r.GetAsync(validGuid), Times.Once);
            mapperMock.Verify(m => m.Map<PackageResponse>(package), Times.Once);
        }

        // ExchangeStatusAsync(string packageId, int status)
        [Fact]
        public async Task ExchangeStatusAsync_WhenPackageIdIsInvalid_ReturnsFailureResult()
        {
            // Arrange
            var invalidPackageId = "invalid-guid";
            var status = 1;

            // Act
            var result = await service.ExchangeStatusAsync(invalidPackageId, status);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("Invalid packageId", result.ErrorMessage);
            Assert.Null(result.Data);
            packageRepoMock.Verify(r => r.GetAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ExchangeStatusAsync_WhenPackageNotFound_ReturnsFailureResult()
        {
            // Arrange
            var validGuid = Guid.NewGuid();
            var validPackageId = validGuid.ToString();
            var status = 1;

            packageRepoMock.Setup(r => r.GetAsync(validGuid))
                           .ReturnsAsync((Package)null!);

            // Act
            var result = await service.ExchangeStatusAsync(validPackageId, status);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("package not found", result.ErrorMessage);
            Assert.Null(result.Data);
            packageRepoMock.Verify(r => r.GetAsync(validGuid), Times.Once);
        }

        [Fact]
        public async Task ExchangeStatusAsync_WhenStatusIsSameAsCurrent_ReturnsSuccessWithoutUpdate()
        {
            // Arrange
            var validGuid = Guid.NewGuid();
            var validPackageId = validGuid.ToString();
            var currentStatus = PackageStatus.Sent;
            var package = fixture.Build<Package>()
                                 .With(p => p.CurrentStatus, currentStatus)
                                 .Create();
            var expectedResponse = fixture.Create<PackageResponse>();

            packageRepoMock.Setup(r => r.GetAsync(validGuid))
                           .ReturnsAsync(package);

            mapperMock.Setup(m => m.Map<PackageResponse>(package))
                      .Returns(expectedResponse);

            // Act
            var result = await service.ExchangeStatusAsync(validPackageId, (int)currentStatus);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedResponse, result.Data);

            // Verify no updates were made
            validStatusTransitionMock.Verify(v => v.Check(It.IsAny<PackageStatus>(), It.IsAny<PackageStatus>()), Times.Never);
            packageStatusHistoryRepoMock.Verify(r => r.AddAsync(It.IsAny<PackageStatusHistory>()), Times.Never);
            packageRepoMock.Verify(r => r.UpdateAsync(It.IsAny<PackageStatus>(), It.IsAny<Package>()), Times.Never);
        }

        [Fact]
        public async Task ExchangeStatusAsync_WhenStatusTransitionIsInvalid_ReturnsFailureResult()
        {
            // Arrange
            var validGuid = Guid.NewGuid();
            var validPackageId = validGuid.ToString();
            var currentStatus = PackageStatus.Sent;
            var newStatus = PackageStatus.Accepted;
            var package = fixture.Build<Package>()
                                 .With(p => p.CurrentStatus, currentStatus)
                                 .Create();

            packageRepoMock.Setup(r => r.GetAsync(validGuid))
                           .ReturnsAsync(package);

            validStatusTransitionMock.Setup(v => v.Check(currentStatus, newStatus))
                                     .Returns(false);

            // Act
            var result = await service.ExchangeStatusAsync(validPackageId, (int)newStatus);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("Invalid status transition", result.ErrorMessage);
            Assert.Null(result.Data);

            // Verify no updates were made
            packageStatusHistoryRepoMock.Verify(r => r.AddAsync(It.IsAny<PackageStatusHistory>()), Times.Never);
            packageRepoMock.Verify(r => r.UpdateAsync(It.IsAny<PackageStatus>(), It.IsAny<Package>()), Times.Never);
        }

        [Fact]
        public async Task ExchangeStatusAsync_WhenValidStatusTransition_UpdatesStatusAndReturnsSuccess()
        {
            // Arrange
            var validGuid = Guid.NewGuid();
            var validPackageId = validGuid.ToString();
            var currentStatus = PackageStatus.Sent;
            var newStatus = PackageStatus.Accepted;
            var package = fixture.Build<Package>()
                                 .With(p => p.Id, validGuid)
                                 .With(p => p.CurrentStatus, currentStatus)
                                 .Create();
            var updatedPackage = fixture.Build<Package>()
                                        .With(p => p.CurrentStatus, newStatus)
                                        .Create();
            var expectedResponse = fixture.Create<PackageResponse>();

            packageRepoMock.Setup(r => r.GetAsync(validGuid))
                           .ReturnsAsync(package);

            validStatusTransitionMock.Setup(v => v.Check(currentStatus, newStatus))
                                     .Returns(true);

            packageRepoMock.Setup(r => r.UpdateAsync(newStatus, package))
                           .ReturnsAsync(updatedPackage);

            mapperMock.Setup(m => m.Map<PackageResponse>(updatedPackage))
                      .Returns(expectedResponse);

            // Act
            var result = await service.ExchangeStatusAsync(validPackageId, (int)newStatus);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedResponse, result.Data);

            // Verify all updates were made
            packageStatusHistoryRepoMock.Verify(r => r.AddAsync(It.Is<PackageStatusHistory>(h =>
                h.PackageId == validGuid &&
                h.Status == currentStatus)), Times.Once);
            packageRepoMock.Verify(r => r.UpdateAsync(newStatus, package), Times.Once);
            mapperMock.Verify(m => m.Map<PackageResponse>(updatedPackage), Times.Once);
        }

        //FilterAllPackagesAsync(string? trackingNumber, int? status)
        [Fact]
        public async Task FilterAllPackagesAsync_WhenBothParametersProvided_ReturnsFailureResult()
        {
            // Arrange
            var trackingNumber = "TRK123";
            var status = 1;

            // Act
            var result = await service.FilterAllPackagesAsync(trackingNumber, status);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("Provide exactly one filter: either trackingNumber OR status (but not both).", result.ErrorMessage);
            Assert.Null(result.Data);
            packageRepoMock.Verify(r => r.FilterAllAsync(It.IsAny<string>(), It.IsAny<PackageStatus?>()), Times.Never);
        }

        [Fact]
        public async Task FilterAllPackagesAsync_WhenNoParametersProvided_ReturnsFailureResult()
        {
            // Arrange
            string? trackingNumber = null;
            int? status = null;

            // Act
            var result = await service.FilterAllPackagesAsync(trackingNumber, status);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("Provide exactly one filter: either trackingNumber OR status (but not both).", result.ErrorMessage);
            Assert.Null(result.Data);
            packageRepoMock.Verify(r => r.FilterAllAsync(It.IsAny<string>(), It.IsAny<PackageStatus?>()), Times.Never);
        }

        [Fact]
        public async Task FilterAllPackagesAsync_WhenTrackingNumberProvidedButNoPackagesFound_ReturnsFailureResult()
        {
            // Arrange
            var trackingNumber = "TRK123";
            int? status = null;

            packageRepoMock.Setup(r => r.FilterAllAsync(trackingNumber, null))
                           .ReturnsAsync(new List<Package>());

            // Act
            var result = await service.FilterAllPackagesAsync(trackingNumber, status);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("No packages found", result.ErrorMessage);
            Assert.Null(result.Data);
            packageRepoMock.Verify(r => r.FilterAllAsync(trackingNumber, null), Times.Once);
            mapperMock.Verify(m => m.Map<List<PackageResponse>>(It.IsAny<List<Package>>()), Times.Never);
        }

        [Fact]
        public async Task FilterAllPackagesAsync_WhenTrackingNumberProvidedAndPackagesFound_ReturnsSuccessResult()
        {
            // Arrange
            var trackingNumber = "TRK123";
            int? status = null;
            var packages = new List<Package>
            {
                fixture.Create<Package>(),
                fixture.Create<Package>()
            };
                    var expectedResponses = new List<PackageResponse>
            {
                fixture.Create<PackageResponse>(),
                fixture.Create<PackageResponse>()
            };

            packageRepoMock.Setup(r => r.FilterAllAsync(trackingNumber, null))
                           .ReturnsAsync(packages);

            mapperMock.Setup(m => m.Map<List<PackageResponse>>(packages))
                      .Returns(expectedResponses);

            // Act
            var result = await service.FilterAllPackagesAsync(trackingNumber, status);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedResponses, result.Data);
            packageRepoMock.Verify(r => r.FilterAllAsync(trackingNumber, null), Times.Once);
            mapperMock.Verify(m => m.Map<List<PackageResponse>>(packages), Times.Once);
        }

        [Fact]
        public async Task FilterAllPackagesAsync_WhenStatusProvidedButNoPackagesFound_ReturnsFailureResult()
        {
            // Arrange
            string? trackingNumber = null;
            var status = (int)PackageStatus.Sent;
            var packageStatus = PackageStatus.Sent;

            packageRepoMock.Setup(r => r.FilterAllAsync(null, packageStatus))
                           .ReturnsAsync(new List<Package>());

            // Act
            var result = await service.FilterAllPackagesAsync(trackingNumber, status);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("No packages found", result.ErrorMessage);
            Assert.Null(result.Data);
            packageRepoMock.Verify(r => r.FilterAllAsync(null, packageStatus), Times.Once);
            mapperMock.Verify(m => m.Map<List<PackageResponse>>(It.IsAny<List<Package>>()), Times.Never);
        }

        [Fact]
        public async Task FilterAllPackagesAsync_WhenStatusProvidedAndPackagesFound_ReturnsSuccessResult()
        {
            // Arrange
            string? trackingNumber = null;
            var status = (int)PackageStatus.Sent;
            var packageStatus = PackageStatus.Sent;
            var packages = new List<Package>
        {
            fixture.Create<Package>(),
            fixture.Create<Package>()
        };
                var expectedResponses = new List<PackageResponse>
        {
            fixture.Create<PackageResponse>(),
            fixture.Create<PackageResponse>()
        };

            packageRepoMock.Setup(r => r.FilterAllAsync(null, packageStatus))
                           .ReturnsAsync(packages);

            mapperMock.Setup(m => m.Map<List<PackageResponse>>(packages))
                      .Returns(expectedResponses);

            // Act
            var result = await service.FilterAllPackagesAsync(trackingNumber, status);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedResponses, result.Data);
            packageRepoMock.Verify(r => r.FilterAllAsync(null, packageStatus), Times.Once);
            mapperMock.Verify(m => m.Map<List<PackageResponse>>(packages), Times.Once);
        }
        //GetStatusHistory(string packageId)

        [Fact]
        public async Task GetStatusHistory_WhenPackageIdIsInvalid_ReturnsFailureResult()
        {
            // Arrange
            var invalidPackageId = "invalid-guid";

            // Act
            var result = await service.GetStatusHistory(invalidPackageId);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("Invalid packageId", result.ErrorMessage);
            Assert.Null(result.Data);
            packageRepoMock.Verify(r => r.GetAsync(It.IsAny<Guid>()), Times.Never);
            mapperMock.Verify(m => m.Map<List<PackageStatusHistoryResponse>>(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task GetStatusHistory_WhenValidIdButPackageNotFound_ReturnsFailureResult()
        {
            // Arrange
            var validGuid = Guid.NewGuid();
            var validPackageId = validGuid.ToString();

            packageRepoMock.Setup(r => r.GetAsync(validGuid))
                           .ReturnsAsync((Package)null!);

            // Act
            var result = await service.GetStatusHistory(validPackageId);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("package not found", result.ErrorMessage);
            Assert.Null(result.Data);
            packageRepoMock.Verify(r => r.GetAsync(validGuid), Times.Once);
            mapperMock.Verify(m => m.Map<List<PackageStatusHistoryResponse>>(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task GetStatusHistory_WhenValidIdAndPackageExists_ReturnsSuccessResult()
        {
            // Arrange
            var validGuid = Guid.NewGuid();
            var validPackageId = validGuid.ToString();
            var statusHistory = new List<PackageStatusHistory>
    {
        fixture.Create<PackageStatusHistory>(),
        fixture.Create<PackageStatusHistory>()
    };
            var package = fixture.Build<Package>()
                                 .With(p => p.StatusHistory, statusHistory)
                                 .Create();
            var expectedResponses = new List<PackageStatusHistoryResponse>
    {
        fixture.Create<PackageStatusHistoryResponse>(),
        fixture.Create<PackageStatusHistoryResponse>()
    };

            packageRepoMock.Setup(r => r.GetAsync(validGuid))
                           .ReturnsAsync(package);

            mapperMock.Setup(m => m.Map<List<PackageStatusHistoryResponse>>(statusHistory))
                      .Returns(expectedResponses);

            // Act
            var result = await service.GetStatusHistory(validPackageId);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedResponses, result.Data);
            packageRepoMock.Verify(r => r.GetAsync(validGuid), Times.Once);
            mapperMock.Verify(m => m.Map<List<PackageStatusHistoryResponse>>(statusHistory), Times.Once);
        }
    }
}

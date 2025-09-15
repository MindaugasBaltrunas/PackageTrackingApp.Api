using AutoFixture;
using PackageTrackingApp.Service.Dtos;
using PackageTrackingApp.Service.Validators;

namespace Tests.Validators
{
    public class PackageValidatorTests
    {
        private readonly PackageValidator validator;

        public PackageValidatorTests()
        {
            validator = new PackageValidator();
        }

        [Fact]
        public async Task ValidateAsync_WhenValidPackageRequest_ShouldPassValidation()
        {
            // Arrange
            var packageRequest = new PackageRequest
            {
                TrackingNumber = "TRK123456",
                SenderId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid()
            };

            // Act
            var result = await validator.ValidateAsync(packageRequest);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_WhenTrackingNumberIsNull_ShouldFailValidation()
        {
            // Arrange
            var packageRequest = new PackageRequest
            {
                TrackingNumber = "",
                SenderId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid()
            };

            // Act
            var result = await validator.ValidateAsync(packageRequest);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tracking number is required");
        }

        [Fact]
        public async Task ValidateAsync_WhenTrackingNumberIsEmpty_ShouldFailValidation()
        {
            // Arrange
            var packageRequest = new PackageRequest
            {
                TrackingNumber = "",
                SenderId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid()
            };

            // Act
            var result = await validator.ValidateAsync(packageRequest);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tracking number is required");
        }

        [Fact]
        public async Task ValidateAsync_WhenTrackingNumberIsWhitespace_ShouldFailValidation()
        {
            // Arrange
            var packageRequest = new PackageRequest
            {
                TrackingNumber = "   ",
                SenderId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid()
            };

            // Act
            var result = await validator.ValidateAsync(packageRequest);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tracking number is required");
        }

        [Fact]
        public async Task ValidateAsync_WhenTrackingNumberTooLong_ShouldFailValidation()
        {
            // Arrange
            var packageRequest = new PackageRequest
            {
                TrackingNumber = new string('A', 51), 
                SenderId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid()
            };

            // Act
            var result = await validator.ValidateAsync(packageRequest);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tracking number must be between 1 and 50 characters");
        }

        [Fact]
        public async Task ValidateAsync_WhenSenderIdIsEmpty_ShouldFailValidation()
        {
            // Arrange
            var packageRequest = new PackageRequest
            {
                TrackingNumber = "TRK123456",
                SenderId = Guid.Empty,
                RecipientId = Guid.NewGuid()
            };

            // Act
            var result = await validator.ValidateAsync(packageRequest);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "SenderId is required");
        }

        [Fact]
        public async Task ValidateAsync_WhenRecipientIdIsEmpty_ShouldFailValidation()
        {
            // Arrange
            var packageRequest = new PackageRequest
            {
                TrackingNumber = "TRK123456",
                SenderId = Guid.NewGuid(),
                RecipientId = Guid.Empty
            };

            // Act
            var result = await validator.ValidateAsync(packageRequest);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "RecipientId is required");
        }


        [Fact]
        public async Task ValidateAsync_WhenMultipleFieldsInvalid_ShouldFailValidationWithMultipleErrors()
        {
            // Arrange
            var packageRequest = new PackageRequest
            {
                TrackingNumber = "",
                SenderId = Guid.Empty, 
                RecipientId = Guid.Empty 
            };

            // Act
            var result = await validator.ValidateAsync(packageRequest);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(4, result.Errors.Count); 
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tracking number is required");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tracking number must be between 1 and 50 characters");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "SenderId is required");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "RecipientId is required");
        }
    }
}
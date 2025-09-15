using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Service.Validators;

namespace Tests.Validators
{
    public class IsValidStatusTransitionTests
    {
        private readonly IsValidStatusTransition validator;

        public IsValidStatusTransitionTests()
        {
            validator = new IsValidStatusTransition();
        }

        [Fact]
        public void Check_WhenCreatedToSent_ReturnsTrue()
        {
            // Act
            var result = validator.Check(PackageStatus.Created, PackageStatus.Sent);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Check_WhenCreatedToCancelled_ReturnsTrue()
        {
            // Act
            var result = validator.Check(PackageStatus.Created, PackageStatus.Cancelled);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Check_WhenCreatedToInvalidStatus_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(validator.Check(PackageStatus.Created, PackageStatus.Accepted));
            Assert.False(validator.Check(PackageStatus.Created, PackageStatus.Returned));
        }

        [Fact]
        public void Check_WhenSentToAccepted_ReturnsTrue()
        {
            // Act
            var result = validator.Check(PackageStatus.Sent, PackageStatus.Accepted);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Check_WhenSentToReturned_ReturnsTrue()
        {
            // Act
            var result = validator.Check(PackageStatus.Sent, PackageStatus.Returned);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Check_WhenSentToCancelled_ReturnsTrue()
        {
            // Act
            var result = validator.Check(PackageStatus.Sent, PackageStatus.Cancelled);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Check_WhenSentToInvalidStatus_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(validator.Check(PackageStatus.Sent, PackageStatus.Created));
        }

        [Fact]
        public void Check_WhenReturnedToSent_ReturnsTrue()
        {
            // Act
            var result = validator.Check(PackageStatus.Returned, PackageStatus.Sent);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Check_WhenReturnedToCancelled_ReturnsTrue()
        {
            // Act
            var result = validator.Check(PackageStatus.Returned, PackageStatus.Cancelled);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Check_WhenReturnedToInvalidStatus_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(validator.Check(PackageStatus.Returned, PackageStatus.Created));
            Assert.False(validator.Check(PackageStatus.Returned, PackageStatus.Accepted));
        }

        // Terminal Status Tests
        [Fact]
        public void Check_WhenAcceptedToAnyStatus_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(validator.Check(PackageStatus.Accepted, PackageStatus.Created));
            Assert.False(validator.Check(PackageStatus.Accepted, PackageStatus.Sent));
            Assert.False(validator.Check(PackageStatus.Accepted, PackageStatus.Returned));
            Assert.False(validator.Check(PackageStatus.Accepted, PackageStatus.Cancelled));
            Assert.False(validator.Check(PackageStatus.Accepted, PackageStatus.Accepted));
        }

        [Fact]
        public void Check_WhenCancelledToAnyStatus_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(validator.Check(PackageStatus.Cancelled, PackageStatus.Created));
            Assert.False(validator.Check(PackageStatus.Cancelled, PackageStatus.Sent));
            Assert.False(validator.Check(PackageStatus.Cancelled, PackageStatus.Returned));
            Assert.False(validator.Check(PackageStatus.Cancelled, PackageStatus.Accepted));
            Assert.False(validator.Check(PackageStatus.Cancelled, PackageStatus.Cancelled));
        }

        // Same Status Tests
        [Fact]
        public void Check_WhenSameStatus_ReturnsFalseForAllStatuses()
        {
            // Act & Assert
            Assert.False(validator.Check(PackageStatus.Created, PackageStatus.Created));
            Assert.False(validator.Check(PackageStatus.Sent, PackageStatus.Sent));
            Assert.False(validator.Check(PackageStatus.Returned, PackageStatus.Returned));
            Assert.False(validator.Check(PackageStatus.Accepted, PackageStatus.Accepted));
            Assert.False(validator.Check(PackageStatus.Cancelled, PackageStatus.Cancelled));
        }
    }
}

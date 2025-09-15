using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Service.Validators;
namespace Tests.Validators
{
    public class BaseEntityValidatorTests
    {
        private readonly TestBaseEntityValidator validator;

        public BaseEntityValidatorTests()
        {
            validator = new TestBaseEntityValidator();
        }

        [Fact]
        public async Task ValidateAsync_WhenAllFieldsValid_ShouldPassValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "John Doe",
                Address = "123 Main Street, City",
                Phone = "1234567890"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_WhenNameIsEmpty_ShouldFailValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "",
                Address = "123 Main Street, City",
                Phone = "1234567890"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Name is required");
        }

        [Fact]
        public async Task ValidateAsync_WhenNameTooShort_ShouldFailValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "A", 
                Address = "123 Main Street, City",
                Phone = "1234567890"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Name must be between 2 and 100 characters");
        }

        [Fact]
        public async Task ValidateAsync_WhenNameTooLong_ShouldFailValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = new string('A', 101), 
                Address = "123 Main Street, City",
                Phone = "1234567890"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Name must be between 2 and 100 characters");
        }

        [Fact]
        public async Task ValidateAsync_WhenNameContainsInvalidCharacters_ShouldFailValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "John@Doe123", 
                Address = "123 Main Street, City",
                Phone = "1234567890"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Name can only contain letters, spaces, hyphens, apostrophes, and periods");
        }

        [Fact]
        public async Task ValidateAsync_WhenAddressIsEmpty_ShouldFailValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "John Doe",
                Address = "",
                Phone = "1234567890"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Address is required");
        }

        [Fact]
        public async Task ValidateAsync_WhenAddressTooShort_ShouldFailValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "John Doe",
                Address = "123", 
                Phone = "1234567890"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Address must be between 5 and 250 characters");
        }

        [Fact]
        public async Task ValidateAsync_WhenAddressContainsInvalidCharacters_ShouldFailValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "John Doe",
                Address = "123 Main St @#$%", 
                Phone = "1234567890"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Address contains invalid characters");
        }

        [Fact]
        public async Task ValidateAsync_WhenPhoneIsEmpty_ShouldFailValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "John Doe",
                Address = "123 Main Street, City",
                Phone = ""
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Phone number is required");
        }

        [Fact]
        public async Task ValidateAsync_WhenPhoneTooShort_ShouldFailValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "John Doe",
                Address = "123 Main Street, City",
                Phone = "123456789"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Phone number must be between 10 and 15 digits");
        }

        [Fact]
        public async Task ValidateAsync_WhenPhoneFormatInvalid_ShouldFailValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "John Doe",
                Address = "123 Main Street, City",
                Phone = "123-456-7890" 
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Phone number format is invalid");
        }

        [Fact]
        public async Task ValidateAsync_WhenMultipleFieldsInvalid_ShouldFailValidationWithMultipleErrors()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "", 
                Address = "", 
                Phone = "" 
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(9, result.Errors.Count);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Name is required");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Address is required");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Phone number is required");
        }

        [Fact]
        public async Task ValidateAsync_WhenNameContainsValidSpecialCharacters_ShouldPassValidation()
        {
            // Arrange
            var entity = new BaseEntity
            {
                Name = "Mary O'Connor-Smith Jr.",
                Address = "123 Main Street, City",
                Phone = "1234567890"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ValidateAsync_WhenPhoneHasValidFormats_ShouldPassValidation()
        {
            var validPhones = new[] { "1234567890", "+1234567890", "+12345678901234" };

            foreach (var phone in validPhones)
            {
                // Arrange
                var entity = new BaseEntity
                {
                    Name = "John Doe",
                    Address = "123 Main Street, City",
                    Phone = phone
                };

                // Act
                var result = await validator.ValidateAsync(entity);

                // Assert
                Assert.True(result.IsValid, $"Phone {phone} should be valid");
            }
        }
    }
}

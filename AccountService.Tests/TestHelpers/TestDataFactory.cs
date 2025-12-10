using AccountEntities;

namespace AccountService.Tests.TestHelpers
{
    internal static class TestDataFactory
    {
        private static readonly Random _rng = new Random(Guid.NewGuid().GetHashCode());
        private static readonly string[] _cities = new[]
        {
            "New York", "Los Angeles", "Chicago", "Houston", "Phoenix",
            "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose",
            "Austin", "Jacksonville", "Fort Worth", "Columbus", "Charlotte"
        };

        /// <summary>
        /// Create a random Account entity with unique-ish values suitable for tests.
        /// BirthDate will be between (now - 82 years) and (now - 18 years) by default.
        /// PetCount will be between 0 and 10.
        /// </summary>
        public static Account CreateRandomAccount(
            int? minAge = 18,
            int? maxAge = 82)
        {
            var now = DateTime.UtcNow.Date;
            var maxYears = maxAge ?? 82;
            var minYears = minAge ?? 18;
            if (minYears < 0) minYears = 0;
            if (maxYears < minYears) maxYears = minYears + 1;

            var maxDob = now.AddYears(-minYears);
            var minDob = now.AddYears(-maxYears);

            var daysRange = (maxDob - minDob).Days;
            var birthDate = minDob.AddDays(_rng.Next(0, Math.Max(1, daysRange)));

            var uniqueSuffix = Guid.NewGuid().ToString("n").Substring(0, 8);
            var firstName = "TestFirst" + uniqueSuffix;
            var lastName = "TestLast" + uniqueSuffix;
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com";
            var city = _cities[_rng.Next(0, _cities.Length)];
            var petCount = _rng.Next(0, 11); // 0..10

            return new Account
            {
                FirstName = firstName,
                LastName = lastName,
                BirthDate = birthDate,
                EmailAddress = email,
                City = city,
                PetCount = petCount,
                IsActive = true
            };
        }
    }
}

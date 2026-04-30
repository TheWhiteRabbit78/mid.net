using AbySalto.Mid.Domain.Identity;
using AbySalto.Mid.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AbySalto.Mid.Application.Tests.Fixtures
{
    /// <summary>
    /// Per-test database factory backed by SQLite in-memory. Each call to <see cref="CreateContext"/>
    /// returns a fresh, fully isolated <see cref="ApplicationDbContext"/> instance. All connections
    /// are tracked and disposed when the fixture is disposed.
    /// </summary>
    public class DatabaseFixture : IDisposable
    {
        private readonly List<SqliteConnection> _connections = new();
        private readonly List<ApplicationDbContext> _contexts = new();

        public ApplicationDbContext CreateContext()
        {
            SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            _connections.Add(connection);

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            ApplicationDbContext context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            _contexts.Add(context);

            return context;
        }

        /// <summary>
        /// Seeds an <see cref="ApplicationUser"/> directly into the supplied context for tests
        /// that need an existing user without going through Identity flows.
        /// </summary>
        public static async Task<ApplicationUser> SeedUserAsync(ApplicationDbContext context, string? userName = null)
        {
            ApplicationUser user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userName ?? $"test-{Guid.NewGuid():N}",
                Email = $"{Guid.NewGuid():N}@test.local",
                FirstName = "Test",
                LastName = "User",
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        public void Dispose()
        {
            foreach (ApplicationDbContext context in _contexts)
            {
                context.Dispose();
            }

            foreach (SqliteConnection connection in _connections)
            {
                connection.Close();
                connection.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}

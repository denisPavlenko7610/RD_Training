using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        
    }

    public DbSet<UserTaskProgress> UserTaskProgresses { get; set; }
    
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if there are any UserTaskProgress entries
            if (context.UserTaskProgresses.Any())
            {
                return; // DB has been seeded
            }

            // Seed initial data
            var userTaskProgresses = new UserTaskProgress[]
            {
                new UserTaskProgress { UserId = "demo-user", TaskId = 0, IsCompleted = false },
                new UserTaskProgress { UserId = "demo-user", TaskId = 1, IsCompleted = false },
                new UserTaskProgress { UserId = "demo-user", TaskId = 2, IsCompleted = false },
                // Add more initial tasks as needed
            };

            foreach (var task in userTaskProgresses)
            {
                context.UserTaskProgresses.Add(task);
            }
            context.SaveChanges();
        }
    }
}
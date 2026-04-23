using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE CONFIGURATION
// Connection string for SQL Server running inside Docker/WSL2
// 'localhost' works because WSL2 maps ports directly to Windows
//var connectionString = "Server=localhost;Database=BancaDB;User Id=sa;Password=PasswordMoltoSicura123!;TrustServerCertificate=True;";
// We add "127.0.0.1" and "Connect Timeout" to be more explicit
var connectionString = "Server=127.0.0.1,1433;Database=BancaDB;User Id=sa;Password=PasswordMoltoSicura123!;TrustServerCertificate=True;Connect Timeout=30;";

// Register the Database Context using SQL Server provider
builder.Services.AddDbContext<BankDbContext>(opt => 
    opt.UseSqlServer(connectionString));

// 2. CORS POLICY
// Allows the Vue.js frontend (running on a different port) to communicate with this API
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable the CORS policy
app.UseCors();

// 3. DATABASE INITIALIZATION (AUTO-MIGRATION)
// On startup, check if the database and tables exist. If not, create them automatically.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
    db.Database.EnsureCreated(); 
}

// 4. ENDPOINTS

// POST: Process a new payment and update daily spending totals
app.MapPost("/paga", async (Transaction tx, BankDbContext db) => {
    // Add the single transaction to the history
    db.Transactions.Add(tx);
    
    // Get the current date (without time)
    var today = DateTime.Now.Date;
    
    // Find if a record for this category (MCC) and today already exists
    var spending = await db.DailySpendings
        .FirstOrDefaultAsync(s => s.Date == today && s.Mcc == tx.Mcc);

    if (spending == null) {
        // Create a new record for this category if it's the first expense of the day
        db.DailySpendings.Add(new DailySpending { 
            Mcc = tx.Mcc, 
            Date = today, 
            TotalAmount = tx.Amount 
        });
    } else {
        // Increment the existing total amount for the category
        spending.TotalAmount += tx.Amount;
    }

    // Save changes to SQL Server
    await db.SaveChangesAsync();
    return Results.Ok($"Saved! You spent {tx.Amount} for {tx.Mcc}");
});

// GET: Retrieve spending data for the Vue.js Chart
app.MapGet("/api/spending", async (BankDbContext db) => {
    return Results.Ok(await db.DailySpendings.ToListAsync());
});

app.Run();

// --- DATA MODELS ---

// Represents a single expense record
public class Transaction {
    public int Id { get; set; }
    public string Mcc { get; set; } = "";
    public decimal Amount { get; set; }
}

// Represents the aggregated spending per category per day
public class DailySpending {
    public int Id { get; set; }
    public string Mcc { get; set; } = "";
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
}

// Entity Framework Database Context (The "Manager" of our DB)
public class BankDbContext : DbContext {
    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<DailySpending> DailySpendings => Set<DailySpending>();
}
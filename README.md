💰 Financial Dashboard: Full-Stack Expense Tracker
This project is a professional-grade financial tracking application built with a decoupled architecture. It demonstrates a complete Modern Development Stack by bridging a Linux-based data layer with a .NET backend and a reactive Vue.js frontend.

🏗️ System Architecture
The application is structured into three independent layers to ensure scalability and isolation:

Data Layer (Linux/Docker): SQL Server 2022 running on a native Linux kernel via WSL2.

Backend API (.NET 10): A RESTful service managing business logic and database persistence.

Frontend (Vue.js 3): A Single Page Application (SPA) providing real-time data visualization.

🛠️ Infrastructure & Setup
1. WSL2 & Ubuntu Configuration
The database is hosted within a Linux subsystem to simulate a real-world production environment:

Environment: Enabled "Virtual Machine Platform" and installed Ubuntu via wsl --install.

Docker Integration: Configured Docker Desktop to use the WSL2 native engine, allowing SQL Server to run on a Linux kernel for superior performance and isolation.

2. Containerized Database (Docker)
The SQL Server instance is fully containerized.

Deployment Command:

Bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=PasswordMoltoSicura123!" -p 1433:1433 --name sqlserver_banca -d mcr.microsoft.com/mssql/server:2022-latest
🖥️ Backend Implementation (.NET 10)
The backend is built using a "Lean API" approach. Below is the commented logic of the Program.cs file.

Project Setup
Bash
# Create the Web API project
dotnet new webapi -n BankApi --no-openapi
cd BankApi

# Install the SQL Server provider for Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
Core Logic (Program.cs)
C#
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE CONNECTION CONFIGURATION
// We use 127.0.0.1 (localhost) to point to the Docker container port 1433
var connectionString = "Server=127.0.0.1,1433;Database=BancaDB;User Id=sa;Password=PasswordMoltoSicura123!;TrustServerCertificate=True;";

// Register the DbContext with Dependency Injection
builder.Services.AddDbContext<BankDbContext>(opt => 
    opt.UseSqlServer(connectionString));

// 2. CORS CONFIGURATION (Essential for Full-Stack)
// This allows the Vue.js frontend (running on a different port) to communicate with this API
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin() // In production, replace with specific frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable CORS and apply the policy
app.UseCors();

// 3. AUTOMATED DATABASE INITIALIZATION
// On startup, we ensure the SQL Server database and tables are created automatically
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
    db.Database.EnsureCreated(); // EF Core creates the schema based on C# classes
}

// 4. API ENDPOINTS
// GET: Returns aggregated data for the frontend charts
app.MapGet("/api/spending", async (BankDbContext db) => 
    await db.DailySpendings.ToListAsync());

// POST: Handles new transactions and performs Real-Time Aggregation
app.MapPost("/paga", async (Transaction tx, BankDbContext db) => {
    // Save the raw transaction for history
    db.Transactions.Add(tx);
    
    // Logic: Aggregate total spending per category for the current day
    var today = DateTime.Now.Date;
    var spending = await db.DailySpendings
        .FirstOrDefaultAsync(s => s.Date == today && s.Mcc == tx.Mcc);

    if (spending == null) {
        // Create new category entry for today
        db.DailySpendings.Add(new DailySpending { 
            Mcc = tx.Mcc, 
            Date = today, 
            TotalAmount = tx.Amount 
        });
    } else {
        // Update existing category entry
        spending.TotalAmount += tx.Amount;
    }

    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();
🎨 Frontend Implementation (Vue.js 3)
The frontend uses Vite and the Composition API for high performance.

Project Setup
Bash
# Scaffold the project using Vite
npm create vite@latest bank-frontend -- --template vue
cd bank-frontend
npm install

# Install visualization dependencies
npm install chart.js vue-chartjs
API Integration (App.vue snippet)
JavaScript
// --- API SERVICES ---

/**
 * Fetch spending statistics from the .NET Backend
 * Note: We use the specific port assigned by .NET (e.g., 5138)
 */
async function fetchChartData() {
  try {
    const response = await fetch('http://localhost:5138/api/spending');
    if (response.ok) {
      const data = await response.json();
      
      // Update reactive state for the Chart.js component
      chartData.value = {
        labels: data.map(item => item.mcc),
        datasets: [{
          label: 'Spending per Category ($)',
          backgroundColor: ['#42b883', '#34495e', '#3498db', '#f1c40f'],
          data: data.map(item => item.totalAmount)
        }]
      };
    }
  } catch (error) {
    console.error("API Error:", error);
  }
}
📈 Technical Highlights for Interviews
Hybrid Infrastructure: Expert management of cross-platform communication between Windows and Linux via WSL2.

CORS & Security: Deep understanding of web security policies and cross-origin communication.

Data Consistency: Implementation of an ORM (Entity Framework Core) with a Code-First approach.

DevOps Mindset: Use of Docker to containerize services, ensuring portability and reproducible environments.

Real-Time Analytics: Backend-driven data aggregation to reduce frontend computational load.

Financial Dashboard: Full-Stack Expense Tracker
This project is a professional-grade financial tracking application built with a decoupled architecture. It demonstrates a complete Modern Development Stack by bridging a Linux-based data layer with a .NET backend and a reactive Vue.js frontend.

🏗️ System Architecture
The application is structured into three independent layers to ensure scalability and isolation:

Data Layer (Linux/Docker): SQL Server 2022 running on a native Linux kernel via WSL2.

Backend API (.NET 10): A RESTful service managing business logic and database persistence.

Frontend (Vue.js 3): A Single Page Application (SPA) providing real-time data visualization.

🛠️ Infrastructure & Implementation Details
1. WSL2 & Ubuntu Configuration
The database is hosted within a Linux subsystem to simulate a real-world production environment:

Environment: Enabled "Virtual Machine Platform" and installed Ubuntu via wsl --install.

Docker Integration: Configured Docker Desktop to use the WSL2 native engine, allowing SQL Server to run on a Linux kernel for superior performance and isolation.

2. Containerized Database (Docker)
The SQL Server instance is fully containerized.

Command used to deploy:

Bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=PasswordMoltoSicura123!" -p 1433:1433 --name sqlserver_banca -d mcr.microsoft.com/mssql/server:2022-latest
3. Backend Implementation (.NET 10 & EF Core)
The API manages the bridge between the UI and SQL Server using Entity Framework Core.

Database Connector & Connection String:

C#
// Connecting to SQL Server running on Docker/WSL2
var connectionString = "Server=localhost,1433;Database=BancaDB;User Id=sa;Password=PasswordMoltoSicura123!;TrustServerCertificate=True;";

builder.Services.AddDbContext<BankDbContext>(opt => 
    opt.UseSqlServer(connectionString));
Automated Schema Generation (Code-First):

C#
// This ensures the database and tables are created automatically on startup
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
    db.Database.EnsureCreated(); 
}
Business Logic (Transaction Aggregation):

C#
app.MapPost("/paga", async (Transaction tx, BankDbContext db) => {
    db.Transactions.Add(tx); // Save the raw transaction
    
    // Aggregate spending for the dashboard
    var today = DateTime.Now.Date;
    var spending = await db.DailySpendings
        .FirstOrDefaultAsync(s => s.Date == today && s.Mcc == tx.Mcc);

    if (spending == null) {
        db.DailySpendings.Add(new DailySpending { Mcc = tx.Mcc, Date = today, TotalAmount = tx.Amount });
    } else {
        spending.TotalAmount += tx.Amount;
    }
    await db.SaveChangesAsync();
    return Results.Ok();
});
4. Reactive Frontend (Vue 3 & Vite)
The UI focuses on speed and real-time feedback using the Composition API.

Asynchronous Data Fetching:

JavaScript
// Fetching aggregated data from the .NET API
async function fetchChartData() {
  const response = await fetch('http://localhost:5138/api/spending');
  if (response.ok) {
    const data = await response.json();
    // Update reactive state
    chartData.value.labels = data.map(item => item.mcc);
    chartData.value.datasets[0].data = data.map(item => item.totalAmount);
  }
}
🚀 Installation & Running the Project
Phase 1: Data Layer
Ensure Docker is running on your Ubuntu WSL2 instance, then start the container:

Bash
docker start sqlserver_banca
Phase 2: Backend API
Navigate to the API folder: cd BankApi

Run: dotnet run
The API will listen on http://localhost:5138.

Phase 3: Frontend UI
Navigate to the frontend folder: cd bank-frontend

Install dependencies: npm install

Start the dev server: npm run dev

📈 Technical Highlights for Interviews
Hybrid Environment Management: Seamless integration across Windows (Frontend/Backend) and Linux (Database via WSL2).

Infrastructure as Code: Repeatable database setup via Docker, eliminating environment-specific bugs.

Decoupled Architecture: Clean separation of concerns between UI, Business Logic, and Persistence.

Advanced EF Core: Practical use of Code-First approach and automated database initialization.
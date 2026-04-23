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

Deployment Command:

Bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=PasswordMoltoSicura123!" -p 1433:1433 --name sqlserver_banca -d mcr.microsoft.com/mssql/server:2022-latest
3. Backend Development (.NET 10 & EF Core)
The API manages the bridge between the UI and SQL Server.

Project Creation via CLI:

Bash
# Create the Web API project without OpenAPI for a lean structure
dotnet new webapi -n BankApi --no-openapi
cd BankApi

# Install the SQL Server provider for Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
Database Connector & Connection String:

C#
// Connecting to SQL Server running on Docker/WSL2
var connectionString = "Server=127.0.0.1,1433;Database=BancaDB;User Id=sa;Password=PasswordMoltoSicura123!;TrustServerCertificate=True;";

builder.Services.AddDbContext<BankDbContext>(opt => 
    opt.UseSqlServer(connectionString));
Automated Schema Generation (Code-First):

C#
// Ensures the database and tables are created automatically on startup
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
    db.Database.EnsureCreated(); 
}
4. Frontend Development (Vue 3 & Vite)
The UI focuses on speed and real-time feedback using the Composition API.

Project Creation via CLI:

Bash
# Scaffold the project using Vite
npm create vite@latest bank-frontend -- --template vue
cd bank-frontend
npm install

# Install visualization dependencies
npm install chart.js vue-chartjs
Asynchronous Data Fetching:

JavaScript
// Fetching aggregated data from the .NET API (Listening on Port 5138)
async function fetchChartData() {
  const response = await fetch('http://localhost:5138/api/spending');
  if (response.ok) {
    const data = await response.json();
    chartData.value.labels = data.map(item => item.mcc);
    chartData.value.datasets[0].data = data.map(item => item.totalAmount);
  }
}
🚀 Execution Guide
Phase 1: Data Layer
Ensure Docker is running on your Ubuntu WSL2 instance:

Bash
docker start sqlserver_banca
Phase 2: Backend API
From the BankApi folder:

Bash
dotnet run
Note: The API automatically initializes the BancaDB schema on SQL Server.

Phase 3: Frontend UI
From the bank-frontend folder:

Bash
npm run dev
The UI will be available at http://localhost:5173.

📈 Technical Highlights for Interviews
Hybrid Infrastructure: Demonstrated ability to bridge Windows-based applications with Linux-based services (WSL2).

CLI Proficiency: Full project scaffolding and dependency management using .NET CLI and npm.

Environment Isolation: Used Docker to prevent "dependency hell" and ensure a portable database environment.

Full-Stack Integration: Managed the entire data lifecycle, from SQL storage to C# processing and JavaScript visualization.
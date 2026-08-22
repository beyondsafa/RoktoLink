# RoktoLink

RoktoLink (Rokto = Bengali for blood) is a Blood Requisition and Donor Coordination System built for CSC 440 Visual Programming Lab, Fall 2025. It solves a real problem: in Bangladesh, urgent blood requests happen via Facebook posts and phone chains. RoktoLink gives hospitals and donors a structured, role-aware web platform.

## Features

- **Role-based Authentication**: Admin, Coordinator, and Donor roles with distinct dashboards.
- **Donor Profiles & Eligibility**: Tracks donor blood types and enforces a 90-day wait period between donations.
- **Blood Request Management**: Coordinators can create and manage blood requests, mark them fulfilled, or cancel them.
- **Compatibility Engine**: Automatically matches urgent requests with eligible donors having compatible blood types.
- **Admin Insights**: Analytics dashboard with Chart.js visualization for donation activity and system health.

## Tech Stack

- ASP.NET Core MVC (.NET 8/10) / C#
- Entity Framework Core with SQLite
- ASP.NET Core Identity
- Bootstrap 5
- Chart.js

## How to Run

1. Clone the repository and navigate to the root directory.
2. Ensure you have the .NET SDK installed.
3. Apply database migrations:
   ```bash
   dotnet ef database update
   ```
4. Run the application:
   ```bash
   dotnet run
   ```
5. Open your browser and go to `https://localhost:5001` (or the port specified in your console output).

## Seed Credentials

The database seeds with the following default users upon first run:

- **Admin**: `admin@roktolink.local` / `Admin@1234`
- **Coordinator**: `coordinator@roktolink.local` / `Coord@1234`
- **Donor**: `donor@roktolink.local` / `Donor@1234`

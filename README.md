# 🏢 Enterprise Employee Management System

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Angular 17](https://img.shields.io/badge/Angular-17-DD0031?style=flat-square&logo=angular&logoColor=white)](https://angular.io/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20%2F%20DDD-blue?style=flat-square)](https://blog.cleancoder.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)
[![Tests: xUnit](https://img.shields.io/badge/Tests-xUnit%20%2B%20Moq-brightgreen?style=flat-square)](tests/)

A production-grade, enterprise Employee & HR Management platform architected with **Clean Architecture (Domain-Driven Design)** in **ASP.NET Core 8** and a reactive **Angular 17** Single Page Application powered by **Signals** and **PrimeNG**.

---

## 🏛️ System Architecture

```
EmployeeManagement/
├── EmployeeManagement.sln                   # .NET Solution File
├── src/
│   ├── EmployeeManagement.Domain/           # Enterprise Business Core
│   │   ├── Entities/                        # BaseEntity, Employee, Department, LeaveRequest, Attendance
│   │   ├── Enums/                           # EmploymentStatus, LeaveType, UserRole
│   │   └── Interfaces/                      # IRepository<T>, IEmployeeRepository, IUnitOfWork
│   │
│   ├── EmployeeManagement.Application/      # Use Cases & Orchestration
│   │   ├── DTOs/                            # Immutable C# Records, PagedResponse
│   │   └── Services/                        # IEmployeeService, EmployeeService (business logic)
│   │
│   ├── EmployeeManagement.Infrastructure/   # Data Access & External Adapters
│   │   ├── Persistence/                     # AppDbContext (EF Core Fluent API, Seed Data)
│   │   └── Repositories/                    # GenericRepository, EmployeeRepository, UnitOfWork
│   │
│   └── EmployeeManagement.API/              # Presentation Web API
│       ├── Controllers/                     # EmployeesController (RESTful, JWT, Swagger)
│       ├── Program.cs                       # DI, JWT Bearer, CORS, Swagger Configuration
│       └── appsettings.json                 # Connection strings, JWT settings
│
├── tests/
│   └── EmployeeManagement.UnitTests/        # Automated Test Suite
│       └── Services/                        # EmployeeServiceTests (xUnit + Moq AAA pattern)
│
├── frontend/                                # Angular 17 SPA (Standalone Components & Signals)
│   ├── src/app/
│   │   ├── models/                          # TypeScript Interfaces & Enums
│   │   ├── services/                        # EmployeeService (Signals + HttpClient)
│   │   └── components/
│   │       ├── employee-list/               # Server-side pagination, search, status filters
│   │       ├── employee-form/               # Reactive forms with validation
│   │       └── dashboard/                   # Real-time workforce KPIs & Department charts
│   └── package.json
│
└── docker-compose.yml                       # Containerized API + SQL Server + Redis
```

---

## 🚀 Key Engineering Highlights

- **Clean Architecture 4-Layer Separation**: Zero framework dependency in Domain. Application defines use cases and contracts.
- **Repository + Unit of Work Pattern**: Ensures atomic transaction boundaries across multiple entity operations.
- **Global Query Filters & Soft-Delete**: Automated deletion filtering via EF Core `HasQueryFilter(e => !e.IsDeleted)`.
- **C# Records for DTOs**: Guarantee immutability and eliminate boilerplate.
- **Fine-Grained Frontend Reactivity**: Angular 17 Signals (`signal()`, `computed()`) eliminate RxJS subscription leak risks.
- **Automated Testing**: 100% passing xUnit and Moq unit test suite covering business rules (unique email enforcement, department validation, pagination clamping).

---

## 🛠️ Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) & [Angular CLI](https://angular.io/cli)
- [Docker Desktop](https://www.docker.com/) (Optional, for SQL Server & Redis containers)

### 1. Run Backend (.NET 8)
```bash
# Clone the repository
git clone https://github.com/minhlaptech/employee-management.git
cd employee-management

# Restore dependencies
dotnet restore

# Run automated tests
dotnet test EmployeeManagement.sln

# Run API (Swagger available at http://localhost:5000/swagger)
cd src/EmployeeManagement.API
dotnet run
```

### 2. Run Frontend (Angular 17)
```bash
cd frontend
npm install
npm start
# Navigate to http://localhost:4200
```

### 3. Run with Docker Compose
```bash
docker-compose up -d
```

---

## 📄 License
MIT License — Copyright (c) 2026 Pham Van Minh (Minh Lap).

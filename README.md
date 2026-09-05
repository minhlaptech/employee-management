# 👥 Employee Management System

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-17+-DD0031?logo=angular)](https://angular.io/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A full-stack employee management system built with **Clean Architecture** principles. Features employee CRUD, department management, leave request workflow, attendance tracking, and dashboard analytics.

---

## 🏗️ Architecture

This project follows **Clean Architecture** (Onion Architecture) with clear separation of concerns:

```
┌─────────────────────────────────────────────────────┐
│                   API Layer                          │
│         Controllers, Middleware, DI Config           │
├─────────────────────────────────────────────────────┤
│              Application Layer                       │
│          Services, DTOs, Validators                  │
├─────────────────────────────────────────────────────┤
│             Infrastructure Layer                     │
│      EF Core, Repositories, External Services        │
├─────────────────────────────────────────────────────┤
│                Domain Layer                          │
│       Entities, Enums, Interfaces (Core)             │
└─────────────────────────────────────────────────────┘
```

**Dependency Rule:** Dependencies only point inward. Domain has zero external dependencies.

---

## 🌟 Features

- **Employee Management** — Full CRUD with search, filter, pagination
- **Department Management** — Organize employees by department with capacity tracking
- **Leave Request Workflow** — Submit → Review → Approve/Reject lifecycle
- **Attendance Tracking** — Daily check-in/check-out with working hours calculation
- **Dashboard KPIs** — Real-time analytics (headcount, salary avg, department breakdown)
- **JWT Authentication** — Secure API with role-based access (Admin, Manager, Employee)
- **Soft Delete** — Audit-friendly deletion with global query filters
- **Auto Audit** — CreatedAt, UpdatedAt, CreatedBy timestamps on all entities

---

## 🛠️ Tech Stack

| Layer | Technologies |
|---|---|
| **Backend** | ASP.NET Core 8 Web API, C# 12 |
| **Frontend** | Angular 17 (Standalone Components, Signals, RxJS) |
| **ORM** | Entity Framework Core 8 (Code-First, Fluent API) |
| **Database** | SQL Server 2022 |
| **Caching** | Redis (IDistributedCache) |
| **Auth** | JWT Bearer Tokens, RBAC |
| **Testing** | xUnit, Moq, FluentAssertions |
| **DevOps** | Docker, Docker Compose |
| **API Docs** | Swagger / OpenAPI 3.0 |

---

## 📁 Project Structure

```
employee-management/
├── src/
│   ├── EmployeeManagement.Domain/           # Entities, Enums, Interfaces
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs                # Audit fields, soft-delete
│   │   │   └── Entities.cs                  # Employee, Department, LeaveRequest, Attendance
│   │   ├── Enums/
│   │   │   └── Enums.cs                     # EmploymentStatus, LeaveType, LeaveStatus, UserRole
│   │   └── Interfaces/
│   │       └── IRepository.cs               # Generic + specialized repository interfaces
│   │
│   ├── EmployeeManagement.Application/      # Business logic orchestration
│   │   ├── DTOs/
│   │   │   └── DTOs.cs                      # Request/Response models, PagedResponse<T>
│   │   └── Services/
│   │       └── EmployeeService.cs           # CRUD, pagination, dashboard KPIs
│   │
│   ├── EmployeeManagement.Infrastructure/   # Data access & external services
│   │   └── Persistence/
│   │       └── AppDbContext.cs              # EF Core context, Fluent API configs, seed data
│   │
│   └── EmployeeManagement.API/             # HTTP entry point
│       └── Controllers/
│           └── EmployeesController.cs       # RESTful endpoints, auth, error handling
│
├── tests/
│   └── EmployeeManagement.UnitTests/
│       └── Services/
│           └── EmployeeServiceTests.cs      # 6 unit tests with Moq
│
├── docker-compose.yml                       # API + SQL Server + Redis
├── README.md
└── LICENSE
```

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (or Docker)
- [Node.js 18+](https://nodejs.org/) (for Angular frontend)

### Option 1: Docker (Recommended)
```bash
docker compose up -d
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Option 2: Local Development
```bash
# Backend
cd src/EmployeeManagement.API
dotnet restore
dotnet ef database update
dotnet run

# Frontend
cd src/EmployeeManagement.Angular
npm install
ng serve
```

### Run Tests
```bash
dotnet test --verbosity normal
```

---

## 📡 API Endpoints

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| `GET` | `/api/v1/employees` | Paginated employee list (search, filter) | ✅ |
| `GET` | `/api/v1/employees/{id}` | Get employee by ID | ✅ |
| `POST` | `/api/v1/employees` | Create new employee | Admin/Manager |
| `PUT` | `/api/v1/employees/{id}` | Update employee (partial) | Admin/Manager |
| `DELETE` | `/api/v1/employees/{id}` | Soft-delete employee | Admin only |
| `GET` | `/api/v1/employees/dashboard` | Dashboard KPIs | ✅ |

### Example Request
```bash
curl -X GET "http://localhost:5000/api/v1/employees?pageNumber=1&pageSize=10&search=john" \
  -H "Authorization: Bearer <token>"
```

### Example Response
```json
{
  "items": [
    {
      "id": "a1b2c3d4-...",
      "firstName": "John",
      "lastName": "Doe",
      "fullName": "John Doe",
      "email": "john.doe@company.com",
      "jobTitle": "Senior Angular Developer",
      "status": "Active",
      "departmentName": "Engineering",
      "annualLeaveDaysRemaining": 10
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 45,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## 🧪 Testing Strategy

- **Unit Tests** — Service layer with Moq (business rule validation)
- **Integration Tests** — API endpoints with WebApplicationFactory (planned)
- **Test Coverage** — Core business logic (create, update, delete, edge cases)

---

## 📄 License
This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.

---

**Author:** Minh Lap (Pham Van Minh)  
🔗 [GitHub](https://github.com/minhlaptech) · [LinkedIn](https://www.linkedin.com/in/minhlaptech/)

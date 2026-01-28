# 🛡️ Binet Identity Service

**Binet Identity Service** is a modern, extensible authentication and authorization server built with **.NET 8**, **OpenIddict**, and **ASP.NET Core Identity**.  
It’s designed using **Clean Architecture principles** to provide a modular, testable, and scalable identity foundation for any enterprise system.

---

## 🧩 Architecture Overview

Domain → Application → Infrastructure → WebApi

markdown
Copy code

| Layer | Responsibility |
|--------|----------------|
| **Domain** | Core entities, business logic, and rules. |
| **Application** | Interfaces (contracts), DTOs, and use cases (no external dependencies). |
| **Infrastructure** | Implements contracts with EF Core, Identity, OpenIddict, and external providers. |
| **WebApi** | Thin controllers, configuration, and startup pipeline (the composition root). |

---

## 🌟 Key Features

- **OpenIddict-based OAuth2 & OpenID Connect server**
  - Supports *Password*, *Client Credentials*, and *Refresh Token* flows.
- **Dynamic Claims Management**
  - Add, remove, or list user claims at runtime through the API.
- **Dynamic Client Registration**
  - Create, update, rotate, or delete OpenIddict clients on the fly.
- **Multi-Provider Login Ready**
  - Plug in Google, Facebook, X (Twitter), and Microsoft login easily (config-driven).
- **Clean Architecture**
  - Clear separation between domain logic, application contracts, infrastructure, and presentation.
- **EF Core + PostgreSQL**
  - Persistent storage for users, roles, clients, and claims.
- **Admin API**
  - Manage clients and user claims securely via REST endpoints.

---

## ⚙️ Tech Stack

| Component | Technology |
|------------|-------------|
| **Framework** | .NET 8 (ASP.NET Core Web API) |
| **Identity Provider** | OpenIddict |
| **Authentication** | ASP.NET Core Identity |
| **Database** | PostgreSQL (EF Core) |
| **External Login Providers** | Google, Facebook, Microsoft, Twitter |
| **Architecture** | Clean Architecture |
| **ORM** | Entity Framework Core |
| **Language** | C# |

---

## 🧱 Project Structure

src/   
├─ BinetIdentityService.Domain/   
│ └─ Entities/   
│ ├─ ClaimDefinition.cs   
│ ├─ UserClaimAssignment.cs   
│ ├─ AppUser.cs   
│ └─ AppRole.cs  
│     
├─ BinetIdentityService.Application/    
│ ├─ Abstractions/ # Interfaces: IClaimsService, IClientService, IScopeService      
│ ├─ DTOs/ # DTOs like ClientDescriptor, ClaimDto       
│ └─ Common/ # Shared Result<T> type      
│      
├─ BinetIdentityService.Infrastructure/      
│ ├─ Persistence/ # EF Core DbContext      
│ ├─ Auth/ # Identity, OpenIddict, External Providers, Claims Services      
│ ├─ Seed/ # Seeder for default clients/scopes      
│ └─ DI/ # DependencyInjection.cs      
│        
└─ BinetIdentityService.WebApi/        
├─ Controllers/ # Admin controllers for Claims and Clients      
└─ Program.cs # Composition root        
      
yaml    
Copy code    

---

## 🚀 Quick Start

### 1️⃣ Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- (Optional) pgAdmin or DBeaver for DB management

### 2️⃣ Clone and Restore

git clone https://github.com/<your-username>/BinetIdentityService.git
cd BinetIdentityService
dotnet restore
3️⃣ Configure Connection
Edit src/BinetIdentityService.WebApi/appsettings.json:

json
Copy code
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=binet_identity;Username=postgres;Password=postgres"
}
💡 Keep secrets and provider credentials (Google, Facebook, etc.) in environment variables — never commit real keys.

4️⃣ Apply Migrations
bash
Copy code
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitIdentity -p src/BinetIdentityService.Infrastructure -s src/BinetIdentityService.WebApi
dotnet ef database update -p src/BinetIdentityService.Infrastructure -s src/BinetIdentityService.WebApi
5️⃣ Run the API
bash
Copy code
dotnet run --project src/BinetIdentityService.WebApi
Swagger UI:
👉 https://localhost:5001/swagger

🔐 Example Admin APIs
🧾 Manage Claims
Method	Endpoint	Description
GET	/api/admin/claims/{userId}	Get all claims for a user
POST	/api/admin/claims/{userId}	Add a claim to a user
DELETE	/api/admin/claims/{userId}	Remove a claim from a user

Example JSON:

json
Copy code
{
  "type": "OrgName",
  "value": "Tribhuvan University",
  "issuer": "Binet"
}
🧩 Manage Clients
Method	Endpoint	Description
POST	/api/admin/clients	Create or update a client
POST	/api/admin/clients/{clientId}/rotate	Rotate secret
DELETE	/api/admin/clients/{clientId}	Delete client

Example Client JSON:

json
Copy code
{
  "clientId": "binet-swagger",
  "displayName": "Swagger Client",
  "clientSecret": "dev-secret",
  "redirectUris": ["https://localhost:5001/swagger/oauth2-redirect.html"],
  "permissions": [
    "endpoints:token",
    "grant_types:client_credentials",
    "scopes:api"
  ]
}
🧠 Roadmap
 Token endpoint with dynamic claim injection

 Refresh token rotation & PKCE support

 Admin UI (Blazor / React)

 Email verification & password reset flows

 Docker and CI/CD pipeline

 Logging, monitoring, and telemetry (Serilog + OpenTelemetry)

🧾 License
This project is open-source and available under the MIT License.

👤 Author
Developed by: [Your Name]
💼 Software Engineer | .NET & Identity Systems
🔗 LinkedIn   |   GitHub

🌟 Star the repo if you find it useful — it helps others discover modern Identity setups built on OpenIddict + Clean Architecture!
yaml
Copy code

---


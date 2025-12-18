# WebStorage - Cloud Storage Management System

A full-featured cloud storage management system built with ASP.NET Core 9.0, featuring file uploads, folder management, subscription plans, and automated error notifications.

## 🏗️ Architecture

This solution follows a clean three-layer architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│  ┌────────────────────┐         ┌────────────────────┐      │
│  │   WebAPIClient     │         │   WebMVCAdmin      │      │
│  │  (REST API for     │         │  (Admin Panel)     │      │
│  │   Client Apps)     │         │                    │      │
│  └────────────────────┘         └────────────────────┘      │
└─────────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────────┐
│                      Business Layer                          │
│  ┌────────────────────────────────────────────────┐          │
│  │             ServiceLayer                       │          │
│  │  • FileService (with caching decorator)        │          │
│  │  • Business logic & validation                 │          │
│  │  • Plan limits enforcement                     │          │
│  └────────────────────────────────────────────────┘          │
└─────────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────────┐
│                       Data Layer                             │
│  ┌─────────────────┐  ┌──────────────────┐                  │
│  │ DataAccessLayer │  │ PersistenceLayer │                  │
│  │  • Accessors    │  │  • EF Core       │                  │
│  │  • Queries      │  │  • Migrations    │                  │
│  └─────────────────┘  └──────────────────┘                  │
└─────────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────────┐
│               Supporting Layers                              │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────┐        │
│  │ ModelLibrary│  │LoggingLayer │  │ Validators   │        │
│  │  • Entities │  │ • Serilog   │  │ • Fluent     │        │
│  │  • DTOs     │  │ • Email     │  │ • AutoMapper │        │
│  └─────────────┘  └─────────────┘  └──────────────┘        │
└─────────────────────────────────────────────────────────────┘
                          │
                   PostgreSQL Database
```

## 🚀 Features

### File Management
- ✅ File upload/download with 5GB file size support
- ✅ Folder organization with recursive tree structure
- ✅ Root folder auto-creation per user
- ✅ File search by name
- ✅ Recent files tracking
- ✅ Bulk file operations (move, delete)
- ✅ Soft delete for files and folders

### User Management
- ✅ JWT authentication with ASP.NET Identity
- ✅ Role-based authorization (User, Admin)
- ✅ Storage quota tracking
- ✅ User soft delete
- ✅ Password change functionality

### Subscription & Plans
- ✅ Tiered subscription plans
- ✅ Storage limits enforcement
- ✅ File size restrictions per plan
- ✅ Active subscription tracking

### Performance & Reliability
- ✅ Memory caching with decorator pattern
- ✅ Configurable cache TTL
- ✅ Automated error email notifications
- ✅ Structured logging with Serilog
- ✅ Request validation with FluentValidation

## 📋 Prerequisites

- **.NET 9.0 SDK** or later
- **PostgreSQL 12+**
- **Gmail account** (for SMTP error notifications)
- **Git** (for version control)

## 🔧 Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd Proiect_WebStorage
```

### 2. Database Setup

**Create PostgreSQL Database:**
```sql
CREATE DATABASE PPAW;
```

**Run Migrations:**
```bash
cd PersistanceLayer
dotnet ef database update
```

This will:
- Create all tables (Users, Files, Folders, Plans, Subscriptions, etc.)
- Seed initial data (default plans, admin user)

### 3. Configure User Secrets

User secrets keep sensitive data out of source control. Configure them for the WebAPIClient project:

```bash
cd WebAPIClient
dotnet user-secrets init
```

**Required Secrets:**

```bash
# Database Connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=PPAW;Username=postgres;Password=YOUR_DB_PASSWORD"

# JWT Authentication
dotnet user-secrets set "Jwt:Key" "YOUR_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG"

# SMTP Configuration (for error notifications)
dotnet user-secrets set "Smtp:Username" "your-email@gmail.com"
dotnet user-secrets set "Smtp:Password" "your-gmail-app-password"

# Developer Email (receives error notifications)
dotnet user-secrets set "DevEmail" "developer@example.com"
```

**Get Gmail App Password:**
1. Enable 2-Step Verification on your Google Account
2. Go to [Google App Passwords](https://myaccount.google.com/apppasswords)
3. Generate a new app password for "Mail"
4. Use that 16-character password (without spaces) in the secret

**Verify Secrets:**
```bash
dotnet user-secrets list
```

### 4. Build the Solution

```bash
cd ..
dotnet build
```

### 5. Run the Application

**API Server:**
```bash
cd WebAPIClient
dotnet run
```

API will be available at: `http://localhost:5226`

**Admin Panel:**
```bash
cd WebMVCAdmin
dotnet run
```

Admin panel will be available at: `http://localhost:5000`

## 📡 API Documentation

### Authentication Endpoints

**Register User:**
```http
POST /api/auth/register
Content-Type: application/json

{
  "userName": "johndoe",
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

**Login:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

Response includes JWT token to use in subsequent requests.

**Change Password:**
```http
POST /api/auth/change-password
Authorization: Bearer {token}
Content-Type: application/json

{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass123!"
}
```

### File Endpoints

All file endpoints require authentication: `Authorization: Bearer {token}`

**Upload File:**
```http
POST /api/files/upload
Content-Type: multipart/form-data
Authorization: Bearer {token}

fileName: document.pdf
folderId: 1 (optional, defaults to root folder)
file: [binary data]
```

**Download File:**
```http
GET /api/files/{id}/download
Authorization: Bearer {token}
```

**Get User Files:**
```http
GET /api/files
Authorization: Bearer {token}
```

**Search Files:**
```http
GET /api/files/search?query=report
Authorization: Bearer {token}
```

**Move File:**
```http
PUT /api/files/{id}/move?targetFolderId=5
Authorization: Bearer {token}
```

**Delete File:**
```http
DELETE /api/files/{id}
Authorization: Bearer {token}
```

### Folder Endpoints

**Get Folder Tree:**
```http
GET /api/folders/{id}/tree
Authorization: Bearer {token}
```

Returns recursive folder structure with file/subfolder counts at each level.

**Create Folder:**
```http
POST /api/folders
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Documents",
  "parentFolderId": 1
}
```

**Get Root Folders:**
```http
GET /api/folders/root
Authorization: Bearer {token}
```

### User Endpoints

**Get Profile:**
```http
GET /api/users/profile
Authorization: Bearer {token}
```

Returns user info including storage used, storage limit, and root folder ID.

**Update Profile:**
```http
PUT /api/users/profile
Content-Type: application/json
Authorization: Bearer {token}

{
  "userName": "newusername",
  "email": "newemail@example.com"
}
```

**Delete Account:**
```http
DELETE /api/users/account
Authorization: Bearer {token}
```

Soft deletes the user account.

## 🧪 Testing Error Notifications

Test that email notifications are working:

```bash
curl http://localhost:5226/api/test/trigger-error
```

You should receive an email at the configured `DevEmail` with full exception details.

## 🗂️ Project Structure

```
WebStorage/
├── DataAccessLayer/         # Database queries and accessors
│   ├── Accessors/          # Data access objects (FileAccessor, UserAccessor, etc.)
│   └── Interfaces/         # IDataAccessor interface
├── LoggingLayer/           # Logging and email notifications
│   ├── EmailService.cs     # Error notification emails
│   └── LoggerExtensions.cs # Structured logging helpers
├── ModelLibrary/           # Domain models and DTOs
│   └── Models/            # Entity classes (User, File, Folder, etc.)
├── PersistenceLayer/       # Entity Framework Core
│   ├── Migrations/        # Database migrations
│   ├── WebStorageContext.cs
│   └── DataSeeder.cs      # Initial data seeding
├── ServiceLayer/           # Business logic
│   ├── Implementations/   # FileService, CachedFileService
│   └── Interfaces/        # IFileService
├── WebAPIClient/           # REST API
│   ├── Controllers/       # API endpoints
│   ├── DTOs/             # Request/Response models
│   ├── Validators/       # FluentValidation rules
│   └── Mappers/          # AutoMapper profiles
└── WebMVCAdmin/           # Admin panel (MVC)
    ├── Controllers/       # MVC controllers
    ├── Views/            # Razor views
    └── ViewModels/       # View models
```

## 🛠️ Technologies Used

- **Framework:** ASP.NET Core 9.0
- **Database:** PostgreSQL with Entity Framework Core 9.0
- **Authentication:** ASP.NET Identity with JWT Bearer tokens
- **Validation:** FluentValidation 11.3.0
- **Mapping:** AutoMapper 12.0.1
- **Logging:** Serilog with file and console sinks
- **Email:** MailKit 4.14.1
- **Caching:** IMemoryCache
- **API Documentation:** OpenAPI/Swagger

## 🔐 Security Features

- **JWT Authentication:** Secure token-based authentication
- **Password Hashing:** ASP.NET Identity password hashing
- **User Secrets:** Sensitive data never in source control
- **Role-Based Authorization:** User and Admin roles
- **Request Validation:** FluentValidation on all inputs
- **Soft Delete:** User data preserved, not permanently deleted
- **HTTPS:** Enforced in production

## 📊 Configuration Options

### appsettings.json

```json
{
  "CacheOptions": {
    "Enabled": true,              // Enable/disable caching
    "DefaultTtlSeconds": 120,     // Cache expiration time
    "MaxItemsPerSet": 1000        // Max items in cache collections
  },
  "Kestrel": {
    "Limits": {
      "MaxRequestBodySize": 5368709120,  // 5GB max file size
      "RequestHeadersTimeout": "00:05:00" // 5 minute timeout
    }
  }
}
```

## 🐛 Troubleshooting

**Build Errors:**
- Ensure .NET 9.0 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`

**Database Connection Issues:**
- Verify PostgreSQL is running: `sudo systemctl status postgresql`
- Check connection string in user secrets
- Ensure database exists: `psql -U postgres -l`

**Email Notifications Not Working:**
- Verify SMTP secrets are set: `dotnet user-secrets list`
- Check Gmail app password is correct (16 characters, no spaces)
- Ensure 2-Step Verification is enabled on Gmail account
- Check spam folder

**Authentication Fails:**
- Verify JWT:Key secret is at least 32 characters
- Check token expiration (24 hours by default)
- Ensure Authorization header format: `Bearer {token}`


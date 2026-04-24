# 03 - Kiến trúc hệ thống

## Mục lục

1. [Kiến trúc tổng thể](#kiến-trúc-tổng-thể)
2. [Clean Architecture Principles](#clean-architecture-principles)
3. [Sơ đồ luồng dữ liệu](#sơ-đồ-luồng-dữ-liệu)
4. [Các thành phần hệ thống](#các-thành-phần-hệ-thống)
5. [Công nghệ sử dụng](#công-nghệ-sử-dụng)
6. [Chi tiết .NET Clean Architecture](#chi-tiết-net-clean-architecture)
7. [Kiến trúc React Frontend](#kiến-trúc-react-frontend)
8. [Tài liệu tham khảo](#tài-liệu-tham-khảo)

---

## Kiến trúc tổng thể

Hệ thống được thiết kế theo kiến trúc **Clean Architecture** (còn gọi là Onion Architecture) với .NET Backend và React Frontend:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Client Layer                                │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                      React Frontend (SPA)                           ││
│  │   ┌─────────────┐    ┌─────────────┐    ┌─────────────┐             ││
│  │   │   Upload    │    │  Dashboard  │    │   Player    │             ││
│  │   │   Component │    │  Component  │    │  Component  │             ││
│  │   └─────────────┘    └─────────────┘    └─────────────┘             ││
│  └─────────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                            API Gateway                                   │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │  YARP / Ocelot │ Rate Limiting │ Authentication │ API Routing      ││
│  └─────────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    .NET Backend (Clean Architecture)                     │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                         API Layer                                    ││
│  │   Controllers │ Middleware │ Filters │ Model Binders                ││
│  └─────────────────────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                      Application Layer                               ││
│  │   Use Cases │ DTOs │ Interfaces │ Validators │ Mappers              ││
│  └─────────────────────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                       Domain Layer                                   ││
│  │   Entities │ Value Objects │ Domain Events │ Specifications         ││
│  └─────────────────────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                      Infrastructure Layer                            ││
│  │   Repositories │ EF Core │ External Services │ File Storage         ││
│  └─────────────────────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                        Core Layer                                    ││
│  │   Extensions │ Constants │ Helpers │ Shared Enums │ Security Utils  ││
│  └─────────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                          Data Layer                                      │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────────────────┐  │
│  │  PostgreSQL │    │    Redis    │    │   Blob Storage              │  │
│  │  (Metadata) │    │   (Cache)   │    │   (Azure Blob / S3 / Local) │  │
│  └─────────────┘    └─────────────┘    └─────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      AI/ML Services (Python)                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │    ASR      │  │     VAD     │  │    TTS      │  │    AI       │    │
│  │  (Whisper)  │  │  (Silero)   │  │  (VnTTS)    │  │  Context    │    │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Clean Architecture Principles

### Nguyên tắc cốt lõi

Clean Architecture dựa trên các nguyên tắc sau:

1. **Dependency Rule (Nguyên tắc phụ thuộc)**
   - Các phụ thuộc chỉ hướng từ ngoài vào trong
   - Lớp bên trong không biết gì về lớp bên ngoài
   - Domain Layer là trung tâm, không phụ thuộc vào bất kỳ layer nào khác

2. **Separation of Concerns (Tách biệt mối quan tâm)**
   - Mỗi layer có trách nhiệm rõ ràng
   - Business logic tập trung ở Domain và Application layers

3. **Testability (Khả năng kiểm thử)**
   - Các layer bên trong có thể được test độc lập
   - Dễ dàng mock các dependencies

4. **Independence of Frameworks (Độc lập với framework)**
   - Business logic không phụ thuộc vào UI, Database, hay framework

### Dependency Flow Diagram

```
                    ┌──────────────────┐
                    │   API Layer      │
                    │  (Controllers)   │
                    └────────┬─────────┘
                             │ depends on
                             ▼
                    ┌──────────────────┐
                    │ Application      │
                    │    Layer         │
                    │ (Use Cases/DTOs) │
                    └────────┬─────────┘
                             │ depends on
              ┌──────────────┴──────────────┐
              │                             │
              ▼                             ▼
┌──────────────────┐            ┌──────────────────┐
│    Domain        │◄───────────│ Infrastructure   │
│    Layer         │  depends   │    Layer         │
│(Entities/Events) │     on     │(Repositories/    │
└──────────────────┘            │  External Svc)   │
       │                        └──────────────────┘
       │ depends on                      │
       ▼                                 ▼
┌──────────────────────────────────────────────────┐
│                  Core Layer                       │
│  (Extensions, Constants, Helpers, Shared Enums)  │
└──────────────────────────────────────────────────┘
```

### Core Layer (AutoProcess.Core)

**Vị trí trong kiến trúc:**

Core Layer là layer thấp nhất, chứa các tiện ích cross-cutting mà tất cả các layer khác đều có thể phụ thuộc vào. Khác với Domain Layer (chứa business logic cốt lõi), Core Layer chứa các utility không mang tính business-specific.

**Trách nhiệm:**

- **Constants**: Định nghĩa các hằng số sử dụng xuyên suốt ứng dụng
- **Extensions**: Các extension methods cho các kiểu dữ liệu cơ bản (string, DateTime, IEnumerable, etc.)
- **Helpers**: Các static helper classes cho các thao tác thường gặp
- **Shared Enums**: Các enum sử dụng ở nhiều layer
- **Cross-layer Interfaces**: Các interface cơ bản như `IAuditableEntity`, `ISoftDelete`
- **Security Utilities**: Các utility về mã hóa, hashing, JWT
- **Validation**: Các validator và validation rules chung

**Dependency Rule:**

```
Core Layer ← Domain Layer ← Application Layer ← Infrastructure Layer ← API Layer
     ↑____________↑____________↑_______________↑_______________↑
                    Tất cả các layer đều có thể phụ thuộc vào Core
```

**Khi nào dùng Core vs Domain:**

| Core Layer                               | Domain Layer                                |
| ---------------------------------------- | ------------------------------------------- |
| `StringExtensions.ToSlug()`              | `Video.Title` (entity property)             |
| `ApplicationConstants.MaxFileSize`       | `JobStatus` (business state)                |
| `PathHelper.Combine()`                   | `Video.StartProcessing()` (domain behavior) |
| `HashingService.HashPassword()`          | `BusinessRuleViolationException`            |
| `IAuditableEntity` (technical interface) | `IDomainEvent` (business concept)           |

### Các nguyên tắc thiết kế đi kèm

| Nguyên tắc               | Mô tả                             | Áp dụng           |
| ------------------------ | --------------------------------- | ----------------- |
| **SOLID**                | 5 nguyên tắc thiết kế OOP         | Toàn bộ codebase  |
| **CQRS**                 | Tách biệt Command và Query        | Application Layer |
| **Domain-Driven Design** | Tập trung vào domain logic        | Domain Layer      |
| **Repository Pattern**   | Trừu tượng hóa truy cập dữ liệu   | Data Access       |
| **Unit of Work**         | Quản lý transaction               | Data Access       |
| **Mediator Pattern**     | Giảm coupling giữa các thành phần | Application Layer |

---

## Sơ đồ luồng dữ liệu

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  React App   │────▶│  .NET API    │────▶│  PostgreSQL  │
│  (Upload)    │     │  (Controller)│     │  (Metadata)  │
└──────────────┘     └──────────────┘     └──────────────┘
                             │
                             ▼
                      ┌──────────────┐
                      │  Blob Storage│
                      │  (Video File)│
                      └──────────────┘
                             │
                             ▼
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  .NET        │────▶│  Hangfire    │────▶│  Python      │
│  (Queue Job) │     │  (Background)│     │  Services    │
└──────────────┘     └──────────────┘     └──────────────┘
                             │                       │
                             ▼                       ▼
                      ┌──────────────┐     ┌──────────────┐
                      │  Redis       │     │  GPU Worker  │
                      │  (Status)    │     │  (Processing)│
                      └──────────────┘     └──────────────┘
```

---

## Các thành phần hệ thống

### 1. React Frontend

| Thuộc tính           | Mô tả                    |
| -------------------- | ------------------------ |
| **Framework**        | React 18+ với TypeScript |
| **State Management** | Redux Toolkit / Zustand  |
| **UI Library**       | Material-UI / Ant Design |
| **HTTP Client**      | Axios / React Query      |
| **Build Tool**       | Vite                     |

### 2. .NET Backend API

| Thuộc tính            | Mô tả                 |
| --------------------- | --------------------- |
| **Framework**         | .NET 8 (ASP.NET Core) |
| **Architecture**      | Clean Architecture    |
| **ORM**               | Entity Framework Core |
| **Background Jobs**   | Hangfire              |
| **API Documentation** | Swagger/OpenAPI       |

### 3. AI/ML Services (Python)

| Service     | Model         | Free Option         | Paid Option            |
| ----------- | ------------- | ------------------- | ---------------------- |
| ASR         | Whisper       | whisper.cpp (local) | Azure Speech API       |
| VAD         | Silero VAD    | Local               | Local                  |
| TTS         | VnTTS         | VnTTS (local)       | Azure TTS / Google TTS |
| Translation | NLLB / Google | NLLB (local)        | Google Translate API   |
| AI Context  | Llama 2       | Llama 2 (local)     | GPT-4 / Claude         |

---

## Công nghệ sử dụng

### Backend (.NET)

| Component       | Công nghệ             | Phiên bản |
| --------------- | --------------------- | --------- |
| Framework       | .NET 8 (ASP.NET Core) | 8.0+      |
| Language        | C#                    | 12.0+     |
| ORM             | Entity Framework Core | 8.0+      |
| Background Jobs | Hangfire              | 1.8+      |
| Validation      | FluentValidation      | 11.0+     |
| Mapping         | AutoMapper            | 12.0+     |
| Logging         | Serilog               | 3.0+      |
| Testing         | xUnit / NUnit         | Latest    |
| API Docs        | Swashbuckle           | 6.0+      |
| Mediator        | MediatR               | 12.0+     |

### Frontend (React)

| Component  | Công nghệ                      | Phiên bản |
| ---------- | ------------------------------ | --------- |
| Framework  | React                          | 18.0+     |
| Language   | TypeScript                     | 5.0+      |
| Build Tool | Vite                           | 5.0+      |
| State      | Redux Toolkit                  | 2.0+      |
| HTTP       | React Query                    | 5.0+      |
| UI         | Material-UI                    | 5.0+      |
| Forms      | React Hook Form                | 7.0+      |
| Testing    | Vitest / React Testing Library | Latest    |

### AI/ML Services (Python)

| Component   | Công nghệ              | Phiên bản |
| ----------- | ---------------------- | --------- |
| Framework   | FastAPI                | 0.100+    |
| ASR         | Whisper / whisper.cpp  | Latest    |
| VAD         | Silero VAD             | Latest    |
| TTS         | VnTTS / Coqui TTS      | Latest    |
| Translation | NLLB / Transformers    | Latest    |
| AI Context  | Llama 2 / Transformers | Latest    |

### Infrastructure

| Component | Free Tier               | Paid Option                 |
| --------- | ----------------------- | --------------------------- |
| Hosting   | Azure App Service (F1)  | Azure App Service (P1V3)    |
| Database  | Azure PostgreSQL (Free) | Azure PostgreSQL (Standard) |
| Cache     | Redis (self-hosted)     | Azure Cache for Redis       |
| Storage   | Azure Blob (5GB free)   | Azure Blob (Pay-as-you-go)  |
| GPU       | -                       | Azure NCas T4 v3 / AWS g4dn |

---

## Chi tiết .NET Clean Architecture

### Project Structure (Clean Architecture)

```
src/
 ├── AutoProcess.Core/                       # Core Layer (Cross-cutting concerns)
 │   ├── Common/
 │   │   ├── Constants/                      # Application constants
 │   │   │   ├── ApplicationConstants.cs     # App-wide constants (e.g., CORS policies, claim types)
 │   │   │   ├── ErrorCodes.cs               # Standardized error codes
 │   │   │   ├── RolesConstants.cs           # Role names (Admin, User)
 │   │   │   └── ProcessingConstants.cs      # Processing-related constants
 │   │   ├── Extensions/                     # C# extension methods
 │   │   │   ├── StringExtensions.cs         # String utilities (ToSlug, Truncate, etc.)
 │   │   │   ├── DateTimeExtensions.cs       # DateTime utilities (ToUnixTimestamp, ToVnTime, etc.)
 │   │   │   ├── EnumerableExtensions.cs     # Collection utilities (ForEach, Paginate, etc.)
 │   │   │   ├── GuidExtensions.cs           # Guid utilities
 │   │   │   └── ObjectExtensions.cs         # Object utilities (ToJson, Clone, etc.)
 │   │   ├── Helpers/                        # Static helper classes
 │   │   │   ├── PathHelper.cs               # Path manipulation utilities
 │   │   │   ├── FileHelper.cs               # File operations helper
 │   │   │   ├── ValidationHelper.cs         # Validation utilities
 │   │   │   └── EncryptionHelper.cs         # Encryption/Hashing utilities
 │   │   ├── Enums/                          # Shared enums across layers
 │   │   │   ├── ResultStatus.cs             # Operation result status
 │   │   │   ├── ErrorCode.cs                # Standardized error codes
 │   │   │   └── LanguageCode.cs             # ISO language codes
 │   │   └── Interfaces/                     # Cross-layer interfaces
 │   │       ├── IAuditableEntity.cs         # Interface for entities with audit info
 │   │       ├── ISoftDelete.cs              # Interface for soft-delete entities
 │   │       └── IEntity.cs                  # Base entity interface
 │   ├── Security/
 │   │   ├── Cryptography/
 │   │   │   ├── HashingService.cs           # Password hashing utilities
 │   │   │   └── EncryptionService.cs        # Encryption/Decryption utilities
 │   │   └── Jwt/
 │   │       ├── JwtSettings.cs              # JWT configuration options
 │   │       └── JwtConstants.cs             # JWT-related constants
 │   ├── Validation/
 │   │   ├── Validators/                     # Shared validators
 │   │   │   ├── EmailValidator.cs           # Email validation rules
 │   │   │   ├── PasswordValidator.cs        # Password validation rules
 │   │   │   └── FileValidator.cs            # File validation rules
 │   │   └── Rules/                          # Validation rules
 │   │       ├── IValidationRule.cs          # Validation rule interface
 │   │       └── ValidationResult.cs         # Validation result model
 │   └── AutoProcess.Core.csproj
 │
 ├── AutoProcess.Api/                        # API Layer (Presentation)
│   ├── Controllers/
│   │   ├── VideoController.cs              # API endpoints cho Video
│   │   ├── AudioController.cs              # API endpoints cho Audio
│   │   └── AuthController.cs               # API endpoints cho Auth
│   ├── Middleware/
│   │   ├── ExceptionHandlingMiddleware.cs  # Xử lý exception toàn cục
│   │   ├── RequestLoggingMiddleware.cs     # Logging requests
│   │   └── AuthenticationMiddleware.cs     # Authentication
│   ├── Filters/
│   │   └── ValidationFilter.cs             # Validation filter
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs  # DI setup
│   │   └── MiddlewareExtensions.cs         # Middleware pipeline
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Program.cs                          # Application entry point
│   └── AutoProcess.Api.csproj
│
├── AutoProcess.Application/                # Application Layer
│   ├── Features/                           # Tổ chức theo tính năng (Feature Folders)
│   │   ├── Videos/
│   │   │   ├── Commands/
│   │   │   │   ├── UploadVideo/
│   │   │   │   │   ├── UploadVideoCommand.cs
│   │   │   │   │   ├── UploadVideoCommandHandler.cs
│   │   │   │   │   └── UploadVideoCommandValidator.cs
│   │   │   │   ├── ProcessVideo/
│   │   │   │   │   ├── ProcessVideoCommand.cs
│   │   │   │   │   └── ProcessVideoCommandHandler.cs
│   │   │   │   └── UpdateVideoStatus/
│   │   │   │       ├── UpdateVideoStatusCommand.cs
│   │   │   │       └── UpdateVideoStatusCommandHandler.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetVideoById/
│   │   │   │   │   ├── GetVideoByIdQuery.cs
│   │   │   │   │   ├── GetVideoByIdQueryHandler.cs
│   │   │   │   │   └── VideoDto.cs
│   │   │   │   ├── GetVideoList/
│   │   │   │   │   ├── GetVideoListQuery.cs
│   │   │   │   │   ├── GetVideoListQueryHandler.cs
│   │   │   │   │   └── VideoListItemDto.cs
│   │   │   │   └── GetProcessingStatus/
│   │   │   │       ├── GetProcessingStatusQuery.cs
│   │   │   │       └── ProcessingStatusDto.cs
│   │   │   └── EventHandlers/
│   │   │       ├── VideoUploadedEventHandler.cs
│   │   │       └── VideoProcessedEventHandler.cs
│   │   ├── Audio/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   └── Auth/
│   │       ├── Commands/
│   │       │   ├── Login/
│   │       │   └── Register/
│   │       └── Queries/
│   │           └── GetCurrentUser/
│   ├── Common/
│   │   ├── Interfaces/                     # Port/Abstractions
│   │   │   ├── Repositories/
│   │   │   │   ├── IVideoRepository.cs
│   │   │   │   ├── IProcessingJobRepository.cs
│   │   │   │   ├── IUserRepository.cs
│   │   │   │   └── IRepository{T}.cs
│   │   │   ├── Services/
│   │   │   │   ├── IStorageService.cs
│   │   │   │   ├── IPythonAIService.cs
│   │   │   │   └── IEmailService.cs
│   │   │   └── Identity/
│   │   │       ├── ICurrentUser.cs
│   │   │       └── IJwtTokenService.cs
│   │   ├── Behaviors/                      # MediatR Pipeline Behaviors
│   │   │   ├── ValidationBehavior.cs
│   │   │   ├── LoggingBehavior.cs
│   │   │   └── PerformanceBehavior.cs
│   │   ├── Exceptions/
│   │   │   ├── ValidationException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   └── BusinessException.cs
│   │   ├── Mappings/
│   │   │   └── MappingProfile.cs           # AutoMapper profiles
│   │   ├── Models/
│   │   │   ├── PagedResult.cs
│   │   │   ├── Result.cs
│   │   │   └── PaginatedList.cs
│   │   └── Specifications/
│   │       └── BaseSpecification.cs
│   ├── DependencyInjection.cs              # Application DI setup
│   └── AutoProcess.Application.csproj
│
├── AutoProcess.Domain/                     # Domain Layer (Core)
│   ├── Entities/                           # Domain Entities
│   │   ├── BaseEntity.cs                   # Base class với Id, CreatedAt, UpdatedAt
│   │   ├── Video.cs                        # Video aggregate root
│   │   ├── ProcessingJob.cs                # Processing job entity
│   │   ├── User.cs                         # User entity
│   │   └── AuditLog.cs                     # Audit logging
│   ├── ValueObjects/                       # Value Objects (DDD)
│   │   ├── VideoPath.cs
│   │   ├── Duration.cs
│   │   └── Language.cs
│   ├── Enums/
│   │   ├── JobStatus.cs                    # Queued, Processing, Completed, Failed
│   │   ├── VideoFormat.cs                  # MP4, AVI, MKV, etc.
│   │   ├── ProcessingStep.cs               # Extract, VAD, ASR, Translate, TTS, Merge
│   │   └── UserRole.cs                     # Admin, User
│   ├── Events/                             # Domain Events
│   │   ├── IDomainEvent.cs
│   │   ├── VideoUploadedEvent.cs
│   │   ├── VideoProcessingStartedEvent.cs
│   │   ├── VideoProcessedEvent.cs
│   │   └── VideoProcessingFailedEvent.cs
│   ├── Exceptions/
│   │   ├── DomainException.cs
│   │   └── BusinessRuleViolationException.cs
│   ├── Specifications/                     # Specification Pattern
│   │   ├── ISpecification.cs
│   │   └── VideoByStatusSpecification.cs
│   ├── Primitives/                         # Guard clauses
│   │   └── Guard.cs
│   └── AutoProcess.Domain.csproj
│
├── AutoProcess.Infrastructure/             # Infrastructure Layer
│   ├── Persistence/
│   │   ├── AppDbContext.cs                 # EF Core DbContext
│   │   ├── DbContextSeeder.cs              # Database seeding
│   │   ├── Configurations/                 # Entity Configurations
│   │   │   ├── VideoConfiguration.cs
│   │   │   ├── ProcessingJobConfiguration.cs
│   │   │   └── UserConfiguration.cs
│   │   ├── Migrations/                     # EF Core Migrations
│   │   │   └── (generated migrations)
│   │   └── Interceptors/
│   │       ├── AuditableEntityInterceptor.cs
│   │       └── DispatchDomainEventsInterceptor.cs
│   ├── Repositories/                       # Repository Implementations
│   │   ├── Repository{T}.cs                # Generic repository base
│   │   ├── VideoRepository.cs
│   │   ├── ProcessingJobRepository.cs
│   │   └── UserRepository.cs
│   ├── Services/                           # External Service Implementations
│   │   ├── StorageService.cs               # Blob storage (Azure/S3/Local)
│   │   ├── PythonAIService.cs              # HTTP client to Python services
│   │   ├── EmailService.cs                 # Email sending
│   │   └── DateTimeService.cs              # Abstraction for testing
│   ├── BackgroundJobs/                     # Hangfire Jobs
│   │   ├── VideoProcessingJob.cs           # Background job for video processing
│   │   └── CleanupOldFilesJob.cs           # Scheduled cleanup job
│   ├── Identity/
│   │   ├── IdentityService.cs
│   │   ├── CurrentUser.cs
│   │   └── JwtTokenService.cs
│   ├── ExternalClients/
│   │   ├── PythonApiClient.cs              # Typed HttpClient for Python API
│   │   └── PollyPolicies.cs                # Resilience policies
│   ├── Caching/
│   │   ├── CacheService.cs
│   │   └── DistributedCacheExtensions.cs
│   ├── DependencyInjection.cs              # Infrastructure DI setup
│   └── AutoProcess.Infrastructure.csproj
│
└── tests/
    ├── AutoProcess.Core.Tests/             # Unit tests for Core
    │   ├── Common/
    │   │   ├── Extensions/
    │   │   │   ├── StringExtensionsTests.cs
    │   │   │   └── DateTimeExtensionsTests.cs
    │   │   └── Helpers/
    │   │       └── PathHelperTests.cs
    │   └── Security/
    │       └── Cryptography/
    │           └── HashingServiceTests.cs
    ├── AutoProcess.Domain.Tests/           # Unit tests for Domain
    │   ├── Entities/
    │   │   └── VideoTests.cs
    │   └── ValueObjects/
    │       └── DurationTests.cs
    ├── AutoProcess.Application.Tests/      # Unit tests for Application
    │   ├── Features/
    │   │   └── Videos/
    │   │       └── Commands/
    │   │           └── UploadVideoCommandHandlerTests.cs
    │   └── Common/
    │       └── Behaviors/
    │           └── ValidationBehaviorTests.cs
    ├── AutoProcess.Infrastructure.Tests/   # Integration tests
    │   ├── Persistence/
    │   │   └── AppDbContextTests.cs
    │   └── Services/
    │       └── StorageServiceTests.cs
    └── AutoProcess.Api.Tests/              # API Integration tests
        ├── Controllers/
        │   └── VideoControllerTests.cs
        └── Middleware/
            └── ExceptionHandlingMiddlewareTests.cs
```

### Layer Responsibilities

#### 1. Domain Layer (Core)

**Trách nhiệm:**

- Chứa business logic cốt lõi
- Định nghĩa Entities, Value Objects, Domain Events
- Business rules và validation rules
- Hoàn toàn độc lập, không phụ thuộc vào bên ngoài

**Không chứa:**

- Database code
- API code
- External service calls
- Framework dependencies

```csharp
// Domain/Entities/Video.cs
public class Video : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string? OutputPath { get; private set; }
    public VideoFormat Format { get; private set; }
    public ProcessingJob? ProcessingJob { get; private set; }
    public Guid UserId { get; private set; }

    // Domain behavior
    public void StartProcessing()
    {
        if (ProcessingJob != null && ProcessingJob.Status != JobStatus.Completed)
        {
            throw new BusinessRuleViolationException("Video is already being processed");
        }

        ProcessingJob = new ProcessingJob
        {
            Id = Guid.NewGuid(),
            VideoId = Id,
            Status = JobStatus.Queued,
            StartedAt = DateTime.UtcNow
        };

        AddDomainEvent(new VideoProcessingStartedEvent(Id));
    }
}
```

#### 2. Application Layer

**Trách nhiệm:**

- Orchestrate domain objects để thực hiện use cases
- Định nghĩa interfaces (ports) cho infrastructure
- DTOs và mapping
- Validation ( FluentValidation)
- CQRS với MediatR

```csharp
// Application/Features/Videos/Commands/UploadVideo/UploadVideoCommandHandler.cs
public class UploadVideoCommandHandler : IRequestHandler<UploadVideoCommand, Result<VideoDto>>
{
    private readonly IVideoRepository _videoRepository;
    private readonly IStorageService _storageService;
    private readonly IMapper _mapper;
    private readonly ILogger<UploadVideoCommandHandler> _logger;

    public UploadVideoCommandHandler(
        IVideoRepository videoRepository,
        IStorageService storageService,
        IMapper mapper,
        ILogger<UploadVideoCommandHandler> logger)
    {
        _videoRepository = videoRepository;
        _storageService = storageService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<VideoDto>> Handle(UploadVideoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Upload file to storage
            var filePath = await _storageService.UploadVideoAsync(
                request.File,
                request.FileName,
                cancellationToken);

            // 2. Create video entity
            var video = new Video
            {
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                FilePath = filePath,
                Format = VideoFormat.MP4,
                UserId = request.UserId
            };

            // 3. Persist
            await _videoRepository.AddAsync(video, cancellationToken);
            await _videoRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Video uploaded successfully: {VideoId}", video.Id);

            return Result<VideoDto>.Success(_mapper.Map<VideoDto>(video));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload video: {FileName}", request.FileName);
            return Result<VideoDto>.Failure("Failed to upload video");
        }
    }
}
```

#### 3. Infrastructure Layer

**Trách nhiệm:**

- Implement các interfaces định nghĩa ở Application layer
- EF Core DbContext và repositories
- External services (storage, email, AI services)
- Background jobs (Hangfire)

```csharp
// Infrastructure/Repositories/VideoRepository.cs
public class VideoRepository : Repository<Video>, IVideoRepository
{
    public VideoRepository(AppDbContext context) : base(context) { }

    public async Task<Video?> GetWithProcessingJobAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(v => v.ProcessingJob)
            .FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task<IList<Video>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);
    }
}
```

#### 4. API Layer (Presentation)

**Trách nhiệm:**

- Controllers nhận HTTP requests
- Middleware pipeline
- Model binding và validation
- Response formatting

```csharp
// Api/Controllers/VideoController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VideoController : ControllerBase
{
    private readonly ISender _mediator;

    public VideoController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(500 * 1024 * 1024)] // 500MB
    [RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
    public async Task<ActionResult<Result<VideoDto>>> UploadVideo(
        [FromForm] UploadVideoCommand command,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        command.File = file;
        command.UserId = GetCurrentUserId();

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<VideoDto>>> GetVideo(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVideoByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result)
            : NotFound(result);
    }

    [HttpGet("status/{jobId:guid}")]
    public async Task<ActionResult<Result<ProcessingStatusDto>>> GetProcessingStatus(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProcessingStatusQuery(jobId), cancellationToken);

        return result.IsSuccess
            ? Ok(result)
            : NotFound(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim?.Value ?? throw new UnauthorizedAccessException());
    }
}
```

### Dependency Injection Setup

```csharp
// Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add layers
builder.Services.AddApplicationLayer(builder.Configuration);
builder.Services.AddInfrastructureLayer(builder.Configuration);

// Add controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AutoProcess API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard
app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.MapControllers();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
```

### Best Practices

#### 1. Error Handling

```csharp
// Application/Common/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}
```

#### 2. Unit of Work Pattern

```csharp
// Infrastructure/Persistence/AppDbContext.cs
public class AppDbContext : DbContext, IUnitOfWork
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUserService,
        IMediator mediator) : base(options)
    {
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Add audit information
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService.UserId;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedBy = _currentUserService.UserId;
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
            }
        }

        // Dispatch domain events
        var domainEntities = ChangeTracker.Entries<IDomainEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        domainEntities.ForEach(e => e.Entity.ClearDomainEvents());

        var result = await base.SaveChangesAsync(cancellationToken);

        // Publish domain events
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
```

#### 3. Resilience with Polly

```csharp
// Infrastructure/ExternalClients/PollyPolicies.cs
public static class PollyPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                TimeSpan.FromMilliseconds(new Random().Next(0, 100)));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
```

---

## Kiến trúc React Frontend (Scalable & Reusable)

### Architecture Principles

Để đạt được khả năng **scale** và **tái sử dụng** tối đa, React Frontend được thiết kế theo các nguyên tắc:

1. **Atomic Design Pattern** - Phân chia component theo cấp bậc nguyên tử
2. **Feature-Sliced Design (FSD)** - Tổ chức code theo domain/feature
3. **Composition Pattern** - Ưu tiên composition over inheritance
4. **Custom Hooks** - Tách logic khỏi UI
5. **Render Props / Compound Components** - Tăng tính linh hoạt

### Atomic Design Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                         Pages / Screens                          │  # Trang hoàn chỉnh
├─────────────────────────────────────────────────────────────────┤
│                         Widgets                                  │  # Khối UI độc lập (Header, Sidebar)
├─────────────────────────────────────────────────────────────────┤
│                      Features                                    │  # User actions (Upload, Search)
├─────────────────────────────────────────────────────────────────┤
│                    UI Components                                 │  # Reusable UI (Button, Modal)
├─────────────────────────────────────────────────────────────────┤
│                         Primitives                               │  # Base components (Box, Text)
└─────────────────────────────────────────────────────────────────┘
```

### Project Structure (Scalable)

```
src/
├── app/
│   ├── providers/                          # App providers
│   │   ├── StoreProvider.tsx               # Redux/Zustand provider
│   │   ├── QueryProvider.tsx               # React Query provider
│   │   ├── ThemeProvider.tsx               # Theme provider
│   │   └── index.ts
│   ├── styles/                             # Global styles
│   │   ├── globals.css
│   │   └── variables.css
│   └── router/                             # Routing configuration
│       ├── routes.tsx
│       └── ProtectedRoute.tsx
│
├── pages/                                  # Page components (route-level)
│   ├── HomePage/
│   │   ├── HomePage.tsx
│   │   ├── HomePage.test.tsx
│   │   └── index.ts
│   ├── UploadPage/
│   │   ├── UploadPage.tsx
│   │   └── index.ts
│   ├── DashboardPage/
│   │   ├── DashboardPage.tsx
│   │   └── index.ts
│   └── NotFoundPage/
│       └── NotFoundPage.tsx
│
├── widgets/                                # Complex UI blocks
│   ├── Header/
│   │   ├── Header.tsx
│   │   ├── Header.test.tsx
│   │   └── index.ts
│   ├── Sidebar/
│   │   ├── Sidebar.tsx
│   │   └── index.ts
│   ├── VideoPlayer/
│   │   ├── VideoPlayer.tsx
│   │   ├── hooks/
│   │   │   └── useVideoControls.ts
│   │   └── index.ts
│   └── ProcessingQueue/
│       ├── ProcessingQueue.tsx
│       └── index.ts
│
├── features/                               # User interactions / Business logic
│   ├── auth/
│   │   ├── components/
│   │   │   ├── LoginForm/
│   │   │   │   ├── LoginForm.tsx
│   │   │   │   ├── LoginForm.test.tsx
│   │   │   │   └── index.ts
│   │   │   └── RegisterForm/
│   │   ├── api/
│   │   │   ├── authApi.ts
│   │   │   └── authApi.test.ts
│   │   ├── hooks/
│   │   │   └── useAuth.ts
│   │   ├── store/
│   │   │   ├── authSlice.ts
│   │   │   └── authSelectors.ts
│   │   ├── types/
│   │   │   └── auth.types.ts
│   │   └── index.ts
│   │
│   ├── video-upload/
│   │   ├── components/
│   │   │   ├── VideoUploader/
│   │   │   │   ├── VideoUploader.tsx
│   │   │   │   ├── VideoUploader.test.tsx
│   │   │   │   └── index.ts
│   │   │   ├── DropZone/
│   │   │   └── UploadProgress/
│   │   ├── api/
│   │   │   └── uploadApi.ts
│   │   ├── hooks/
│   │   │   └── useVideoUpload.ts
│   │   ├── utils/
│   │   │   └── fileValidation.ts
│   │   └── index.ts
│   │
│   ├── video-processing/
│   │   ├── components/
│   │   │   ├── ProcessingStatus/
│   │   │   ├── ProcessingSteps/
│   │   │   └── ProgressIndicator/
│   │   ├── api/
│   │   │   └── processingApi.ts
│   │   ├── hooks/
│   │   │   └── useProcessingStatus.ts
│   │   └── index.ts
│   │
│   └── video-list/
│       ├── components/
│       │   ├── VideoList/
│       │   ├── VideoCard/
│       │   ├── VideoFilters/
│       │   └── VideoPagination/
│       ├── api/
│       │   └── videoListApi.ts
│       ├── hooks/
│       │   └── useVideoList.ts
│       └── index.ts
│
├── entities/                               # Business entities (data models)
│   ├── User/
│   │   ├── types/
│   │   │   └── user.types.ts
│   │   ├── api/
│   │   │   └── userApi.ts
│   │   ├── hooks/
│   │   │   └── useUser.ts
│   │   └── index.ts
│   ├── Video/
│   │   ├── types/
│   │   │   └── video.types.ts
│   │   ├── api/
│   │   │   └── videoApi.ts
│   │   ├── hooks/
│   │   │   └── useVideo.ts
│   │   ├── utils/
│   │   │   └── videoFormatters.ts
│   │   └── index.ts
│   └── ProcessingJob/
│       ├── types/
│       │   └── job.types.ts
│       ├── utils/
│       │   └── jobStatusHelpers.ts
│       └── index.ts
│
├── shared/                                 # Shared code (reusable across features)
│   ├── ui/                                 # UI Kit (Atomic components)
│   │   ├── primitives/                     # Headless UI components
│   │   │   ├── Button/
│   │   │   │   ├── Button.tsx
│   │   │   │   ├── Button.test.tsx
│   │   │   │   ├── Button.stories.tsx      # Storybook
│   │   │   │   └── index.ts
│   │   │   ├── Input/
│   │   │   ├── Select/
│   │   │   ├── Modal/
│   │   │   ├── Dropdown/
│   │   │   ├── Tooltip/
│   │   │   ├── Portal/
│   │   │   └── Slot/
│   │   ├── components/                     # Composed UI components
│   │   │   ├── AlertDialog/
│   │   │   ├── DataTable/
│   │   │   ├── FormField/
│   │   │   ├── Avatar/
│   │   │   ├── Badge/
│   │   │   ├── Card/
│   │   │   ├── Progress/
│   │   │   ├── Toast/
│   │   │   └── Skeleton/
│   │   └── index.ts
│   │
│   ├── lib/                                # Utilities & helpers
│   │   ├── api/
│   │   │   ├── axios.ts                    # Axios instance config
│   │   │   ├── queryClient.ts              # React Query config
│   │   │   └── api.types.ts
│   │   ├── utils/
│   │   │   ├── cn.ts                       # Class name utility
│   │   │   ├── formatDate.ts
│   │   │   ├── formatSize.ts
│   │   │   └── debounce.ts
│   │   ├── constants/
│   │   │   ├── apiEndpoints.ts
│   │   │   ├── errorMessages.ts
│   │   │   └── validationRules.ts
│   │   └── types/
│   │       ├── common.types.ts
│   │       └── api.types.ts
│   │
│   ├── hooks/                              # Shared custom hooks
│   │   ├── useLocalStorage.ts
│   │   ├── useDebounce.ts
│   │   ├── useMediaQuery.ts
│   │   ├── useOnClickOutside.ts
│   │   └── useKeyboardShortcut.ts
│   │
│   ├── context/                            # React contexts
│   │   ├── ToastContext.tsx
│   │   ├── ConfirmationContext.tsx
│   │   └── ThemeContext.tsx
│   │
│   └── config/                             # App configuration
│       ├── env.ts
│       └── constants.ts
│
└── main.tsx
```

### Component Design Patterns

#### 1. Compound Components Pattern

Cho phép component linh hoạt trong việc compose:

```tsx
// shared/ui/components/Modal/Modal.tsx
import { createContext, useContext, ReactNode } from "react";

interface ModalContextType {
  isOpen: boolean;
  onClose: () => void;
}

const ModalContext = createContext<ModalContextType | null>(null);

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  children: ReactNode;
}

export const Modal = ({ isOpen, onClose, children }: ModalProps) => {
  return (
    <ModalContext.Provider value={{ isOpen, onClose }}>
      {isOpen && (
        <div className="modal-overlay" onClick={onClose}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            {children}
          </div>
        </div>
      )}
    </ModalContext.Provider>
  );
};

// Compound components
Modal.Header = function ModalHeader({ children }: { children: ReactNode }) {
  return <div className="modal-header">{children}</div>;
};

Modal.Body = function ModalBody({ children }: { children: ReactNode }) {
  return <div className="modal-body">{children}</div>;
};

Modal.Footer = function ModalFooter({ children }: { children: ReactNode }) {
  const context = useContext(ModalContext);
  if (!context) throw new Error("Modal.Footer must be used within Modal");

  return <div className="modal-footer">{children}</div>;
};

// Usage:
// <Modal isOpen={isOpen} onClose={onClose}>
//   <Modal.Header>Title</Modal.Header>
//   <Modal.Body>Content</Modal.Body>
//   <Modal.Footer>
//     <Button onClick={onClose}>Cancel</Button>
//     <Button onClick={handleSubmit}>Submit</Button>
//   </Modal.Footer>
// </Modal>
```

#### 2. Render Props / Custom Hooks Pattern

Tách logic khỏi UI:

```tsx
// features/video-upload/hooks/useVideoUpload.ts
import { useState, useCallback } from "react";

interface UseVideoUploadOptions {
  maxSize?: number;
  allowedTypes?: string[];
  onProgress?: (progress: number) => void;
}

export const useVideoUpload = (options: UseVideoUploadOptions = {}) => {
  const {
    maxSize = 500 * 1024 * 1024,
    allowedTypes = ["video/mp4", "video/avi"],
  } = options;

  const [file, setFile] = useState<File | null>(null);
  const [progress, setProgress] = useState(0);
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const validateFile = useCallback(
    (file: File): string | null => {
      if (!allowedTypes.includes(file.type)) {
        return "Invalid file type. Allowed: MP4, AVI";
      }
      if (file.size > maxSize) {
        return `File too large. Max size: ${maxSize / 1024 / 1024}MB`;
      }
      return null;
    },
    [allowedTypes, maxSize],
  );

  const selectFile = useCallback(
    (selectedFile: File) => {
      const validationError = validateFile(selectedFile);
      if (validationError) {
        setError(validationError);
        return false;
      }
      setFile(selectedFile);
      setError(null);
      return true;
    },
    [validateFile],
  );

  const upload = useCallback(async () => {
    if (!file) return null;

    setIsUploading(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append("file", file);

      // Simulate upload with progress
      const response = await uploadVideoApi(formData, {
        onUploadProgress: (progressEvent) => {
          const percentCompleted = Math.round(
            (progressEvent.loaded * 100) / progressEvent.total!,
          );
          setProgress(percentCompleted);
          options.onProgress?.(percentCompleted);
        },
      });

      return response.data;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Upload failed");
      return null;
    } finally {
      setIsUploading(false);
    }
  }, [file, options]);

  const reset = useCallback(() => {
    setFile(null);
    setProgress(0);
    setIsUploading(false);
    setError(null);
  }, []);

  return {
    // State
    file,
    progress,
    isUploading,
    error,

    // Actions
    selectFile,
    upload,
    reset,

    // Computed
    isValid: !!file && !error,
    progressText: `${progress}% uploaded`,
  };
};
```

```tsx
// features/video-upload/components/VideoUploader/VideoUploader.tsx
import { useVideoUpload } from "../../hooks/useVideoUpload";
import { DropZone } from "../DropZone";
import { UploadProgress } from "../UploadProgress";
import { Button, Alert } from "@/shared/ui";

export const VideoUploader = () => {
  const { file, progress, isUploading, error, selectFile, upload, reset } =
    useVideoUpload({
      maxSize: 500 * 1024 * 1024,
      onProgress: (p) => console.log(`Upload progress: ${p}%`),
    });

  const handleFileSelect = (selectedFile: File) => {
    if (selectFile(selectedFile)) {
      upload();
    }
  };

  return (
    <div className="video-uploader">
      {!file ? (
        <DropZone onFileSelect={handleFileSelect} accept="video/*" />
      ) : (
        <div className="upload-progress-container">
          <UploadProgress
            fileName={file.name}
            progress={progress}
            onCancel={reset}
          />
        </div>
      )}

      {error && <Alert severity="error">{error}</Alert>}
    </div>
  );
};
```

#### 3. Polymorphic Components

Component có thể render as different HTML elements:

```tsx
// shared/ui/primitives/Text/Text.tsx
import { ElementType, ComponentPropsWithoutRef } from "react";
import { cn } from "@/shared/lib/utils/cn";

type TextSize = "xs" | "sm" | "base" | "lg" | "xl" | "2xl";
type TextWeight = "normal" | "medium" | "semibold" | "bold";
type TextColor = "default" | "muted" | "primary" | "error";

interface TextProps<C extends ElementType> {
  as?: C;
  size?: TextSize;
  weight?: TextWeight;
  color?: TextColor;
  children: React.ReactNode;
  className?: string;
}

type PolymorphicText = <C extends ElementType = "span">(
  props: TextProps<C> & ComponentPropsWithoutRef<C>,
) => React.ReactElement | null;

export const Text: PolymorphicText = <C extends ElementType = "span">({
  as,
  size = "base",
  weight = "normal",
  color = "default",
  className,
  children,
  ...props
}: TextProps<C> & ComponentPropsWithoutRef<C>) => {
  const Component = as || "span";

  const classNames = cn(
    "text-base",
    size && `text-${size}`,
    weight && `font-${weight}`,
    color && `text-${color}`,
    className,
  );

  return (
    <Component className={classNames} {...props}>
      {children}
    </Component>
  );
};

// Usage:
// <Text as="h1" size="2xl" weight="bold">Title</Text>
// <Text as="p" size="sm" color="muted">Description</Text>
// <Text as="span" weight="semibold" color="error">Error message</Text>
```

#### 4. Controlled vs Uncontrolled Components

```tsx
// shared/ui/components/FormField/FormField.tsx
import { forwardRef, useImperativeHandle, useState } from "react";

interface FormFieldProps {
  value?: string;
  defaultValue?: string;
  onChange?: (value: string) => void;
  label?: string;
  error?: string;
  placeholder?: string;
  disabled?: boolean;
}

export const FormField = forwardRef<HTMLInputElement, FormFieldProps>(
  (
    {
      value: controlledValue,
      defaultValue = "",
      onChange,
      label,
      error,
      placeholder,
      disabled,
    },
    ref,
  ) => {
    const [uncontrolledValue, setUncontrolledValue] = useState(defaultValue);

    const isControlled = controlledValue !== undefined;
    const currentValue = isControlled ? controlledValue : uncontrolledValue;

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      const newValue = e.target.value;

      if (!isControlled) {
        setUncontrolledValue(newValue);
      }

      onChange?.(newValue);
    };

    useImperativeHandle(ref, () => ({
      focus: () => inputRef.current?.focus(),
      value: currentValue,
    }));

    const inputRef = ref as React.MutableRefObject<HTMLInputElement | null>;

    return (
      <div className="form-field">
        {label && <label className="form-label">{label}</label>}
        <input
          ref={inputRef}
          type="text"
          value={currentValue}
          onChange={handleChange}
          placeholder={placeholder}
          disabled={disabled}
          className={cn("form-input", error && "form-input--error")}
        />
        {error && <span className="form-error">{error}</span>}
      </div>
    );
  },
);

FormField.displayName = "FormField";
```

### State Management Strategy

```
┌─────────────────────────────────────────────────────────────────┐
│                      State Management                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Server State                                                    │
│  └── React Query (TanStack Query)                                │
│      • Caching & background updates                              │
│      • Deduplication & request cancellation                      │
│      • Pagination & infinite queries                             │
│                                                                  │
│  Client State                                                    │
│  └── Zustand (or Redux Toolkit)                                  │
│      • UI state (modals, sidebars)                               │
│      • Authentication state                                      │
│      • Form state (complex multi-step)                           │
│                                                                  │
│  URL State                                                       │
│  └── React Router + URLSearchParams                              │
│      • Filters, sorting, pagination                              │
│      • Shareable state                                           │
│                                                                  │
│  Local State                                                     │
│  └── useState / useReducer                                       │
│      • Component-specific state                                  │
│      • Form input state                                          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

#### React Query Setup (Server State)

```tsx
// shared/lib/api/queryClient.ts
import { QueryClient } from "@tanstack/react-query";

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

// entities/Video/api/videoApi.ts
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/shared/lib/api/axios";
import { Video, VideoListParams } from "../types";

export const videoKeys = {
  all: ["videos"] as const,
  lists: () => [...videoKeys.all, "list"] as const,
  list: (filters: VideoListParams) => [...videoKeys.lists(), filters] as const,
  details: () => [...videoKeys.all, "detail"] as const,
  detail: (id: string) => [...videoKeys.details(), id] as const,
  processingStatus: (jobId: string) =>
    [...videoKeys.detail(jobId), "status"] as const,
};

export const useVideoList = (params: VideoListParams) => {
  return useQuery({
    queryKey: videoKeys.list(params),
    queryFn: () => api.get("/videos", { params }).then((res) => res.data),
  });
};

export const useVideo = (id: string) => {
  return useQuery({
    queryKey: videoKeys.detail(id),
    queryFn: () => api.get(`/videos/${id}`).then((res) => res.data),
    enabled: !!id,
  });
};

export const useProcessingStatus = (jobId: string) => {
  return useQuery({
    queryKey: videoKeys.processingStatus(jobId),
    queryFn: () => api.get(`/videos/${jobId}/status`).then((res) => res.data),
    refetchInterval: 5000, // Poll every 5 seconds
    enabled: !!jobId,
  });
};

export const useUploadVideo = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (formData: FormData) =>
      api
        .post("/videos/upload", formData, {
          headers: { "Content-Type": "multipart/form-data" },
        })
        .then((res) => res.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: videoKeys.lists() });
    },
  });
};
```

#### Zustand Setup (Client State)

```tsx
// shared/store/createUIStore.ts
import { create } from "zustand";
import { persist } from "zustand/middleware";

interface UIState {
  // Sidebar
  isSidebarOpen: boolean;
  toggleSidebar: () => void;
  closeSidebar: () => void;

  // Theme
  theme: "light" | "dark";
  toggleTheme: () => void;

  // Toast
  toasts: Array<{
    id: string;
    message: string;
    type: "success" | "error" | "info";
  }>;
  addToast: (message: string, type: "success" | "error" | "info") => void;
  removeToast: (id: string) => void;

  // Modal
  activeModal: string | null;
  modalData: Record<string, unknown>;
  openModal: (modalId: string, data?: Record<string, unknown>) => void;
  closeModal: () => void;
}

export const useUIStore = create<UIState>()(
  persist(
    (set) => ({
      // Sidebar
      isSidebarOpen: true,
      toggleSidebar: () =>
        set((state) => ({ isSidebarOpen: !state.isSidebarOpen })),
      closeSidebar: () => set({ isSidebarOpen: false }),

      // Theme
      theme: "light",
      toggleTheme: () =>
        set((state) => ({
          theme: state.theme === "light" ? "dark" : "light",
        })),

      // Toast
      toasts: [],
      addToast: (message, type) => {
        const id = Math.random().toString(36).substr(2, 9);
        set((state) => ({
          toasts: [...state.toasts, { id, message, type }],
        }));
        setTimeout(() => {
          set((state) => ({
            toasts: state.toasts.filter((t) => t.id !== id),
          }));
        }, 5000);
      },
      removeToast: (id) =>
        set((state) => ({
          toasts: state.toasts.filter((t) => t.id !== id),
        })),

      // Modal
      activeModal: null,
      modalData: {},
      openModal: (modalId, data = {}) =>
        set({ activeModal: modalId, modalData: data }),
      closeModal: () => set({ activeModal: null, modalData: {} }),
    }),
    {
      name: "ui-storage",
      partialize: (state) => ({ theme: state.theme }),
    },
  ),
);
```

### Component Composition Examples

#### Example 1: Reusable DataTable

```tsx
// shared/ui/components/DataTable/DataTable.tsx
import { ReactNode } from "react";
import { cn } from "@/shared/lib/utils/cn";

interface Column<T> {
  key: string;
  header: string;
  render?: (item: T) => ReactNode;
  sortable?: boolean;
  width?: string;
}

interface DataTableProps<T> {
  data: T[];
  columns: Column<T>[];
  isLoading?: boolean;
  emptyMessage?: string;
  onRowClick?: (item: T) => void;
  className?: string;
}

export function DataTable<T extends { id: string | number }>({
  data,
  columns,
  isLoading,
  emptyMessage = "No data available",
  onRowClick,
  className,
}: DataTableProps<T>) {
  if (isLoading) {
    return <DataTableSkeleton columns={columns.length} rows={5} />;
  }

  if (data.length === 0) {
    return (
      <div className="data-table-empty">
        <p>{emptyMessage}</p>
      </div>
    );
  }

  return (
    <div className={cn("data-table-container", className)}>
      <table className="data-table">
        <thead>
          <tr>
            {columns.map((col) => (
              <th
                key={col.key}
                style={{ width: col.width }}
                className={cn(col.sortable && "sortable")}
              >
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((item) => (
            <tr
              key={item.id}
              onClick={() => onRowClick?.(item)}
              className={cn(onRowClick && "clickable")}
            >
              {columns.map((col) => (
                <td key={col.key}>
                  {col.render
                    ? col.render(item)
                    : ((item as Record<string, unknown>)[col.key] as ReactNode)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// Usage:
// <DataTable
//   data={videos}
//   columns={[
//     { key: 'title', header: 'Title', render: (v) => <VideoTitle video={v} /> },
//     { key: 'status', header: 'Status', render: (v) => <StatusBadge status={v.status} /> },
//     { key: 'createdAt', header: 'Created', render: (v) => formatDate(v.createdAt) },
//     { key: 'actions', header: '', render: (v) => <VideoActions video={v} /> },
//   ]}
//   onRowClick={(video) => navigate(`/video/${video.id}`)}
// />
```

#### Example 2: Composable Form

```tsx
// shared/ui/components/Form/Form.tsx
import { FormProvider, useForm } from "react-hook-form";
import { ReactNode } from "react";

interface FormProps<T extends Record<string, unknown>> {
  children: ReactNode;
  onSubmit: (data: T) => void;
  defaultValues?: Partial<T>;
  className?: string;
}

export function Form<T extends Record<string, unknown>>({
  children,
  onSubmit,
  defaultValues,
  className,
}: FormProps<T>) {
  const methods = useForm<T>({ defaultValues });

  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(onSubmit)} className={className}>
        {children}
      </form>
    </FormProvider>
  );
}

// shared/ui/components/Form/FormField.tsx
import { useFormContext, Controller } from "react-hook-form";

interface FormFieldProps {
  name: string;
  label?: string;
  error?: string;
  children: (props: {
    value: unknown;
    onChange: (value: unknown) => void;
    onBlur: () => void;
  }) => ReactNode;
}

export function FormField({ name, label, error, children }: FormFieldProps) {
  const { control } = useFormContext();

  return (
    <div className="form-field">
      {label && <label className="form-label">{label}</label>}
      <Controller
        name={name}
        control={control}
        render={({ field }) => children(field)}
      />
      {error && <span className="form-error">{error}</span>}
    </div>
  );
}

// Usage:
// <Form<UploadFormData> onSubmit={handleSubmit}>
//   <FormField name="title" label="Title">
//     {({ value, onChange }) => (
//       <Input value={value} onChange={onChange} />
//     )}
//   </FormField>
//   <FormField name="description" label="Description">
//     {({ value, onChange }) => (
//       <Textarea value={value} onChange={onChange} />
//     )}
//   </FormField>
//   <Button type="submit">Submit</Button>
// </Form>
```

### Best Practices for Scalability

1. **Colocation**: Đặt code gần nơi nó được sử dụng
2. **Barrel Exports**: Sử dụng `index.ts` để export public API
3. **Type Safety**: Luôn define TypeScript interfaces cho props
4. **Storybook**: Document components với Storybook
5. **Testing**: Viết tests cho từng layer (unit, integration, e2e)
6. **Performance**: Sử dụng `React.memo`, `useMemo`, `useCallback` hợp lý
7. **Accessibility**: Tuân thủ WCAG guidelines
8. **Code Splitting**: Lazy load components khi cần

### External Resources

- [Feature-Sliced Design](https://feature-sliced.design/)
- [Atomic Design by Brad Frost](https://bradfrost.com/blog/post/atomic-web-design/)
- [React Patterns](https://reactpatterns.com/)
- [Radix UI](https://www.radix-ui.com/) - Unstyled, accessible components
- [Storybook](https://storybook.js.org/) - Component documentation
- [React Hook Form](https://react-hook-form.com/) - Performant form library
- [TanStack Query](https://tanstack.com/query) - Server state management
- [Zustand](https://zustand-demo.pmnd.rs/) - State management

```

---

## Tài liệu tham khảo

- [Tổng quan dự án](../01-overview/README.md)
- [Yêu cầu hệ thống](../02-requirements/README.md)
- [Đặc tả kỹ thuật](../05-technical-specs/README.md)
- [Deployment](../07-deployment/README.md)
- [Testing Guide](../08-testing/README.md)

### External Resources

- [Microsoft Clean Architecture Guide](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Domain-Driven Design](https://martinfowler.com/tags/domain%20driven%20design.html)
```

# Phase 1: Core Foundation & Video Upload

**Mục tiêu:** Thiết lập nền tảng cơ bản, infrastructure và chức năng upload video.

**Thời gian ước tính:** 2-3 tuần

**Người phụ trách:** Backend Team

---

## Tổng quan

Phase 1 tập trung vào việc thiết lập nền tảng cơ bản của hệ thống:

- Clean Architecture solution structure
- Database setup với PostgreSQL
- Core layer với các utilities
- Authentication system
- Video upload functionality

---

## User Stories Chi Tiết

### US-1.1: Project Setup & Infrastructure

**Là một** Developer  
**Tôi muốn** có project structure rõ ràng theo Clean Architecture  
**Để** dễ dàng maintain và mở rộng về sau

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 5

**Acceptance Criteria:**

- [ ] Tạo solution với các projects: Api, Application, Domain, Infrastructure, Core
- [ ] Cấu hình Dependency Injection cho từng layer
- [ ] Setup Serilog logging
- [ ] Cấu hình CORS policies
- [ ] Setup Swagger documentation

**Tasks:**

1. Tạo .NET solution với 5 projects
2. Cài đặt NuGet packages (MediatR, EF Core, FluentValidation, AutoMapper, Serilog)
3. Cấu hình Program.cs với DI setup
4. Tạo folder structure theo Clean Architecture
5. Setup Serilog với file và console sinks

**Dependencies:** Không có

---

### US-1.2: Database Setup & Entity Framework

**Là một** Developer  
**Tôi muốn** có database schema và EF Core setup  
**Để** lưu trữ dữ liệu videos, users, processing jobs

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 8

**Acceptance Criteria:**

- [ ] Tạo DbContext với các entities: Video, ProcessingJob, User, AuditLog
- [ ] Cấu hình connection string cho PostgreSQL
- [ ] Tạo và apply migrations
- [ ] Setup Repository pattern
- [ ] Setup Unit of Work pattern

**Tasks:**

1. Thiết kế database schema
2. Tạo entities trong Domain layer
3. Tạo DbContext trong Infrastructure layer
4. Cấu hình entity configurations
5. Tạo initial migration
6. Implement generic repository
7. Implement Unit of Work

**Dependencies:** US-1.1

---

### US-1.3: Core Layer Implementation

**Là một** Developer  
**Tôi muốn** có Core layer với các utilities chung  
**Để** tái sử dụng code across layers

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 8

**Acceptance Criteria:**

- [ ] Implement Constants (ApplicationConstants, ErrorCodes, RolesConstants)
- [ ] Implement Extensions (StringExtensions, DateTimeExtensions, EnumerableExtensions)
- [ ] Implement Helpers (PathHelper, FileHelper, ValidationHelper)
- [ ] Implement Shared Enums (ResultStatus, ErrorCode)
- [ ] Implement Cross-layer Interfaces (IAuditableEntity, ISoftDelete, IEntity)

**Tasks:**

1. Tạo project AutoProcess.Core
2. Implement Common/Constants
3. Implement Common/Extensions
4. Implement Common/Helpers
5. Implement Common/Enums
6. Implement Common/Interfaces
7. Implement Security/Cryptography
8. Implement Security/Jwt
9. Implement Validation/Validators

**Dependencies:** US-1.1

---

### US-1.4: Authentication & Authorization

**Là một** User  
**Tôi muốn** có thể đăng ký và đăng nhập  
**Để** sử dụng các tính năng của hệ thống

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 8

**Acceptance Criteria:**

- [ ] Implement JWT authentication
- [ ] API endpoints: POST /auth/register, POST /auth/login
- [ ] Password hashing với BCrypt
- [ ] Role-based authorization (Admin, User)
- [ ] Token refresh mechanism

**Tasks:**

1. Tạo User entity với password hash
2. Implement HashingService trong Core layer
3. Tạo Auth commands/queries trong Application layer
4. Implement JwtTokenService
5. Tạo AuthController
6. Cấu hình JWT authentication trong Program.cs
7. Implement authorization policies

**Dependencies:** US-1.2, US-1.3

---

### US-1.5: Video Upload API

**Là một** User  
**Tôi muốn** upload video file  
**Để** hệ thống xử lý và dịch

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 13

**Acceptance Criteria:**

- [ ] API endpoint: POST /video/upload
- [ ] Hỗ trợ các định dạng: MP4, AVI, MKV, MOV
- [ ] Max file size: 2GB
- [ ] Lưu file vào Blob Storage (Azure/S3/Local)
- [ ] Tạo Video entity và ProcessingJob
- [ ] Trả về job_id và status

**Tasks:**

1. Tạo UploadVideoCommand và Handler
2. Implement FileValidator
3. Tạo IStorageService interface
4. Implement Azure Blob Storage service
5. Implement S3 Storage service
6. Implement Local Storage service
7. Tạo VideoController với upload endpoint
8. Xử lý multipart form data
9. Tạo ProcessingJob khi upload thành công

**Dependencies:** US-1.2, US-1.3, US-1.4

---

### US-1.6: Video Status Tracking

**Là một** User  
**Tôi muốn** kiểm tra trạng thái xử lý video  
**Để** biết khi nào hoàn thành

**Độ ưu tiên:** Trung bình  
**Điểm ước tính:** 5

**Acceptance Criteria:**

- [ ] API endpoint: GET /video/{job_id}/status
- [ ] Trả về: status, progress, current_step, steps_completed
- [ ] Real-time status updates

**Tasks:**

1. Tạo GetProcessingStatusQuery và Handler
2. Tạo ProcessingStatusDto
3. Tạo endpoint trong VideoController
4. Implement status calculation logic

**Dependencies:** US-1.5

---

## Deliverables

| Deliverable                                | Status | Notes |
| ------------------------------------------ | ------ | ----- |
| Clean Architecture solution với 5 projects | ☐      |       |
| Database PostgreSQL với schema hoàn chỉnh  | ☐      |       |
| Authentication system với JWT              | ☐      |       |
| Video upload API hoạt động                 | ☐      |       |
| Blob storage integration                   | ☐      |       |
| Status tracking API                        | ☐      |       |

---

## Technical Notes

### Database Schema (Phase 1)

```sql
-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Videos table
CREATE TABLE videos (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    title VARCHAR(200) NOT NULL,
    description TEXT,
    file_path VARCHAR(500) NOT NULL,
    format VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Processing Jobs table
CREATE TABLE processing_jobs (
    id UUID PRIMARY KEY,
    video_id UUID NOT NULL REFERENCES videos(id),
    status VARCHAR(50) NOT NULL,
    progress INTEGER NOT NULL DEFAULT 0,
    current_step VARCHAR(100),
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    error_message TEXT
);

-- Audit Logs table
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY,
    entity_type VARCHAR(100) NOT NULL,
    entity_id UUID NOT NULL,
    action VARCHAR(50) NOT NULL,
    old_value JSONB,
    new_value JSONB,
    user_id UUID REFERENCES users(id),
    created_at TIMESTAMP NOT NULL
);
```

### API Endpoints (Phase 1)

| Method | Endpoint           | Description           | Auth Required |
| ------ | ------------------ | --------------------- | ------------- |
| POST   | /auth/register     | Register new user     | No            |
| POST   | /auth/login        | Login user            | No            |
| POST   | /video/upload      | Upload video          | Yes           |
| GET    | /video/{id}/status | Get processing status | Yes           |

---

## Tài liệu tham khảo

- [Kiến trúc hệ thống](../../03-architecture/README.md)
- [Đặc tả kỹ thuật](../../05-technical-specs/README.md)
- [CQRS Implementation](../../05-technical-specs/README.md#cqrs-implementation-với-mediatr)

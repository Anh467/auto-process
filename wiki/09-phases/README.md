# 09 - Development Phases & User Stories

## Tổng quan

Dự án được chia thành 4 phases chính, mỗi phase tập trung vào một nhóm tính năng cụ thể để đảm bảo delivery đúng tiến độ và có thể demo được sau mỗi phase.

## Mục lục

1. [Phase 1: Core Foundation & Video Upload](#phase-1-core-foundation--video-upload)
2. [Phase 2: Audio Processing & ASR](#phase-2-audio-processing--asr)
3. [Phase 3: Translation & AI Context](#phase-3-translation--ai-context)
4. [Phase 4: TTS & Video Merge](#phase-4-tts--video-merge)

---

## Phase 1: Core Foundation & Video Upload

**Mục tiêu:** Thiết lập nền tảng cơ bản, infrastructure và chức năng upload video.

**Thời gian ước tính:** 2-3 tuần

### User Stories

#### US-1.1: Project Setup & Infrastructure

- **Là một** Developer
- **Tôi muốn** có project structure rõ ràng theo Clean Architecture
- **Để** dễ dàng maintain và mở rộng về sau

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

---

#### US-1.2: Database Setup & Entity Framework

- **Là một** Developer
- **Tôi muốn** có database schema và EF Core setup
- **Để** lưu trữ dữ liệu videos, users, processing jobs

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

---

#### US-1.3: Core Layer Implementation

- **Là một** Developer
- **Tôi muốn** có Core layer với các utilities chung
- **Để** tái sử dụng code across layers

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

---

#### US-1.4: Authentication & Authorization

- **Là một** User
- **Tôi muốn** có thể đăng ký và đăng nhập
- **Để** sử dụng các tính năng của hệ thống

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

---

#### US-1.5: Video Upload API

- **Là một** User
- **Tôi muốn** upload video file
- **Để** hệ thống xử lý và dịch

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

---

#### US-1.6: Video Status Tracking

- **Là một** User
- **Tôi muốn** kiểm tra trạng thái xử lý video
- **Để** biết khi nào hoàn thành

**Acceptance Criteria:**

- [ ] API endpoint: GET /video/{job_id}/status
- [ ] Trả về: status, progress, current_step, steps_completed
- [ ] Real-time status updates

**Tasks:**

1. Tạo GetProcessingStatusQuery và Handler
2. Tạo ProcessingStatusDto
3. Tạo endpoint trong VideoController
4. Implement status calculation logic

---

### Deliverables Phase 1

- [ ] Clean Architecture solution với 5 projects
- [ ] Database PostgreSQL với schema hoàn chỉnh
- [ ] Authentication system với JWT
- [ ] Video upload API hoạt động
- [ ] Blob storage integration
- [ ] Status tracking API

---

## Phase 2: Audio Processing & ASR

**Mục tiêu:** Extract audio từ video và chuyển giọng nói thành text.

**Thời gian ước tính:** 3-4 tuần

### User Stories

#### US-2.1: Audio Extraction

- **Là một** System
- **Tôi muốn** extract audio từ video file
- **Để** xử lý ASR

**Acceptance Criteria:**

- [ ] Sử dụng FFmpeg để extract audio
- [ ] Output: WAV format, 16kHz, mono, 16-bit
- [ ] Lưu audio file vào storage
- [ ] Xử lý async với Hangfire

**Tasks:**

1. Cài đặt FFmpeg
2. Tạo ProcessVideoCommand
3. Implement audio extraction service
4. Cấu hình Hangfire background jobs
5. Lưu audio file vào storage

---

#### US-2.2: Voice Activity Detection (VAD)

- **Là một** System
- **Tôi muốn** phát hiện segments có giọng nói
- **Để** chỉ xử lý ASR trên segments cần thiết

**Acceptance Criteria:**

- [ ] Sử dụng Silero VAD model
- [ ] Output: danh sách speech segments
- [ ] Tách speech và non-speech audio

**Tasks:**

1. Setup Python VAD service
2. Tạo IPythonAIService interface
3. Implement VAD API call
4. Xử lý VAD response
5. Lưu speech segments vào database

---

#### US-2.3: Automatic Speech Recognition (ASR)

- **Là một** System
- **Tôi muốn** chuyển giọng nói thành text
- **Để** dịch sang tiếng Việt

**Acceptance Criteria:**

- [ ] Sử dụng Whisper model (large-v3)
- [ ] Hỗ trợ multiple languages (Chinese, English)
- [ ] Output: segments với timestamps
- [ ] Confidence score cho mỗi segment

**Tasks:**

1. Setup Python ASR service với Whisper
2. Implement batch processing
3. Xử lý GPU optimization
4. Lưu ASR results vào database
5. Handle ASR errors and retries

---

#### US-2.4: ASR Result Management

- **Là một** User
- **Tôi muốn** xem transcript của video
- **Để** kiểm tra độ chính xác

**Acceptance Criteria:**

- [ ] API endpoint: GET /video/{job_id}/transcript
- [ ] Hiển thị segments với timestamps
- [ ] Confidence scores
- [ ] Detected language

**Tasks:**

1. Tạo GetTranscriptQuery và Handler
2. Tạo TranscriptDto
3. Tạo endpoint trong VideoController
4. Format transcript response

---

### Deliverables Phase 2

- [ ] Audio extraction service
- [ ] VAD integration
- [ ] ASR service với Whisper
- [ ] Transcript API
- [ ] Background job processing

---

## Phase 3: Translation & AI Context

**Mục tiêu:** Dịch transcript sang tiếng Việt và sử dụng AI để hiệu chỉnh ngữ cảnh.

**Thời gian ước tính:** 2-3 tuần

### User Stories

#### US-3.1: Text Translation

- **Là một** System
- **Tôi muốn** dịch transcript sang tiếng Việt
- **Để** người xem hiểu nội dung

**Acceptance Criteria:**

- [ ] Sử dụng Google Translate API (hoặc NLLB local)
- [ ] Dịch từng segment giữ nguyên timestamps
- [ ] Batch translation optimization
- [ ] Cache translation results

**Tasks:**

1. Tạo TranslateCommand và Handler
2. Implement translation service
3. Xử lý batch translation
4. Lưu translated segments
5. Handle translation errors

---

#### US-3.2: AI Context Refinement

- **Là một** System
- **Tôi muốn** AI hiệu chỉnh bản dịch theo ngữ cảnh
- **Để** bản dịch tự nhiên và chính xác hơn

**Acceptance Criteria:**

- [ ] Sử dụng GPT-4 (hoặc Llama 2 local)
- [ ] Cung cấp context: video topic, target audience, tone
- [ ] AI refine từng segment
- [ ] Lưu notes về changes

**Tasks:**

1. Tạo RefineTranslationCommand
2. Implement AI context service
3. Design context prompt template
4. Xử lý AI response
5. Lưu refined segments

---

#### US-3.3: Translation Review API

- **Là một** User
- **Tôi muốn** xem và so sánh bản dịch gốc và đã refine
- **Để** đánh giá chất lượng

**Acceptance Criteria:**

- [ ] API endpoint: GET /video/{job_id}/translations
- [ ] Hiển thị original translation và refined translation
- [ ] AI notes và explanations

**Tasks:**

1. Tạo GetTranslationsQuery
2. Tạo TranslationCompareDto
3. Tạo endpoint so sánh
4. Format response

---

### Deliverables Phase 3

- [ ] Translation service
- [ ] AI context refinement
- [ ] Translation comparison API
- [ ] Batch processing optimization

---

## Phase 4: TTS & Video Merge

**Mục tiêu:** Chuyển text thành giọng nói và merge vào video gốc.

**Thời gian ước tính:** 3-4 tuần

### User Stories

#### US-4.1: Text-to-Speech Generation

- **Là một** System
- **Tôi muốn** chuyển translated text thành giọng nói
- **Để** tạo audio tiếng Việt

**Acceptance Criteria:**

- [ ] Sử dụng VnTTS (hoặc Azure/Google TTS)
- [ ] Hỗ trợ multiple voices (male, female)
- [ ] Điều chỉnh speed và pitch
- [ ] Generate audio cho từng segment

**Tasks:**

1. Tạo GenerateTTSCommand
2. Implement TTS service
3. Hỗ trợ multiple voice options
4. Xử lý speed/pitch adjustment
5. Lưu TTS audio files

---

#### US-4.2: Audio Mixing

- **Là một** System
- **Tôi muốn** mix TTS audio với background audio
- **Để** giữ lại âm thanh nền gốc

**Acceptance Criteria:**

- [ ] Giảm volume original audio trong segments có speech
- [ ] Mix với TTS audio
- [ ] Giữ nguyên non-speech audio
- [ ] Smooth transitions

**Tasks:**

1. Tạo MixAudioCommand
2. Implement audio mixing service
3. Xử lý volume adjustment
4. Merge audio tracks
5. Handle timing synchronization

---

#### US-4.3: Video Merge

- **Là một** System
- **Tôi muốn** replace audio trong video bằng audio đã dịch
- **Để** tạo video hoàn chỉnh

**Acceptance Criteria:**

- [ ] Sử dụng FFmpeg
- [ ] Replace audio track
- [ ] Giữ nguyên video quality
- [ ] Output: MP4 H.264

**Tasks:**

1. Tạo MergeVideoCommand
2. Implement video merge service
3. Xử lý FFmpeg encoding
4. Optimize output quality/size
5. Lưu final video

---

#### US-4.4: Video Download

- **Là một** User
- **Tôi muốn** download video đã xử lý
- **Để** xem offline

**Acceptance Criteria:**

- [ ] API endpoint: GET /video/{job_id}/download
- [ ] Streaming download
- [ ] CDN integration (optional)
- [ ] Expiry mechanism

**Tasks:**

1. Tạo download endpoint
2. Implement file streaming
3. Xử lý CDN integration
4. Setup download expiry
5. Track download analytics

---

#### US-4.5: Processing Pipeline Optimization

- **Là một** System
- **Tôi muốn** optimize toàn bộ pipeline
- **Để** giảm thời gian xử lý

**Acceptance Criteria:**

- [ ] Parallel processing khi có thể
- [ ] Retry mechanism với exponential backoff
- [ ] Progress tracking chi tiết
- [ ] Error recovery

**Tasks:**

1. Refactor processing pipeline
2. Implement parallel processing
3. Add retry policies với Polly
4. Improve progress tracking
5. Add error recovery logic

---

### Deliverables Phase 4

- [ ] TTS service
- [ ] Audio mixing service
- [ ] Video merge service
- [ ] Download API
- [ ] Optimized processing pipeline
- [ ] Complete end-to-end flow

---

## Technical Debt & Future Improvements

### Sau Phase 4

- [ ] Implement caching layer (Redis)
- [ ] Add video preview generation
- [ ] Implement subtitle export (SRT, VTT)
- [ ] Add batch video processing
- [ ] Implement user quotas and rate limiting
- [ ] Add analytics dashboard
- [ ] Implement webhook notifications
- [ ] Add multi-language support (beyond Vietnamese)

---

## Tài liệu tham khảo

- [Kiến trúc hệ thống](../03-architecture/README.md)
- [Đặc tả kỹ thuật](../05-technical-specs/README.md)
- [API Documentation](../06-api-docs/README.md)

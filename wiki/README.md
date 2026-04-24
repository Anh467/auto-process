# Wiki - Auto Process Video Localization System

## Giới thiệu

Dự án **Auto Process** là hệ thống tự động xử lý video để localize (bản địa hóa) nội dung từ tiếng Trung/Anh sang tiếng Việt với sự hỗ trợ của AI.

## Tech Stack

| Layer        | Technology                                  |
| ------------ | ------------------------------------------- |
| **Frontend** | React 18+ với TypeScript, Vite, Material-UI |
| **Backend**  | .NET 8 (ASP.NET Core), Clean Architecture   |
| **Database** | PostgreSQL, Redis                           |
| **AI/ML**    | Python, Whisper, VnTTS, NLLB, Llama 2       |
| **Storage**  | Azure Blob Storage / AWS S3                 |

## Cấu trúc Wiki

Wiki được tổ chức thành các thư mục sau:

### 📁 [01-overview](./01-overview/README.md) - Tổng quan dự án

- Mục tiêu dự án
- Phạm vi dự án
- Các bên liên quan
- Thuật ngữ chuyên ngành

### 📁 [02-requirements](./02-requirements/README.md) - Yêu cầu hệ thống

- Yêu cầu chức năng
- Yêu cầu phi chức năng
- Yêu cầu kỹ thuật
- Use cases

### 📁 [03-architecture](./03-architecture/README.md) - Kiến trúc hệ thống

- Kiến trúc tổng thể (.NET + React)
- Sơ đồ luồng dữ liệu
- Các thành phần hệ thống
- Clean Architecture pattern
- React project structure

### 📁 [04-workflow](./04-workflow/README.md) - Quy trình xử lý

- Lưu đồ xử lý chi tiết
- Các bước thực hiện
- Xử lý lỗi
- Tối ưu hóa

### 📁 [05-technical-specs](./05-technical-specs/README.md) - Đặc tả kỹ thuật

- Đặc tả API
- Định dạng dữ liệu
- Cấu hình hệ thống
- Performance requirements

### 📁 [06-api-docs](./06-api-docs/README.md) - Tài liệu API

- API endpoints
- Request/Response examples
- Authentication
- Rate limiting

### 📁 [07-deployment](./07-deployment/README.md) - Triển khai

- **Free Tier Options** - Cấu hình miễn phí cho giai đoạn đầu
- **Paid Options** - Nâng cấp khi mở rộng
- **GPU Server Pricing** - Bảng giá GPU chi tiết
- **Video Storage Solutions** - Lưu trữ video (Azure Blob / S3)
- CI/CD pipeline
- Migration strategy

### 📁 [08-testing](./08-testing/README.md) - Kiểm thử

- Test strategy
- Test cases
- Automation tests (xUnit, Playwright)
- Performance testing

## Quy trình xử lý video chính

```
┌─────────────┐    ┌─────────────────────┐    ┌──────────────────────┐
│   Video     │───▶│  Tách âm thanh      │───▶│  Nhận diện giọng nói │
│   Input     │    │  (Speech/Non-speech)│    │  (ASR Trung/Anh)     │
└─────────────┘    └─────────────────────┘    └──────────────────────┘
                                                        │
                                                        ▼
┌─────────────┐    ┌─────────────────────┐    ┌──────────────────────┐
│   Video     │◀───│  Ghép âm thanh      │◀───│  Chuyển text sang    │
│   Output    │    │  + Subtitle         │    │  giọng nói (TTS)     │
└─────────────┘    └─────────────────────┘    └──────────────────────┘
                                                        ▲
                                                        │
┌─────────────┐    ┌─────────────────────┐    ┌──────────────────────┐
│  Context    │◀───│  Dịch sang tiếng    │◀───│  AI nắm context      │
│  AI         │    │  Việt + AI context  │    │                      │
└─────────────┘    └─────────────────────┘    └──────────────────────┘
```

## Deployment Options

### Free Tier (Giai đoạn đầu)

| Component | Free Option            |
| --------- | ---------------------- |
| Frontend  | Vercel (Free)          |
| Backend   | Azure App Service F1   |
| Database  | Azure PostgreSQL Free  |
| Storage   | Azure Blob 5GB Free    |
| AI        | Self-hosted CPU models |

**Cost: $0/month**

### Paid Tier (Khi mở rộng)

| Component | Paid Option               | Cost        |
| --------- | ------------------------- | ----------- |
| Backend   | Azure P1V3                | ~$70/month  |
| Database  | Azure PostgreSQL Standard | ~$50/month  |
| GPU       | AWS g4dn.xlarge Spot      | ~$115/month |
| AI APIs   | Azure Speech, GPT-4       | Pay-per-use |

## Phiên bản

- **Version:** 2.0.0
- **Last Updated:** 2026-04-24
- **Status:** Planning Phase
- **Tech Stack:** .NET 8 + React 18

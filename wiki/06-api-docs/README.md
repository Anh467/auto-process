# 06 - Tài liệu API

## Mục lục

1. [Tổng quan API](#tổng-quan-api)
2. [Authentication](#authentication)
3. [Video API](#video-api)
4. [Audio API](#audio-api)
5. [Translation API](#translation-api)
6. [TTS API](#tts-api)
7. [Error Codes](#error-codes)
8. [Rate Limiting](#rate-limiting)

---

## Tổng quan API

### Base URLs

| Environment | URL                                       |
| ----------- | ----------------------------------------- |
| Production  | `https://api.auto-process.com/v1`         |
| Staging     | `https://staging-api.auto-process.com/v1` |
| Development | `http://localhost:8000/v1`                |

### API Versioning

API được version qua URL path:

- `/v1/` - Version 1 (current)

### Response Format

Tất cả responses đều theo định dạng JSON:

```json
{
  "success": true,
  "data": { ... },
  "error": null,
  "timestamp": "2026-04-24T21:00:00Z"
}
```

Hoặc khi có lỗi:

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input parameters",
    "details": { ... }
  },
  "timestamp": "2026-04-24T21:00:00Z"
}
```

---

## Authentication

### JWT Token

Hầu hết các endpoints yêu cầu JWT token trong Authorization header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Obtaining Token

#### POST /auth/login

Đăng nhập để lấy token.

**Request:**

```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "token_type": "bearer",
    "expires_in": 3600,
    "refresh_token": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
  }
}
```

#### POST /auth/refresh

Làm mới access token.

**Request:**

```json
{
  "refresh_token": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "access_token": "bmV3IGFjY2VzcyB0b2tlbiBoZXJl...",
    "token_type": "bearer",
    "expires_in": 3600
  }
}
```

---

## Video API

### Upload Video

#### POST /video/upload

Upload video để xử lý.

**Headers:**

```
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

**Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| file | binary | Yes | Video file (MP4, AVI, MKV, MOV) |
| options | JSON string | No | Processing options |

**Options JSON:**

```json
{
  "source_language": "auto",
  "target_language": "vi",
  "enable_ai_context": true,
  "voice": "vietnamese_female",
  "subtitle_format": "srt",
  "keep_original_audio": false
}
```

**Response (202 Accepted):**

```json
{
  "success": true,
  "data": {
    "job_id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "queued",
    "file_name": "video.mp4",
    "file_size": 104857600,
    "created_at": "2026-04-24T21:00:00Z",
    "estimated_time": 1200,
    "queue_position": 3
  }
}
```

### Get Job Status

#### GET /video/{job_id}/status

Kiểm tra trạng thái job.

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| job_id | UUID | Job ID |

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "job_id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "processing",
    "progress": 45,
    "current_step": "translating",
    "current_step_description": "Đang dịch sang tiếng Việt",
    "steps_completed": [
      { "name": "uploaded", "completed_at": "2026-04-24T21:00:05Z" },
      { "name": "extracted_audio", "completed_at": "2026-04-24T21:00:10Z" },
      { "name": "vad", "completed_at": "2026-04-24T21:01:00Z" },
      { "name": "asr", "completed_at": "2026-04-24T21:05:00Z" }
    ],
    "steps_remaining": [
      { "name": "translate", "description": "Dịch thuật" },
      { "name": "ai_context", "description": "AI Context" },
      { "name": "tts", "description": "Text-to-Speech" },
      { "name": "merge", "description": "Ghép video" }
    ],
    "estimated_completion": "2026-04-24T21:20:00Z",
    "created_at": "2026-04-24T21:00:00Z",
    "updated_at": "2026-04-24T21:05:00Z"
  }
}
```

**Status Values:**
| Status | Description |
|--------|-------------|
| `queued` | Đang chờ trong hàng |
| `processing` | Đang xử lý |
| `completed` | Hoàn thành |
| `failed` | Thất bại |
| `cancelled` | Đã hủy |

### Download Result

#### GET /video/{job_id}/download

Download video đã xử lý.

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| job_id | UUID | Job ID |

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| include_subtitle | boolean | Include subtitle file in response |
| format | string | Output format (mp4, mkv) |

**Response (200 OK):**

- Content-Type: video/mp4
- Content-Disposition: attachment; filename="video_vietnamese.mp4"
- Body: Binary video data

### Cancel Job

#### DELETE /video/{job_id}

Hủy job đang xử lý.

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "job_id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "cancelled",
    "cancelled_at": "2026-04-24T21:10:00Z"
  }
}
```

### List Jobs

#### GET /video/jobs

Danh sách jobs của user.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| page | integer | 1 | Page number |
| limit | integer | 20 | Items per page |
| status | string | all | Filter by status |
| from_date | datetime | - | Filter from date |
| to_date | datetime | - | Filter to date |

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "jobs": [
      {
        "job_id": "550e8400-e29b-41d4-a716-446655440000",
        "status": "completed",
        "file_name": "video1.mp4",
        "progress": 100,
        "created_at": "2026-04-24T20:00:00Z",
        "completed_at": "2026-04-24T20:20:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "total": 50,
      "total_pages": 3
    }
  }
}
```

---

## Audio API

### Speech to Text

#### POST /audio/speech-to-text

Chuyển giọng nói thành text.

**Headers:**

```
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

**Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| audio | binary | Yes | Audio file (WAV, MP3, AAC) |
| language | string | No | Language code (zh, en, auto) |

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "segments": [
      {
        "id": 1,
        "start": 0.5,
        "end": 3.2,
        "text": "Hello, welcome to our channel",
        "language": "en",
        "confidence": 0.98
      },
      {
        "id": 2,
        "start": 3.5,
        "end": 6.8,
        "text": "今天我们要介绍一个新产品",
        "language": "zh",
        "confidence": 0.95
      }
    ],
    "duration": 120.5,
    "detected_language": "mixed",
    "processing_time": 15.2
  }
}
```

### Voice Activity Detection

#### POST /audio/voice-activity-detection

Tách âm thanh giọng nói và không giọng nói.

**Headers:**

```
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

**Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| audio | binary | Yes | Audio file |

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "speech_segments": [
      { "start": 0.5, "end": 3.2, "confidence": 0.99 },
      { "start": 5.0, "end": 10.1, "confidence": 0.97 }
    ],
    "non_speech_segments": [
      { "start": 0.0, "end": 0.5 },
      { "start": 3.2, "end": 5.0 }
    ],
    "speech_file_url": "https://storage.auto-process.com/audio/speech_abc123.wav",
    "non_speech_file_url": "https://storage.auto-process.com/audio/non_speech_abc123.wav",
    "total_speech_duration": 7.8,
    "total_non_speech_duration": 2.3
  }
}
```

---

## Translation API

### Translate Text

#### POST /translate/text

Dịch text sang tiếng Việt.

**Headers:**

```
Authorization: Bearer <token>
Content-Type: application/json
```

**Body:**

```json
{
  "text": "Hello, welcome to our channel",
  "source_language": "en",
  "target_language": "vi"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "original_text": "Hello, welcome to our channel",
    "translated_text": "Xin chào, chào mừng đến với kênh của chúng tôi",
    "source_language": "en",
    "target_language": "vi",
    "confidence": 0.95,
    "detected_language": "en",
    "translation_engine": "google"
  }
}
```

### Batch Translate

#### POST /translate/batch

Dịch nhiều đoạn text cùng lúc.

**Body:**

```json
{
  "segments": [
    {
      "id": 1,
      "text": "Hello",
      "start": 0.5,
      "end": 1.0
    },
    {
      "id": 2,
      "text": "World",
      "start": 1.5,
      "end": 2.0
    }
  ],
  "source_language": "en",
  "target_language": "vi"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "translated_segments": [
      {
        "id": 1,
        "original_text": "Hello",
        "translated_text": "Xin chào",
        "start": 0.5,
        "end": 1.0
      },
      {
        "id": 2,
        "original_text": "World",
        "translated_text": "Thế giới",
        "start": 1.5,
        "end": 2.0
      }
    ],
    "total_segments": 2,
    "processing_time": 1.2
  }
}
```

---

## TTS API

### Generate Speech

#### POST /tts/generate

Chuyển text thành giọng nói.

**Headers:**

```
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

**Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| text | string | Yes | Text to convert |
| voice | string | No | Voice type (vietnamese_female, vietnamese_male) |
| speed | float | No | Speed multiplier (0.5 - 2.0) |
| pitch | float | No | Pitch multiplier (0.5 - 2.0) |
| sample_rate | integer | No | Sample rate (16000, 22050, 44100) |

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "audio_url": "https://storage.auto-process.com/tts/output_xyz789.wav",
    "duration": 3.5,
    "sample_rate": 16000,
    "format": "wav",
    "channels": 1,
    "bit_depth": 16,
    "file_size": 112000,
    "voice_used": "vietnamese_female",
    "processing_time": 2.1
  }
}
```

### Available Voices

#### GET /tts/voices

Lấy danh sách giọng nói có sẵn.

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "voices": [
      {
        "id": "vietnamese_female",
        "name": "Giọng nữ Việt Nam",
        "language": "vi",
        "gender": "female",
        "sample_url": "https://storage.auto-process.com/samples/vietnamese_female.mp3"
      },
      {
        "id": "vietnamese_male",
        "name": "Giọng nam Việt Nam",
        "language": "vi",
        "gender": "male",
        "sample_url": "https://storage.auto-process.com/samples/vietnamese_male.mp3"
      }
    ]
  }
}
```

---

## Error Codes

### HTTP Status Codes

| Code | Description                                           |
| ---- | ----------------------------------------------------- |
| 200  | OK - Request successful                               |
| 201  | Created - Resource created                            |
| 202  | Accepted - Request accepted for processing            |
| 400  | Bad Request - Invalid input                           |
| 401  | Unauthorized - Invalid or missing token               |
| 403  | Forbidden - Insufficient permissions                  |
| 404  | Not Found - Resource not found                        |
| 409  | Conflict - Resource conflict                          |
| 422  | Unprocessable Entity - Validation error               |
| 429  | Too Many Requests - Rate limit exceeded               |
| 500  | Internal Server Error                                 |
| 502  | Bad Gateway - Upstream service error                  |
| 503  | Service Unavailable - Service temporarily unavailable |

### Application Error Codes

| Code                    | Description              | HTTP Status |
| ----------------------- | ------------------------ | ----------- |
| `VALIDATION_ERROR`      | Invalid input parameters | 400         |
| `AUTHENTICATION_FAILED` | Invalid credentials      | 401         |
| `TOKEN_EXPIRED`         | JWT token expired        | 401         |
| `PERMISSION_DENIED`     | Insufficient permissions | 403         |
| `RESOURCE_NOT_FOUND`    | Resource not found       | 404         |
| `FILE_TOO_LARGE`        | File exceeds size limit  | 400         |
| `UNSUPPORTED_FORMAT`    | Unsupported file format  | 400         |
| `PROCESSING_FAILED`     | Processing failed        | 500         |
| `TIMEOUT_ERROR`         | Request timeout          | 504         |
| `RATE_LIMIT_EXCEEDED`   | Rate limit exceeded      | 429         |

### Error Response Format

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input parameters",
    "details": {
      "field": "file",
      "reason": "File size exceeds 2GB limit"
    }
  },
  "timestamp": "2026-04-24T21:00:00Z",
  "request_id": "req_abc123"
}
```

---

## Rate Limiting

### Limits

| Tier       | Requests/minute | Requests/day | Concurrent Jobs |
| ---------- | --------------- | ------------ | --------------- |
| Free       | 10              | 100          | 1               |
| Basic      | 60              | 1000         | 3               |
| Pro        | 300             | 10000        | 10              |
| Enterprise | 1000            | Unlimited    | Unlimited       |

### Rate Limit Headers

Response includes rate limit information:

```http
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1682371200
Retry-After: 60
```

### Rate Limit Exceeded Response

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Rate limit exceeded. Please try again later.",
    "details": {
      "limit": 60,
      "reset_at": "2026-04-24T21:01:00Z",
      "retry_after": 60
    }
  },
  "timestamp": "2026-04-24T21:00:00Z"
}
```

---

## Tài liệu tham khảo

- [Đặc tả kỹ thuật](../05-technical-specs/README.md)
- [Deployment](../07-deployment/README.md)

# 05 - Đặc tả kỹ thuật

## Mục lục

1. [Đặc tả API](#đặc-tả-api)
2. [Định dạng dữ liệu](#định-dạng-dữ-liệu)
3. [Cấu hình hệ thống](#cấu-hình-hệ-thống)
4. [Performance Requirements](#performance-requirements)

---

## Đặc tả API

### Base URL

```
Production: https://api.auto-process.com/v1
Staging: https://staging-api.auto-process.com/v1
Development: http://localhost:8000/v1
```

### Authentication

Tất cả API endpoints (trừ health check) yêu cầu JWT token trong header:

```
Authorization: Bearer <jwt_token>
```

### API Endpoints

#### 1. Video Processing

##### POST /video/upload

Upload video để xử lý.

**Request:**

```http
POST /video/upload
Content-Type: multipart/form-data
Authorization: Bearer <token>

file: <video_file>
options: {
  "source_language": "auto",
  "target_language": "vi",
  "enable_ai_context": true,
  "voice": "vietnamese_female"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "job_id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "queued",
    "created_at": "2026-04-24T21:00:00Z",
    "estimated_time": 1200
  }
}
```

##### GET /video/{job_id}/status

Kiểm tra trạng thái xử lý video.

**Response:**

```json
{
  "success": true,
  "data": {
    "job_id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "processing",
    "progress": 45,
    "current_step": "translating",
    "steps_completed": ["uploaded", "extracted_audio", "vad", "asr"],
    "steps_remaining": ["translate", "ai_context", "tts", "merge"],
    "estimated_completion": "2026-04-24T21:20:00Z"
  }
}
```

##### GET /video/{job_id}/download

Download video đã xử lý.

**Response:**

- Content-Type: video/mp4
- Content-Disposition: attachment; filename="video_vietnamese.mp4"

---

#### 2. Audio Processing

##### POST /audio/speech-to-text

Chuyển giọng nói thành text.

**Request:**

```http
POST /audio/speech-to-text
Content-Type: multipart/form-data
Authorization: Bearer <token>

audio: <audio_file>
language: "zh" | "en" | "auto"
```

**Response:**

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
      }
    ],
    "duration": 120.5,
    "detected_language": "en"
  }
}
```

##### POST /audio/voice-activity-detection

Tách âm thanh giọng nói và không giọng nói.

**Request:**

```http
POST /audio/voice-activity-detection
Content-Type: multipart/form-data
Authorization: Bearer <token>

audio: <audio_file>
```

**Response:**

```json
{
  "success": true,
  "data": {
    "speech_segments": [
      { "start": 0.5, "end": 3.2 },
      { "start": 5.0, "end": 10.1 }
    ],
    "speech_file_url": "https://storage/.../speech.wav",
    "non_speech_file_url": "https://storage/.../non_speech.wav"
  }
}
```

---

#### 3. Translation

##### POST /translate/text

Dịch text sang tiếng Việt.

**Request:**

```http
POST /translate/text
Content-Type: application/json
Authorization: Bearer <token>

{
  "text": "Hello, welcome to our channel",
  "source_language": "en",
  "target_language": "vi"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "original_text": "Hello, welcome to our channel",
    "translated_text": "Xin chào, chào mừng đến với kênh của chúng tôi",
    "source_language": "en",
    "target_language": "vi",
    "confidence": 0.95
  }
}
```

##### POST /translate/batch

Dịch nhiều đoạn text cùng lúc.

**Request:**

```json
{
  "segments": [
    { "id": 1, "text": "Hello", "start": 0.5, "end": 1.0 },
    { "id": 2, "text": "World", "start": 1.5, "end": 2.0 }
  ],
  "source_language": "en",
  "target_language": "vi"
}
```

---

#### 4. AI Context

##### POST /ai-context/refine

AI hiệu chỉnh bản dịch theo ngữ cảnh.

**Request:**

```json
{
  "segments": [
    { "original": "Hello", "translated": "Xin chào" },
    { "original": "World", "translated": "Thế giới" }
  ],
  "context": {
    "video_topic": "Technology review",
    "target_audience": "General",
    "tone": "Casual"
  }
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "refined_segments": [
      { "original": "Hello", "translated": "Xin chào", "notes": "" },
      {
        "original": "World",
        "translated": "mọi người",
        "notes": "Adapted for casual tone"
      }
    ],
    "model_used": "gpt-4",
    "processing_time": 2.5
  }
}
```

---

#### 5. Text-to-Speech

##### POST /tts/generate

Chuyển text thành giọng nói.

**Request:**

```http
POST /tts/generate
Content-Type: multipart/form-data
Authorization: Bearer <token>

text: "Xin chào, chào mừng đến với kênh của chúng tôi"
voice: "vietnamese_female" | "vietnamese_male"
speed: 1.0
pitch: 1.0
```

**Response:**

```json
{
  "success": true,
  "data": {
    "audio_url": "https://storage/.../tts_output.wav",
    "duration": 3.5,
    "sample_rate": 16000,
    "format": "wav"
  }
}
```

---

#### 6. Health & Status

##### GET /health

Kiểm tra sức khỏe hệ thống.

**Response:**

```json
{
  "status": "healthy",
  "services": {
    "api": "up",
    "database": "up",
    "redis": "up",
    "storage": "up",
    "asr": "up",
    "tts": "up"
  },
  "timestamp": "2026-04-24T21:00:00Z"
}
```

---

## Định dạng dữ liệu

### Video Formats

| Format | Extension | Codec        | Max Resolution | Max File Size |
| ------ | --------- | ------------ | -------------- | ------------- |
| MP4    | .mp4      | H.264/H.265  | 4K             | 2GB           |
| AVI    | .avi      | Various      | 4K             | 2GB           |
| MKV    | .mkv      | Various      | 4K             | 2GB           |
| MOV    | .mov      | H.264/ProRes | 4K             | 2GB           |

### Audio Formats

| Format | Sample Rate           | Channels     | Bit Depth      |
| ------ | --------------------- | ------------ | -------------- |
| WAV    | 16kHz, 44.1kHz, 48kHz | Mono, Stereo | 16-bit, 24-bit |
| MP3    | 16kHz, 44.1kHz, 48kHz | Mono, Stereo | N/A            |
| AAC    | 16kHz, 44.1kHz, 48kHz | Mono, Stereo | N/A            |

### Subtitle Formats

| Format | Extension | Description                    |
| ------ | --------- | ------------------------------ |
| SRT    | .srt      | SubRip Subtitle                |
| VTT    | .vtt      | WebVTT (Web Video Text Tracks) |
| ASS    | .ass      | Advanced SubStation Alpha      |

### JSON Data Schemas

#### Segment Schema

```json
{
  "type": "object",
  "properties": {
    "id": { "type": "integer", "minimum": 1 },
    "start": { "type": "number", "minimum": 0 },
    "end": { "type": "number", "minimum": 0 },
    "text": { "type": "string", "minLength": 1 },
    "language": { "type": "string", "enum": ["zh", "en", "vi"] },
    "confidence": { "type": "number", "minimum": 0, "maximum": 1 }
  },
  "required": ["id", "start", "end", "text"]
}
```

#### Job Schema

```json
{
  "type": "object",
  "properties": {
    "job_id": { "type": "string", "format": "uuid" },
    "user_id": { "type": "string" },
    "status": {
      "type": "string",
      "enum": ["queued", "processing", "completed", "failed", "cancelled"]
    },
    "progress": { "type": "integer", "minimum": 0, "maximum": 100 },
    "created_at": { "type": "string", "format": "date-time" },
    "completed_at": {
      "type": "string",
      "format": "date-time",
      "nullable": true
    },
    "error": { "type": "string", "nullable": true }
  },
  "required": ["job_id", "status", "created_at"]
}
```

---

## Cấu hình hệ thống

### Server Configuration

```yaml
# config/server.yaml
server:
  host: "0.0.0.0"
  port: 8000
  workers: 4
  debug: false

database:
  host: "localhost"
  port: 5432
  name: "auto_process"
  user: "auto_process"
  pool_size: 20

redis:
  host: "localhost"
  port: 6379
  db: 0
  password: ""

storage:
  type: "s3" # or "local", "minio"
  bucket: "auto-process-files"
  region: "ap-southeast-1"
  access_key: "${AWS_ACCESS_KEY_ID}"
  secret_key: "${AWS_SECRET_ACCESS_KEY}"
```

### AI Model Configuration

```yaml
# config/models.yaml
asr:
  model: "whisper"
  variant: "large-v3"
  device: "cuda"
  batch_size: 16
  language: ["zh", "en"]

vad:
  model: "silero"
  threshold: 0.5
  min_speech_duration: 0.25
  min_silence_duration: 0.1

tts:
  engine: "vn_tts" # or "google", "azure"
  voice: "vietnamese_female"
  sample_rate: 16000
  speed: 1.0

translation:
  engine: "google" # or "deepl", "azure"
  source_languages: ["zh", "en"]
  target_language: "vi"

ai_context:
  model: "gpt-4"
  temperature: 0.3
  max_tokens: 2000
```

### Processing Configuration

```yaml
# config/processing.yaml
processing:
  max_retries: 3
  retry_delay: 5
  timeout: 3600 # 1 hour

  audio:
    sample_rate: 16000
    channels: 1
    bit_depth: 16

  video:
    max_resolution: "4K"
    max_file_size: "2GB"
    output_codec: "h264"
    output_bitrate: "5000k"

  batch:
    max_concurrent_jobs: 10
    max_videos_per_batch: 50
```

---

## Performance Requirements

### Response Time

| Operation                   | P50 | P95 | P99  |
| --------------------------- | --- | --- | ---- |
| Upload video (100MB)        | 2s  | 5s  | 10s  |
| Audio extraction            | 1s  | 2s  | 3s   |
| VAD processing (1min audio) | 2s  | 5s  | 10s  |
| ASR (1min audio)            | 10s | 20s | 30s  |
| Translation (1000 chars)    | 1s  | 3s  | 5s   |
| AI Context (1000 chars)     | 5s  | 10s | 15s  |
| TTS (100 chars)             | 2s  | 5s  | 10s  |
| Video merge (10min video)   | 30s | 60s | 120s |

### Throughput

| Metric                   | Target      |
| ------------------------ | ----------- |
| Concurrent users         | 1000        |
| Videos processed per day | 10,000      |
| API requests per second  | 1000        |
| Max video duration       | 120 minutes |

### Resource Usage

| Component        | CPU     | Memory | GPU               |
| ---------------- | ------- | ------ | ----------------- |
| API Server       | 2 cores | 4GB    | -                 |
| ASR Service      | 4 cores | 8GB    | 1x GPU (8GB VRAM) |
| TTS Service      | 2 cores | 4GB    | Optional GPU      |
| Video Processing | 4 cores | 8GB    | Optional GPU      |
| Database         | 2 cores | 4GB    | -                 |
| Redis            | 1 core  | 2GB    | -                 |

### Scalability

- **Horizontal scaling**: Auto-scale based on CPU/memory usage
- **Queue-based processing**: Celery workers scale with queue length
- **GPU sharing**: Multiple models on single GPU with batching
- **CDN**: Video files served via CDN for global access

---

## Tài liệu tham khảo

- [Kiến trúc hệ thống](../03-architecture/README.md)
- [Quy trình xử lý](../04-workflow/README.md)
- [API Documentation](../06-api-docs/README.md)
- [Deployment](../07-deployment/README.md)

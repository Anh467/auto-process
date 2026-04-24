# Phase 2: Audio Processing & ASR

**Mục tiêu:** Extract audio từ video và chuyển giọng nói thành text.

**Thời gian ước tính:** 3-4 tuần

**Người phụ trách:** Backend Team + AI/ML Team

---

## Tổng quan

Phase 2 tập trung vào việc xử lý audio từ video đã upload:

- Extract audio từ video file
- Voice Activity Detection (VAD) để phát hiện segments có giọng nói
- Automatic Speech Recognition (ASR) sử dụng Whisper
- Quản lý và lưu trữ transcript

---

## User Stories Chi Tiết

### US-2.1: Audio Extraction

**Là một** System  
**Tôi muốn** extract audio từ video file  
**Để** xử lý ASR

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 8

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

**Dependencies:** Phase 1 (Video Upload)

---

### US-2.2: Voice Activity Detection (VAD)

**Là một** System  
**Tôi muốn** phát hiện segments có giọng nói  
**Để** chỉ xử lý ASR trên segments cần thiết

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 8

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

**Dependencies:** US-2.1

---

### US-2.3: Automatic Speech Recognition (ASR)

**Là một** System  
**Tôi muốn** chuyển giọng nói thành text  
**Để** dịch sang tiếng Việt

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 13

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

**Dependencies:** US-2.2

---

### US-2.4: ASR Result Management

**Là một** User  
**Tôi muốn** xem transcript của video  
**Để** kiểm tra độ chính xác

**Độ ưu tiên:** Trung bình  
**Điểm ước tính:** 5

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

**Dependencies:** US-2.3

---

## Deliverables

| Deliverable               | Status | Notes |
| ------------------------- | ------ | ----- |
| Audio extraction service  | ☐      |       |
| VAD integration           | ☐      |       |
| ASR service với Whisper   | ☐      |       |
| Transcript API            | ☐      |       |
| Background job processing | ☐      |       |

---

## Technical Notes

### Audio Processing Pipeline

```
Video File → FFmpeg Extract → WAV Audio → VAD → Speech Segments → Whisper ASR → Transcript
```

### Database Schema (Phase 2)

```sql
-- Speech Segments table
CREATE TABLE speech_segments (
    id UUID PRIMARY KEY,
    processing_job_id UUID NOT NULL REFERENCES processing_jobs(id),
    start_time DECIMAL(10,3) NOT NULL,
    end_time DECIMAL(10,3) NOT NULL,
    confidence DECIMAL(3,2)
);

-- ASR Results table
CREATE TABLE asr_results (
    id UUID PRIMARY KEY,
    speech_segment_id UUID NOT NULL REFERENCES speech_segments(id),
    text TEXT NOT NULL,
    language VARCHAR(10) NOT NULL,
    confidence DECIMAL(3,2),
    created_at TIMESTAMP NOT NULL
);
```

### API Endpoints (Phase 2)

| Method | Endpoint               | Description          | Auth Required |
| ------ | ---------------------- | -------------------- | ------------- |
| GET    | /video/{id}/transcript | Get video transcript | Yes           |

### Python Services

| Service | Model            | Endpoint             |
| ------- | ---------------- | -------------------- |
| VAD     | Silero VAD       | POST /vad/detect     |
| ASR     | Whisper large-v3 | POST /asr/transcribe |

---

## Tài liệu tham khảo

- [Kiến trúc hệ thống](../../03-architecture/README.md)
- [Đặc tả kỹ thuật](../../05-technical-specs/README.md)
- [Phase 1](../phase-1/README.md)

# Phase 4: TTS & Video Merge

**Mục tiêu:** Chuyển text thành giọng nói và merge vào video gốc.

**Thời gian ước tính:** 3-4 tuần

**Người phụ trách:** Backend Team + AI/ML Team

---

## Tổng quan

Phase 4 là phase cuối cùng, hoàn thiện toàn bộ pipeline xử lý video:

- Text-to-Speech (TTS) để chuyển translated text thành giọng nói tiếng Việt
- Audio mixing để kết hợp TTS với background audio
- Video merge để replace audio trong video
- Download video đã xử lý
- Optimization toàn bộ pipeline

---

## User Stories Chi Tiết

### US-4.1: Text-to-Speech Generation

**Là một** System  
**Tôi muốn** chuyển translated text thành giọng nói  
**Để** tạo audio tiếng Việt

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 8

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

**Dependencies:** Phase 3 (Translations)

---

### US-4.2: Audio Mixing

**Là một** System  
**Tôi muốn** mix TTS audio với background audio  
**Để** giữ lại âm thanh nền gốc

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 8

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

**Dependencies:** US-4.1

---

### US-4.3: Video Merge

**Là một** System  
**Tôi muốn** replace audio trong video bằng audio đã dịch  
**Để** tạo video hoàn chỉnh

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 13

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

**Dependencies:** US-4.2

---

### US-4.4: Video Download

**Là một** User  
**Tôi muốn** download video đã xử lý  
**Để** xem offline

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 5

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

**Dependencies:** US-4.3

---

### US-4.5: Processing Pipeline Optimization

**Là một** System  
**Tôi muốn** optimize toàn bộ pipeline  
**Để** giảm thời gian xử lý

**Độ ưu tiên:** Trung bình  
**Điểm ước tính:** 8

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

**Dependencies:** US-4.4

---

## Deliverables

| Deliverable                   | Status | Notes |
| ----------------------------- | ------ | ----- |
| TTS service                   | ☐      |       |
| Audio mixing service          | ☐      |       |
| Video merge service           | ☐      |       |
| Download API                  | ☐      |       |
| Optimized processing pipeline | ☐      |       |
| Complete end-to-end flow      | ☐      |       |

---

## Technical Notes

### Video Processing Pipeline

```
Translated Text → TTS → Audio Segments → Audio Mixing → Mixed Audio → Video Merge → Final Video
```

### Database Schema (Phase 4)

```sql
-- TTS Results table
CREATE TABLE tts_results (
    id UUID PRIMARY KEY,
    translation_id UUID NOT NULL REFERENCES translations(id),
    audio_url VARCHAR(500) NOT NULL,
    duration DECIMAL(10,3) NOT NULL,
    voice VARCHAR(50) NOT NULL,
    speed DECIMAL(3,2) DEFAULT 1.0,
    pitch DECIMAL(3,2) DEFAULT 1.0,
    created_at TIMESTAMP NOT NULL
);

-- Final Videos table
CREATE TABLE final_videos (
    id UUID PRIMARY KEY,
    processing_job_id UUID NOT NULL REFERENCES processing_jobs(id),
    video_url VARCHAR(500) NOT NULL,
    duration DECIMAL(10,3) NOT NULL,
    resolution VARCHAR(20) NOT NULL,
    file_size BIGINT NOT NULL,
    download_expires_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL
);
```

### API Endpoints (Phase 4)

| Method | Endpoint             | Description    | Auth Required |
| ------ | -------------------- | -------------- | ------------- |
| GET    | /video/{id}/download | Download video | Yes           |

### AI Services

| Service | Model | Endpoint           |
| ------- | ----- | ------------------ |
| TTS     | VnTTS | POST /tts/generate |

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

- [Kiến trúc hệ thống](../../03-architecture/README.md)
- [Đặc tả kỹ thuật](../../05-technical-specs/README.md)
- [Phase 3](../phase-3/README.md)

# Phase 3: Translation & AI Context

**Mục tiêu:** Dịch transcript sang tiếng Việt và sử dụng AI để hiệu chỉnh ngữ cảnh.

**Thời gian ước tính:** 2-3 tuần

**Người phụ trách:** Backend Team + AI/ML Team

---

## Tổng quan

Phase 3 tập trung vào việc dịch transcript từ Phase 2 sang tiếng Việt và sử dụng AI để cải thiện chất lượng bản dịch:

- Text translation từ English/Chinese sang Vietnamese
- AI context refinement để bản dịch tự nhiên hơn
- Translation review API để so sánh bản dịch

---

## User Stories Chi Tiết

### US-3.1: Text Translation

**Là một** System  
**Tôi muốn** dịch transcript sang tiếng Việt  
**Để** người xem hiểu nội dung

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 8

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

**Dependencies:** Phase 2 (ASR Results)

---

### US-3.2: AI Context Refinement

**Là một** System  
**Tôi muốn** AI hiệu chỉnh bản dịch theo ngữ cảnh  
**Để** bản dịch tự nhiên và chính xác hơn

**Độ ưu tiên:** Cao  
**Điểm ước tính:** 13

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

**Dependencies:** US-3.1

---

### US-3.3: Translation Review API

**Là một** User  
**Tôi muốn** xem và so sánh bản dịch gốc và đã refine  
**Để** đánh giá chất lượng

**Độ ưu tiên:** Trung bình  
**Điểm ước tính:** 5

**Acceptance Criteria:**

- [ ] API endpoint: GET /video/{job_id}/translations
- [ ] Hiển thị original translation và refined translation
- [ ] AI notes và explanations

**Tasks:**

1. Tạo GetTranslationsQuery
2. Tạo TranslationCompareDto
3. Tạo endpoint so sánh
4. Format response

**Dependencies:** US-3.2

---

## Deliverables

| Deliverable                   | Status | Notes |
| ----------------------------- | ------ | ----- |
| Translation service           | ☐      |       |
| AI context refinement         | ☐      |       |
| Translation comparison API    | ☐      |       |
| Batch processing optimization | ☐      |       |

---

## Technical Notes

### Translation Pipeline

```
ASR Transcript → Batch Translation → AI Context Refinement → Final Translation
```

### Database Schema (Phase 3)

```sql
-- Translations table
CREATE TABLE translations (
    id UUID PRIMARY KEY,
    asr_result_id UUID NOT NULL REFERENCES asr_results(id),
    original_text TEXT NOT NULL,
    translated_text TEXT NOT NULL,
    refined_text TEXT,
    ai_notes TEXT,
    confidence DECIMAL(3,2),
    created_at TIMESTAMP NOT NULL
);
```

### API Endpoints (Phase 3)

| Method | Endpoint                 | Description            | Auth Required |
| ------ | ------------------------ | ---------------------- | ------------- |
| GET    | /video/{id}/translations | Get video translations | Yes           |

### AI Services

| Service       | Model  | Endpoint                |
| ------------- | ------ | ----------------------- |
| Translation   | Google | POST /translate/text    |
| AI Refinement | GPT-4  | POST /ai-context/refine |

---

## Tài liệu tham khảo

- [Kiến trúc hệ thống](../../03-architecture/README.md)
- [Đặc tả kỹ thuật](../../05-technical-specs/README.md)
- [Phase 2](../phase-2/README.md)

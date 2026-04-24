# 01 - Tổng quan dự án

## Mục lục

1. [Mục tiêu dự án](#mục-tiêu-dự-án)
2. [Phạm vi dự án](#phạm-vi-dự-án)
3. [Các bên liên quan](#các-bên-liên-quan)
4. [Thuật ngữ chuyên ngành](#thuật-ngữ-chuyên-ngành)

---

## Mục tiêu dự án

### Mục tiêu chính

Xây dựng hệ thống tự động xử lý video để bản địa hóa nội dung từ tiếng Trung/Anh sang tiếng Việt, bao gồm:

1. **Tách âm thanh**: Phân tách âm thanh video thành phần có giọng nói và không có giọng nói
2. **Nhận diện giọng nói (ASR)**: Chuyển đổi giọng nói Trung/Anh thành text
3. **Dịch thuật**: Dịch text từ Trung/Anh sang tiếng Việt
4. **AI Context**: Sử dụng AI để nắm bắt ngữ cảnh và cải thiện chất lượng dịch
5. **Text-to-Speech (TTS)**: Chuyển text tiếng Việt thành giọng nói tiếng Việt
6. **Ghép video**: Kết hợp âm thanh tiếng Việt và subtitle vào video gốc

### Mục tiêu kỹ thuật

- Tự động hóa 90% quy trình xử lý video
- Độ chính xác nhận diện giọng nói > 95%
- Thời gian xử lý < 2x thời lượng video
- Hỗ trợ nhiều định dạng video (MP4, AVI, MKV, MOV)
- Đồng bộ chính xác giữa âm thanh và hình ảnh

---

## Phạm vi dự án

### Trong phạm vi (In Scope)

- [x] Xử lý video có âm thanh tiếng Trung/Anh
- [x] Tách âm thanh giọng nói và âm thanh nền
- [x] Nhận diện giọng nói (Speech-to-Text)
- [x] Dịch thuật tự động sang tiếng Việt
- [x] AI hỗ trợ context và cải thiện chất lượng dịch
- [x] Text-to-Speech tiếng Việt
- [x] Tạo subtitle tiếng Việt
- [x] Ghép âm thanh và subtitle vào video
- [x] Xử lý batch (nhiều video cùng lúc)

### Ngoài phạm vi (Out of Scope)

- [ ] Nhận diện giọng nói cho ngôn ngữ khác (Nhật, Hàn, v.v.)
- [ ] Xử lý video chất lượng 8K
- [ ] Real-time streaming processing
- [ ] Mobile application
- [ ] Live translation

---

## Các bên liên quan

| Vai trò            | Trách nhiệm                     | Liên hệ       |
| ------------------ | ------------------------------- | ------------- |
| Project Manager    | Quản lý dự án, lập kế hoạch     | PM            |
| AI/ML Engineer     | Phát triển mô hình AI, ASR, TTS | AI Team       |
| Backend Developer  | Xây dựng API, xử lý video       | Backend Team  |
| Frontend Developer | Giao diện người dùng            | Frontend Team |
| DevOps Engineer    | Triển khai, CI/CD               | DevOps Team   |
| QA Engineer        | Kiểm thử chất lượng             | QA Team       |

---

## Thuật ngữ chuyên ngành

### Thuật ngữ kỹ thuật

| Thuật ngữ                        | Viết tắt | Mô tả                                                               |
| -------------------------------- | -------- | ------------------------------------------------------------------- |
| **Automatic Speech Recognition** | ASR      | Nhận diện giọng nói tự động, chuyển giọng nói thành text            |
| **Text-to-Speech**               | TTS      | Chuyển văn bản thành giọng nói                                      |
| **Voice Activity Detection**     | VAD      | Phát hiện hoạt động giọng nói, tách phần có giọng và không có giọng |
| **Natural Language Processing**  | NLP      | Xử lý ngôn ngữ tự nhiên                                             |
| **Machine Translation**          | MT       | Dịch máy tự động                                                    |
| **Neural Machine Translation**   | NMT      | Dịch máy sử dụng mạng neural                                        |
| **Speaker Diarization**          | -        | Phân biệt người nói trong audio                                     |
| **Audio Separation**             | -        | Tách các nguồn âm thanh khác nhau                                   |

### Thuật ngữ dự án

| Thuật ngữ            | Mô tả                                        |
| -------------------- | -------------------------------------------- |
| **Source Audio**     | Âm thanh gốc có giọng nói Trung/Anh          |
| **Non-speech Audio** | Âm thanh nền, nhạc, hiệu ứng âm thanh        |
| **Target Audio**     | Âm thanh giọng nói tiếng Việt đã xử lý       |
| **Context AI**       | AI mô-đun nắm bắt ngữ cảnh để cải thiện dịch |
| **Time-aligned TTS** | TTS đồng bộ thời gian với video gốc          |

---

## Tài liệu tham khảo

- [Yêu cầu hệ thống](../02-requirements/README.md)
- [Kiến trúc hệ thống](../03-architecture/README.md)
- [Quy trình xử lý](../04-workflow/README.md)

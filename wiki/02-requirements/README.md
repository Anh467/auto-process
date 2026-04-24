# 02 - Yêu cầu hệ thống

## Mục lục

1. [Yêu cầu chức năng](#yêu-cầu-chức-năng)
2. [Yêu cầu phi chức năng](#yêu-cầu-phi-chức-năng)
3. [Yêu cầu kỹ thuật](#yêu-cầu-kỹ-thuật)
4. [Use Cases](#use-cases)

---

## Yêu cầu chức năng

### FC-01: Upload và xử lý video

| ID                     | FC-01                                                                                 |
| ---------------------- | ------------------------------------------------------------------------------------- |
| **Mô tả**              | Hệ thống cho phép người dùng upload video và tự động xử lý                            |
| **Đầu vào**            | File video (MP4, AVI, MKV, MOV)                                                       |
| **Đầu ra**             | Video đã được localize sang tiếng Việt                                                |
| **Tiêu chí chấp nhận** | - Hỗ trợ tối thiểu 4 định dạng video<br>- Tối đa 2GB/file<br>- Hiển thị tiến độ xử lý |

### FC-02: Tách âm thanh giọng nói

| ID                     | FC-02                                                                                              |
| ---------------------- | -------------------------------------------------------------------------------------------------- |
| **Mô tả**              | Tách âm thanh video thành 2 track: có giọng nói và không có giọng nói                              |
| **Đầu vào**            | Âm thanh từ video                                                                                  |
| **Đầu ra**             | 2 audio files: speech.wav, non-speech.wav                                                          |
| **Tiêu chí chấp nhận** | - Độ chính xác tách > 95%<br>- Giữ nguyên chất lượng âm thanh nền<br>- Đồng bộ thời gian chính xác |

### FC-03: Nhận diện giọng nói (ASR)

| ID                     | FC-03                                                                                                      |
| ---------------------- | ---------------------------------------------------------------------------------------------------------- |
| **Mô tả**              | Chuyển đổi giọng nói Trung/Anh thành text với timestamp                                                    |
| **Đầu vào**            | Audio file có giọng nói                                                                                    |
| **Đầu ra**             | Text với timestamp: `[{start: 0.5, end: 3.2, text: "Hello world"}]`                                        |
| **Tiêu chí chấp nhận** | - Độ chính xác > 95%<br>- Hỗ trợ tiếng Trung (phổ thông) và tiếng Anh<br>- Có timestamp chính xác đến 0.1s |

### FC-04: Dịch thuật sang tiếng Việt

| ID                     | FC-04                                                                                           |
| ---------------------- | ----------------------------------------------------------------------------------------------- |
| **Mô tả**              | Dịch text từ Trung/Anh sang tiếng Việt                                                          |
| **Đầu vào**            | Text Trung/Anh với timestamp                                                                    |
| **Đầu ra**             | Text tiếng Việt với timestamp                                                                   |
| **Tiêu chí chấp nhận** | - Dịch tự nhiên, dễ hiểu<br>- Giữ nguyên ý nghĩa gốc<br>- Độ dài text phù hợp với thời gian nói |

### FC-05: AI Context

| ID                     | FC-05                                                                                       |
| ---------------------- | ------------------------------------------------------------------------------------------- |
| **Mô tả**              | AI phân tích ngữ cảnh để cải thiện chất lượng dịch                                          |
| **Đầu vào**            | Text đã dịch, nội dung video                                                                |
| **Đầu ra**             | Text đã được AI hiệu chỉnh                                                                  |
| **Tiêu chí chấp nhận** | - Hiểu ngữ cảnh tổng thể<br>- Hiệu chỉnh thuật ngữ chuyên ngành<br>- Đảm bảo tính nhất quán |

### FC-06: Text-to-Speech (TTS)

| ID                     | FC-06                                                                             |
| ---------------------- | --------------------------------------------------------------------------------- |
| **Mô tả**              | Chuyển text tiếng Việt thành giọng nói đồng bộ thời gian                          |
| **Đầu vào**            | Text tiếng Việt với timestamp                                                     |
| **Đầu ra**             | Audio giọng nói tiếng Việt                                                        |
| **Tiêu chí chấp nhận** | - Giọng nói tự nhiên<br>- Đồng bộ thời gian với video gốc<br>- Tốc độ nói phù hợp |

### FC-07: Tạo subtitle

| ID                     | FC-07                                                                   |
| ---------------------- | ----------------------------------------------------------------------- |
| **Mô tả**              | Tạo subtitle tiếng Việt từ text đã dịch                                 |
| **Đầu vào**            | Text tiếng Việt với timestamp                                           |
| **Đầu ra**             | File SRT/VTT                                                            |
| **Tiêu chí chấp nhận** | - Định dạng chuẩn SRT/VTT<br>- Thời gian chính xác<br>- Font chữ dễ đọc |

### FC-08: Ghép video

| ID                     | FC-08                                                                                  |
| ---------------------- | -------------------------------------------------------------------------------------- |
| **Mô tả**              | Ghép âm thanh tiếng Việt và subtitle vào video                                         |
| **Đầu vào**            | Video gốc, audio tiếng Việt, subtitle                                                  |
| **Đầu ra**             | Video hoàn chỉnh với âm thanh và subtitle tiếng Việt                                   |
| **Tiêu chí chấp nhận** | - Âm thanh đồng bộ<br>- Subtitle hiển thị đúng vị trí<br>- Chất lượng video không giảm |

---

## Yêu cầu phi chức năng

### NFC-01: Hiệu năng

| ID          | NFC-01                                                                  |
| ----------- | ----------------------------------------------------------------------- |
| **Mô tả**   | Thời gian xử lý video                                                   |
| **Yêu cầu** | Thời gian xử lý ≤ 2x thời lượng video (video 10 phút → xử lý ≤ 20 phút) |

### NFC-02: Khả năng mở rộng

| ID          | NFC-02                                         |
| ----------- | ---------------------------------------------- |
| **Mô tả**   | Hệ thống có thể xử lý nhiều video đồng thời    |
| **Yêu cầu** | Hỗ trợ xử lý batch tối thiểu 10 video cùng lúc |

### NFC-03: Độ tin cậy

| ID          | NFC-03                              |
| ----------- | ----------------------------------- |
| **Mô tả**   | Hệ thống hoạt động ổn định          |
| **Yêu cầu** | Uptime ≥ 99%, tự động retry khi lỗi |

### NFC-04: Bảo mật

| ID          | NFC-04                                                                         |
| ----------- | ------------------------------------------------------------------------------ |
| **Mô tả**   | Bảo vệ dữ liệu người dùng                                                      |
| **Yêu cầu** | - Mã hóa dữ liệu<br>- Xóa file sau khi xử lý xong<br>- Authentication bắt buộc |

### NFC-05: Khả năng sử dụng

| ID          | NFC-05                                                                                |
| ----------- | ------------------------------------------------------------------------------------- |
| **Mô tả**   | Giao diện dễ sử dụng                                                                  |
| **Yêu cầu** | - Người dùng không cần đào tạo<br>- Hỗ trợ kéo thả file<br>- Hiển thị tiến độ rõ ràng |

---

## Yêu cầu kỹ thuật

### TC-01: Ngôn ngữ và Framework

| Component        | Công nghệ            |
| ---------------- | -------------------- |
| Backend          | Python 3.9+, FastAPI |
| AI/ML            | PyTorch, TensorFlow  |
| Video Processing | FFmpeg, MoviePy      |
| Database         | PostgreSQL, Redis    |
| Message Queue    | RabbitMQ/Celery      |

### TC-02: API Standards

- RESTful API
- JSON request/response
- JWT authentication
- Rate limiting: 100 requests/minute

### TC-03: Video Requirements

| Thuộc tính   | Yêu cầu            |
| ------------ | ------------------ |
| Định dạng    | MP4, AVI, MKV, MOV |
| Độ phân giải | Tối đa 4K          |
| Thời lượng   | Tối đa 120 phút    |
| Kích thước   | Tối đa 2GB         |

### TC-04: Audio Requirements

| Thuộc tính  | Yêu cầu               |
| ----------- | --------------------- |
| Sample rate | 16kHz, 44.1kHz, 48kHz |
| Channels    | Mono, Stereo          |
| Format      | WAV, MP3, AAC         |

---

## Use Cases

### UC-01: Xử lý video đơn

```
Actor: Người dùng
Mục tiêu: Upload và xử lý 1 video

Flow chính:
1. Người dùng login
2. Upload video
3. Hệ thống xử lý tự động
4. Người dùng download kết quả

Flow phụ:
- 3a. Hiển thị tiến độ xử lý
- 3b. Thông báo khi hoàn thành
```

### UC-02: Xử lý batch

```
Actor: Người dùng
Mục tiêu: Xử lý nhiều video cùng lúc

Flow chính:
1. Người dùng login
2. Upload nhiều video
3. Hệ thống xử lý song song
4. Người dùng download tất cả kết quả
```

### UC-03: Hiệu chỉnh dịch

```
Actor: Người dùng, AI
Mục tiêu: Cải thiện chất lượng dịch

Flow chính:
1. Hệ thống dịch tự động
2. AI phân tích context
3. Hiệu chỉnh thuật ngữ
4. Người dùng xem trước và approve
```

---

## Tài liệu tham khảo

- [Tổng quan dự án](../01-overview/README.md)
- [Kiến trúc hệ thống](../03-architecture/README.md)
- [Đặc tả kỹ thuật](../05-technical-specs/README.md)

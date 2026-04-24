# 04 - Quy trình xử lý

## Mục lục

1. [Lưu đồ xử lý chi tiết](#lưu-đồ-xử-lý-chi-tiết)
2. [Các bước thực hiện](#các-bước-thực-hiện)
3. [Xử lý lỗi](#xử-lý-lỗi)
4. [Tối ưu hóa](#tối-ưu-hóa)

---

## Lưu đồ xử lý chi tiết

```
                                    ┌─────────────────┐
                                    │    START        │
                                    └────────┬────────┘
                                             │
                                    ┌────────▼────────┐
                                    │  1. Upload      │
                                    │     Video       │
                                    └────────┬────────┘
                                             │
                                    ┌────────▼────────┐
                                    │  2. Extract     │
                                    │    Audio from   │
                                    │     Video       │
                                    └────────┬────────┘
                                             │
                                    ┌────────▼────────┐
                                    │  3. Voice       │
                                    │  Activity       │
                                    │  Detection      │
                                    │  (VAD)          │
                                    └────────┬────────┘
                                             │
                             ┌───────────────┴───────────────┐
                             │                               │
                    ┌────────▼────────┐            ┌────────▼────────┐
                    │  Speech Audio   │            │ Non-Speech      │
                    │   (Giọng nói)   │            │ Audio (Nền)     │
                    └────────┬────────┘            └────────┬────────┘
                             │                               │
                    ┌────────▼────────┐                      │
                    │  4. ASR         │                      │
                    │  (Trung/Anh →   │                      │
                    │   Text + Time)  │                      │
                    └────────┬────────┘                      │
                             │                               │
                    ┌────────▼────────┐                      │
                    │  5. Translate   │                      │
                    │  (→ Tiếng Việt) │                      │
                    └────────┬────────┘                      │
                             │                               │
                    ┌────────▼────────┐                      │
                    │  6. AI Context  │                      │
                    │  (Refine dịch)  │                      │
                    └────────┬────────┘                      │
                             │                               │
                    ┌────────▼────────┐                      │
                    │  7. TTS         │                      │
                    │  (Text → Speech │                      │
                    │   Tiếng Việt)   │                      │
                    └────────┬────────┘                      │
                             │                               │
                             │                               │
                    ┌────────▼───────────────────────────────▼────────┐
                    │           8. Merge Audio                        │
                    │  - Speech (Tiếng Việt) + Non-Speech (Nền)       │
                    │  - Sync timing với video gốc                    │
                    └────────┬────────────────────────────────────────┘
                             │
                    ┌────────▼────────┐
                    │  9. Generate    │
                    │   Subtitle      │
                    │   (SRT/VTT)     │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │  10. Merge      │
                    │   Video + Audio │
                    │   + Subtitle    │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │     OUTPUT      │
                    │   Video (Vi)    │
                    └────────┬────────┘
                             │
                                    ┌─────────────────┐
                                    │      END        │
                                    └─────────────────┘
```

---

## Các bước thực hiện

### Bước 1: Upload Video

**Mô tả**: Người dùng upload video cần xử lý

**Đầu vào**:

- File video (MP4, AVI, MKV, MOV)
- Thông tin người dùng
- Tùy chọn xử lý (nếu có)

**Xử lý**:

```python
# Pseudocode
def upload_video(file, user_id):
    # Validate file type and size
    validate_file(file)

    # Generate unique ID
    video_id = generate_uuid()

    # Store to object storage
    storage_path = save_to_storage(file, video_id)

    # Create processing job
    job = create_job(video_id, user_id, storage_path)

    # Queue for processing
    queue_processing(job)

    return job
```

**Đầu ra**: Job ID, status = "queued"

---

### Bước 2: Extract Audio

**Mô tả**: Tách âm thanh từ video gốc

**Xử lý**:

```bash
# FFmpeg command
ffmpeg -i input_video.mp4 -vn -acodec pcm_s16le -ar 16000 -ac 1 audio.wav
```

**Tham số**:

- `-vn`: No video
- `-acodec pcm_s16le`: PCM 16-bit Little Endian
- `-ar 16000`: Sample rate 16kHz
- `-ac 1`: Mono channel

**Đầu ra**: `audio.wav` (16kHz, mono, 16-bit)

---

### Bước 3: Voice Activity Detection (VAD)

**Mô tả**: Tách audio thành 2 phần: có giọng nói và không có giọng nói

**Xử lý**:

```python
# Pseudocode
def voice_activity_detection(audio_path):
    # Load audio
    audio, sr = librosa.load(audio_path, sr=16000)

    # Apply VAD model
    vad_model = load_vad_model()
    segments = vad_model.detect(audio)

    # Separate audio
    speech_audio = extract_segments(audio, segments, include_speech=True)
    non_speech_audio = extract_segments(audio, segments, include_speech=False)

    # Save to files
    save_audio(speech_audio, "speech.wav")
    save_audio(non_speech_audio, "non_speech.wav")

    # Return segments with timestamps
    return segments
```

**Mô hình VAD sử dụng**:

- Silero VAD
- WebRTC VAD
- PyAnnote

**Đầu ra**:

- `speech.wav`: Chỉ chứa giọng nói
- `non_speech.wav`: Chỉ chứa âm thanh nền
- `segments.json`: Timestamps của các đoạn giọng nói

---

### Bước 4: ASR (Automatic Speech Recognition)

**Mô tả**: Chuyển giọng nói Trung/Anh thành text với timestamp

**Xử lý**:

```python
# Pseudocode
def speech_to_text(audio_path, language="auto"):
    # Load Whisper model
    model = whisper.load_model("large-v3")

    # Transcribe with options
    result = model.transcribe(
        audio_path,
        language=language,  # "zh" for Chinese, "en" for English
        task="translate",   # Translate to English if needed
        word_timestamps=True
    )

    # Format output
    segments = format_segments(result["segments"])

    return segments
```

**Định dạng output**:

```json
[
  {
    "id": 1,
    "start": 0.5,
    "end": 3.2,
    "text": "Hello, welcome to our channel",
    "language": "en"
  },
  {
    "id": 2,
    "start": 3.5,
    "end": 6.8,
    "text": "今天我们要介绍一个新产品",
    "language": "zh"
  }
]
```

---

### Bước 5: Translate (Dịch thuật)

**Mô tả**: Dịch text từ Trung/Anh sang tiếng Việt

**Xử lý**:

```python
# Pseudocode
def translate_text(segments, target_lang="vi"):
    translated_segments = []

    for segment in segments:
        # Detect source language
        src_lang = detect_language(segment["text"])

        # Translate
        translated_text = translate(
            segment["text"],
            source=src_lang,
            target=target_lang
        )

        # Keep timing
        translated_segments.append({
            "id": segment["id"],
            "start": segment["start"],
            "end": segment["end"],
            "original_text": segment["text"],
            "translated_text": translated_text,
            "src_lang": src_lang,
            "target_lang": target_lang
        })

    return translated_segments
```

**Dịch vụ dịch**:

- Google Translate API
- DeepL API
- Azure Translator
- NMT model tự train

---

### Bước 6: AI Context (Hiệu chỉnh ngữ cảnh)

**Mô tả**: AI phân tích ngữ cảnh để cải thiện chất lượng bản dịch

**Xử lý**:

```python
# Pseudocode
def ai_context_refinement(translated_segments, video_context=None):
    # Prepare context
    full_text = " ".join([s["translated_text"] for s in translated_segments])

    # Build prompt for AI
    prompt = f"""
    Video context: {video_context}

    Original text and translations:
    {json.dumps(translated_segments, ensure_ascii=False)}

    Please review and refine the Vietnamese translations:
    1. Ensure natural Vietnamese phrasing
    2. Maintain consistency in terminology
    3. Adapt idioms and cultural references appropriately
    4. Keep timing constraints in mind (text should fit the speaking duration)
    """

    # Call AI API
    refined = call_ai_api(prompt)

    # Parse and update segments
    refined_segments = parse_ai_response(refined, translated_segments)

    return refined_segments
```

**AI Models**:

- GPT-4
- Claude 3
- Gemini Pro

---

### Bước 7: TTS (Text-to-Speech)

**Mô tả**: Chuyển text tiếng Việt thành giọng nói, đồng bộ thời gian

**Xử lý**:

```python
# Pseudocode
def text_to_speech(segments, voice="vietnamese_female"):
    audio_segments = []

    for segment in segments:
        duration = segment["end"] - segment["start"]
        text = segment["translated_text"]

        # Generate speech
        speech = generate_speech(
            text=text,
            voice=voice,
            speed=calculate_speed(text, duration),
            sample_rate=16000
        )

        # Adjust to match duration
        adjusted_speech = time_stretch(speech, duration)

        audio_segments.append({
            "audio": adjusted_speech,
            "start": segment["start"],
            "end": segment["end"]
        })

    # Merge all speech segments
    full_speech = merge_segments(audio_segments)

    return full_speech
```

**TTS Engines**:

- VnTTS (Vietnamese TTS)
- Google Cloud TTS
- Azure Cognitive Services
- FPT AI

---

### Bước 8: Merge Audio

**Mô tả**: Kết hợp âm thanh giọng nói tiếng Việt với âm thanh nền

**Xử lý**:

```python
# Pseudocode
def merge_audio(speech_audio, non_speech_audio, segments):
    # Create output audio
    output = np.zeros(max_duration * sample_rate)

    # Add non-speech audio (background)
    output += non_speech_audio * background_volume

    # Add speech audio at correct timestamps
    for seg in segments:
        start_sample = int(seg["start"] * sample_rate)
        speech_segment = extract_speech(speech_audio, seg)
        output[start_sample:start_sample + len(speech_segment)] += speech_segment

    # Normalize
    output = normalize(output)

    return output
```

---

### Bước 9: Generate Subtitle

**Mô tả**: Tạo file subtitle từ text đã dịch

**Xử lý**:

```python
# Pseudocode
def generate_subtitle(segments, format="srt"):
    if format == "srt":
        return generate_srt(segments)
    elif format == "vtt":
        return generate_vtt(segments)

def generate_srt(segments):
    srt_content = ""
    for i, seg in enumerate(segments, 1):
        start_time = format_time(seg["start"])
        end_time = format_time(seg["end"])
        text = seg["translated_text"]
        srt_content += f"{i}\n{start_time} --> {end_time}\n{text}\n\n"
    return srt_content
```

---

### Bước 10: Merge Video

**Mô tả**: Ghép video gốc + âm thanh mới + subtitle

**Xử lý**:

```bash
# FFmpeg command
ffmpeg -i video_original.mp4 \
       -i audio_vietnamese.wav \
       -i subtitle.vtt \
       -c:v copy \
       -c:a aac -b:a 192k \
       -c:s mov_text \
       -map 0:v:0 \
       -map 1:a:0 \
       -map 2:s:0 \
       output_vietnamese.mp4
```

---

## Xử lý lỗi

### Error Handling Strategy

```python
# Pseudocode
class ProcessingPipeline:
    def __init__(self):
        self.max_retries = 3
        self.retry_delay = 5  # seconds

    def process(self, job):
        try:
            for step in self.steps:
                success = False
                for attempt in range(self.max_retries):
                    try:
                        step.execute(job)
                        success = True
                        break
                    except RecoverableError as e:
                        log_warning(f"Attempt {attempt+1} failed: {e}")
                        sleep(self.retry_delay * (attempt + 1))  # Exponential backoff
                    except FatalError as e:
                        log_error(f"Fatal error in step {step.name}: {e}")
                        raise

                if not success:
                    raise ProcessingFailedError(f"Step {step.name} failed after {self.max_retries} attempts")

        except Exception as e:
            self.handle_failure(job, e)
            raise
```

### Error Types

| Error Type         | Description                  | Handling                          |
| ------------------ | ---------------------------- | --------------------------------- |
| `FileNotFound`     | File không tồn tại           | Retry với path khác, báo lỗi user |
| `InvalidFormat`    | Định dạng không hỗ trợ       | Báo lỗi user, yêu cầu upload lại  |
| `ASRFailure`       | Nhận diện giọng nói thất bại | Retry với model khác              |
| `TranslationError` | Dịch thất bại                | Fallback sang dịch vụ khác        |
| `TTSError`         | TTS thất bại                 | Sử dụng giọng nói dự phòng        |
| `OutOfMemory`      | Hết bộ nhớ                   | Giảm chất lượng, chia nhỏ xử lý   |
| `TimeoutError`     | Timeout                      | Tăng timeout, retry               |

---

## Tối ưu hóa

### Performance Optimizations

1. **Parallel Processing**
   - Xử lý nhiều video cùng lúc
   - Pipeline các bước độc lập

2. **Caching**
   - Cache kết quả ASR cho audio giống nhau
   - Cache translation cho text lặp lại

3. **Batch Processing**
   - Gom nhiều request thành batch
   - Giảm overhead của API calls

4. **Resource Management**
   - Auto-scaling dựa trên queue length
   - GPU sharing cho AI models

### Speed Optimizations

| Optimization              | Impact      | Implementation                      |
| ------------------------- | ----------- | ----------------------------------- |
| Use smaller ASR model     | 2x faster   | Whisper base/small instead of large |
| Parallel audio processing | 1.5x faster | Process segments in parallel        |
| GPU acceleration          | 5x faster   | Use CUDA for AI models              |
| Audio chunking            | 1.3x faster | Process audio in chunks             |
| Streaming TTS             | 1.2x faster | Stream TTS output                   |

---

## Tài liệu tham khảo

- [Kiến trúc hệ thống](../03-architecture/README.md)
- [Đặc tả kỹ thuật](../05-technical-specs/README.md)
- [API Documentation](../06-api-docs/README.md)

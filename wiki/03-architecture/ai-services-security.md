# AI Services Security Architecture

## Giới thiệu

Tài liệu này mô tả kiến trúc bảo mật cho các AI/ML Services trong hệ thống Auto Process, đảm bảo chỉ có .NET Backend API mới có thể gọi được các AI services, đồng thời cho phép các AI services giao tiếp nội bộ với nhau.

## Mục lục

1. [Tổng quan kiến trúc](#tổng-quan-kiến-trúc)
2. [Security Layers](#security-layers)
3. [Network Isolation](#network-isolation)
4. [Authentication & Authorization](#authentication--authorization)
5. [Service-to-Service Communication](#service-to-service-communication)
6. [Implementation Guide](#implementation-guide)
7. [Security Checklist](#security-checklist)

---

## Tổng quan kiến trúc

### Luồng giao tiếp

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           Public Internet                                │
└─────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         API Gateway (YARP/Ocelot)                        │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │  • JWT Authentication  • Rate Limiting  • Request Validation        ││
│  └─────────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      .NET Backend API (Port 5000)                        │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │  • User Authentication  • Authorization  • Business Logic           ││
│  │  • Orchestrates AI Services  • Manages Jobs  • Handles Responses    ││
│  └─────────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────────┘
                                     │ Internal Network Only
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    Internal AI Services Network                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │    ASR      │◄►│     VAD     │◄►│    TTS      │◄►│    AI       │    │
│  │  (Port 8001)│  │  (Port 8002)│  │  (Port 8003)│  │ Context     │    │
│  │             │  │             │  │             │  │ (Port 8004) │    │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │
│                                                                          │
│  🔒 Internal Communication: mTLS + Internal API Keys                    │
│  🚫 No direct external access - Only .NET API can reach these ports    │
└─────────────────────────────────────────────────────────────────────────┘
```

### Nguyên tắc thiết kế

1. **Chỉ .NET API mới có thể gọi AI services** - Tất cả các AI services chỉ lắng nghe trên internal network
2. **AI services có thể gọi lẫn nhau** - Thông qua signed requests với internal API key
3. **Defense in Depth** - Nhiều lớp bảo mật từ network đến application layer
4. **Zero Trust** - Mọi request đều được xác thực và ủy quyền

---

## Security Layers

### Layer 1: Network Isolation

Sử dụng Docker networks để cô lập các AI services khỏi public internet:

```yaml
# docker-compose.yml
version: "3.8"

services:
  # Public-facing .NET API
  dotnet-api:
    image: auto-process/api
    ports:
      - "5000:5000" # Only public port
    networks:
      - backend-network
      - ai-network
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - JWT_SECRET=${JWT_SECRET}
      - INTERNAL_API_KEY=${INTERNAL_API_KEY}

  # AI Services (Internal Only - No published ports)
  asr-service:
    image: auto-process/asr
    expose:
      - "8001" # Internal only
    networks:
      - ai-network
    environment:
      - INTERNAL_API_KEY=${INTERNAL_API_KEY}
      - SERVICE_NAME=asr-service

  vad-service:
    image: auto-process/vad
    expose:
      - "8002" # Internal only
    networks:
      - ai-network
    environment:
      - INTERNAL_API_KEY=${INTERNAL_API_KEY}
      - SERVICE_NAME=vad-service

  tts-service:
    image: auto-process/tts
    expose:
      - "8003" # Internal only
    networks:
      - ai-network
    environment:
      - INTERNAL_API_KEY=${INTERNAL_API_KEY}
      - SERVICE_NAME=tts-service

  ai-context-service:
    image: auto-process/ai-context
    expose:
      - "8004" # Internal only
    networks:
      - ai-network
    environment:
      - INTERNAL_API_KEY=${INTERNAL_API_KEY}
      - SERVICE_NAME=ai-context-service

networks:
  backend-network:
    driver: bridge
  ai-network:
    driver: bridge
    internal: true # No external access
```

### Layer 2: API Key Authentication

Tất cả các AI services yêu cầu `X-Internal-API-Key` header trong mọi request:

```python
# Python AI Service - Authentication Middleware
import os
import hmac
from fastapi import FastAPI, Request, HTTPException

app = FastAPI()

INTERNAL_API_KEY = os.getenv("INTERNAL_API_KEY")

@app.middleware("http")
async def authentication_middleware(request: Request, call_next):
    # Skip auth for health checks
    if request.url.path in ["/health", "/ready"]:
        return await call_next(request)

    # Validate internal API key
    api_key = request.headers.get("X-Internal-API-Key")
    if not api_key:
        raise HTTPException(status_code=401, detail="Missing internal API key")

    if not hmac.compare_digest(api_key, INTERNAL_API_KEY):
        raise HTTPException(status_code=403, detail="Invalid internal API key")

    return await call_next(request)
```

### Layer 3: Request Signing

Các AI services sử dụng HMAC signatures khi gọi lẫn nhau:

```python
# Internal service communication with signed requests
import hmac
import hashlib
import time
import json
import httpx
from typing import Dict, Any

class InternalServiceClient:
    def __init__(self, service_name: str, api_key: str):
        self.service_name = service_name
        self.api_key = api_key
        self.client = httpx.AsyncClient()

    def _generate_signature(self, method: str, path: str, body: Dict) -> str:
        """Generate HMAC signature for request"""
        timestamp = str(int(time.time()))
        body_str = json.dumps(body, sort_keys=True) if body else ""

        message = f"{method}:{path}:{timestamp}:{body_str}"
        signature = hmac.new(
            self.api_key.encode(),
            message.encode(),
            hashlib.sha256
        ).hexdigest()

        return f"t={timestamp},v1={signature}"

    async def call_service(
        self,
        target_service: str,
        port: int,
        method: str,
        path: str,
        body: Dict[str, Any]
    ) -> Dict[str, Any]:
        """Call another internal service with authentication"""
        url = f"http://{target_service}:{port}{path}"
        signature = self._generate_signature(method, path, body)

        headers = {
            "X-Internal-API-Key": self.api_key,
            "X-Service-Signature": signature,
            "X-Caller-Service": self.service_name,
            "Content-Type": "application/json"
        }

        response = await self.client.request(
            method=method,
            url=url,
            json=body,
            headers=headers,
            timeout=300.0  # 5 minutes for AI operations
        )

        response.raise_for_status()
        return response.json()
```

### Layer 4: .NET API Gateway

.NET API đóng vai trò là orchestrator và gatekeeper:

```csharp
// Infrastructure/Services/PythonAIService.cs
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoProcess.Infrastructure.Services
{
    public interface IPythonAIService
    {
        Task<TranscriptionResult> TranscribeAsync(Guid jobId, string audioPath, CancellationToken ct = default);
        Task<TranslationResult> TranslateAndRefineAsync(Guid jobId, TranscriptionResult transcription, CancellationToken ct = default);
        Task<TTSResult> GenerateSpeechAsync(Guid jobId, string text, string voice, CancellationToken ct = default);
    }

    public class PythonAIService : IPythonAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _internalApiKey;
        private readonly ILogger<PythonAIService> _logger;

        // Service endpoints (internal network)
        private readonly string _asrServiceUrl;
        private readonly string _vadServiceUrl;
        private readonly string _ttsServiceUrl;
        private readonly string _aiContextServiceUrl;

        public PythonAIService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PythonAIService> logger)
        {
            _httpClient = httpClient;
            _internalApiKey = configuration["InternalApiKey"];
            _logger = logger;

            _asrServiceUrl = configuration["Services:ASR:Url"];
            _vadServiceUrl = configuration["Services:VAD:Url"];
            _ttsServiceUrl = configuration["Services:TTS:Url"];
            _aiContextServiceUrl = configuration["Services:AIContext:Url"];
        }

        private async Task<T> CallInternalServiceAsync<T>(
            string serviceUrl,
            string endpoint,
            object requestData,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid().ToString();

            var request = new HttpRequestMessage(HttpMethod.Post, $"{serviceUrl}/{endpoint}")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json")
            };

            // Add authentication headers
            request.Headers.Add("X-Internal-API-Key", _internalApiKey);
            request.Headers.Add("X-Request-ID", requestId);
            request.Headers.Add("X-Caller", "dotnet-api");

            _logger.LogInformation("Calling {Service}/{Endpoint} with RequestId={RequestId}",
                serviceUrl, endpoint, requestId);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI Service error: {Error}", error);
                throw new AIServiceException($"Failed to call {endpoint}: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent);
        }

        public async Task<TranscriptionResult> TranscribeAsync(
            Guid jobId,
            string audioPath,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Call VAD to detect speech segments
            var vadResult = await CallInternalServiceAsync<VadResult>(
                _vadServiceUrl,
                "api/v1/detect",
                new { audio_path = audioPath },
                cancellationToken);

            // Step 2: Call ASR to transcribe speech segments
            var asrResult = await CallInternalServiceAsync<TranscriptionResult>(
                _asrServiceUrl,
                "api/v1/transcribe",
                new {
                    audio_path = audioPath,
                    segments = vadResult.Segments,
                    languages = new[] { "zh", "en" }
                },
                cancellationToken);

            return asrResult;
        }

        public async Task<TranslationResult> TranslateAndRefineAsync(
            Guid jobId,
            TranscriptionResult transcription,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Translate using translation service
            var translation = await CallInternalServiceAsync<TranslationResult>(
                _translationServiceUrl,
                "api/v1/translate",
                new {
                    segments = transcription.Segments,
                    target_language = "vi"
                },
                cancellationToken);

            // Step 2: Refine with AI context
            var refined = await CallInternalServiceAsync<RefinementResult>(
                _aiContextServiceUrl,
                "api/v1/refine",
                new {
                    translations = translation.Translations,
                    context = new {
                        video_topic = "education",
                        target_audience = "general",
                        tone = "casual"
                    }
                },
                cancellationToken);

            return refined.TranslationResult;
        }

        public async Task<TTSResult> GenerateSpeechAsync(
            Guid jobId,
            string text,
            string voice,
            CancellationToken cancellationToken = default)
        {
            return await CallInternalServiceAsync<TTSResult>(
                _ttsServiceUrl,
                "api/v1/generate",
                new {
                    text = text,
                    voice = voice,
                    sample_rate = 16000
                },
                cancellationToken);
        }
    }
}
```

---

## Network Isolation

### Docker Network Configuration

```yaml
# docker-compose.yml - Network configuration
networks:
  public-network:
    driver: bridge
    ipam:
      config:
        - subnet: 172.20.0.0/16

  backend-network:
    driver: bridge
    internal: false # Can reach internet for external APIs
    ipam:
      config:
        - subnet: 172.21.0.0/16

  ai-network:
    driver: bridge
    internal: true # No external access - completely isolated
    ipam:
      config:
        - subnet: 172.22.0.0/16
```

### Firewall Rules (nếu dùng VM/Cloud)

```bash
# Chỉ cho phép .NET API nhận traffic từ public
sudo ufw allow 5000/tcp

# Chỉ cho phép .NET API kết nối đến AI services
sudo ufw allow from 172.21.0.0/16 to 172.22.0.0/16 proto tcp port 8001:8004

# Chặn tất cả traffic khác đến AI network
sudo ufw deny from any to 172.22.0.0/16
```

---

## Authentication & Authorization

### Internal API Key Generation

```bash
# Generate a secure random key (run once and store in secrets manager)
openssl rand -hex 32
# Output: your_64_character_hex_key_here
```

### Environment Configuration

```bash
# .env file (never commit to version control)
# Internal API Key for service-to-service authentication
INTERNAL_API_KEY=your_super_secret_64_character_key_here

# Service URLs (internal Docker network)
SERVICES__ASR__URL=http://asr-service:8001
SERVICES__VAD__URL=http://vad-service:8002
SERVICES__TTS__URL=http://tts-service:8003
SERVICES__AICONTEXT__URL=http://ai-context-service:8004

# JWT Secret for user authentication
JWT_SECRET=your_jwt_secret_key_here
```

### Azure Key Vault Integration (Production)

```csharp
// Program.cs - Azure Key Vault integration
var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault
builder.Configuration.AddAzureKeyVault(
    new Uri(builder.Configuration["KeyVault:Endpoint"]),
    new DefaultAzureCredential());

// Now INTERNAL_API_KEY is securely retrieved from Key Vault
var internalApiKey = builder.Configuration["InternalApiKey"];
```

---

## Service-to-Service Communication

### Python AI Service Example

```python
# ai_services/asr_service/main.py
from fastapi import FastAPI, HTTPException, Depends, Header
from typing import Optional
import os
import hmac
import hashlib

app = FastAPI(title="ASR Service", version="1.0.0")

INTERNAL_API_KEY = os.getenv("INTERNAL_API_KEY")
SERVICE_NAME = os.getenv("SERVICE_NAME", "asr-service")

def verify_internal_request(
    x_internal_api_key: str = Header(...),
    x_caller: str = Header(None)
) -> bool:
    """Verify request is from authorized internal service"""
    if not hmac.compare_digest(x_internal_api_key, INTERNAL_API_KEY):
        raise HTTPException(status_code=403, detail="Unauthorized")

    # Log the caller for audit
    print(f"Request from {x_caller} to {SERVICE_NAME}")

    return True

@app.post("/api/v1/transcribe")
async def transcribe(
    request: TranscriptionRequest,
    authorized: bool = Depends(verify_internal_request)
):
    """Transcribe audio to text (only callable by .NET API or authorized services)"""
    # Implementation here
    return {"segments": [...], "duration": 120.5}

@app.get("/health")
async def health_check():
    """Public health check (no auth required)"""
    return {"status": "healthy", "service": SERVICE_NAME}
```

### VAD Service Calling ASR Service

```python
# ai_services/vad_service/client.py
class ASRClient(InternalServiceClient):
    def __init__(self):
        super().__init__(
            service_name="vad-service",
            api_key=os.getenv("INTERNAL_API_KEY")
        )

    async def transcribe_segments(self, audio_path: str, segments: list) -> dict:
        """Call ASR service to transcribe speech segments"""
        return await self.call_service(
            target_service="asr-service",
            port=8001,
            method="POST",
            path="/api/v1/transcribe",
            body={
                "audio_path": audio_path,
                "segments": segments,
                "languages": ["zh", "en"]
            }
        )
```

---

## Implementation Guide

### Step 1: Setup Docker Networks

```yaml
# docker-compose.yml
version: "3.8"

networks:
  ai-network:
    driver: bridge
    internal: true
```

### Step 2: Configure AI Services

```python
# Each AI service needs:
# 1. Authentication middleware
# 2. Health check endpoint (public)
# 3. API endpoints (protected)

@app.middleware("http")
async def auth_middleware(request: Request, call_next):
    if request.url.path in ["/health", "/ready"]:
        return await call_next(request)

    api_key = request.headers.get("X-Internal-API-Key")
    if not api_key or not hmac.compare_digest(api_key, os.getenv("INTERNAL_API_KEY")):
        raise HTTPException(status_code=403, detail="Unauthorized")

    return await call_next(request)
```

### Step 3: Configure .NET API

```csharp
// appsettings.json
{
  "InternalApiKey": "${INTERNAL_API_KEY}",
  "Services": {
    "ASR": { "Url": "http://asr-service:8001" },
    "VAD": { "Url": "http://vad-service:8002" },
    "TTS": { "Url": "http://tts-service:8003" },
    "AIContext": { "Url": "http://ai-context-service:8004" }
  }
}
```

### Step 4: Register HTTP Clients

```csharp
// Program.cs
services.AddHttpClient<IPythonAIService, PythonAIService>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddPolicyHandler(GetTimeoutPolicy(TimeSpan.FromMinutes(5)));
```

---

## Security Checklist

| Security Measure      | Implementation                                               | Status |
| --------------------- | ------------------------------------------------------------ | ------ |
| **Network Isolation** | Docker internal networks, no port publishing for AI services | ☐      |
| **API Key Auth**      | All internal requests require `X-Internal-API-Key` header    | ☐      |
| **Request Signing**   | HMAC signatures for service-to-service communication         | ☐      |
| **Rate Limiting**     | Per-service rate limits to prevent abuse                     | ☐      |
| **Circuit Breaker**   | Prevent cascade failures                                     | ☐      |
| **Audit Logging**     | All inter-service calls are logged                           | ☐      |
| **TLS/mTLS**          | Encrypt all internal communication                           | ☐      |
| **Secret Management** | API keys stored in environment variables/secrets manager     | ☐      |
| **Input Validation**  | All requests validated before processing                     | ☐      |
| **Health Checks**     | Separate public health endpoints                             | ☐      |

---

## Tài liệu tham khảo

- [Kiến trúc hệ thống](./README.md)
- [Đặc tả kỹ thuật](../05-technical-specs/README.md)
- [Triển khai](../07-deployment/README.md)

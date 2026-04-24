# 07 - Triển khai

## Mục lục

1. [Môi trường triển khai](#môi-trường-triển-khai)
2. [Free Tier Options (Giai đoạn đầu)](#free-tier-options-giai-đoạn-đầu)
3. [Paid Options (Khi mở rộng)](#paid-options-khi-mở-rộng)
4. [GPU Server Pricing](#gpu-server-pricing)
5. [Video Storage Solutions](#video-storage-solutions)
6. [CI/CD Pipeline](#cicd-pipeline)
7. [Migration Strategy](#migration-strategy)

---

## Môi trường triển khai

### Environment Overview

| Environment | Purpose           | URL                              | Access    |
| ----------- | ----------------- | -------------------------------- | --------- |
| Development | Local development | http://localhost:5000            | Team only |
| Staging     | Testing & QA      | https://staging.auto-process.com | Team only |
| Production  | Live service      | https://api.auto-process.com     | Public    |

---

## Free Tier Options (Giai đoạn đầu)

### Complete Free Stack

Đây là cấu hình hoàn toàn miễn phí cho giai đoạn phát triển và testing:

#### 1. Backend (.NET)

| Service         | Free Option                           | Limits                           |
| --------------- | ------------------------------------- | -------------------------------- |
| **Hosting**     | Azure App Service F1                  | 60 min CPU/day, 1GB storage      |
| **Alternative** | Render.com                            | 750 hours/month, 512MB RAM       |
| **Alternative** | Railway.app                           | $5 credit/month                  |
| **Database**    | Azure PostgreSQL Flexible (Free tier) | 32GB storage, 1 vCore            |
| **Alternative** | Supabase                              | 500MB database, 1GB file storage |
| **Cache**       | Redis (self-hosted on same VM)        | Shared resources                 |

#### 2. Frontend (React)

| Service         | Free Option           | Limits                  |
| --------------- | --------------------- | ----------------------- |
| **Hosting**     | Vercel                | 100GB bandwidth/month   |
| **Alternative** | Netlify               | 100GB bandwidth/month   |
| **Alternative** | GitHub Pages          | Unlimited (static only) |
| **Alternative** | Azure Static Web Apps | 100GB bandwidth/month   |

#### 3. AI/ML Services (Free Self-hosted)

| Service          | Free Model    | Requirements              |
| ---------------- | ------------- | ------------------------- |
| **ASR**          | Whisper.cpp   | 4GB RAM, CPU only (slow)  |
| **ASR (better)** | Whisper small | 2GB RAM + GPU recommended |
| **VAD**          | Silero VAD    | 500MB RAM, CPU            |
| **TTS**          | VnTTS         | 2GB RAM, CPU              |
| **Translation**  | NLLB-200      | 4GB RAM, CPU              |
| **AI Context**   | Llama 2 7B    | 8GB RAM + GPU recommended |

#### 4. Storage

| Service          | Free Option              | Limits                       |
| ---------------- | ------------------------ | ---------------------------- |
| **File Storage** | Azure Blob Storage       | 5GB free, 10K read ops/month |
| **Alternative**  | Backblaze B2             | 10GB free                    |
| **Alternative**  | Local storage (dev only) | Limited by disk              |

### Free Tier Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         FREE TIER STACK                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Frontend (React)           Backend (.NET)         AI Services  │
│  ┌─────────────┐           ┌─────────────┐       ┌─────────────┐│
│  │   Vercel    │──────────▶│ Azure F1    │──────▶│   Python    ││
│  │   (Free)    │           │   (Free)    │       │  (Local)    ││
│  └─────────────┘           └──────┬──────┘       └─────────────┘│
│                                    │                              │
│                                    ▼                              │
│                            ┌─────────────┐                       │
│                            │   Azure     │                       │
│                            │ PostgreSQL  │                       │
│                            │  (Free)     │                       │
│                            └──────┬──────┘                       │
│                                   │                               │
│                                   ▼                               │
│                            ┌─────────────┐                       │
│                            │ Azure Blob  │                       │
│                            │  (5GB Free) │                       │
│                            └─────────────┘                       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Free Tier Limitations

| Limitation               | Impact                  | Workaround                          |
| ------------------------ | ----------------------- | ----------------------------------- |
| Azure F1: 60 min CPU/day | Limited processing time | Queue jobs, process during off-peak |
| No GPU in free tier      | Slow AI processing      | Use smaller models, CPU-only        |
| 5GB storage limit        | ~5-10 videos            | Auto-delete after processing        |
| Shared resources         | Variable performance    | Set expectations with users         |

---

## Paid Options (Khi mở rộng)

### Recommended Paid Stack

Khi có người dùng và cần chất lượng cao hơn:

#### 1. Backend (.NET) - Paid

| Service         | Paid Option               | Cost (Monthly) |
| --------------- | ------------------------- | -------------- |
| **Hosting**     | Azure App Service P1V3    | ~$70           |
| **Alternative** | AWS ECS Fargate           | ~$50-100       |
| **Database**    | Azure PostgreSQL Standard | ~$50           |
| **Cache**       | Azure Cache for Redis C0  | ~$17           |

#### 2. Frontend (React) - Still Free!

Frontend có thể vẫn dùng free tier vì chỉ là static files:

- **Vercel Pro**: $20/month (nếu cần team features)
- **Netlify Pro**: $19/month
- **Azure Static Web Apps Standard**: $9/month

#### 3. AI Services - Paid (Quality Upgrade)

| Service         | Free Model  | Paid Upgrade         | Cost                  |
| --------------- | ----------- | -------------------- | --------------------- |
| **ASR**         | Whisper.cpp | Azure Speech API     | $1/15 min audio       |
| **TTS**         | VnTTS       | Azure Neural TTS     | $1/1M characters      |
| **Translation** | NLLB        | Google Translate API | $20/1M characters     |
| **AI Context**  | Llama 2     | GPT-4 API            | $0.03/1K input tokens |

#### 4. Storage - Paid

| Service        | Free | Paid          | Cost            |
| -------------- | ---- | ------------- | --------------- |
| **Azure Blob** | 5GB  | Pay-as-you-go | ~$0.02/GB/month |
| **CDN**        | None | Azure CDN     | ~$0.087/GB      |

---

## GPU Server Pricing

GPU servers là cần thiết để xử lý AI nhanh chóng. Đây là pricing chi tiết:

### Azure GPU VMs

| VM Size          | GPU     | VRAM  | vCPU | RAM   | Cost/Hour | Cost/Month (24/7) |
| ---------------- | ------- | ----- | ---- | ----- | --------- | ----------------- |
| **NC4as T4 v3**  | 1x T4   | 16GB  | 4    | 28GB  | ~$0.70    | ~$504             |
| **NC8as T4 v3**  | 1x T4   | 16GB  | 8    | 56GB  | ~$1.00    | ~$720             |
| **NC16as T4 v3** | 1x T4   | 16GB  | 16   | 110GB | ~$1.60    | ~$1,152           |
| **ND96asr v4**   | 8x A100 | 640GB | 96   | 960GB | ~$30      | ~$21,600          |

### AWS GPU Instances

| Instance Type     | GPU     | VRAM  | vCPU | RAM    | Cost/Hour | Cost/Month (24/7) |
| ----------------- | ------- | ----- | ---- | ------ | --------- | ----------------- |
| **g4dn.xlarge**   | 1x T4   | 16GB  | 4    | 16GB   | ~$0.53    | ~$382             |
| **g4dn.2xlarge**  | 1x T4   | 16GB  | 8    | 32GB   | ~$0.75    | ~$540             |
| **g4dn.12xlarge** | 4x T4   | 64GB  | 48   | 192GB  | ~$4.00    | ~$2,880           |
| **p4d.24xlarge**  | 8x A100 | 640GB | 96   | 1152GB | ~$32      | ~$23,040          |

### Cost Optimization Strategies

1. **Spot Instances**: 60-90% discount (AWS Spot, Azure Spot VMs)
   - g4dn.xlarge Spot: ~$0.16/hour (~$115/month)
   - Risk: Can be interrupted

2. **Auto-scaling**: Only run GPU when needed
   - Scale to 0 when no jobs
   - Scale up when queue has jobs

3. **Hybrid Approach**:
   - Free tier for demo/small videos
   - Paid GPU for production/large videos

### Recommended GPU Setup

**For Startup (Low Volume)**:

```
- 1x AWS g4dn.xlarge Spot (~$115/month)
- Auto-scale to 0 when idle
- Queue-based processing
```

**For Growth (Medium Volume)**:

```
- 2x AWS g4dn.xlarge On-demand (~$1,080/month)
- Load balanced
- 99% uptime
```

**For Scale (High Volume)**:

```
- 4-8x AWS g4dn.2xlarge (~$4,320-8,640/month)
- Kubernetes cluster
- Auto-scaling group
```

---

## Video Storage Solutions

### Where to Store Uploaded Videos

#### Option 1: Azure Blob Storage (Recommended)

```csharp
// Azure Blob Storage Configuration
public class AzureBlobStorageService : IStorageService
{
    private readonly BlobContainerClient _container;

    public async Task<string> UploadAsync(Stream fileStream, string fileName)
    {
        var blobClient = _container.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            TransferOptions = new Azure.Storage.Transfers.StorageTransferOptions
            {
                MaximumTransferSize = 4 * 1024 * 1024, // 4MB chunks
                InitialTransferLength = 4 * 1024 * 1024
            }
        });

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string blobPath)
    {
        var blobClient = new BlobClient(new Uri(blobPath));
        var stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream);
        stream.Position = 0;
        return stream;
    }
}
```

**Cost**:

- Storage: $0.0184/GB/month (Hot tier)
- Write operations: $0.06/10K
- Read operations: $0.01/10K
- Data transfer out: $0.087/GB (first 100TB)

#### Option 2: AWS S3

```csharp
// AWS S3 Configuration
public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public async Task<string> UploadAsync(Stream fileStream, string fileName)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            InputStream = fileStream,
            StorageClass = S3StorageClass.Standard
        };

        await _s3Client.PutObjectAsync(putRequest);
        return $"s3://{_bucketName}/{fileName}";
    }
}
```

**Cost**:

- Storage: $0.023/GB/month (Standard)
- Write requests: $0.005/1K
- Read requests: $0.0004/1K
- Data transfer out: $0.09/GB (first 10TB)

#### Option 3: Local Storage (Development Only)

```csharp
// Local file storage for development
public class LocalStorageService : IStorageService
{
    private readonly string _basePath;

    public async Task<string> UploadAsync(Stream fileStream, string fileName)
    {
        var path = Path.Combine(_basePath, "uploads", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        using var fileStreamOutput = File.Create(path);
        await fileStream.CopyToAsync(fileStreamOutput);

        return path;
    }
}
```

### Storage Lifecycle Management

```json
// Azure Blob Lifecycle Policy
{
  "rules": [
    {
      "name": "MoveToCoolAfter7Days",
      "type": "Lifecycle",
      "enabled": true,
      "filters": {
        "blobTypes": ["blockBlob"],
        "prefixMatch": ["uploads/"]
      },
      "actions": {
        "tierToCool": {
          "daysAfterModificationGreaterThan": 7
        },
        "tierToArchive": {
          "daysAfterModificationGreaterThan": 30
        },
        "delete": {
          "daysAfterModificationGreaterThan": 90
        }
      }
    }
  ]
}
```

### Estimated Storage Costs

| Videos/Month | Avg Size | Storage Needed | Monthly Cost |
| ------------ | -------- | -------------- | ------------ |
| 100          | 100MB    | 10GB           | ~$0.20       |
| 1,000        | 100MB    | 100GB          | ~$2.00       |
| 10,000       | 100MB    | 1TB            | ~$20.00      |

---

## CI/CD Pipeline

### GitHub Actions Workflow

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test-backend:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "8.0.x"

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal

  test-frontend:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: "20.x"

      - name: Install dependencies
        run: npm ci

      - name: Build
        run: npm run build

      - name: Test
        run: npm test

  deploy-staging:
    needs: [test-backend, test-frontend]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    environment: staging
    steps:
      - name: Deploy to Azure (Staging)
        uses: azure/webapps-deploy@v2
        with:
          app-name: "auto-process-staging"
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: "./src/AutoProcess.Api"

  deploy-production:
    needs: deploy-staging
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment: production
    steps:
      - name: Deploy to Azure (Production)
        uses: azure/webapps-deploy@v2
        with:
          app-name: "auto-process-prod"
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: "./src/AutoProcess.Api"
```

---

## Migration Strategy

### Phase 1: Free Tier (Development)

```
Duration: 1-3 months
Goal: MVP with basic functionality
Cost: $0/month

Components:
- Frontend: Vercel (Free)
- Backend: Azure F1 or Render (Free)
- Database: Azure PostgreSQL Free or Supabase
- Storage: Azure Blob 5GB Free
- AI: Self-hosted CPU models
```

### Phase 2: Paid Basic (Early Users)

```
Duration: 3-6 months
Goal: Better quality, more reliability
Cost: ~$200-500/month

Components:
- Frontend: Vercel Pro ($20)
- Backend: Azure P1V3 ($70)
- Database: Azure PostgreSQL Standard ($50)
- Storage: Azure Blob Pay-as-you-go (~$10)
- AI: Mix of free + paid APIs (~$100-300)
- GPU: 1x Spot instance (~$115)
```

### Phase 3: Scale (Growth)

```
Duration: 6+ months
Goal: High quality, high reliability
Cost: ~$1,000-5,000/month

Components:
- Frontend: Vercel Enterprise or self-hosted
- Backend: Multiple Azure instances or Kubernetes
- Database: Azure PostgreSQL Hyperscale
- Storage: Azure Blob + CDN
- AI: All paid APIs for best quality
- GPU: Multiple on-demand instances
```

### Migration Checklist

- [ ] Set up monitoring (Application Insights)
- [ ] Configure auto-scaling rules
- [ ] Set up backup procedures
- [ ] Configure CDN for static assets
- [ ] Set up rate limiting
- [ ] Configure SSL certificates
- [ ] Set up custom domain
- [ ] Configure email notifications
- [ ] Set up error tracking (Sentry)
- [ ] Configure log aggregation

---

## Tài liệu tham khảo

- [Kiến trúc hệ thống](../03-architecture/README.md)
- [Đặc tả kỹ thuật](../05-technical-specs/README.md)
- [API Documentation](../06-api-docs/README.md)
- [Testing](../08-testing/README.md)

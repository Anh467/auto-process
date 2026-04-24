# 08 - Kiểm thử

## Mục lục

1. [Test Strategy](#test-strategy)
2. [Testing in Clean Architecture](#testing-in-clean-architecture)
3. [Test Cases](#test-cases)
4. [Automation Tests](#automation-tests)
5. [Performance Testing](#performance-testing)

---

## Test Strategy

### Testing Pyramid

```
                    ┌─────────────┐
                    │     E2E     │  10%
                   ┌┴─────────────┴┐
                  ┌┴───────────────┴┐
                  │   Integration   │  20%
                 ┌┴─────────────────┴┐
                ┌┴───────────────────┴┐
                │       Unit          │  70%
                └─────────────────────┘
```

### Test Levels

| Level       | Description            | Tools               | Coverage Target |
| ----------- | ---------------------- | ------------------- | --------------- |
| Unit        | Individual components  | xUnit, NUnit, Moq   | 80%             |
| Integration | Component interactions | TestServer, Moq     | 60%             |
| E2E         | Full user workflows    | Playwright, Cypress | 40%             |
| Performance | Load & stress          | JMeter, k6          | -               |

### Test Environments

| Environment | Purpose             | Data                       |
| ----------- | ------------------- | -------------------------- |
| Local       | Developer testing   | Mock data                  |
| Dev         | Integration testing | Sample data                |
| Staging     | Pre-production      | Anonymized production data |
| Production  | Canary testing      | Real data (read-only)      |

---

## Testing in Clean Architecture

### Testing Strategy by Layer

Clean Architecture enables testing each layer independently with clear boundaries:

```
┌─────────────────────────────────────────────────────────────────┐
│                         Testing Layers                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                   Domain Layer Tests                        │ │
│  │   • Entity behavior tests                                   │ │
│  │   • Value Object validation tests                           │ │
│  │   • Domain Event tests                                      │ │
│  │   • Business rule validation tests                          │ │
│  │   ► No dependencies, pure unit tests                        │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              ▲                                   │
│                              │                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                Application Layer Tests                      │ │
│  │   • Command/Query handler tests                             │ │
│  │   • Validation behavior tests                               │ │
│  │   • Mapping profile tests                                   │ │
│  │   ► Mock repository and service interfaces                  │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              ▲                                   │
│                              │                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │               Infrastructure Layer Tests                    │ │
│  │   • Repository implementation tests                         │ │
│  │   • External service integration tests                      │ │
│  │   • Database context tests                                  │ │
│  │   ► Use in-memory database or test containers               │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              ▲                                   │
│                              │                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    API Layer Tests                          │ │
│  │   • Controller tests                                        │ │
│  │   • Middleware tests                                        │ │
│  │   • API integration tests                                   │ │
│  │   ► Use WebApplicationFactory for integration tests         │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 1. Domain Layer Tests

Domain layer tests are pure unit tests with no external dependencies:

```csharp
// tests/AutoProcess.Domain.Tests/Entities/VideoTests.cs
public class VideoTests
{
    [Fact]
    public void StartProcessing_WhenNotProcessing_CreatesProcessingJob()
    {
        // Arrange
        var video = new Video
        {
            Id = Guid.NewGuid(),
            Title = "Test Video",
            FilePath = "/path/to/video.mp4"
        };

        // Act
        video.StartProcessing();

        // Assert
        video.ProcessingJob.Should().NotBeNull();
        video.ProcessingJob.Status.Should().Be(JobStatus.Queued);
        video.ProcessingJob.VideoId.Should().Be(video.Id);
        video.DomainEvents.Should().ContainSingle(e => e is VideoProcessingStartedEvent);
    }

    [Fact]
    public void StartProcessing_WhenAlreadyProcessing_ThrowsException()
    {
        // Arrange
        var video = new Video
        {
            Id = Guid.NewGuid(),
            ProcessingJob = new ProcessingJob
            {
                Status = JobStatus.Processing
            }
        };

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() => video.StartProcessing());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetTitle_WhenTitleIsEmpty_ThrowsException(string? title)
    {
        // Arrange
        var video = new Video();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => video.SetTitle(title));
    }
}
```

```csharp
// tests/AutoProcess.Domain.Tests/ValueObjects/DurationTests.cs
public class DurationTests
{
    [Fact]
    public void Create_WhenNegativeSeconds_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Duration.FromSeconds(-1));
    }

    [Fact]
    public void Create_WhenValidSeconds_ReturnsDuration()
    {
        // Act
        var duration = Duration.FromSeconds(120);

        // Assert
        duration.TotalSeconds.Should().Be(120);
        duration.Minutes.Should().Be(2);
    }

    [Fact]
    public void Add_WhenAddingDurations_ReturnsSum()
    {
        // Arrange
        var duration1 = Duration.FromSeconds(60);
        var duration2 = Duration.FromSeconds(30);

        // Act
        var result = duration1.Add(duration2);

        // Assert
        result.TotalSeconds.Should().Be(90);
    }
}
```

### 2. Application Layer Tests

Application layer tests mock the infrastructure dependencies:

```csharp
// tests/AutoProcess.Application.Tests/Features/Videos/Commands/UploadVideoCommandHandlerTests.cs
public class UploadVideoCommandHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UploadVideoCommandHandler>> _loggerMock;
    private readonly UploadVideoCommandHandler _handler;

    public UploadVideoCommandHandlerTests()
    {
        _videoRepositoryMock = new Mock<IVideoRepository>();
        _storageServiceMock = new Mock<IStorageService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UploadVideoCommandHandler>>();

        _handler = new UploadVideoCommandHandler(
            _videoRepositoryMock.Object,
            _storageServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenFileIsValid_ReturnsVideoDto()
    {
        // Arrange
        var command = new UploadVideoCommand
        {
            Title = "Test Video",
            FileName = "test.mp4",
            File = new FormFile(Stream.Null, 0, 1000, "file", "test.mp4"),
            UserId = Guid.NewGuid()
        };

        var videoPath = "/storage/videos/test.mp4";
        var video = new Video { Id = Guid.NewGuid(), Title = command.Title };
        var videoDto = new VideoDto { Id = video.Id, Title = video.Title };

        _storageServiceMock.Setup(s => s.UploadVideoAsync(
            It.IsAny<IFormFile>(), command.FileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoPath);

        _videoRepositoryMock.Setup(r => r.AddAsync(
            It.IsAny<Video>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        _mapperMock.Setup(m => m.Map<VideoDto>(video)).Returns(videoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(videoDto);
        _videoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStorageFails_ReturnsFailure()
    {
        // Arrange
        var command = new UploadVideoCommand
        {
            Title = "Test Video",
            FileName = "test.mp4",
            File = new FormFile(Stream.Null, 0, 1000, "file", "test.mp4"),
            UserId = Guid.NewGuid()
        };

        _storageServiceMock.Setup(s => s.UploadVideoAsync(
            It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Storage error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to upload video");
    }
}
```

### 3. Infrastructure Layer Tests

Infrastructure tests use in-memory databases or test containers:

```csharp
// tests/AutoProcess.Infrastructure.Tests/Persistence/AppDbContextTests.cs
public class AppDbContextTests : IDisposable
{
    private readonly AppDbContext _context;

    public AppDbContextTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options, null, null);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAddingVideo_SetsCreatedAt()
    {
        // Arrange
        var video = new Video
        {
            Title = "Test Video",
            FilePath = "/path/to/video.mp4",
            UserId = Guid.NewGuid()
        };

        // Act
        await _context.Videos.AddAsync(video);
        await _context.SaveChangesAsync();

        // Assert
        video.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        video.CreatedBy.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenUpdatingVideo_SetsModifiedAt()
    {
        // Arrange
        var video = new Video
        {
            Title = "Test Video",
            FilePath = "/path/to/video.mp4",
            UserId = Guid.NewGuid()
        };
        await _context.Videos.AddAsync(video);
        await _context.SaveChangesAsync();

        // Act
        video.Title = "Updated Title";
        await _context.SaveChangesAsync();

        // Assert
        video.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

### 4. API Layer Tests

API tests use WebApplicationFactory for integration testing:

```csharp
// tests/AutoProcess.Api.Tests/Controllers/VideoControllerTests.cs
public class VideoControllerTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;

    public VideoControllerTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UploadVideo_WhenUnauthorized_Returns401()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(Array.Empty<byte>()), "file", "test.mp4");

        // Act
        var response = await _client.PostAsync("/api/video/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVideo_WhenVideoExists_Returns200()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        await SeedVideoAsync(videoId);
        AddJwtToken();

        // Act
        var response = await _client.GetAsync($"/api/video/{videoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<VideoDto>>();
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    private async Task SeedVideoAsync(Guid videoId)
    {
        using var scope = _client.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Videos.Add(new Video
        {
            Id = videoId,
            Title = "Test Video",
            FilePath = "/path/to/video.mp4",
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000001")
        });

        await context.SaveChangesAsync();
    }
}
```

### Test Project Structure

```
tests/
├── AutoProcess.Domain.Tests/
│   ├── Entities/
│   │   ├── VideoTests.cs
│   │   ├── ProcessingJobTests.cs
│   │   └── UserTests.cs
│   ├── ValueObjects/
│   │   ├── DurationTests.cs
│   │   ├── VideoPathTests.cs
│   │   └── LanguageTests.cs
│   └── Events/
│       └── DomainEventTests.cs
│
├── AutoProcess.Application.Tests/
│   ├── Features/
│   │   ├── Videos/
│   │   │   ├── Commands/
│   │   │   │   ├── UploadVideoCommandHandlerTests.cs
│   │   │   │   └── ProcessVideoCommandHandlerTests.cs
│   │   │   └── Queries/
│   │   │       ├── GetVideoByIdQueryHandlerTests.cs
│   │   │       └── GetVideoListQueryHandlerTests.cs
│   │   ├── Audio/
│   │   └── Auth/
│   ├── Common/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehaviorTests.cs
│   │   └── Mappings/
│   │       └── MappingProfileTests.cs
│   └── AutoProcess.Application.Tests.csproj
│
├── AutoProcess.Infrastructure.Tests/
│   ├── Persistence/
│   │   ├── AppDbContextTests.cs
│   │   └── RepositoryTests.cs
│   ├── Services/
│   │   ├── StorageServiceTests.cs
│   │   └── PythonAIServiceTests.cs
│   └── AutoProcess.Infrastructure.Tests.csproj
│
├── AutoProcess.Api.Tests/
│   ├── Controllers/
│   │   ├── VideoControllerTests.cs
│   │   └── AuthControllerTests.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddlewareTests.cs
│   ├── IntegrationTestFactory.cs
│   └── AutoProcess.Api.Tests.csproj
│
└── AutoProcess.Integration.Tests/
    ├── VideoProcessingTests.cs
    ├── AuthenticationTests.cs
    └── AutoProcess.Integration.Tests.csproj
```

### Integration Test Factory

```csharp
// tests/AutoProcess.Api.Tests/IntegrationTestFactory.cs
public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IServiceScope _scope;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace DbContext with in-memory database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Build service provider
            var sp = services.BuildServiceProvider();

            // Create scope and get DbContext
            _scope = sp.CreateScope();
            var scopedServices = _scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();

            // Ensure database is created and seeded
            db.Database.EnsureCreated();
            InitializeTestDatabase(db);
        });
    }

    private void InitializeTestDatabase(AppDbContext context)
    {
        // Seed test data
        context.Users.AddRange(
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Email = "test@example.com" }
        );
        context.SaveChanges();
    }

    public async Task InitializeAsync()
    {
        // Any async initialization
        await Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<AppDbContext>();

        await db.Database.EnsureDeletedAsync();
        db.Dispose();

        _scope.Dispose();
    }
}
```

---

## Test Cases

### Functional Test Cases

#### FC-01: Video Upload

| Test ID  | Description           | Input          | Expected Output               | Status |
| -------- | --------------------- | -------------- | ----------------------------- | ------ |
| TC-01-01 | Upload valid MP4      | 100MB MP4 file | 202 Accepted, job_id returned |        |
| TC-01-02 | Upload valid AVI      | 50MB AVI file  | 202 Accepted, job_id returned |        |
| TC-01-03 | Upload invalid format | PDF file       | 400 Bad Request               |        |
| TC-01-04 | Upload file too large | 3GB MP4 file   | 400 Bad Request, size error   |        |
| TC-01-05 | Upload without auth   | Any file       | 401 Unauthorized              |        |

#### FC-02: Voice Activity Detection

| Test ID  | Description            | Input                     | Expected Output          | Status |
| -------- | ---------------------- | ------------------------- | ------------------------ | ------ |
| TC-02-01 | VAD with clear speech  | Clean audio with speech   | Speech segments detected |        |
| TC-02-02 | VAD with noise only    | Audio with no speech      | Empty speech segments    |        |
| TC-02-03 | VAD with mixed content | Speech + background music | Correct separation       |        |
| TC-02-04 | VAD with silence       | Silent audio              | Empty segments           |        |

#### FC-03: ASR (Speech to Text)

| Test ID  | Description               | Input                | Expected Output                 | Status |
| -------- | ------------------------- | -------------------- | ------------------------------- | ------ |
| TC-03-01 | ASR English               | English speech audio | Accurate English transcript     |        |
| TC-03-02 | ASR Chinese               | Chinese speech audio | Accurate Chinese transcript     |        |
| TC-03-03 | ASR with accent           | Accented speech      | Transcript with minor errors    |        |
| TC-03-04 | ASR with background noise | Noisy speech         | Transcript with [noise] markers |        |
| TC-03-05 | ASR multiple speakers     | Conversation         | Transcript with speaker labels  |        |

#### FC-04: Translation

| Test ID  | Description              | Input             | Expected Output                 | Status |
| -------- | ------------------------ | ----------------- | ------------------------------- | ------ |
| TC-04-01 | Translate English        | "Hello world"     | "Xin chào thế giới"             |        |
| TC-04-02 | Translate Chinese        | "你好世界"        | "Xin chào thế giới"             |        |
| TC-04-03 | Translate technical text | Technical terms   | Context-appropriate translation |        |
| TC-04-04 | Translate slang          | Informal language | Natural Vietnamese equivalent   |        |

#### FC-05: AI Context

| Test ID  | Description             | Input                    | Expected Output        | Status |
| -------- | ----------------------- | ------------------------ | ---------------------- | ------ |
| TC-05-01 | Context refinement      | Literal translation      | Natural Vietnamese     |        |
| TC-05-02 | Terminology consistency | Multiple segments        | Consistent terminology |        |
| TC-05-03 | Cultural adaptation     | Culture-specific content | Appropriately adapted  |        |

#### FC-06: TTS (Text to Speech)

| Test ID  | Description          | Input                 | Expected Output           | Status |
| -------- | -------------------- | --------------------- | ------------------------- | ------ |
| TC-06-01 | TTS Vietnamese text  | Vietnamese text       | Natural Vietnamese speech |        |
| TC-06-02 | TTS with timing      | Text with duration    | Speech matching duration  |        |
| TC-06-03 | TTS speed variation  | Text with speed param | Correct speaking speed    |        |
| TC-06-04 | TTS different voices | Text with voice param | Different voice output    |        |

#### FC-07: Video Merge

| Test ID  | Description          | Input             | Expected Output                | Status |
| -------- | -------------------- | ----------------- | ------------------------------ | ------ |
| TC-07-01 | Merge audio to video | Video + audio     | Video with new audio           |        |
| TC-07-02 | Merge subtitle       | Video + SRT       | Video with burned-in subtitles |        |
| TC-07-03 | Full pipeline        | Original video    | Localized video                |        |
| TC-07-04 | Audio sync check     | Video with timing | Synchronized output            |        |

### Non-Functional Test Cases

#### Performance

| Test ID  | Description           | Criteria    | Expected       |
| -------- | --------------------- | ----------- | -------------- |
| TC-PF-01 | API response time     | P95 latency | < 500ms        |
| TC-PF-02 | Video processing time | 10min video | < 20min        |
| TC-PF-03 | Concurrent users      | 100 users   | No degradation |
| TC-PF-04 | Memory usage          | Peak load   | < 85%          |

#### Security

| Test ID   | Description        | Criteria         | Expected     |
| --------- | ------------------ | ---------------- | ------------ |
| TC-SEC-01 | SQL Injection      | Malicious input  | Rejected     |
| TC-SEC-02 | XSS                | Script injection | Sanitized    |
| TC-SEC-03 | Auth bypass        | Missing token    | 401 response |
| TC-SEC-04 | File upload attack | Malicious file   | Rejected     |

---

## Automation Tests

### Unit Tests

#### Example: Translation Service Test

```csharp
// TranslationServiceTests.cs
using Xunit;
using AutoProcess.Services;
using FluentAssertions;

public class TranslationServiceTests
{
    private readonly ITranslationService _translationService;

    public TranslationServiceTests()
    {
        _translationService = new TranslationService();
    }

    [Fact]
    public async Task Translate_EnglishToVietnamese_ReturnsCorrectTranslation()
    {
        // Arrange
        var sourceText = "Hello, welcome to our channel";
        var sourceLang = "en";
        var targetLang = "vi";

        // Act
        var result = await _translationService.TranslateAsync(
            sourceText, sourceLang, targetLang);

        // Assert
        result.Success.Should().BeTrue();
        result.TranslatedText.Should().Contain("Xin chào");
    }

    [Theory]
    [InlineData("Hello", "en", "vi", "Xin chào")]
    [InlineData("你好", "zh", "vi", "Xin chào")]
    [InlineData("Thank you", "en", "vi", "Cảm ơn")]
    public async Task Translate_MultipleLanguages_ReturnsCorrectTranslation(
        string sourceText, string sourceLang, string targetLang, string expected)
    {
        // Act
        var result = await _translationService.TranslateAsync(
            sourceText, sourceLang, targetLang);

        // Assert
        result.TranslatedText.Should().Contain(expected);
    }
}
```

#### Example: VAD Service Test

```csharp
// VoiceActivityDetectionTests.cs
using Xunit;
using AutoProcess.Services;
using FluentAssertions;

public class VoiceActivityDetectionTests
{
    private readonly IVoiceActivityDetectionService _vadService;

    public VoiceActivityDetectionTests()
    {
        _vadService = new VoiceActivityDetectionService();
    }

    [Fact]
    public async Task Detect_SpeechAudio_ReturnsSpeechSegments()
    {
        // Arrange
        var audioPath = "test_data/speech_sample.wav";

        // Act
        var result = await _vadService.DetectAsync(audioPath);

        // Assert
        result.SpeechSegments.Should().NotBeEmpty();
        result.SpeechSegments.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task Detect_SilenceAudio_ReturnsEmptySegments()
    {
        // Arrange
        var audioPath = "test_data/silence_sample.wav";

        // Act
        var result = await _vadService.DetectAsync(audioPath);

        // Assert
        result.SpeechSegments.Should().BeEmpty();
    }
}
```

### Integration Tests

#### Example: Video Processing Pipeline Test

```csharp
// VideoProcessingPipelineTests.cs
using Xunit;
using AutoProcess.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

public class VideoProcessingPipelineTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public VideoProcessingPipelineTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProcessVideo_CompletePipeline_ReturnsProcessedVideo()
    {
        // Arrange
        var videoFile = new ByteArrayContent(
            await File.ReadAllBytesAsync("test_data/sample.mp4"));
        var content = new MultipartFormDataContent();
        content.Add(videoFile, "file", "sample.mp4");

        // Act - Upload
        var uploadResponse = await _client.PostAsync("/api/v1/video/upload", content);
        uploadResponse.EnsureSuccessStatusCode();

        var job = await uploadResponse.Content.ReadFromJsonAsync<JobResponse>();

        // Act - Wait for processing (polling)
        JobStatusResponse status;
        do
        {
            await Task.Delay(5000);
            var statusResponse = await _client.GetAsync($"/api/v1/video/{job.JobId}/status");
            status = await statusResponse.Content.ReadFromJsonAsync<JobStatusResponse>();
        } while (status.Status == "processing");

        // Assert
        status.Status.Should().Be("completed");

        // Act - Download
        var downloadResponse = await _client.GetAsync($"/api/v1/video/{job.JobId}/download");
        downloadResponse.EnsureSuccessStatusCode();

        var downloadedVideo = await downloadResponse.Content.ReadAsByteArrayAsync();
        downloadedVideo.Length.Should().BeGreaterThan(0);
    }
}
```

### E2E Tests

#### Example: Playwright E2E Test

```csharp
// VideoUploadE2ETests.cs
using Microsoft.Playwright;
using Xunit;

public class VideoUploadE2ETests : IAsyncLifetime
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IBrowserContext _context;
    private IPage _page;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task UploadVideo_UserCanUploadAndDownload()
    {
        // Navigate to upload page
        await _page.GotoAsync("https://auto-process.com/upload");

        // Login
        await _page.FillAsync("#email", "test@example.com");
        await _page.FillAsync("#password", "password123");
        await _page.ClickAsync("button[type='submit']");

        // Wait for navigation
        await _page.WaitForURLAsync("**/dashboard");

        // Upload video
        await _page.SetInputFilesAsync("input[type='file']", "test_data/sample.mp4");
        await _page.ClickAsync("button:has-text('Upload')");

        // Wait for processing
        await _page.WaitForSelectorAsync(".job-completed", new() { Timeout = 300000 });

        // Download result
        var download = await _page.ExpectDownloadAsync(async () =>
        {
            await _page.ClickAsync("button:has-text('Download')");
        });

        // Verify download
        var downloadedFile = await download.PathAsync();
        Assert.True(File.Exists(downloadedFile));
    }
}
```

### Test Configuration

#### `appsettings.Test.json`

```json
{
  "Testing": {
    "UseMockServices": true,
    "MockDelay": 100
  },
  "Services": {
    "ASR": {
      "UseMock": true,
      "MockTranscript": "This is a test transcript"
    },
    "Translation": {
      "UseMock": true,
      "MockTranslation": "Đây là bản dịch kiểm thử"
    },
    "TTS": {
      "UseMock": true,
      "MockAudioPath": "test_data/mock_tts.wav"
    }
  }
}
```

---

## Performance Testing

### Load Testing with k6

#### `tests/performance/load_test.js`

```javascript
import http from "k/http";
import { check, sleep } from "k";

export const options = {
  stages: [
    { duration: "2m", target: 10 }, // Ramp up to 10 users
    { duration: "5m", target: 10 }, // Stay at 10 users
    { duration: "2m", target: 50 }, // Ramp up to 50 users
    { duration: "5m", target: 50 }, // Stay at 50 users
    { duration: "2m", target: 100 }, // Ramp up to 100 users
    { duration: "5m", target: 100 }, // Stay at 100 users
    { duration: "2m", target: 0 }, // Ramp down to 0
  ],
  thresholds: {
    http_req_duration: ["p(95)<500"], // 95% of requests should complete below 500ms
    http_req_failed: ["rate<0.01"], // Error rate should be less than 1%
  },
};

export default function () {
  const token = "Bearer test_token";
  const headers = { Authorization: token };

  // Test upload endpoint
  const uploadRes = http.post(
    "https://api.auto-process.com/v1/video/upload",
    {},
    { headers },
  );

  check(uploadRes, {
    "upload status is 202": (r) => r.status === 202,
  });

  sleep(1);

  // Test status endpoint
  const statusRes = http.get(
    `https://api.auto-process.com/v1/video/${uploadRes.json().data.job_id}/status`,
    { headers },
  );

  check(statusRes, {
    "status check is 200": (r) => r.status === 200,
  });

  sleep(1);
}
```

### Performance Test Scenarios

| Scenario    | Users     | Duration | Target              |
| ----------- | --------- | -------- | ------------------- |
| Smoke Test  | 5         | 5 min    | Basic functionality |
| Load Test   | 50        | 30 min   | Normal load         |
| Stress Test | 200       | 30 min   | Peak load           |
| Soak Test   | 20        | 8 hours  | Memory leaks        |
| Spike Test  | 10→100→10 | 30 min   | Sudden traffic      |

### Performance Metrics

| Metric              | Target      | Warning      | Critical   |
| ------------------- | ----------- | ------------ | ---------- |
| Response Time (P50) | < 200ms     | 200-500ms    | > 500ms    |
| Response Time (P95) | < 500ms     | 500-1000ms   | > 1000ms   |
| Error Rate          | < 0.1%      | 0.1-1%       | > 1%       |
| Throughput          | > 100 req/s | 50-100 req/s | < 50 req/s |
| CPU Usage           | < 70%       | 70-85%       | > 85%      |
| Memory Usage        | < 80%       | 80-90%       | > 90%      |

---

## Tài liệu tham khảo

- [Yêu cầu hệ thống](../02-requirements/README.md)
- [Đặc tả kỹ thuật](../05-technical-specs/README.md)
- [Kiến trúc hệ thống](../03-architecture/README.md)
- [Deployment](../07-deployment/README.md)

### External Resources

- [xUnit Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [Moq Framework Documentation](https://github.com/devlooped/moq)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)

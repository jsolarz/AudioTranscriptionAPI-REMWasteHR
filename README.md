# Accent Analyzer and Audio Transcription API

A sophisticated API for analyzing English accents from video recordings. Built using .NET 8.0 and following IDesign architecture principles.

## 🎯 Overview

The Accent Analyzer is a tool designed to help HR teams evaluate candidates' English speaking skills during hiring processes. The application accepts video URLs (like Loom links or direct MP4 files), extracts audio, and analyzes speech patterns to classify different English accents along with a confidence score.

This project was developed in response to the REMWaste HR coding challenge to create an intelligent tool for evaluating spoken English in recruitment.

### Key Features

- **Video Processing**: Downloads videos from URLs with efficient stream-based processing
- **Multi-format Support**: Handles various audio/video formats (WAV, MP3, MP4, FLAC, OGG, OPUS, M4A)
- **Speech Analysis**: Transcribes speech using Azure Cognitive Services Speech Services
- **Accent Classification**: Identifies English accent types with confidence scores
- **Clean Architecture**: Built with IDesign principles for maintainability and testability

## 🛠️ System Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Azure Speech Services API key (for speech-to-text capabilities)
- Modern web browser for accessing the UI

## 🚀 Getting Started

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/accent-analyzer.git
   cd accent-analyzer
   ```

2. **Configure application settings**
   
   Create or update the `appsettings.Development.json` file:
   ```json
   {
     "AzureSpeech": {
       "SubscriptionKey": "your-azure-speech-key",
       "Region": "eastus"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft": "Warning",
         "Microsoft.Hosting.Lifetime": "Information"
       }
     }
   }
   ```

3. **Run the application**
   ```bash
   cd AudioTranscriptionAPI
   dotnet run
   ```

4. **Access the API and Web Interface**
   
   The API will be available at:
   - http://localhost:2607
   
   API documentation will be available at `/swagger` when running in Development mode.
   
   The web interface for testing accent analysis will be available at the root URL.

### Running Tests
```bash
python test_api.py
```
## 🏗️ Deployment

The project is configured for easy deployment to Azure App Service:

### Azure Resources Required

- **App Service**: Hosts the Accent Analyzer API
- **Azure Cognitive Services (Speech)**: For speech-to-text capabilities

### Deployment Options

1. **Visual Studio Publish**:
   
   Use the included publish profile in `Properties/PublishProfiles/app-accent-analyzer-tester - Zip Deploy.pubxml`

2. **Azure CLI**:
   ```bash
   # Create resources
   az group create --name rg-accent-analyzer --location eastus
   az cognitiveservices account create --name accent-analyzer-speech --resource-group rg-accent-analyzer --kind SpeechServices --sku S0 --location eastus
   az webapp create --name app-accent-analyzer --resource-group rg-accent-analyzer --plan BasicPlan --runtime "DOTNET|8.0"
   
   # Deploy the app
   dotnet publish -c Release
   cd bin/Release/net8.0/publish
   zip -r ../publish.zip .
   az webapp deployment source config-zip --resource-group rg-accent-analyzer --name app-accent-analyzer --src ../publish.zip
   ```

3. **GitHub Actions**:
   
   The repository can be configured with GitHub Actions for continuous deployment.

## 🐳 Docker Deployment

The project can be containerized for Docker deployment.

### Using Docker

1. **Create a Dockerfile**
   
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
   WORKDIR /app
   EXPOSE 80
   
   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   WORKDIR /src
   COPY ["AudioTranscriptionAPI.csproj", "./"]
   RUN dotnet restore "AudioTranscriptionAPI.csproj"
   COPY . .
   RUN dotnet build "AudioTranscriptionAPI.csproj" -c Release -o /app/build
   
   FROM build AS publish
   RUN dotnet publish "AudioTranscriptionAPI.csproj" -c Release -o /app/publish
   
   FROM base AS final
   WORKDIR /app
   COPY --from=publish /app/publish .
   ENTRYPOINT ["dotnet", "AudioTranscriptionAPI.dll"]
   ```

2. **Build and run the container**
   ```bash
   docker build -t accent-analyzer .
   docker run -d -p 8080:80 --name accent-analyzer-api \
     -e AzureSpeech__SubscriptionKey=your-key-here \
     -e AzureSpeech__Region=eastus \
     accent-analyzer
   ```
   
   The API will be available at:
   - http://localhost:8080

3. **Stop the container**
   ```bash
   docker stop accent-analyzer-api
   docker rm accent-analyzer-api
   ```
## 📊 API Endpoints

### Main Endpoints

- `POST /api/Transcription/transcribe` - Submit a video URL for accent analysis
  ```json
  {
    "videoUrl": "https://example.com/video.mp4"
  }
  ```

- `POST /api/Transcription/upload` - Upload an audio file for analysis
  (multipart/form-data with audioFile parameter)

### Response Format
```json
{
  "transcribedText": "The transcribed text from the audio",
  "confidence": 0.95,
  "durationSeconds": 30.5,
  "language": "en-US",
  "audioFormat": "mp3",
  "isSuccessful": true,
  "errorMessage": null,
  "timestamp": "2025-05-26T14:22:43.511Z",
  "accentClassification": "American",
  "confidenceScore": 92
}
```
## 🏗️ Architecture

The application follows IDesign architecture principles with the following layers:

- **Controllers**: API endpoints and HTTP request handling (`TranscriptionController`)
- **Services**: Core business logic and service implementations (`TranscriptionService`)
- **Models**: Data structures and DTOs (`TranscriptionResult`, `VideoUrlRequest`, `AudioDataRequest`)
- **Interfaces**: Service contracts for dependency injection (`ITranscriptionService`)

### System Flow

```
HTTP Request → TranscriptionController → ITranscriptionService → Azure Speech Services → Response
       ↑                                       ↓
       |                               Audio Format Detection
       |                                       ↓ 
      UI ←────────────────────────── Response Formatting
```

The architecture provides:
1. Clean separation of concerns
2. Testable components through dependency injection
3. Stream-based processing for memory efficiency 
4. Automatic format detection for multiple audio/video types
## 🔒 Configuration Options

The following settings can be configured in `appsettings.json` or through environment variables:

| Setting | Description | Default |
|---------|-------------|---------|
| `AzureSpeech:SubscriptionKey` | Azure Speech Services API key | - |
| `AzureSpeech:Region` | Azure Speech Services region | `eastus` |
| `Logging:LogLevel:Default` | Default logging level | `Information` |
| `Logging:LogLevel:Microsoft` | Microsoft framework logging level | `Warning` |

## 🔍 Monitoring and Troubleshooting

### Common Issues

1. **Video format not supported**
   
   The API supports many formats (WAV, MP3, MP4, FLAC, OGG, OPUS, M4A), but some proprietary formats may not be compatible.

2. **Azure Speech API errors**
   
   Verify your Azure Speech API key and region configuration.

3. **Video download issues**
   
   Check if the video URL is publicly accessible and not protected by authentication.

### Logs

The application uses standard .NET logging:

- In development: Console logs
- In production: Application Insights and structured logs (when configured)

## 📚 Implementation Details

For a comprehensive explanation of the architecture, implementation decisions, and features of this project, see the [implementation summary](docs/implementation-summary.md) document.

## 🎯 Challenge Requirements Met

This project satisfies all the requirements of the original coding challenge:
- ✅ Accepting video URLs for analysis
- ✅ Extracting audio from videos
- ✅ Analyzing speaker's accent
- ✅ Providing accent classification
- ✅ Including confidence scores
- ✅ Clean, testable implementation

## 📄 License

This project is licensed under the [MIT License](LICENSE) - see the [LICENSE](LICENSE) file for details.


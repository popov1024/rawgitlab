# RawGitLab

Proxy service for retrieving files from private GitLab repositories without requiring client-side authentication.

## Purpose

The service allows you to retrieve file contents from private self-hosted GitLab repositories via a simple HTTP API, hiding the authentication token and providing access to files via a public URL.

### Use Cases

- Embedding files from GitLab into public documents (PlantUML diagrams, Markdown)
- Accessing configuration files from external systems
- CDN for static files from private repositories

## Quick Start

### Via .NET CLI

```bash
# Run in development mode
dotnet run --project src/rawgitlab.csproj

# Build
dotnet build rawgitlab.sln

# Publish
dotnet publish -c Release -o ./publish
```

The server will start on `http://localhost:5071`

### Via Docker Compose

```bash
# Build and start
docker-compose up --build -d

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

## Configuration

### appsettings.Development.json

```json
{
  "GitLab": {
    "BaseUrl": "https://git.example.com",
    "PrivateToken": "glpat-your-token-here"
  }
}
```

| Parameter | Description |
|-----------|-------------|
| `BaseUrl` | URL of your self-hosted GitLab |
| `PrivateToken` | Personal Access Token with `read_api` scope |

### Environment Variables

```bash
GitLab__BaseUrl=https://git.example.com
GitLab__PrivateToken=glpat-xxx
```

## Usage

### Request Format

```
GET /{group}/{project}/-/raw/{ref}/{file_path}
```

| Parameter | Description |
|-----------|-------------|
| `group/project` | Project path (supports subgroups: `group/subgroup/project`) |
| `ref` | Branch, tag, or commit hash |
| `file_path` | Path to the file in the repository |

### Examples

#### Health check

```bash
curl http://localhost:5071/
# {"status":"OK","gitLabUrl":"https://git.example.com"}
```

#### Get file

```bash
# PlantUML diagram
curl http://localhost:5071/arch/components/-/raw/main/person-customer.iuml

# README from repository root
curl http://localhost:5071/mygroup/myproject/-/raw/main/README.md

# File from subdirectory
curl http://localhost:5071/mygroup/myproject/-/raw/main/src/Program.cs

# File from different branch
curl http://localhost:5071/mygroup/myproject/-/raw/develop/config.json

# File by tag
curl http://localhost:5071/mygroup/myproject/-/raw/v1.0.0/CHANGELOG.md

# Subgroups (nested groups)
curl http://localhost:5071/group/subgroup/project/-/raw/main/file.txt
```

### PlantUML Integration

Example usage in PlantUML files:

```text
@startuml
!include http://localhost:5071/arch/components/-/raw/main/person-customer.iuml
@enduml
```

Run PlantUML with this code to render the diagram.

### Markdown Integration

Example of embedding an image from GitLab:

```text
![Diagram](http://localhost:5071/docs/diagrams/-/raw/main/architecture.png)
```

## API

### GET /{path}

Returns file content from GitLab.

**Parameters:**
- `path` — path in format `group/project/-/raw/ref/file_path`

**Response:**
- `200 OK` — file content with appropriate `Content-Type`
- `400 Bad Request` — invalid path format
- `404 Not Found` — file or project not found
- `500 Internal Server Error` — server error

**Response Headers:**
- `Content-Type` — file MIME type (from GitLab API)
- `Content-Disposition` — file name

## Creating a GitLab Token

1. Open GitLab → Settings → Access Tokens
2. Create a new token:
   - Name: `rawgitlab`
   - Scopes: `read_api`
3. Save the token and add it to the configuration

## Project Structure

```
rawgitlab/
├── src/
│   ├── Program.cs              # Entry point, endpoints
│   ├── Models/
│   │   ├── GitLabSettings.cs   # GitLab configuration
│   │   └── RepositoryConfig.cs # Repository configuration
│   ├── Services/
│   │   ├── IGitLabService.cs   # Service interface
│   │   └── GitLabService.cs    # GitLab API client
│   ├── appsettings.Development.json
│   └── rawgitlab.csproj
├── Dockerfile
├── docker-compose.yml
├── docker-compose.override.yml
└── rawgitlab.sln
```

## Requirements

- .NET 10.0
- Docker (optional)
- Access to self-hosted GitLab

## License

MIT

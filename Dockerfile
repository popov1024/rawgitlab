# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY src/*.csproj ./src/
RUN dotnet restore ./src/src.csproj

# Copy source code
COPY src/ ./src/
WORKDIR /src/src
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser

COPY --from=build /app/publish .
RUN chown -R appuser:appuser /app

USER appuser

ENTRYPOINT ["dotnet", "rawgitlab.dll"]

# =============================
# 1. Build stage
# =============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy solution and restore
COPY *.sln ./
COPY API/ ./API/
COPY BU/ ./BU/
COPY Common/ ./Common/
COPY Data/ ./Data/
COPY Model/ ./Model/

RUN dotnet restore

# Build project (release mode)
RUN dotnet publish -c Release -o /app/publish

# =============================
# 2. Runtime stage
# =============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

# Expose port if needed (tuỳ theo app listen ở đâu)
EXPOSE 5000

# Start app
ENTRYPOINT ["dotnet", "API.dll"]

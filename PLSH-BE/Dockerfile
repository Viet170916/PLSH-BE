# Stage 1: Build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy toàn bộ source code
COPY . .

# Restore, build và publish
RUN dotnet restore PLSH-BE.sln
RUN dotnet build PLSH-BE.sln -c Release --no-restore
RUN dotnet publish API/API.csproj -c Release -o /app/publish --no-build

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy từ stage build
COPY --from=build /app/publish .

# Thiết lập biến môi trường nếu cần (tùy chọn)
# ENV ASPNETCORE_ENVIRONMENT=Production

# Port mặc định (bạn có thể đổi nếu app của bạn dùng port khác)
EXPOSE 5000

# Lệnh chạy ứng dụng
ENTRYPOINT ["dotnet", "API.dll"]

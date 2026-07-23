# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CourierDTS/CourierDTS.csproj", "CourierDTS/"]
RUN dotnet restore "CourierDTS/CourierDTS.csproj"

COPY . .
WORKDIR /src/CourierDTS
RUN dotnet publish -c Release -o /app/publish --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render (ve benzeri platformlar) PORT ortam değişkenini kendisi veriyor,
# yerelde Docker ile çalıştırırken de 8080'e düşüyor.
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet CourierDTS.dll"]

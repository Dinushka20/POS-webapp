# Stage 1 (build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["POS.Api.csproj", "./"]
RUN dotnet restore "./POS.Api.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "POS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2 (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "POS.Api.dll"]

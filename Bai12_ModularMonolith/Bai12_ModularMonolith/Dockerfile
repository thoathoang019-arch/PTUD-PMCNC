# syntax=docker/dockerfile:1
# Multi-stage build cho ECommerce.Api (.NET 10) — kế thừa Bài 6/7.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY . .
RUN dotnet restore src/ECommerce.Api/ECommerce.Api.csproj
RUN dotnet publish src/ECommerce.Api/ECommerce.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# User non-root (least privilege) — kế thừa Bài Linux/Docker
RUN groupadd -r appgrp && useradd -r -g appgrp -s /bin/false appuser
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
USER appuser
ENTRYPOINT ["dotnet", "ECommerce.Api.dll"]

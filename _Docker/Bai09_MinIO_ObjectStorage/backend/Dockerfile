# syntax=docker/dockerfile:1
# =====================================================================
# Dockerfile cho app backend cua nhom (OnlineShop - ASP.NET Core)
# Multi-stage build: tach STAGE BUILD (co SDK) va STAGE RUNTIME (nho gon)
#
# Cau truc Dockerfile co ban (theo yeu cau buoi 6):
#   FROM      - Chi dinh base image
#   WORKDIR   - Thiet lap thu muc lam viec
#   COPY      - Sao chep file/thu muc tu host vao image
#   RUN       - Thuc thi lenh trong qua trinh build image
#   EXPOSE    - Khai bao cong ma container se lang nghe
#   ENV       - Thiet lap bien moi truong (runtime - ton tai khi container chay)
#   ARG       - Thiet lap bien luc build (build-time - chi ton tai khi build image)
#   CMD       - Lenh mac dinh se chay khi container khoi dong (co the ghi de)
#   ENTRYPOINT- Lenh co dinh se chay khi container khoi dong (khong bi ghi de)
# =====================================================================

# ---------- STAGE 1: BUILD (image co .NET SDK de compile/publish) ----------

# FROM: chi dinh base image - o day la .NET SDK de co the compile code
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# ARG: bien chi ton tai luc build, dung de truyen tham so tu ben ngoai
#      vd: docker build --build-arg BUILD_CONFIGURATION=Debug ...
#      Khac voi ENV: ARG khong ton tai khi container chay.
ARG BUILD_CONFIGURATION=Release

# WORKDIR: thiet lap thu muc lam viec ben trong image
WORKDIR /src

# (1) COPY rieng cac file .csproj truoc -> tan dung cache layer:
#     chi restore lai khi file .csproj thay doi, khong phai moi lan sua code.
COPY nuget.config ./
COPY Shop.Business/Shop.Business.csproj          Shop.Business/
COPY Shop.DataAccess/Shop.DataAccess.csproj      Shop.DataAccess/
COPY Shop.Presentation/Shop.Presentation.csproj  Shop.Presentation/

# RUN: thuc thi lenh trong qua trinh build - o day la restore NuGet packages
RUN dotnet restore Shop.Presentation/Shop.Presentation.csproj

# (2) COPY toan bo ma nguon va publish ban Release (hoac Debug tuy ARG)
COPY . .
RUN dotnet publish Shop.Presentation/Shop.Presentation.csproj \
    -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

# ---------- STAGE 2: RUNTIME (chi ASP.NET runtime - KHONG mang theo SDK) ----------
# Multi-stage build: image cuoi chi co runtime, KHONG co SDK/ma nguon/cong cu build
# -> image nho hon nhieu, it lo hong bao mat, deploy nhanh.

# FROM: base image chi co ASP.NET runtime (khong co SDK)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# WORKDIR: thu muc chua ung dung trong container
WORKDIR /app

# Tao USER KHONG PHAI ROOT (lien he kien thuc phan quyen buoi Linux).
# Nguyen tac least privilege: neu app bi khai thac lo hong, hacker chi co quyen
# cua user thuong (appuser), khong the chiem quyen root cua he thong.
# (Image .NET 8+ da co san user 'app'; o day tu tao de minh hoa ky nang Linux.)
RUN groupadd -r appgrp \
 && useradd -r -g appgrp -s /bin/false appuser

# COPY ket qua publish tu stage build (chi lay /app/publish, khong lay ma nguon/SDK)
COPY --from=build /app/publish .

# ENV: bien moi truong (runtime) - cau hinh ung dung qua bien moi truong
#      thay vi hard-code, de de dang doi cau hinh giua cac moi truong (dev/test/prod).
#      Khi chay container, co the ghi de bang: docker run -e KEY=VALUE ...
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_ENVIRONMENT=Production

# EXPOSE: khai bao cong ma container se lang nghe.
# Luu y: EXPOSE chi la khai bao (documentation), khong tu mo cong ra ngoai.
# Muon truy cap tu may that phai dung: docker run -p <host_port>:<container_port>
# Dung cong 8080 (>1024) de user khong-root van bind duoc (cong <1024 can root).
EXPOSE 8080

# Chuyen sang user khong-root truoc khi chay ung dung
USER appuser

# ENTRYPOINT + CMD ket hop:
# - ENTRYPOINT: lenh co dinh, khong bi ghi de khi docker run (phan "chuong trinh")
# - CMD: tham so mac dinh, CO THE ghi de khi docker run (phan "doi so")
# Vi du: docker run onlineshop-api:v1              -> chay: dotnet Shop.Presentation.dll
#         docker run onlineshop-api:v1 --urls ...   -> chay: dotnet Shop.Presentation.dll --urls ...
ENTRYPOINT ["dotnet", "Shop.Presentation.dll"]
CMD []

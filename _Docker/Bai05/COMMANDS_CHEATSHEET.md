# Cheat-sheet Docker — Buổi 6 (cầm khi demo)

## Build & image
```
docker build -t onlineshop-api:v1 .    # build image tu Dockerfile (thu muc hien tai)
docker build --build-arg BUILD_CONFIGURATION=Debug -t onlineshop-api:debug .  # truyen ARG khi build
docker images                          # liet ke image (xem image vua tao)
docker rmi onlineshop-api:v1           # xoa image
docker image inspect onlineshop-api:v1 # xem chi tiet (size, layers...)
```

## Chạy & quản lý container
```
docker run -d --name onlineshop -p 8080:8080 onlineshop-api:v1   # -d chay nen, -p anh xa cong
docker run -d --name onlineshop -p 8080:8080 \
   -e ConnectionStrings__Default="Server=db;..." onlineshop-api:v1   # truyen ENV bang -e
docker run -d --name onlineshop -p 8080:8080 \
   --env-file .env.example onlineshop-api:v1                         # truyen ENV bang file
docker ps            # container dang chay
docker ps -a         # tat ca (ke ca da dung)
docker logs onlineshop            # xem log
docker logs -f onlineshop         # theo doi log realtime
docker exec -it onlineshop bash   # vao shell trong container
docker exec -it onlineshop whoami # kiem tra user (ket qua: appuser, khong phai root)
docker exec -it onlineshop ls /app  # chay 1 lenh trong container
docker stop onlineshop            # dung
docker start onlineshop           # chay lai
docker rm onlineshop              # xoa container (dung truoc)
docker rm -f onlineshop           # ep xoa (dung + xoa)
```

## Test API
```
curl http://localhost:8080/api/products
curl "http://localhost:8080/api/shipping/fee-preview?method=Express&itemsAmount=450000"
```

## Dọn dẹp
```
docker stop onlineshop            # dung container
docker rm onlineshop              # xoa container (phai dung truoc khi xoa)
docker rmi onlineshop-api:v1      # xoa image
docker system prune -f            # don rac (layers/cache khong dung)
```

## Phân biệt nhanh
```
# ENV vs ARG:
#   ENV  = bien runtime (ton tai khi container chay)  -> ghi de bang: docker run -e KEY=VALUE
#   ARG  = bien build-time (chi ton tai khi build)    -> truyen bang: docker build --build-arg KEY=VALUE

# ENTRYPOINT vs CMD:
#   ENTRYPOINT = lenh co dinh (khong bi ghi de)       -> "chuong trinh chinh"
#   CMD        = tham so mac dinh (co the ghi de)     -> "doi so truyen vao"
```

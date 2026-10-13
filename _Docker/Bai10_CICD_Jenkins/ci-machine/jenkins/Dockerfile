# ============================================================
# Dockerfile — Jenkins Custom Image — Bài 10
# ============================================================
# Jenkins chạy trong Docker, nhưng cần gọi được lệnh docker
# build/push trên máy host → mount docker.sock
#
# Image này thêm Docker CLI vào Jenkins LTS để pipeline
# có thể chạy: docker build, docker push, docker login
# ============================================================

FROM jenkins/jenkins:lts

# Chuyển sang root để cài package
USER root

# Cài Docker CLI (không cài Docker Engine — dùng của host qua socket)
RUN apt-get update && \
    apt-get install -y \
        apt-transport-https \
        ca-certificates \
        curl \
        gnupg \
        lsb-release && \
    curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg && \
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" > /etc/apt/sources.list.d/docker.list && \
    apt-get update && \
    apt-get install -y docker-ce-cli && \
    rm -rf /var/lib/apt/lists/*

# Cài .NET SDK (để Jenkins chạy dotnet test)
RUN curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0 --install-dir /usr/share/dotnet && \
    ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="${PATH}:/usr/share/dotnet"

# Thêm jenkins user vào docker group (để gọi docker không cần sudo)
RUN groupadd -f docker && usermod -aG docker jenkins

# Quay lại user jenkins
USER jenkins

# Cài plugins mặc định cần thiết
RUN jenkins-plugin-cli --plugins \
    git \
    workflow-aggregator \
    docker-workflow \
    credentials-binding \
    pipeline-stage-view \
    blueocean

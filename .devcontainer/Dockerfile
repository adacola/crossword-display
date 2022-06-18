FROM mcr.microsoft.com/dotnet/sdk:6.0-focal as devcontainer

RUN set -x \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
    && apt-get clean && rm -rf /var/cache/apt/archives/* /var/lib/apt/lists/*

RUN set -x \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        tzdata \
    && apt-get clean && rm -rf /var/cache/apt/archives/* /var/lib/apt/lists/* \
    && ln -sf /usr/share/zoneinfo/Asia/Tokyo /etc/localtime \
    && echo 'Asia/Tokyo' >/etc/timezone

# create vscode user
RUN set -x \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        sudo \
    && apt-get clean && rm -rf /var/cache/apt/archives/* /var/lib/apt/lists/* \
    && echo "ALL ALL=(ALL) NOPASSWD: ALL" >>/etc/sudoers.d/ALL \
    && groupadd \
        --gid 5000 \
        vscode \
    && useradd \
        --uid 5000 \
        --gid 5000 \
        --home-dir /home/vscode \
        --create-home \
        --shell /bin/bash \
        vscode

# vscode extensions cache
# https://code.visualstudio.com/docs/remote/containers-advanced#_avoiding-extension-reinstalls-on-container-rebuild
RUN set -x \
    && mkdir -p /home/vscode/.vscode-server/extensions \
    && mkdir -p /home/vscode/.vscode-server-insiders/extensions \
    && chown -R vscode:vscode /home/vscode/.vscode-server \
    && chown -R vscode:vscode /home/vscode/.vscode-server

# common tools
RUN set -x \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        bash-completion \
        curl \
        git \
        gnupg2 \
        iputils-ping \
        jq \
        less \
        net-tools \
        openssh-client \
        tar \
        time \
        unzip \
        zip \
    && apt-get clean && rm -rf /var/cache/apt/archives/* /var/lib/apt/lists/*

# docker
RUN set -x \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        apt-transport-https \
        ca-certificates \
        curl \
        gnupg-agent \
        software-properties-common \
    && curl -fsSL https://download.docker.com/linux/ubuntu/gpg | apt-key add - \
    && add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        docker-ce-cli \
    && apt-get clean && rm -rf /var/cache/apt/archives/* /var/lib/apt/lists/* \
    && groupadd --gid 9999 docker \
    && usermod -aG docker vscode

# docker compose
RUN set -x \
    && curl -fsSL "https://github.com/docker/compose/releases/download/1.27.4/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose \
    && chmod +x /usr/local/bin/docker-compose \
    && curl -fsSL -o /etc/bash_completion.d/docker-compose "https://raw.githubusercontent.com/docker/compose/1.27.4/contrib/completion/bash/docker-compose"

USER vscode

# setting.json for remote container server
COPY vscode-settings.json /home/vscode/.vscode-server/data/Machine/settings.json
COPY vscode-settings.json /home/vscode/.vscode-server-insiders/data/Machine/settings.json

ADD entrypoint.sh /entrypoint.sh
ENTRYPOINT [ "bash", "-eux", "/entrypoint.sh" ]

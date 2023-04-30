FROM bitnami/dotnet-sdk:7

COPY . /bitnami/

WORKDIR /bitnami/src

RUN dotnet build

ENTRYPOINT [ "/bin/sh", "-c", "dotnet", "run" ]

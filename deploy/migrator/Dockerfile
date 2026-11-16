# One-shot deployment helper; not a service. Build from source, no hard-coded passwords.
FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /src
COPY . .
RUN dotnet tool install --global dotnet-ef --version 9.0.23 \
    && dotnet restore Grotix-backend.sln \
    && dotnet build Grotix-backend.sln --configuration Release --no-restore
ENV PATH="/root/.dotnet/tools:${PATH}"
RUN chmod 755 /src/deploy/migrator/entrypoint.sh
ENTRYPOINT ["/src/deploy/migrator/entrypoint.sh"]

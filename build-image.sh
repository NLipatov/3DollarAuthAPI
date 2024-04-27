mkdir -p distro

rm -f ./distro/appsettings.Development.json

dotnet publish -r linux-x64 -o distro

cat <<EOL > distro/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY ./ ./
ENTRYPOINT ["dotnet", "AuthAPI.dll"]
EOL

sudo docker build -t auth-api distro

rm -rf distro
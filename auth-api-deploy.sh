#!/bin/bash

# Step 1: Pull the latest changes from Git
echo "INFO: Step 1: Pull the latest changes from Git."
git checkout dev
git reset --hard
git pull

echo "INFO: Step 1: updating submodules."
git submodule update --init

# Step 2: Move configuration
echo "INFO: Step 2: Copying configuration."
cp /root/EthaChat/Configuration/AuthAPI/appsettings.json /root/EthaChat/3DollarAuthAPI/AuthAPI/appsettings.json

# Step 3: If the 'distro' folder exists, delete it
echo "INFO: Step 3: If the 'distro' folder exists, delete it"
if [ -d "distro" ]; then
    rm -rf distro
fi

# Step 4: Stop and remove any existing container with the same image
echo "INFO: Step 4: Stop and remove any existing container with the same image"
EXISTING_CONTAINER=$(docker ps -q -f ancestor=auth-api)
if [ "$EXISTING_CONTAINER" ]; then
    docker stop "$EXISTING_CONTAINER"
    docker rm "$EXISTING_CONTAINER"
fi

# Step 5: Remove the old 'auth-api' Docker image
echo "INFO: Step 5: Remove the old 'auth-api' Docker image"
EXISTING_IMAGE=$(docker images -q auth-api)
if [ "$EXISTING_IMAGE" ]; then
    docker rmi "$EXISTING_IMAGE"
fi

# Step 6: Publish the .NET app to 'distro' folder
echo "INFO: Step 7: Publish the .NET app to 'distro' folder"
dotnet publish -r linux-x64 -o distro

# Step 7: Create a Dockerfile in 'distro' folder
echo "INFO: Step 8: Create a Dockerfile in 'distro' folder"
cat <<EOL > distro/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY ./ ./
ENTRYPOINT ["dotnet", "AuthAPI.dll"]
EOL

# Step 8: Build the Docker image 'auth-api'
echo "INFO: Step 9: Build the Docker image 'auth-api'"
docker build -t auth-api distro

# Step 9: Run the Docker container with the new image and restart on failure
echo "INFO: Step 10: Run the Docker container with the new image and restart on failure"
docker run -d --restart=always --network etha-chat --name auth-api -p 1000:443 -p 1001:80 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=1000 -e ASPNETCORE_Kestrel__Certificates__Default__Password="YourSecurePassword" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.pfx -v /root/devcert:/https/ auth-api

# Step 10: Remove the 'distro' folder after deployment
echo "INFO: Step 11: Remove the 'distro' folder after deployment"
rm -rf distro
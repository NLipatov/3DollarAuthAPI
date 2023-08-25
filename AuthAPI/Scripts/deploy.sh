#!/bin/bash

if [ $# -lt 1 ]; then
    echo "Usage: $0 <DB_connection_string>"
    exit 1
fi

# Step 1: Pull the latest changes from Git
echo "INFO: Step 1: Pull the latest changes from Git"
git pull

# Step 2: Update and initialize submodules
echo "INFO: Step 2: Update and initialize submodules"
git submodule update --init

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

# Step 6: Replace the database connection string in appsettings.json
echo "INFO: Step 6: Replace the database connection string in appsettings.json"
new_connection_string="$1"
sed -i "s/Host=79.137.202.134:5432;Username=postgres;Password=password;Database=postgres/$new_connection_string/" appsettings.json

# Step 7: Publish the .NET app to 'distro' folder
echo "INFO: Step 7: Publish the .NET app to 'distro' folder"
dotnet publish -r linux-x64 -o distro

# Step 8: Create a Dockerfile in 'distro' folder
echo "INFO: Step 8: Create a Dockerfile in 'distro' folder"
cat <<EOL > distro/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY ./ ./
ENTRYPOINT ["dotnet", "AuthAPI.dll"]
EOL

# Step 9: Build the Docker image 'auth-api'
echo "INFO: Step 9: Build the Docker image 'auth-api'"
docker build -t auth-api distro

# Step 10: Run the Docker container with the new image and restart on failure
echo "INFO: Step 10: Run the Docker container with the new image and restart on failure"
docker run -d --restart=always --network etha-chat --name auth-api -p 1000:443 -p 1001:80 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=1000 -e ASPNETCORE_Kestrel__Certificates__Default__Password="YourSecurePassword" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.pfx -v /root/devcert:/https/ auth-api

# Step 11: Remove the 'distro' folder after deployment
echo "INFO: Step 11: Remove the 'distro' folder after deployment"
rm -rf distro
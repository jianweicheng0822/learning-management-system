#!/bin/bash
set -e

# Configuration
DOMAIN="your-domain.com"
APP_DIR="/var/www/lms"
SERVICE_NAME="lms"
PROJECT_DIR="Learning Management System"

echo "=== LMS Deployment Script ==="

# Install dependencies
echo "[1/6] Installing dependencies..."
sudo apt-get update
sudo apt-get install -y nginx certbot python3-certbot-nginx

# Install .NET runtime if not present
if ! command -v dotnet &> /dev/null; then
    echo "[*] Installing .NET runtime..."
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --channel 10.0 --runtime aspnetcore
    rm dotnet-install.sh
fi

# Build and publish
echo "[2/6] Publishing application..."
dotnet publish "$PROJECT_DIR" -c Release -o "$APP_DIR"

# Set permissions
sudo chown -R www-data:www-data "$APP_DIR"

# Configure systemd
echo "[3/6] Configuring systemd service..."
sudo cp Deployment/lms.service /etc/systemd/system/${SERVICE_NAME}.service
sudo systemctl daemon-reload
sudo systemctl enable ${SERVICE_NAME}

# Configure nginx
echo "[4/6] Configuring nginx..."
sudo cp Deployment/nginx/lms.conf /etc/nginx/sites-available/${SERVICE_NAME}
sudo ln -sf /etc/nginx/sites-available/${SERVICE_NAME} /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx

# SSL certificate
echo "[5/6] Setting up SSL certificate..."
sudo certbot --nginx -d "$DOMAIN" --non-interactive --agree-tos --email admin@${DOMAIN}

# Start application
echo "[6/6] Starting application..."
sudo systemctl restart ${SERVICE_NAME}
sudo systemctl status ${SERVICE_NAME}

echo "=== Deployment complete ==="
echo "Application is running at https://${DOMAIN}"

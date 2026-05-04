# EC2 Deployment Guide — MusicShop

## Architecture Overview

```
Internet
   │
   ▼
┌──────────────────────────────────┐
│  EC2 Instance (Ubuntu 24.04)     │
│                                  │
│  ┌────────────────────────────┐  │
│  │ Host Nginx (reverse proxy) │  │  ← Port 80/443 (SSL termination)
│  │ + Certbot SSL              │  │
│  └──────┬─────────┬───────────┘  │
│         │         │              │
│    :5000 │    :3000│              │
│  ┌──────┴──┐ ┌────┴───────────┐  │
│  │ API     │ │ Frontend       │  │  ← Docker containers
│  │ .NET 10 │ │ Nginx (static) │  │
│  │ :8080   │ │ :80            │  │
│  └────┬────┘ └────────────────┘  │
│       │                          │
│  ┌────┴──────────────────┐       │
│  │ PostgreSQL (pgvector) │       │  ← Docker container, EBS-backed volume
│  │ :5432                 │       │
│  └───────────────────────┘       │
└──────────────────────────────────┘
```

> [!IMPORTANT]
> For production, strongly consider **Amazon RDS for PostgreSQL** instead of a containerized database. RDS handles backups, patching, replication, and failover automatically.

---

## Strategy Decision

| Approach | Pros | Cons |
|----------|------|------|
| **Docker Compose on EC2** (recommended for your case) | Simple, matches your existing `docker-compose.yml`, single command deploy | No auto-scaling, single point of failure |
| **Bare-metal (systemd + Nginx)** | Slightly less overhead | Manual .NET runtime management, no container isolation |
| **ECS/Fargate** | Auto-scaling, managed infra | More AWS knowledge required, higher complexity |

This guide covers **Docker Compose on EC2** — the fastest path given your existing Dockerfiles.

---

## Phase 1: EC2 Instance Setup

### 1.1 Launch Instance

1. Go to **AWS Console → EC2 → Launch Instance**
2. Configure:

| Setting | Value |
|---------|-------|
| **Name** | `catmusicshop-prod` |
| **AMI** | Ubuntu Server 24.04 LTS (HVM, SSD) |
| **Instance type** | `t3.small` (2 vCPU, 2 GB RAM) — minimum for .NET + PG + Node build |
| **Key pair** | Create new or select existing `.pem` key |
| **Storage** | 30 GB gp3 (EBS) |

> [!WARNING]
> `t2.micro` / `t3.micro` (1 GB RAM) will OOM during Docker build of the .NET project. Use `t3.small` minimum. You can downgrade after build if needed.

### 1.2 Security Group Rules

| Type | Protocol | Port | Source | Description |
|------|----------|------|--------|-------------|
| SSH | TCP | 22 | **Your IP only** (`x.x.x.x/32`) | Admin access |
| HTTP | TCP | 80 | `0.0.0.0/0` | Web traffic (Standard) |
| HTTPS | TCP | 443 | `0.0.0.0/0` | Web traffic (SSL) |

> [!CAUTION]
> Never open port 22 to `0.0.0.0/0`. Restrict to your IP or use AWS Systems Manager Session Manager instead.

### 1.3 Allocate Elastic IP

1. **EC2 → Elastic IPs → Allocate**
2. **Associate** with your instance
3. This ensures a stable public IP that survives instance stop/start

---

## Phase 2: Server Provisioning

### 2.1 Connect via SSH

```bash
chmod 400 your-key.pem
ssh -i your-key.pem ubuntu@3.107.13.5
```

### 2.2 System Update

```bash
sudo apt update && sudo apt upgrade -y
```

### 2.3 Install Docker & Docker Compose

```bash
# Install dependencies
sudo apt install -y apt-transport-https ca-certificates curl software-properties-common

# Add Docker GPG key
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

# Add Docker repository
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker Engine + Compose plugin
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Allow running Docker without sudo
sudo usermod -aG docker $USER

# Apply group change (or log out and back in)
newgrp docker

# Verify
docker --version
docker compose version
```

---

## Phase 3: Deploy Application

### 3.1 Transfer Project to EC2

**Option A — Git clone (recommended):**
```bash
# Install git if not present
sudo apt install -y git

# Clone your repo
git clone https://github.com/<your-username>/CatMusicShop.git
cd CatMusicShop/MusicShop
```

**Option B — SCP from local machine:**
```bash
# From your Windows machine (PowerShell)
scp -i your-key.pem -r D:\CatMusicShop\MusicShop ubuntu@3.107.13.5:~/MusicShop
```

### 3.2 Create `.env`

```bash
cd ~/CatMusicShop/MusicShop   # or ~/MusicShop depending on method
nano .env
```

```env
# Database
POSTGRES_USER=catmusicshop_user
POSTGRES_PASSWORD=<GENERATE-STRONG-PASSWORD-HERE>
POSTGRES_DB=CatMusicShopDb

# API Configuration
ASPNETCORE_ENVIRONMENT=Production
DB_CONNECTION_STRING=Host=postgres;Database=CatMusicShopDb;Username=catmusicshop_user;Password=<SAME-STRONG-PASSWORD>

# JWT Settings (MUST be unique, 64+ chars for production)
JWT_SECRET=<GENERATE-64-CHAR-RANDOM-STRING>
JWT_ISSUER=MusicShop
JWT_AUDIENCE=MusicShop

# AWS S3 Settings
AWS_BUCKET=your-actual-bucket-name
AWS_REGION=ap-southeast-2
AWS_CDN=https://your-cloudfront-domain.cloudfront.net

# Third Party
GOOGLE_CLIENT_ID=your-real-google-client-id
STRIPE_PUBLISHABLE_KEY=pk_live_xxx
STRIPE_SECRET_KEY=sk_live_xxx
STRIPE_WEBHOOK_SECRET=whsec_xxx

# Email
EMAIL_USER=your-email@gmail.com
EMAIL_PASS=your-gmail-app-password
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
EMAIL_FROM=MusicShop
```

> [!CAUTION]
> Generate the JWT secret with: `openssl rand -base64 64`
> Generate the DB password with: `openssl rand -base64 32`
> Never commit `.env` to Git.

### 3.3 Update `docker-compose.yml` for Production

Create a production override file:

```bash
nano docker-compose.prod.yml
```

```yaml
services:
  postgres:
    image: ankane/pgvector:latest
    container_name: catmusicshop-db
    restart: always
    ports:
      - "127.0.0.1:5432:5432"      # Bind to localhost only
    environment:
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: ${POSTGRES_DB}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - catmusicshop-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5

  catmusicshop-api:
    build:
      context: ./src
      dockerfile: MusicShop.API/Dockerfile
    container_name: catmusicshop-api
    restart: always
    ports:
      - "127.0.0.1:5000:8080"      # Bind to localhost only (Nginx handles SSL)
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - AWS__BucketName=${AWS_BUCKET}
      - AWS__Region=${AWS_REGION}
      - AWS__CdnBaseUrl=${AWS_CDN}
      - JwtSettings__Secret=${JWT_SECRET}
      - JwtSettings__Issuer=${JWT_ISSUER}
      - JwtSettings__Audience=${JWT_AUDIENCE}
      - GoogleSettings__ClientId=${GOOGLE_CLIENT_ID}
      - Stripe__SecretKey=${STRIPE_SECRET_KEY}
      - Stripe__WebhookSecret=${STRIPE_WEBHOOK_SECRET}
      - EmailSettings__Email=${EMAIL_USER}
      - EmailSettings__Password=${EMAIL_PASS}
      - EmailSettings__Host=${EMAIL_HOST}
      - EmailSettings__Port=${EMAIL_PORT}
      - EmailSettings__FromName=${EMAIL_FROM}
    depends_on:
      postgres:
        condition: service_healthy
    networks:
      - catmusicshop-network

  catmusicshop-web:
    build:
      context: ./src/musicshop-web
      dockerfile: Dockerfile
      args:
        - VITE_API_URL=https://catmusicshop.duckdns.org/api/v1
        - VITE_GOOGLE_CLIENT_ID=${GOOGLE_CLIENT_ID}
    container_name: catmusicshop-web
    restart: always
    ports:
      - "127.0.0.1:3000:80"        # Bind to localhost only (Nginx handles SSL)
    depends_on:
      - catmusicshop-api
    networks:
      - catmusicshop-network

volumes:
  postgres_data:

networks:
  catmusicshop-network:
    driver: bridge
```

Key production changes vs your current `docker-compose.yml`:
- All ports bound to `0.0.0.0` (Direct access via IP)
- `restart: always` on all services
- Health check on PostgreSQL
- `depends_on` with `condition: service_healthy`
- `VITE_API_URL` points to your DuckDNS domain (`https://catmusicshop.duckdns.org`)

### 3.4 Build & Start

```bash
# Use compose
docker compose -f docker-compose.prod.yml up -d --build

# Verify all containers are running
docker compose -f docker-compose.prod.yml ps

# Check logs
docker logs catmusicshop-api --tail 50
docker logs catmusicshop-db --tail 20
docker logs catmusicshop-web --tail 20
```

---

## Phase 4: Nginx Reverse Proxy + SSL

### 4.1 Install Nginx on Host

```bash
sudo apt update
sudo apt install -y nginx
sudo systemctl enable nginx
```

### 4.2 Configure Nginx

```bash
sudo nano /etc/nginx/sites-available/catmusicshop
```

```nginx
# Redirect HTTP to HTTPS (after SSL is configured)
server {
    listen 80;
    server_name catmusicshop.duckdns.org;

    # API proxy
    location /api/ {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 300s;

        # Large file uploads
        client_max_body_size 20M;
    }

    # Frontend (React SPA)
    location / {
        proxy_pass http://127.0.0.1:3000;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
# Enable the site
sudo ln -s /etc/nginx/sites-available/catmusicshop /etc/nginx/sites-enabled/
sudo rm /etc/nginx/sites-enabled/default

# Test config
sudo nginx -t

# For initial setup (before SSL), you can temporarily comment out the 443 block
# or just run Certbot immediately.
sudo systemctl restart nginx
```

### 4.3 Install SSL with Certbot

```bash
# Install Certbot
sudo apt install snapd -y
sudo snap install core && sudo snap refresh core
sudo snap install --classic certbot
sudo ln -s /snap/bin/certbot /usr/bin/certbot

# Obtain certificate (auto-configures Nginx)
sudo certbot --nginx -d catmusicshop.duckdns.org

# Test auto-renewal
sudo certbot renew --dry-run
```

Certbot will automatically:
- Obtain the SSL certificate
- Update your Nginx config with `ssl_certificate` paths
- Set up auto-renewal via systemd timer

---

## Phase 5: Code Changes Required

### 5.1 Update CORS Policy in `Program.cs`

Your current CORS only allows `localhost` origins. Add your production domain:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://catmusicshop.duckdns.org"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
```

### 5.2 Configure Forwarded Headers

Since Nginx terminates SSL and proxies to Kestrel, you need to configure forwarded headers. Add to `Program.cs` **before** `app.UseHttpsRedirection()`:

```csharp
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});
```

### 5.3 Swagger in Production

Your current config only exposes Swagger in Development. If you want it accessible in production (behind authentication), update accordingly. If not, the current behavior is correct and secure.

### 5.4 Frontend API URL

The `VITE_API_URL` build arg in `docker-compose.prod.yml` must match your production domain:
```
VITE_API_URL=https://catmusicshop.duckdns.org/api/v1
```

---

## Phase 6: Production Checklist

### Security
- [ ] `.env` not committed to Git (add to `.gitignore`)
- [ ] Strong JWT secret (64+ chars, generated randomly)
- [ ] Strong PostgreSQL password
- [ ] SSH restricted to your IP in Security Group
- [ ] Ports 5000/3000/5432 bound to `127.0.0.1` (not exposed publicly)
- [ ] Swagger disabled in Production (current behavior)
- [ ] Hangfire dashboard IP-restricted in Nginx config
- [ ] HTTPS enabled with auto-renewal

### Reliability
- [ ] `restart: always` on all Docker services
- [ ] Health check on PostgreSQL
- [ ] Elastic IP allocated (stable public IP)
- [ ] Database backups configured (see below)

### Monitoring
- [ ] Application logs visible via `docker logs catmusicshop-api`
- [ ] Set up AWS CloudWatch agent (optional)
- [ ] Set up AWS Budget alarm to avoid surprise charges

---

## Phase 7: Maintenance Commands

### Deploy Updates

```bash
cd ~/CatMusicShop/MusicShop

# Pull latest code
git pull origin master 

# Rebuild and restart
docker compose -f docker-compose.prod.yml up -d --build

# Check status
docker compose -f docker-compose.prod.yml ps
```

### Database Backup

```bash
# Manual backup
docker exec musicshop-db pg_dump -U catmusicshop_user CatMusicShopDb > backup_$(date +%Y%m%d_%H%M%S).sql

# Restore
cat backup_20260503.sql | docker exec -i musicshop-db psql -U catmusicshop_user CatMusicShopDb
```

### Automated Daily Backup (cron)

```bash
# Create backup script
mkdir -p ~/backups
cat << 'EOF' > ~/backup-db.sh
#!/bin/bash
BACKUP_DIR=~/backups
FILENAME="CatMusicShopDb_$(date +%Y%m%d_%H%M%S).sql.gz"
docker exec musicshop-db pg_dump -U catmusicshop_user CatMusicShopDb | gzip > "$BACKUP_DIR/$FILENAME"

# Keep only last 7 days
find "$BACKUP_DIR" -name "CatMusicShopDb_*.sql.gz" -mtime +7 -delete
echo "Backup created: $FILENAME"
EOF

chmod +x ~/backup-db.sh

# Schedule daily at 3 AM
(crontab -l 2>/dev/null; echo "0 3 * * * /home/ubuntu/backup-db.sh >> /home/ubuntu/backups/backup.log 2>&1") | crontab -
```

### View Logs

```bash
# All containers
docker compose -f docker-compose.prod.yml logs --tail 100

# Specific service
docker logs catmusicshop-api --tail 50 -f     # follow mode
docker logs catmusicshop-db --tail 50

# Nginx (host)
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

### Restart Services

```bash
# Restart all
docker compose -f docker-compose.prod.yml restart

# Restart single service
docker compose -f docker-compose.prod.yml restart catmusicshop-api

# Full rebuild (after code changes)
docker compose -f docker-compose.prod.yml up -d --build --force-recreate
```

---

## Phase 8: Optional CI/CD with GitHub Actions

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to EC2

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Deploy via SSH
        uses: appleboy/ssh-action@v1.2.2
        with:
          host: ${{ secrets.EC2_HOST }}
          username: ubuntu
          key: ${{ secrets.EC2_SSH_KEY }}
          script: |
            cd ~/CatMusicShop/MusicShop
            git pull origin main
            docker compose -f docker-compose.prod.yml up -d --build
            docker image prune -f
```

**GitHub Secrets required:**
| Secret | Value |
|--------|-------|
| `EC2_HOST` | Your Elastic IP |
| `EC2_SSH_KEY` | Contents of your `.pem` private key |

---

## Quick-Start Summary (TL;DR)

```bash
# 1. SSH into EC2
ssh -i key.pem ubuntu@<IP>

# 2. Install Docker
# (run commands from Phase 2.3)

# 3. Clone repo
git clone https://github.com/<user>/CatMusicShop.git
cd CatMusicShop/MusicShop

# 4. Create .env
# (see Phase 3.2)

# 5. Create docker-compose.prod.yml
# (see Phase 3.3)

# 6. Build & run
docker compose -f docker-compose.prod.yml up -d --build

# 7. Install Nginx + SSL
# (see Phase 4)

# 8. Point domain DNS to Elastic IP
# 9. Run Certbot
```

---

## Cost Estimate (Monthly)

| Resource | Spec | Estimated Cost |
|----------|------|----------------|
| EC2 `t3.small` | 2 vCPU, 2 GB | ~$15/month |
| EBS | 30 GB gp3 | ~$2.40/month |
| Elastic IP | (while attached) | Free |
| Data transfer | First 100 GB/month | ~$9/month |
| **Total** | | **~$26/month** |

> [!TIP]
> If you're still in the AWS Free Tier period (account < 12 months old), `t3.micro` is free for 750 hrs/month. You can use it for low-traffic staging — just be aware that .NET builds may OOM on 1 GB RAM. Build locally or in CI, push images to ECR, and pull on EC2 instead.

# Script para iniciar os serviços do Docker Compose

# Força o rebuild das imagens e inicia os contêineres em modo detached
docker-compose up --build -d

Write-Host "Serviços iniciados em background. Use 'docker-compose logs -f' para ver os logs."


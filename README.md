# Arquitectura Hexagonal con CQRS y Mediator (Net core 9)

## 1. Nivel de Contexto
Los usuarios interactúan con la API mediante solicitudes HTTP.

## 2. Nivel de Contenedor
### API (Capa de Presentación)
- Expone endpoints HTTP.
- Aplica políticas de autenticación y autorización(Jwt ó keycloak).
- Maneja excepciones, middlewares y filtros.

### Application (Capa de Aplicación)
- Implementa CQRS con Mediator.
- Contiene los comandos y queries que transforman la informacion en dtos por medio de mapper.
- Aplica validaciones por medio de behaviours y fluentValidation.
- Define interfaces para la logica de negocio (Alfresco, Hangfire, Kafka, MinIO, RabbitMQ, Twillio).

### Core (Capa de Dominio)
- Contiene Dtos y constantes bases para la configuracion del sistema.
- Contiene las entidades de negocio.
- Contiene mensajes centralizados.
- Define interfaces transversales (Repository, UnitOfWork, CircuitBreaker, Crypto, CurrentUser, Jwt, Redis, SecretVault, Settings).
- Maneja excepciones y enums.
- Extensiones transversales.

### Infrastructure (Capa de Infraestructura)
- Maneja acceso a la base de datos implementando Repository, UnitOfWork, Contex y Configuracion de entidades.
- Maneja extenciones para el accedo de datos.
- Implementa los servicios de interfaces definidas en la capa Application y Core.
- Gestiona configuración sensible del sistema por medio de secretos.
- Conecta integraciones externas del sistema.

### Base de Datos
- Sql server.
- Persiste entidades definidas en Core.
- Accedida a través de Repository y UnitOfWork en Infrastructure.

## 3. Nivel de Componente (Ejemplo de Flujo de una Consulta)
1. El usuario envía una solicitud HTTP a la API.
2. La API delega la solicitud a Application mediante un Query.
3. Application usa Mediator para manejar la Query.
4. Mediator ejecuta un handler que consulta el Repository de UnitOfWork en Infrastructure.
5. Accede a la Base de Datos y devuelve los datos.
6. La API responde al usuario con los datos obtenidos.

## 4. Integraciones Externas
- **Vault**: Gestión segura de configuración.
- **MinIO**: Almacenamiento de archivos.
- **ElasticSearch**: Logging y búsqueda.
- **Redis**: Caching.
- **Alfresco**: Gestión documental.
- **Hangfire**: Tareas en segundo plano.
- **Kafka**: Mensajería y eventos.
- **RabbitMQ**: Cola de mensajes.
- **Twilio**: Notificaciones y mensajería.
- **CircuitBreaker**: Tolerancia a fallos en servicios externos.
- **Jwt**: Autenticación con JSON Web Tokens.
- **SecretVault**: Lectura segura de secretos.


# Docker
## DockerFile
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln .
COPY Api/*.csproj Api/
COPY Application/*.csproj Application/
COPY Core/*.csproj Core/
COPY Infrastructure/*.csproj Infrastructure/

RUN dotnet restore

COPY . .

RUN dotnet publish Api/Api.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /out ./

ENV ASPNETCORE_ENVIRONMENT=dev
EXPOSE 80

ENTRYPOINT ["dotnet", "Api.dll"]
```
## Docker Compose
```yml
version: "3.8"

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "80:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=dev
      - DOTNET_USE_POLLING_FILE_WATCHER=1
    networks:
      - backend
    volumes:
      - ./logs:/app/logs

networks:
  backend:
    driver: bridge
```
## ELK en Docker
```sh
#!/bin/sh

# Definir variables
NETWORK_NAME="elastic_network"
ES_CONTAINER_NAME="elasticsearch"
KB_CONTAINER_NAME="kibana"
ES_IMAGE="docker.elastic.co/elasticsearch/elasticsearch:8.9.1"
KB_IMAGE="docker.elastic.co/kibana/kibana:8.9.1"
ES_PORT_HTTP="9200"
ES_PORT_TCP="9300"
KB_PORT="5601"
VOLUME_NAME="elasticsearch_data"

if ! docker network ls | grep -q $NETWORK_NAME; then
  echo "Creando red Docker '$NETWORK_NAME'..."
  docker network create -d bridge $NETWORK_NAME
fi

if ! docker volume ls | grep -q $VOLUME_NAME; then
  echo "Creando volumen Docker '$VOLUME_NAME'..."
  docker volume create $VOLUME_NAME
fi

echo "Crear y ejecutar el contenedor de Elasticsearch..."
docker run -d \
  --name "$ES_CONTAINER_NAME" \
  --network "$NETWORK_NAME" \
  -e discovery.type=single-node \
  -e xpack.security.enabled=false \
  -e bootstrap.memory_lock=true \
  -e ES_JAVA_OPTS="-Xms512m -Xmx512m" \
  --ulimit memlock=-1:-1 \
  -v "$VOLUME_NAME:/usr/share/elasticsearch/data" \
  -p "$ES_PORT_HTTP:$ES_PORT_HTTP" \
  -p "$ES_PORT_TCP:$ES_PORT_TCP" \
  "$ES_IMAGE"

echo "Esperando a que Elasticsearch esté listo..."
until curl -s http://localhost:"$ES_PORT_HTTP" > /dev/null; do
  sleep 1
done
echo "Elasticsearch está listo."

echo "Crear y ejecutar el contenedor de Kibana..."
docker run -d \
  --name "$KB_CONTAINER_NAME" \
  --network "$NETWORK_NAME" \
  -e ELASTICSEARCH_HOSTS=http://"$ES_CONTAINER_NAME":"$ES_PORT_HTTP" \
  -p "$KB_PORT:$KB_PORT" \
  --link "$ES_CONTAINER_NAME" \
  "$KB_IMAGE"

echo "Contenedores creados."
```
## MinIO en Docker
```sh
##!/bin/bash

echo "Creando parámetros de configuración..."
CONTAINER_NAME=minio
MINIO_DATA=minio_data
MINIO_CONFIG=minio_config
NETWORK_NAME=minio_network
MINIO_PORT_CONSOLE=9001
MINIO_PORT_API=9000
MINIO_ROOT_USER=minioadmin
MINIO_ROOT_PASSWORD=minioadmin

if ! docker network ls | grep -q $NETWORK_NAME; then
  echo "Creando red Docker '$NETWORK_NAME'..."
  docker network create $NETWORK_NAME
fi

if ! docker volume ls | grep -q $MINIO_DATA; then
  echo "Creando volumen Docker '$MINIO_DATA'..."
  docker volume create $MINIO_DATA

  echo "Establecer los permisos del volumen '$MINIO_DATA'..."
  docker run --rm -v $MINIO_DATA:/data busybox sh -c "chmod -R 777 /data"
fi

if ! docker volume ls | grep -q $MINIO_CONFIG; then
  echo "Creando volumen Docker '$MINIO_CONFIG'..."
  docker volume create $MINIO_CONFIG

  echo "Establecer los permisos del volumen '$MINIO_CONFIG'..."
  docker run --rm -v $MINIO_CONFIG:/root/.minio busybox sh -c "chmod -R 777 /root/.minio"
fi

echo "Crear y ejecutar el contenedor de MinIO..."
docker run -d --name $CONTAINER_NAME --network $NETWORK_NAME \
  -p $MINIO_PORT_API:9000 -p $MINIO_PORT_CONSOLE:9001 \
  -e "MINIO_ROOT_USER=$MINIO_ROOT_USER" \
  -e "MINIO_ROOT_PASSWORD=$MINIO_ROOT_PASSWORD" \
  -v $MINIO_DATA:/data \
  -v $MINIO_CONFIG:/root/.minio \
  minio/minio server /data --console-address ":9001"

  echo "Contenedor creado."
```
## RedisCache en Docker
```sh
###!/bin/bash

echo "Creando parámetros de configuración..."
PORT=6379
CONTAINER_NAME=redis
REDIS_DATA=redis_data
NETWORK_NAME=redis_network

if ! docker network ls | grep -q $NETWORK_NAME; then
  echo "Creando red Docker '$NETWORK_NAME'..."
  docker network create -d bridge $NETWORK_NAME
fi

if ! docker volume ls | grep -q $REDIS_DATA; then
  echo "Creando volumen Docker '$REDIS_DATA'..."
  docker volume create $REDIS_DATA
fi

echo "Crear y ejecutar el contenedor de Redis..."
docker run -d --name $CONTAINER_NAME \
  --network $NETWORK_NAME \
  -p $PORT:6379 \
  -v ./configuration/redis.conf:/usr/local/etc/redis/redis.conf \
  -v $REDIS_DATA:/data:rw \
  redis:latest redis-server /usr/local/etc/redis/redis.conf

echo "Contenedor creado."
```
### redis redis.conf
```conf
# Configurar el usuario developer con contraseña y permisos completos
user developer on >D3v3l0p3r ~* +@all

# Desactivar el usuario por defecto (ejecutar este comando en la consola de Redis si es necesario)
# redis-cli acl setuser default on >default ~* -@all

# Límite de memoria para Redis (256MB)
maxmemory 256mb

# Política de expulsión de claves cuando se alcance el límite de memoria
maxmemory-policy allkeys-lru
```
## Sql Server en Docker
```sh
####!/bin/bash

echo "Creando parámetros de configuración..."
ACCEPT_EULA=Y
SA_PASSWORD=S0yUn4C0ntr4sen1a
PORT=1433
CONTAINER_NAME=sqlserver
EXTENSIBILITY_VOLUME=sqlserver_extensibility_volume
EXTENSIBILITY_DATA_VOLUME=sqlserver_extensibility_data_volume
EXTENSIBILITY_LOG_VOLUME=sqlserver_extensibility_log_volume
NETWORK_NAME=sqlserver_network

# Crear la red Docker si no existe
if ! docker network ls | grep -q $NETWORK_NAME; then
  echo "Creando red Docker '$NETWORK_NAME'..."
  docker network create $NETWORK_NAME
fi

# Crear volúmenes si no existen
if ! docker volume ls | grep -q $EXTENSIBILITY_VOLUME; then
  echo "Creando volumen Docker '$EXTENSIBILITY_VOLUME'..."
  docker volume create $EXTENSIBILITY_VOLUME
fi

if ! docker volume ls | grep -q $EXTENSIBILITY_DATA_VOLUME; then
  echo "Creando volumen Docker '$EXTENSIBILITY_DATA_VOLUME'..."
  docker volume create $EXTENSIBILITY_DATA_VOLUME
fi

if ! docker volume ls | grep -q $EXTENSIBILITY_LOG_VOLUME; then
  echo "Creando volumen Docker '$EXTENSIBILITY_LOG_VOLUME'..."
  docker volume create $EXTENSIBILITY_LOG_VOLUME
fi

echo "Crear y ejecutar el contenedor de SQL Server con Health Check..."

# Ejecutar el contenedor con health check
docker run -e "ACCEPT_EULA=$ACCEPT_EULA" -e "SA_PASSWORD=$SA_PASSWORD" \
  -p $PORT:1433 --name $CONTAINER_NAME \
  -v $EXTENSIBILITY_VOLUME:/var/opt/mssql-extensibility \
  -v $EXTENSIBILITY_DATA_VOLUME:/var/opt/mssql-extensibility/data \
  -v $EXTENSIBILITY_LOG_VOLUME:/var/opt/mssql-extensibility/log \
  --network $NETWORK_NAME \
  --health-interval=10s \
  --health-timeout=5s \
  --health-retries=5 \
  -d mcr.microsoft.com/azure-sql-edge

echo "Contenedor creado."
```
## Vault en Docker
```sh
#####!/bin/bash

echo "Creando parámetros de configuración..."
PORT=8200
CONTAINER_NAME=vault
VAULT_DATA=vault_data
VAULT_FILE=vault_file
VAULT_LOGS=vault_logs
NETWORK_NAME=vault_network
VAULT_ADDR=http://127.0.0.1:8200
USERNAME="developer"
PASSWORD="d3v3l0p3r"
POLICY_NAME="admin-policy"

if ! docker network ls | grep -q $NETWORK_NAME; then
  echo "Creando red Docker '$NETWORK_NAME'..."
  docker network create $NETWORK_NAME
fi

if ! docker volume ls | grep -q $VAULT_DATA; then
  echo "Creando volumen Docker '$VAULT_DATA'..."
  docker volume create $VAULT_DATA
  docker run --rm -v $VAULT_DATA:/vault/data busybox sh -c "chmod -R 777 /vault/data"
fi

if ! docker volume ls | grep -q $VAULT_FILE; then
  echo "Creando volumen Docker '$VAULT_FILE'..."
  docker volume create $VAULT_FILE
  docker run --rm -v $VAULT_FILE:/vault/file busybox sh -c "chmod -R 777 /vault/file"
fi

if ! docker volume ls | grep -q $VAULT_LOGS; then
  echo "Creando volumen Docker '$VAULT_LOGS'..."
  docker volume create $VAULT_LOGS
  docker run --rm -v $VAULT_LOGS:/vault/logs busybox sh -c "chmod -R 777 /vault/logs"
fi

echo "Crear y ejecutar el contenedor de Vault..."
docker run -d --name $CONTAINER_NAME --network $NETWORK_NAME \
  -p $PORT:8200 \
  --cap-add IPC_LOCK \
  -e "VAULT_ADDR=$VAULT_ADDR" \
  -v $VAULT_DATA:/vault/data:rw \
  -v $VAULT_FILE:/vault/file:rw \
  -v $VAULT_LOGS:/vault/logs:rw \
  hashicorp/vault server

echo "Contenedor creado."

echo "Copiando configuración de Vault al volumen..."
docker cp configuration/vault.hcl $CONTAINER_NAME:/vault/config/vault.hcl

echo "Copiando política de Vault..."
docker cp configuration/admin-policy.hcl $CONTAINER_NAME:/vault/data/admin-policy.hcl

echo "Esperando a que Vault se inicie..."
sleep 20

vault_status=$(docker exec $CONTAINER_NAME vault status 2>&1)
if echo "$vault_status" | grep -q "Vault is already initialized"; then
  echo "Vault ya está inicializado. Procediendo sin inicializar ni desempaquetar."
else
  echo "Vault no está inicializado. Inicializando Vault..."
  init_output=$(docker exec $CONTAINER_NAME vault operator init -format=json)

  unseal_keys=$(echo $init_output | jq -r '.unseal_keys_b64[]')
  root_token=$(echo $init_output | jq -r '.root_token')

  echo "Guardando claves de unseal y token raíz en unseal/unseal.txt..."
  echo "Unseal Keys:" > unseal/unseal.txt
  for key in $unseal_keys; do
    echo $key >> unseal/unseal.txt
  done
  echo "Root Token: $root_token" >> unseal/unseal.txt

  echo "Vault inicializado exitosamente. Claves de unseal y token raíz guardadas en unseal/unseal.txt."

  echo "Desempaquetando Vault..."
  for key in $unseal_keys; do
    docker exec $CONTAINER_NAME vault operator unseal $key
  done
fi

echo "Iniciando sesión en Vault con el token raíz..."
docker exec -it $CONTAINER_NAME vault login $(cat unseal/unseal.txt | grep 'Root Token' | awk '{print $3}')

echo "Asignando política a Vault..."
docker exec $CONTAINER_NAME vault policy write $POLICY_NAME /vault/data/admin-policy.hcl

echo "Creando usuario $USERNAME..."
docker exec $CONTAINER_NAME vault auth enable userpass

docker exec $CONTAINER_NAME vault write auth/userpass/users/$USERNAME password=$PASSWORD policies=$POLICY_NAME

kv_status=$(docker exec $CONTAINER_NAME vault secrets list | grep -q 'kv/')
if [ $? -ne 0 ]; then
  echo "Motor de secretos KV no habilitado. Habilitándolo..."
  docker exec -it $CONTAINER_NAME vault secrets enable -version=1 kv
else
  echo "Motor de secretos KV ya está habilitado."
fi

echo "Configuración completada. Puedes iniciar sesión con el usuario '$USERNAME' y la contraseña '$PASSWORD'."
```
### vault admin-policy.hcl
```hcl
# Permitir todos los secretos bajo el path kv/*
path "kv/*" {
  capabilities = ["read", "list", "create", "update", "delete"]
}
```
### vault vault.hcl
```hcl
storage "file" {
  path = "/vault/data"
}

listener "tcp" {
  address     = "0.0.0.0:8200"
  tls_disable = 1
}

disable_mlock = true

api_addr = "http://127.0.0.1:8200"
cluster_addr = "https://127.0.0.1:8201"
ui = true
```
### vault unseal.sh
```sh
###!/bin/bash

FILE="unseal.txt"

echo "leyendo claves de desellado..."
unseal_keys=()
found_unseal_keys=false
while IFS= read -r line; do
  if $found_unseal_keys; then
    unseal_keys+=("$line")
    if [[ ${#unseal_keys[@]} -eq 3 ]]; then
      break
    fi
  elif [[ $line == "Unseal Keys:" ]]; then
    found_unseal_keys=true
  fi
done < "$FILE"

if [[ ${#unseal_keys[@]} -ne 3 ]]; then
  echo "No se encontraron tres claves de desellado en $FILE."
  exit 1
fi

echo "Desempaquetando Vault..."
for key in "${unseal_keys[@]}"; do
  echo "Ejecutando: vault operator unseal $key"
  docker exec vault vault operator unseal "$key"
done

echo "Vault desempaquetado con éxito."
```
## Kafka en Docker
```sh
###!/bin/bash

echo "Creando parámetros de configuración..."
KAFKA_CONTAINER_NAME=kafka
ZOOKEEPER_CONTAINER_NAME=zookeeper
KAFKA_PORT=9093
ZOOKEEPER_PORT=2181
NETWORK_NAME=kafka_network
KAFKA_VOLUME=kafka_data
ZOOKEEPER_DATA_VOLUME=zookeeper_data
ZOOKEEPER_DATALOG_VOLUME=zookeeper_datalog
ZOOKEEPER_TRANSACTION_VOLUME=zookeeper_transaction
TOPIC_NAME=test-topic

if ! docker volume ls | grep -q $KAFKA_VOLUME; then
  echo "Creando volumen Docker '$KAFKA_VOLUME'..."
  docker volume create $KAFKA_VOLUME
fi

if ! docker volume ls | grep -q $ZOOKEEPER_DATA_VOLUME; then
  echo "Creando volumen Docker '$ZOOKEEPER_DATA_VOLUME'..."
  docker volume create $ZOOKEEPER_DATA_VOLUME
fi

if ! docker volume ls | grep -q $ZOOKEEPER_DATALOG_VOLUME; then
  echo "Creando volumen Docker '$ZOOKEEPER_DATALOG_VOLUME'..."
  docker volume create $ZOOKEEPER_DATALOG_VOLUME
fi

if ! docker volume ls | grep -q $ZOOKEEPER_TRANSACTION_VOLUME; then
  echo "Creando volumen Docker '$ZOOKEEPER_TRANSACTION_VOLUME'..."
  docker volume create $ZOOKEEPER_TRANSACTION_VOLUME
fi

if ! docker network ls | grep -q $NETWORK_NAME; then
  echo "Creando red Docker '$NETWORK_NAME'..."
  docker network create $NETWORK_NAME
fi

echo "Creando y ejecutando el contenedor de Zookeeper..."
docker run -d --name $ZOOKEEPER_CONTAINER_NAME \
  --network $NETWORK_NAME \
  -p $ZOOKEEPER_PORT:$ZOOKEEPER_PORT \
  -v $ZOOKEEPER_DATA_VOLUME:/data \
  -v $ZOOKEEPER_DATALOG_VOLUME:/datalog \
  -v $ZOOKEEPER_TRANSACTION_VOLUME:/transaction \
  zookeeper:3.7

echo "Creando y ejecutando el contenedor de Kafka..."
docker run -d --name $KAFKA_CONTAINER_NAME \
  --network $NETWORK_NAME \
  -p $KAFKA_PORT:$KAFKA_PORT \
  -e KAFKA_ZOOKEEPER_CONNECT=$ZOOKEEPER_CONTAINER_NAME:2181 \
  -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9093 \
  -e KAFKA_LISTENERS=PLAINTEXT://0.0.0.0:9093 \
  -e KAFKA_INTER_BROKER_LISTENER_NAME=PLAINTEXT \
  -v $KAFKA_VOLUME:/kafka \
  wurstmeister/kafka

echo "Esperando a que los contenedores Kafka y Zookeeper estén listos..."
sleep 10  # Espera de 10 segundos para permitir que los contenedores se inicien correctamente

echo "Creando Topic '$TOPIC_NAME' en Kafka..."
docker exec -it $KAFKA_CONTAINER_NAME kafka-topics.sh --create --topic $TOPIC_NAME --bootstrap-server kafka:9093 --partitions 1 --replication-factor 1

echo "Contenedores Kafka y Zookeeper creados y en ejecución."
echo "Topic '$TOPIC_NAME' creado."

echo "Si quieres balancear la carga, usa el mismo group.id en los consumidores."
echo "Si quieres que todos reciban los mensajes completos, usa diferentes group.id."
```
## Alfresco en Docker
```sh
#!/bin/bash
set -e # Detener el script ante cualquier error

# 1. Verificar prerrequisitos
echo "Verificando prerrequisitos..."
if ! command -v docker &> /dev/null; then
    echo "Docker no está instalado. Descárgalo desde: https://www.docker.com/"
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "docker-compose no está instalado. Instálalo primero."
    exit 1
fi

# 2. Clonar repositorio oficial
REPO_DIR="acs-deployment"
if [ -d "$REPO_DIR" ]; then
    echo "El directorio $REPO_DIR ya existe. Actualizando..."
    cd $REPO_DIR
    git pull
    cd -
else
    echo "Clonando repositorio oficial..."
    git clone https://github.com/Alfresco/acs-deployment.git $REPO_DIR
fi

# 3. Configurar versión estable
COMPOSE_FILE="community-compose.yaml"
cd $REPO_DIR/docker-compose

# 4. Verificar memoria disponible
MEMORY_LIMIT=$(docker info --format '{{json .MemTotal}}')
if [ $MEMORY_LIMIT -lt 12000000000 ]; then
    echo "Asigna al menos 12GB de RAM a Docker (Actual: $((MEMORY_LIMIT/1000000000))GB)"
    exit 1
fi

# 5. Iniciar servicios
echo "Iniciando Alfresco Community..."
docker-compose -f $COMPOSE_FILE up -d

# 6. Monitorear progreso
echo "Espera 5-7 minutos mientras todos los servicios se inician..."
sleep 60
docker-compose -f $COMPOSE_FILE logs -f alfresco | grep -q "Startup of 'Transformers' subsystem, ID"

# 7. Mostrar información de acceso
echo -e "\n\n Instalación completada!"
echo "========================================"
echo "URLs de acceso:"
echo "- Alfresco Repository: http://localhost:8080/alfresco"
echo "- Share:              http://localhost:8080/share"
echo "- Admin Console:      http://localhost:8080/alfresco/s/enterprise/admin/admin-systemsummary"
echo "- Credenciales:       admin / admin"
echo "========================================"
echo "Para Solr Admin: Instala ModHeader y configura:"
echo "Header: X-Alfresco-Search-Secret = secret"
echo "URL: http://localhost:8083/solr"
```
## Keycloak en Docker
```sh
#!/bin/bash

echo "Creando parámetros de configuración..."
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin
PORT=8180  # Puerto expuesto en la máquina host
CONTAINER_NAME=keycloak
VOLUME_NAME=keycloak_data
NETWORK_NAME=keycloak_network
IMAGE_NAME=quay.io/keycloak/keycloak:latest

# Crear la red Docker si no existe
if ! docker network ls | grep -q $NETWORK_NAME; then
  echo "Creando red Docker '$NETWORK_NAME'..."
  docker network create $NETWORK_NAME
fi

# Crear volumen si no existe
if ! docker volume ls | grep -q $VOLUME_NAME; then
  echo "Creando volumen Docker '$VOLUME_NAME'..."
  docker volume create $VOLUME_NAME
fi

echo "Creando y ejecutando el contenedor de Keycloak con persistencia y health check..."

# Ejecutar el contenedor con health check
docker run -e "KEYCLOAK_ADMIN=$KEYCLOAK_ADMIN" -e "KEYCLOAK_ADMIN_PASSWORD=$KEYCLOAK_ADMIN_PASSWORD" \
  -p $PORT:8080 --name $CONTAINER_NAME \
  -v $VOLUME_NAME:/opt/keycloak/data \
  --network $NETWORK_NAME \
  --health-cmd="curl --fail http://localhost:$PORT/health || exit 1" \
  --health-interval=10s \
  --health-timeout=5s \
  --health-retries=5 \
  -d $IMAGE_NAME start-dev

echo "Contenedor de Keycloak creado y en ejecución en el puerto $PORT."
```
## RabbitMQ en Docker
```sh
#!/bin/bash

echo "Creando parámetros de configuración para RabbitMQ..."

PORT=5672
MANAGEMENT_PORT=15672
CONTAINER_NAME=rabbitmq
NETWORK_NAME=rabbitmq_network
RABBITMQ_DATA=rabbitmq_data
USERNAME="admin"
PASSWORD="admin123"

# Crear red de Docker si no existe
if ! docker network ls | grep -q $NETWORK_NAME; then
  echo "Creando red Docker '$NETWORK_NAME'..."
  docker network create $NETWORK_NAME
fi

# Crear volumen para los datos de RabbitMQ si no existe
if ! docker volume ls | grep -q $RABBITMQ_DATA; then
  echo "Creando volumen Docker '$RABBITMQ_DATA'..."
  docker volume create $RABBITMQ_DATA
fi

# Verificar si el contenedor ya está corriendo
if docker ps -a | grep -q $CONTAINER_NAME; then
  echo "El contenedor '$CONTAINER_NAME' ya existe. Eliminándolo..."
  docker rm -f $CONTAINER_NAME
fi

echo "Creando y ejecutando el contenedor RabbitMQ..."
docker run -d --name $CONTAINER_NAME --network $NETWORK_NAME \
  -p $PORT:5672 \
  -p $MANAGEMENT_PORT:15672 \
  -e RABBITMQ_DEFAULT_USER=$USERNAME \
  -e RABBITMQ_DEFAULT_PASS=$PASSWORD \
  -v $RABBITMQ_DATA:/var/lib/rabbitmq \
  rabbitmq:management

# Verificar si el contenedor se ejecutó correctamente
if [ $? -eq 0 ]; then
  echo "RabbitMQ se ha iniciado exitosamente."
  echo "Puedes acceder al panel de administración en: http://localhost:$MANAGEMENT_PORT"
  echo "Usuario: $USERNAME"
  echo "Contraseña: $PASSWORD"
else
  echo "Hubo un error al iniciar RabbitMQ. Verifica los logs del contenedor."
fi
```

# Datos vaults en KV v1
```json
{
    "Keycloak":
    {
      "Audience": "netcore",
      "Authority": "http://localhost:8180/realms/master",
      "RequireHttpsMetadata": false
    }
    ,
    "Alfresco":{
      "Password": "admin",
      "Uri": "http://localhost:8080",
      "Username": "admin"
    }
    ,
    "CircuitBreaker": {
        "FailCount": 5,
        "FailTime": 60,
        "RetryCount": 3,
        "RetryTime": 2,
        "Timeout": 1000
    }
    ,
    "Crypto": {
        "Key": "9F3Al0wU6q6Q1vBT3XJdCw+L5gYbv5GZL/P7eY7u+Fg="
    }
    ,
    "DataBase": {
        "ConnectionString": "Data Source=127.0.0.1,1433;Initial Catalog=Sample;Persist Security Info=False;User ID=sa;Password=S0yUn4C0ntr4sen1a;TrustServerCertificate=True"
    }
    ,
    "ElasticSearch": {
        "IndexFormat": "logs-{0:yyyy.MM.dd}",
        "Url": "http://localhost:9200"
    }
    ,
    "Jwt": {
        "Audience": "SampleClient",
        "ExpirationTime": 3600,
        "Issuer": "SampleApi",
        "SecretKey": "4bcaknljpxE7R01H6d7V#ww6dj6Eg@f6"
    }
    ,
    "MinIO": {
        "AccessKey": "minioadmin",
        "BucketName": "sample",
        "Endpoint": "http://localhost:9000",
        "FileSizeLimit": "1000MB",
        "SecretKey": "minioadmin"
    }
    ,
    "Redis": {
        "AllowAdmin": true,
        "ConnectionString": "localhost:6379",
        "DataBase": 0,
        "MaxLimitBytes": 244140625,
        "MemoryUsagePercentage": 80,
        "Password": "D3v3l0p3r",
        "User": "developer"
    }
    ,
    "Kafka": {
      "BootstrapServers": "localhost:9093",
      "Topic": "test-topic",
      "GroupId": "consumer-group-1"
    }
    ,
    "Serilog": {
        "Enrich": [
        "FromLogContext",
        "WithMachineName"
        ],
        "MinimumLevel": "Information",
        "Properties": {
        "Application": "Sample",
        "Environment": "Development"
        },
        "Using": [
        "Serilog.Sinks.Elasticsearch",
        "Serilog.Sinks.Console"
        ],
        "WriteTo": [
        {
            "Args": {
            "AutoRegisterTemplate": true,
            "BatchSizeLimit": 100,
            "ConnectionTimeout": 5,
            "IndexFormat": "logs-{0:yyyy.MM.dd}",
            "NodeUris": [
              "http://localhost:9200"
            ],
            "NumberOfReplicas": 1,
            "NumberOfShards": 1,
            "TemplateName": "sample-template",
            "TypeName": "_doc"
            },
            "Name": "Elasticsearch"
        },
        {
            "Name": "Console"
        }
        ]
    }
}
```

# Pruebas y accesos de servicios
## Kibana 
```sh
#enviar un mensaje a kafka:
docker exec -it kafka kafka-console-producer.sh --broker-list localhost:9093 --topic test-topic

#recibir mensaje kafka (se debe abrir otra ventana de consola):
docker exec -it kafka kafka-console-consumer.sh --bootstrap-server localhost:9093 --topic test-topic --group test-group --from-beginning
```
## Redis 
```sh
#guardar un valor en redis:
docker exec -it redis-container redis-cli SET mi_clave "Hola Redis"

#en otra ventana de consola, escribe lo siguiente y espera el mensaje:
docker exec -it kafka kafka-console-consumer.sh --bootstrap-server localhost:9093 --topic test-topic --group test-group --from-beginning
```
## MinIO 
```txt
ingresar a la ruta configurada en Docker.sh con su correspondiente usuario y contraseña

http://localhost:9001/
user: minioadmin
password: minioadmin

y crear el bucket con el mismo nombre que se configuro en el vault : sample

el nombre del bucket siempre debe ser en minuscula
```
## ELK (ElasticSearch Kibana) 
```txt
ingresar a kibana por medio de la url configurada

http://localhost:5601

en el menu dirigirse a log y ahi se visualizara toda la informacion que el sistema pueda capturar.
```

# DDL Base de datos
```sql
CREATE TABLE dbo.[User] (
    Id INT IDENTITY(1,1) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Password NVARCHAR(100) NOT NULL,
    RegistrationDate DATETIME DEFAULT GETDATE() NOT NULL,
    Active BIT DEFAULT 1 NOT NULL,
    PhotoName NVARCHAR(1000) NOT NULL,
    CONSTRAINT Pk_User PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT Chk_User_Email_Format CHECK (Email LIKE '%_@_%._%'),
    CONSTRAINT Uq_User_Email UNIQUE (Email),
    CONSTRAINT Uq_User_PhotoName UNIQUE (PhotoName)
);
CREATE INDEX Ix_User_Email ON dbo.[User](Email);
CREATE INDEX Ix_User_PhotoName ON dbo.[User](PhotoName);

CREATE TABLE dbo.Role (
    Id INT IDENTITY(1,1) NOT NULL,
    Description NVARCHAR(50) NOT NULL,
    CONSTRAINT Pk_Role PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT Uq_Role_Description UNIQUE (Description)
);

CREATE TABLE dbo.UserRole (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CONSTRAINT Pk_UserRole PRIMARY KEY CLUSTERED (UserId, RoleId),
    CONSTRAINT Fk_UserRole_User FOREIGN KEY (UserId) REFERENCES [dbo].[User](Id) ON DELETE CASCADE,
    CONSTRAINT Fk_UserRole_Role FOREIGN KEY (RoleId) REFERENCES dbo.Role(Id) ON DELETE CASCADE
);
CREATE INDEX Ix_UserRole_UserId ON dbo.UserRole(UserId);
```
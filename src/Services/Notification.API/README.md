# Notification API

Serviço principal do sistema MultiChannel Notification System. Responsável por processar e gerenciar notificações multi-canal.

## 🎯 Responsabilidades

- Criação e gerenciamento de notificações
- Integração com Azure Cosmos DB para persistência
- Integração com Azure Service Bus para processamento assíncrono
- Suporte a múltiplos canais (Email, SMS, Push)
- Controle de status e tentativas de envio

## 🛠️ Tecnologias

- .NET 8
- ASP.NET Core Web API
- Azure Cosmos DB (NoSQL)
- Azure Service Bus (Mensageria)
- Serilog para logging
- Swagger/OpenAPI para documentação

## 📋 Endpoints

### Notificações
- `POST /api/notification` - Cria uma nova notificação
- `GET /api/notification/{id}?userId={userId}` - Busca notificação por ID
- `GET /api/notification/user/{userId}` - Lista notificações do usuário
- `POST /api/notification/{id}/process` - Processa notificação manualmente
- `DELETE /api/notification/{id}?userId={userId}` - Cancela notificação
- `GET /api/notification/health` - Health check do serviço

## 🚀 Como Executar

### Desenvolvimento Local

```bash
dotnet run
```

A API estará disponível em `http://localhost:5001`

### Docker

```bash
docker build -t notification-api .
docker run -p 5001:80 notification-api
```

## ⚙️ Configuração

### Cosmos DB
```json
{
  "ConnectionStrings": {
    "CosmosDB": "AccountEndpoint=https://your-account.documents.azure.com:443/;AccountKey=your-key"
  },
  "CosmosDB": {
    "DatabaseName": "NotificationSystem",
    "ContainerName": "Notifications"
  }
}
```

### Service Bus
```json
{
  "ConnectionStrings": {
    "ServiceBus": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key"
  },
  "ServiceBus": {
    "QueueName": "notification-queue"
  }
}
```

## 📊 Modelo de Dados

### NotificationModel
```json
{
  "id": "guid",
  "userId": "string",
  "title": "string",
  "message": "string",
  "channels": ["email", "sms", "push"],
  "status": "Pending|Processing|Sent|Failed|Cancelled",
  "createdAt": "datetime",
  "updatedAt": "datetime",
  "metadata": {},
  "attempts": 0,
  "lastError": "string"
}
```

## 🔄 Fluxo de Processamento

1. **Criação**: Notificação é criada via API
2. **Persistência**: Salva no Cosmos DB com status "Pending"
3. **Fila**: Mensagem enviada para Service Bus
4. **Processamento**: Worker processa a notificação
5. **Envio**: Notificação enviada pelos canais especificados
6. **Atualização**: Status atualizado para "Sent" ou "Failed"

## 🏗️ Arquitetura

```
Controller → Service → Repository → Cosmos DB
     ↓
MessageQueue → Service Bus
```

## 📈 Princípios SOLID Aplicados

- **Single Responsibility**: Cada classe tem uma responsabilidade específica
- **Open/Closed**: Extensível para novos canais de notificação
- **Liskov Substitution**: Interfaces bem definidas
- **Interface Segregation**: Contratos específicos (INotificationService, IMessageQueueService)
- **Dependency Inversion**: Injeção de dependência em todos os serviços 
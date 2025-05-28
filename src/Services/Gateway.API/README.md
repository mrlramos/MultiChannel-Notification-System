# Gateway API

API Gateway do sistema MultiChannel Notification System. Serve como ponto único de entrada para todas as requisições do sistema.

## 🎯 Responsabilidades

- Roteamento de requisições para os microserviços apropriados
- Autenticação e autorização (a ser implementado)
- Rate limiting (a ser implementado)
- Logging centralizado
- Health checks dos serviços

## 🛠️ Tecnologias

- .NET 8
- ASP.NET Core Web API
- Serilog para logging
- Swagger/OpenAPI para documentação

## 📋 Endpoints

### Notificações
- `POST /api/notification/send` - Envia uma notificação
- `GET /api/notification/user/{userId}/preferences` - Busca preferências do usuário

## 🚀 Como Executar

### Desenvolvimento Local

```bash
dotnet run
```

A API estará disponível em `http://localhost:5000`

### Docker

```bash
docker build -t gateway-api .
docker run -p 5000:80 gateway-api
```

## ⚙️ Configuração

As configurações estão no arquivo `appsettings.json`:

- `Services:NotificationAPI` - URL da API de Notificações
- `Services:SubscriptionAPI` - URL da API de Subscrições

## 📊 Health Check

Acesse `/health` para verificar o status do serviço. 
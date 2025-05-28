# Subscription API

Serviço de gerenciamento de preferências e subscrições do sistema MultiChannel Notification System. Responsável por controlar as preferências de notificação dos usuários.

## 🎯 Responsabilidades

- Gerenciamento de subscrições de usuários
- Controle de preferências de canais (Email, SMS, Push)
- Configuração de categorias de notificação
- Gerenciamento de horários de silêncio (Quiet Hours)
- Validação de permissões para envio de notificações

## 🛠️ Tecnologias

- .NET 8
- ASP.NET Core Web API
- Azure Cosmos DB (NoSQL)
- Serilog para logging
- Swagger/OpenAPI para documentação

## 📋 Endpoints

### Subscrições
- `POST /api/subscription` - Cria uma nova subscrição
- `GET /api/subscription/user/{userId}` - Busca subscrição por usuário
- `PUT /api/subscription/user/{userId}` - Atualiza subscrição
- `DELETE /api/subscription/user/{userId}` - Remove subscrição

### Preferências
- `GET /api/subscription/user/{userId}/channels` - Lista canais preferidos
- `GET /api/subscription/user/{userId}/channels/{channel}/enabled` - Verifica se canal está habilitado
- `GET /api/subscription/user/{userId}/categories/{category}/enabled` - Verifica se categoria está habilitada
- `GET /api/subscription/user/{userId}/quiet-hours` - Verifica se está em horário de silêncio

### Utilitários
- `GET /api/subscription/health` - Health check do serviço

## 🚀 Como Executar

### Desenvolvimento Local

```bash
dotnet run
```

A API estará disponível em `http://localhost:5002`

### Docker

```bash
docker build -t subscription-api .
docker run -p 5002:80 subscription-api
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
    "ContainerName": "Subscriptions"
  }
}
```

## 📊 Modelo de Dados

### SubscriptionModel
```json
{
  "id": "guid",
  "userId": "string",
  "email": "string",
  "phoneNumber": "string",
  "preferences": {
    "channels": {
      "email": true,
      "sms": false,
      "push": true
    },
    "categories": {
      "marketing": false,
      "transactional": true,
      "security": true,
      "system": true
    },
    "quietHours": {
      "enabled": false,
      "startTime": "22:00",
      "endTime": "08:00",
      "timezone": "UTC"
    },
    "frequency": "Immediate"
  },
  "isActive": true,
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

## 🔄 Fluxo de Uso

1. **Criação**: Usuário cria subscrição com preferências iniciais
2. **Consulta**: Outros serviços consultam preferências antes de enviar notificações
3. **Validação**: Sistema verifica canais habilitados, categorias permitidas e horários
4. **Atualização**: Usuário pode modificar suas preferências a qualquer momento

## 🏗️ Arquitetura

```
Controller → Service → Repository → Cosmos DB
```

## 📈 Funcionalidades Avançadas

### Horários de Silêncio
- Configuração de horários onde notificações não devem ser enviadas
- Suporte a diferentes fusos horários
- Validação automática antes do envio

### Categorias de Notificação
- **Marketing**: Promoções e campanhas
- **Transactional**: Confirmações e recibos
- **Security**: Alertas de segurança
- **System**: Notificações do sistema

### Canais Múltiplos
- **Email**: Sempre habilitado por padrão
- **SMS**: Requer número de telefone
- **Push**: Para aplicações móveis

## 🔒 Validações

- UserId obrigatório para todas as operações
- Email ou telefone obrigatório para canais específicos
- Validação de formato de horários
- Prevenção de subscrições duplicadas

## 📈 Princípios SOLID Aplicados

- **Single Responsibility**: Cada classe tem responsabilidade específica
- **Open/Closed**: Extensível para novos canais e categorias
- **Liskov Substitution**: Interfaces bem definidas
- **Interface Segregation**: Contratos específicos por funcionalidade
- **Dependency Inversion**: Injeção de dependência em todos os serviços 
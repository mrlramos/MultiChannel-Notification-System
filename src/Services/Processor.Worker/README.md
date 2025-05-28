# Processor Worker

Worker Service responsável pelo processamento assíncrono de notificações do sistema MultiChannel Notification System. Consome mensagens do Azure Service Bus e processa notificações através de múltiplos provedores.

## 🎯 Responsabilidades

- Consumo de mensagens do Azure Service Bus
- Processamento assíncrono de notificações
- Integração com provedores externos (Email, SMS, Push)
- Validação de preferências de usuários
- Atualização de status das notificações
- Implementação de retry policies e circuit breakers
- Tratamento de Dead Letter Queue (DLQ)

## 🛠️ Tecnologias

- .NET 8 Worker Service
- Azure Service Bus para mensageria
- Serilog para logging estruturado
- Polly para resilência (retry, circuit breaker)
- HttpClient para comunicação com APIs
- Simulação de provedores externos

## 🏗️ Arquitetura

```
Service Bus → Worker → Processor → Providers → External Services
                ↓
            Subscription API (validação)
                ↓
            Notification API (status update)
```

## 📋 Componentes

### Workers
- **NotificationWorker**: Worker principal que consome mensagens do Service Bus

### Services
- **INotificationProcessor**: Interface para processamento de notificações
- **NotificationProcessor**: Implementação do processador principal

### Providers
- **INotificationProvider**: Interface base para provedores
- **EmailProvider**: Provedor de email (simulado)
- **SmsProvider**: Provedor de SMS (simulado)
- **PushProvider**: Provedor de push notifications (simulado)

### Models
- **NotificationMessage**: Modelo da mensagem de notificação
- **ProcessingResult**: Resultado do processamento
- **NotificationPriority**: Enum de prioridades

## 🚀 Como Executar

### Desenvolvimento Local

```bash
dotnet run
```

### Docker

```bash
docker build -t processor-worker .
docker run processor-worker
```

### Modo Simulação

Se não houver Service Bus configurado, o worker entra em **modo simulação** e gera notificações de teste automaticamente a cada 30-60 segundos.

## ⚙️ Configuração

### Service Bus
```json
{
  "ConnectionStrings": {
    "ServiceBus": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key"
  },
  "ServiceBus": {
    "QueueName": "notifications",
    "MaxConcurrentCalls": 5
  }
}
```

### APIs Externas
```json
{
  "Services": {
    "NotificationAPI": "http://notification-api",
    "SubscriptionAPI": "http://subscription-api"
  }
}
```

## 🔄 Fluxo de Processamento

1. **Recebimento**: Mensagem recebida do Service Bus
2. **Deserialização**: Conversão para `NotificationMessage`
3. **Validação**: Verificação de preferências via Subscription API
4. **Processamento**: Envio através do provedor apropriado
5. **Atualização**: Status atualizado via Notification API
6. **Confirmação**: Mensagem confirmada ou enviada para DLQ

## 📊 Validações Implementadas

### Validações Básicas
- Campos obrigatórios (ID, UserId, Channel)
- Canal suportado (email, sms, push)

### Validações de Preferências
- Canal habilitado para o usuário
- Categoria permitida
- Horário de silêncio (exceto notificações críticas)

### Validações de Provedor
- Health check do provedor
- Validação de dados específicos (email, telefone, device token)

## 🔧 Funcionalidades Avançadas

### Retry Policy
- 3 tentativas com exponential backoff
- Baseado no `DeliveryCount` da mensagem
- Dead Letter Queue após esgotar tentativas

### Circuit Breaker
- Proteção contra falhas em cascata
- 5 falhas consecutivas abrem o circuito
- 30 segundos de pausa antes de tentar novamente

### Simulação de Provedores
- **Email**: Simula envio com latência realística
- **SMS**: Validação de telefone e truncamento de mensagem
- **Push**: Validação de device token e dados adicionais
- **Falhas**: Simulação de falhas ocasionais (2-5%)

## 📈 Monitoramento

### Logs Estruturados
- Serilog com template customizado
- Níveis apropriados por componente
- Correlação de mensagens

### Health Checks
- Endpoint `/health` (se habilitado)
- Verificação de conectividade com APIs
- Status dos provedores

### Métricas
- Tempo de processamento
- Taxa de sucesso/falha
- Throughput de mensagens

## 🎭 Modo Simulação

Quando não há Service Bus configurado:

```
🎭 Modo simulação ativado - gerando notificações de teste...
🎭 Processando notificação simulada abc123...
✅ Sucesso - Notificação simulada abc123: Processada com sucesso
```

Gera notificações aleatórias com:
- Usuários: user1, user2, user3, user4, user5
- Canais: email, sms, push
- Categorias: marketing, transactional, security, system
- Prioridades: Low, Normal, High, Critical

## 🔒 Tratamento de Erros

### Erros Temporários
- Retry automático com backoff
- Circuit breaker para proteção

### Erros Permanentes
- Envio direto para Dead Letter Queue
- Log detalhado do erro

### Erros de Validação
- Notificação marcada como "failed"
- Não consome tentativas de retry

## 📈 Princípios SOLID Aplicados

- **Single Responsibility**: Cada provedor responsável por um canal
- **Open/Closed**: Extensível para novos provedores
- **Liskov Substitution**: Provedores intercambiáveis
- **Interface Segregation**: Interfaces específicas por tipo
- **Dependency Inversion**: Injeção de dependência em todos os níveis

## 🚀 Próximos Passos

1. **Integração Real**: Substituir simulação por provedores reais
2. **Métricas**: Implementar Application Insights
3. **Scaling**: Configurar auto-scaling baseado em fila
4. **Scheduling**: Implementar processamento de notificações agendadas
5. **Templates**: Sistema de templates para notificações 
# 🧪 Guia de Testes - Collection Insomnia

Este guia explica como usar a collection do Insomnia para testar todos os endpoints do **MultiChannel Notification System**.

## 📥 Como Importar a Collection

1. **Abra o Insomnia**
2. **Clique em "Import/Export"** (ou use `Ctrl+Shift+I`)
3. **Selecione "Import Data"**
4. **Escolha "From File"**
5. **Selecione o arquivo** `insomnia-collection.json`
6. **Clique em "Import"**

## 🚀 Preparando o Ambiente

### 1. Subir os Containers
```bash
docker-compose up -d
```

### 2. Verificar se os Serviços Estão Rodando
```bash
docker-compose ps
```

Você deve ver 4 containers rodando:
- `gateway-api` (porta 5000)
- `notification-api` (porta 5001)
- `subscription-api` (porta 5002)
- `processor-worker`

## 🔧 Configuração das Variáveis

A collection já vem com as variáveis configuradas:

- `base_url_gateway`: `http://localhost:5000`
- `base_url_notification`: `http://localhost:5001`
- `base_url_subscription`: `http://localhost:5002`
- `notification_id`: `coloque_aqui_o_id_da_notificacao`

## 📋 Fluxo de Testes Recomendado

### 1. **Health Checks** (Verificar se tudo está funcionando)
Execute na ordem:
1. `Gateway API > Gateway - Health Check`
2. `Notification API > Health Check`
3. `Subscription API > Health Check`

**Resultado esperado**: Status 200 com `"Healthy"`

### 2. **Criar Subscrição de Usuário**
Execute: `Subscription API > Criar Subscrição`

**Payload incluído**:
```json
{
  "userId": "user123",
  "preferences": {
    "channels": {
      "email": {
        "enabled": true,
        "address": "user@example.com"
      },
      "sms": {
        "enabled": true,
        "phoneNumber": "+5511999999999"
      },
      "push": {
        "enabled": true,
        "deviceToken": "device_token_123"
      }
    },
    "categories": {
      "marketing": {
        "enabled": true,
        "frequency": "Daily"
      },
      "transactional": {
        "enabled": true,
        "frequency": "Immediate"
      }
    },
    "quietHours": {
      "enabled": true,
      "startTime": "22:00",
      "endTime": "08:00",
      "timezone": "America/Sao_Paulo"
    }
  }
}
```

### 3. **Verificar Subscrição Criada**
Execute: `Subscription API > Buscar Subscrição`

### 4. **Criar Notificação**
Execute: `Notification API > Criar Notificação`

**Payload incluído**:
```json
{
  "userId": "user123",
  "title": "Promoção Especial",
  "message": "Aproveite 50% de desconto em todos os produtos!",
  "channels": ["Email", "SMS"],
  "category": "Marketing",
  "priority": "Medium",
  "metadata": {
    "discount_code": "SAVE50",
    "valid_until": "2024-02-15"
  }
}
```

**⚠️ IMPORTANTE**: Copie o `id` retornado na resposta!

### 5. **Atualizar Variável notification_id**
1. No Insomnia, vá em **Environments**
2. Edite o **Base Environment**
3. Cole o ID da notificação na variável `notification_id`

### 6. **Testar Outros Endpoints**
Agora você pode testar:
- `Notification API > Buscar Notificação por ID`
- `Notification API > Listar Notificações do Usuário`
- `Notification API > Processar Notificação`
- `Subscription API > Validar Preferências`

## 🎯 Cenários de Teste Específicos

### Cenário 1: Notificação via Gateway
Execute: `Gateway API > Gateway - Criar Notificação`
- Testa o roteamento através do API Gateway

### Cenário 2: Atualizar Preferências
Execute: `Subscription API > Atualizar Subscrição`
- Testa a atualização de preferências do usuário

### Cenário 3: Validação de Preferências
Execute: `Subscription API > Validar Preferências`
- Testa se o usuário pode receber determinado tipo de notificação

### Cenário 4: Cancelar Notificação
Execute: `Notification API > Cancelar Notificação`
- Testa o cancelamento de uma notificação pendente

## 🔍 Monitoramento dos Logs

Para acompanhar o processamento em tempo real:

```bash
# Ver logs de todos os serviços
docker-compose logs -f

# Ver logs específicos
docker-compose logs -f notification-api
docker-compose logs -f processor-worker
```

## 📊 Exemplos de Respostas

### Notificação Criada com Sucesso
```json
{
  "id": "67890abc-def1-2345-6789-0abcdef12345",
  "userId": "user123",
  "title": "Promoção Especial",
  "message": "Aproveite 50% de desconto em todos os produtos!",
  "channels": ["Email", "SMS"],
  "category": "Marketing",
  "priority": "Medium",
  "status": "Pending",
  "createdAt": "2024-01-15T10:30:00Z",
  "metadata": {
    "discount_code": "SAVE50",
    "valid_until": "2024-02-15"
  }
}
```

### Validação de Preferências
```json
{
  "canReceive": true,
  "reason": "User has enabled Email notifications for Marketing category",
  "channelEnabled": true,
  "categoryEnabled": true,
  "withinQuietHours": false
}
```

## 🚨 Troubleshooting

### Erro 500 - Internal Server Error
- Verifique se todos os containers estão rodando
- Verifique os logs: `docker-compose logs -f`

### Erro 404 - Not Found
- Verifique se as URLs estão corretas
- Confirme se os serviços estão nas portas certas

### Notificação não encontrada
- Verifique se você atualizou a variável `notification_id`
- Confirme se a notificação foi criada com sucesso

### Subscrição não encontrada
- Crie uma subscrição primeiro antes de testar outros endpoints
- Verifique se o `userId` está correto

## 💡 Dicas Avançadas

1. **Use variáveis**: Aproveite as variáveis de ambiente para facilitar os testes
2. **Organize por cenários**: Crie folders adicionais para cenários específicos
3. **Salve respostas**: Use o recurso de "Response History" do Insomnia
4. **Teste em sequência**: Use o "Request Chaining" para automatizar fluxos

## 🎉 Pronto para Testar!

Agora você tem uma collection completa para testar todos os aspectos do sistema de notificações multi-canal. Divirta-se explorando as funcionalidades! 🚀 
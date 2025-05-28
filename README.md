# MultiChannel Notification System

Sistema moderno de notificações multi-canal baseado em microserviços, desenvolvido com .NET 8, Azure Cosmos DB, Azure Service Bus e práticas de Clean Architecture.

## 🎯 Visão Geral

Este projeto demonstra a implementação de um sistema de notificações escalável e resiliente, seguindo os princípios SOLID e padrões de microserviços. O sistema suporta múltiplos canais de notificação (Email, SMS, Push) com processamento assíncrono e gerenciamento avançado de preferências de usuários.

## 🎯 **Destaques da Arquitetura**

- ✅ **API Gateway Completo**: Ponto único de entrada com todos os endpoints
- ✅ **Microserviços Internos**: Sem exposição externa, comunicação via rede Docker
- ✅ **Segurança por Design**: Acesso controlado apenas via Gateway
- ✅ **Pronto para Produção**: Estrutura escalável e resiliente
- ✅ **Fácil de Testar**: Collection Insomnia completa incluída
- ✅ **Zero Dependências**: Repositórios em memória para demonstração

## 🏗️ Arquitetura

```
                    ┌─────────────────────────┐
                    │      Cliente/Web        │
                    │   (Insomnia/Browser)    │
                    └─────────────┬───────────┘
                                  │ HTTP
                                  ▼
                    ┌─────────────────────────┐
                    │     Gateway API         │
                    │    (Port 5000)          │
                    │  ✅ Ponto Único Entrada │
                    └─────────────┬───────────┘
                                  │
                    ┌─────────────┼─────────────┐
                    │             │             │
                    ▼             ▼             ▼
        ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
        │ Notification API│ │ Subscription API│ │ Processor Worker│
        │   (Interno)     │ │   (Interno)     │ │   (Interno)     │
        │ ❌ Sem Porta    │ │ ❌ Sem Porta    │ │ Background Svc  │
        │    Externa      │ │    Externa      │ │                 │
        └─────────┬───────┘ └─────────────────┘ └─────────┬───────┘
                  │                                       │
                  ▼                                       ▼
        ┌─────────────────┐                    ┌─────────────────┐
        │ In-Memory Store │                    │ Message Providers│
        │  (Demo Mode)    │                    │ Email│SMS│Push  │
        └─────────────────┘                    └─────────────────┘
```

### 🔄 **Fluxo de Comunicação:**
1. **Cliente** → Gateway API (porta 5000)
2. **Gateway** → Microserviços internos (sem exposição externa)
3. **Microserviços** → Repositórios/Provedores

## 🚀 Microserviços

### 1. 🌐 Gateway API (Porta 5000) - **PONTO ÚNICO DE ENTRADA**
- **Responsabilidade**: API Gateway completo com todos os endpoints
- **Tecnologias**: .NET 8, Serilog, Swagger, HttpClient
- **Funcionalidades**:
  - ✅ **Todos os endpoints** de Notification e Subscription
  - ✅ Roteamento inteligente para microserviços internos
  - ✅ Health checks centralizados
  - ✅ Logging estruturado com correlação
  - ✅ Documentação Swagger unificada
  - ✅ Tratamento de erros centralizado

### 2. 📧 Notification API (Interno - Sem Porta Externa)
- **Responsabilidade**: Gerenciamento do ciclo de vida das notificações
- **Tecnologias**: .NET 8, In-Memory Repository (demo), Azure Service Bus
- **Funcionalidades**:
  - ✅ CRUD completo de notificações
  - ✅ Enfileiramento para processamento assíncrono
  - ✅ Rastreamento de status em tempo real
  - ✅ Suporte a múltiplos canais (Email, SMS, Push)
  - ✅ Sistema de prioridades
  - ⚠️ **Acesso apenas via Gateway**

### 3. ⚙️ Subscription API (Interno - Sem Porta Externa)
- **Responsabilidade**: Gerenciamento avançado de preferências
- **Tecnologias**: .NET 8, In-Memory Repository (demo)
- **Funcionalidades**:
  - ✅ Preferências granulares por canal
  - ✅ Categorização inteligente de notificações
  - ✅ Horários de silêncio com timezone
  - ✅ Validações de preferências em tempo real
  - ✅ Sistema de subscrições flexível
  - ⚠️ **Acesso apenas via Gateway**

### 4. 🔄 Processor Worker (Background Service)
- **Responsabilidade**: Processamento assíncrono e entrega
- **Tecnologias**: .NET 8 Worker Service, Provedores simulados
- **Funcionalidades**:
  - ✅ Processamento assíncrono de notificações
  - ✅ Integração com provedores simulados
  - ✅ Validação de preferências antes do envio
  - ✅ Tratamento robusto de erros e retry
  - ✅ Modo simulação para desenvolvimento

## 🛠️ Tecnologias Utilizadas

### Backend
- **.NET 8**: Framework principal
- **Azure Cosmos DB**: Banco de dados NoSQL
- **Azure Service Bus**: Mensageria assíncrona
- **Serilog**: Logging estruturado

- **Swagger/OpenAPI**: Documentação de APIs

### DevOps & Infraestrutura
- **Docker**: Containerização
- **Docker Compose**: Orquestração local
- **Terraform**: Infrastructure as Code (planejado)
- **Azure DevOps**: CI/CD (planejado)

### Princípios e Padrões
- **SOLID**: Princípios de design
- **Clean Architecture**: Arquitetura limpa
- **Microserviços**: Arquitetura distribuída
- **Domain-Driven Design**: Modelagem de domínio
- **CQRS**: Separação de comandos e consultas

## 🚀 Como Executar

### Pré-requisitos
- .NET 8 SDK
- Docker Desktop
- Visual Studio 2022 ou VS Code

### Execução Local com Docker

1. **Clone o repositório**:
```bash
git clone https://github.com/seu-usuario/MultiChannel-Notification-System.git
cd MultiChannel-Notification-System
```

2. **Build e execução**:
```bash
docker-compose up --build
```

3. **Acesse o sistema**:
- **Gateway API (ÚNICO PONTO DE ENTRADA)**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health
- ⚠️ **Microserviços internos não são acessíveis diretamente**

### Execução Local com .NET

1. **Restaurar dependências**:
```bash
dotnet restore
```

2. **Build da solução**:
```bash
dotnet build
```

3. **Executar cada serviço** (em terminais separados):
```bash
# Gateway API
cd src/Services/Gateway.API
dotnet run

# Notification API
cd src/Services/Notification.API
dotnet run

# Subscription API
cd src/Services/Subscription.API
dotnet run

# Processor Worker
cd src/Services/Processor.Worker
dotnet run
```

## 📋 Funcionalidades Principais

### Gerenciamento de Notificações
- ✅ Criação de notificações multi-canal
- ✅ Agendamento de notificações
- ✅ Rastreamento de status em tempo real
- ✅ Suporte a prioridades (Low, Normal, High, Critical)
- ✅ Metadados customizáveis

### Preferências de Usuários
- ✅ Configuração por canal (Email, SMS, Push)
- ✅ Categorização (Marketing, Transactional, Security, System)
- ✅ Horários de silêncio com timezone
- ✅ Validações inteligentes

### Processamento Assíncrono
- ✅ Consumo de mensagens do Service Bus
- ✅ Tratamento robusto de erros
- ✅ Dead Letter Queue para falhas
- ✅ Modo simulação para desenvolvimento

### Provedores de Notificação
- ✅ Email Provider (simulado - pronto para SendGrid/AWS SES)
- ✅ SMS Provider (simulado - pronto para Twilio/AWS SNS)
- ✅ Push Provider (simulado - pronto para Firebase/APNS)

## 🔧 Configuração

### Variáveis de Ambiente

```bash
# Cosmos DB
ConnectionStrings__CosmosDB=AccountEndpoint=...;AccountKey=...

# Service Bus
ConnectionStrings__ServiceBus=Endpoint=sb://...;SharedAccessKeyName=...;SharedAccessKey=...

# APIs
Services__NotificationAPI=http://notification-api
Services__SubscriptionAPI=http://subscription-api
```

### Docker Compose

O arquivo `docker-compose.yml` está configurado para desenvolvimento local com:
- ✅ **Rede compartilhada** entre serviços
- ✅ **Variáveis de ambiente** pré-configuradas
- ✅ **Apenas Gateway exposto** (porta 5000)
- ✅ **Microserviços internos** sem exposição externa
- ✅ **Dependências** entre serviços configuradas
- ✅ **Repositórios em memória** para demonstração

## 📊 Monitoramento e Observabilidade

### Logging
- **Serilog** com templates estruturados
- **Correlação** de requests entre serviços
- **Níveis apropriados** por componente
- **Formato JSON** para análise

### Health Checks
- Endpoints `/health` em todas as APIs
- Verificação de dependências externas
- Status dos provedores de notificação
- Integração com Docker HEALTHCHECK

### Métricas (Planejado)
- Application Insights
- Prometheus + Grafana
- Métricas de negócio e técnicas

## 🧪 Testes

### 🚀 **Collection Insomnia (Disponível)**
- ✅ **Collection completa** para testes de todos os endpoints
- ✅ **Arquivo**: `insomnia-collection.json`
- ✅ **Guia detalhado**: `INSOMNIA_GUIDE.md`
- ✅ **Cenários de teste** pré-configurados
- ✅ **Variáveis de ambiente** configuradas
- ✅ **Fluxos completos** de notificação

**Como usar:**
1. Importe `insomnia-collection.json` no Insomnia
2. Siga o guia em `INSOMNIA_GUIDE.md`
3. Execute os cenários de teste
4. Todos os requests passam pelo Gateway (porta 5000)

### Estrutura de Testes (Planejado)
```
tests/
├── Unit/
│   ├── Gateway.API.Tests/
│   ├── Notification.API.Tests/
│   ├── Subscription.API.Tests/
│   └── Processor.Worker.Tests/
├── Integration/
│   └── MultiChannel.Integration.Tests/
└── E2E/
    └── MultiChannel.E2E.Tests/
```

### Tipos de Testes
- **Unit Tests**: Testes unitários com xUnit
- **Integration Tests**: Testes de integração com TestContainers
- **E2E Tests**: Testes end-to-end com Playwright
- **Load Tests**: Testes de carga com NBomber

## 🚀 Roadmap

### Fase 1 - MVP ✅
- [x] **Arquitetura de microserviços** completa
- [x] **API Gateway** como ponto único de entrada
- [x] **APIs RESTful** com todos os endpoints
- [x] **Processamento assíncrono** funcional
- [x] **Containerização** com Docker Compose
- [x] **Collection Insomnia** para testes
- [x] **Repositórios em memória** para demonstração
- [x] **Documentação** completa e atualizada

### Fase 2 - Produção 🔄
- [ ] Integração com provedores reais
- [ ] Testes automatizados
- [ ] CI/CD com Azure DevOps
- [ ] Infrastructure as Code (Terraform)

### Fase 3 - Escala 📋
- [ ] Auto-scaling
- [ ] Métricas avançadas
- [ ] Distributed tracing
- [ ] Event sourcing

### Fase 4 - Inteligência 🔮
- [ ] Machine Learning para otimização
- [ ] A/B testing
- [ ] Personalização inteligente
- [ ] Analytics avançados

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para detalhes.

## 👨‍💻 Autor

**Seu Nome**
- LinkedIn: [seu-perfil](https://linkedin.com/in/seu-perfil)
- GitHub: [@seu-usuario](https://github.com/seu-usuario)
- Email: seu.email@exemplo.com

---

⭐ **Se este projeto foi útil para você, considere dar uma estrela!** ⭐

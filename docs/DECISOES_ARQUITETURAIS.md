# Decisões Arquiteturais - Sistema de Crédito

## Contexto

Este documento detalha as principais decisões arquiteturais tomadas para este desafio durante o desenvolvimento do Sistema de Crédito, explicando o raciocínio por trás de cada escolha e suas implicações.

## 1. Arquitetura: Monolito Modular

### Decisão
Por ser um projeto de um desafio simples, optei por uma arquitetura de **Monolito Modular** em vez de microsserviços distribuídos ou outros estilos arquiteturais.

### Justificativa

Para o cenário proposto no desafio, a arquitetura de Monolito Modular oferece o melhor equilíbrio entre simplicidade operacional e organização de código para este contexto. Embora a aplicação seja implantada como um único serviço, ela é internamente dividida em módulos lógicos independentes (Cadastro de Cliente, Proposta e Cartão), cada um com suas próprias responsabilidades, modelos de dados e serviços.

**Vantagens:**
- ✅ **Simplicidade de Deploy:** Um único artefato para construir, testar e implantar
- ✅ **Baixa Latência:** Chamadas entre módulos são locais (in-process), sem overhead de rede
- ✅ **Facilidade de Desenvolvimento:** Mais fácil de debugar e testar localmente
- ✅ **Transações Locais:** Possibilidade de usar transações de banco de dados quando necessário
- ✅ **Evolução Gradual:** Permite evoluir para microsserviços distribuídos no futuro, pois os módulos já estão desacoplados

**Desvantagens:**
- ❌ **Escalabilidade Limitada:** Não é possível escalar módulos individualmente
- ❌ **Acoplamento de Deploy:** Uma mudança em qualquer módulo requer rebuild e redeploy de toda a aplicação

### Alternativas Consideradas

| Alternativa | Por que foi descartada |
|-------------|------------------------|
| **Microsserviços Distribuídos** | Complexidade operacional excessiva para o escopo do projeto |
| **Monolito Tradicional** | Falta de separação clara de responsabilidades |
| **Serverless (Functions)** | Overhead de cold start e complexidade de orquestração |

**Conclusão:** Para o escopo atual do projeto, as vantagens superam as desvantagens. Se o sistema crescer significativamente, os módulos podem ser extraídos para microsserviços independentes com relativa facilidade.


## 2. Outbox Pattern

### Decisão

Implementei o **Outbox Pattern** para garantir das entregas, ou seja, para que nenhum evento seja perdido, mesmo se o RabbitMQ estiver temporariamente indisponível.

### Justificativa

O Outbox Pattern resolve o problema de **consistência entre banco de dados e mensageria**. Sem ele, existe o risco de salvar uma entidade no banco, mas falhar ao publicar o evento correspondente (ex: RabbitMQ fora do ar), resultando em **perda de dados** e **inconsistência** entre módulos, garantindo a resiliência do serviço e informações.

**Vantagens:**
- ✅ **Garantia de Entrega:** Eventos nunca são perdidos
- ✅ **Atomicidade:** Entidade e evento salvos juntos (tudo ou nada)
- ✅ **Resiliência:** Funciona mesmo com RabbitMQ fora do ar
- ✅ **Auditoria:** Todos os eventos ficam registrados no banco
- ✅ **Retry Automático:** Até 5 tentativas com backoff exponencial

**Desvantagens:**
- ❌ **Latência:** Delay de ~5 segundos entre salvar e publicar
- ❌ **Complexidade:** Requer tabela adicional e background service
- ❌ **Overhead de Banco:** Mais operações de leitura/escrita

### Alternativas Consideradas

| Alternativa | Por que foi descartada |
|-------------|------------------------|
| **Publicação Direta** | Risco de perda de eventos se RabbitMQ estiver fora |
| **Saga Pattern** | Overhead desnecessário para fluxos simples |

**Conclusão:** O Outbox Pattern neste cenário para garantir consistência eventual/resiliência.


## 3. Comunicação Assíncrona com RabbitMQ

### Decisão
Utilizei **RabbitMQ** como message broker para comunicação assíncrona entre os módulos.

### Justificativa

A comunicação assíncrona via mensageria oferece desacoplamento temporal e espacial entre os módulos. Quando um cliente é cadastrado, o módulo de Cadastro publica um evento em uma fila do RabbitMQ. O módulo de Proposta consome esse evento de forma assíncrona, processa a análise de crédito e publica o resultado em outra fila. O módulo de Cartão, por sua vez, consome os eventos de propostas aprovadas e emite os cartões.

**Vantagens:**
- ✅ **Desacoplamento:** Os módulos não precisam conhecer uns aos outros diretamente
- ✅ **Resiliência:** Se um módulo estiver temporariamente indisponível, as mensagens ficam na fila até serem processadas
- ✅ **Escalabilidade:** É possível adicionar múltiplos consumers para uma mesma fila, distribuindo a carga
- ✅ **Auditoria:** As mensagens trafegadas podem ser logadas e auditadas
- ✅ **Durabilidade:** Mensagens persistem mesmo se o broker reiniciar

**Desvantagens:**
- ❌ **Complexidade:** Requer gerenciamento de um serviço adicional (RabbitMQ)
- ❌ **Eventual Consistency:** Não há garantia de consistência imediata entre módulos
- ❌ **Debugging:** Mais difícil rastrear fluxos assíncronos


### Alternativas Consideradas

| Alternativa | Por que foi descartada |
|-------------|------------------------|
| **Azure Service Bus** | Requer infraestrutura Azure (custo adicional) |

**Conclusão:** RabbitMQ neste cenário é a melhor opção entre funcionalidades e simplicidade para o este projeto.


## 4. Resiliência com Polly e Dead-Letter Queues

### Decisão
Implementei políticas de **retry com backoff exponencial** usando a biblioteca **Polly** e **Dead-Letter Queues (DLQ)** no RabbitMQ.

### Justificativa

Falhas temporárias são inevitáveis em sistemas distribuídos (ou modulares). A combinação de retries automáticos com DLQs cria um sistema robusto que pode se recuperar de erros transitórios e isolar mensagens problemáticas para análise posterior.

**Vantagens:**
- ✅ **Recuperação Automática:** Erros transitórios (ex: timeout de rede) são resolvidos automaticamente
- ✅ **Isolamento de Erros:** Mensagens com erro não bloqueiam o processamento de outras mensagens
- ✅ **Visibilidade:** DLQs fornecem visibilidade sobre mensagens problemáticas
- ✅ **Proteção contra Cascata:** Circuit Breaker evita sobrecarga em serviços com problemas

**Desvantagens:**
- ❌ **Complexidade:** Requer configuração e monitoramento de DLQs (não foi feito)
- ❌ **Latência:** Retries adicionam latência em caso de falhas

**Conclusão:** A combinação de Polly + DLQ oferece resiliência contra falhas.

## 5. Banco de Dados In-Memory

### Decisão
Cada módulo utiliza um **banco de dados in-memory** (Entity Framework Core InMemory) separado.

### Justificativa

Para este desafio e como projeto de demonstração, o uso de bancos de dados in-memory simplifica a execução e os testes, eliminando a necessidade de configurar e gerenciar um servidor de banco de dados externo. Cada módulo tem seu próprio `DbContext`, simulando a separação de dados que existiria em microsserviços distribuídos.

- ✅ **Simplicidade:** Não requer instalação ou configuração de banco de dados
- ✅ **Velocidade:** Operações de leitura/escrita são extremamente rápidas
- ✅ **Isolamento:** Cada módulo tem seu próprio banco, evitando acoplamento de dados
- ✅ **Testes:** Facilita testes de integração

**Desvantagens:**
- ❌ **Volatilidade:** Dados são perdidos ao reiniciar a aplicação
- ❌ **Limitações:** Não suporta todas as funcionalidades de um banco de dados relacional (ex: transações distribuídas, índices complexos)


## 6. Padrão de Resposta da API (Result Pattern)

### Decisão
Optei também por um formato de respostas padronizado, ou seja, todas as respostas da API seguem o **Result Pattern**, com um formato padronizado:

```json
{
  "success": true,
  "message": "Operação realizada com sucesso",
  "data": { ... },
  "error": []
}
```

### Justificativa

Padronizar as respostas da API facilita o consumo pelo cliente e o tratamento de sucessos e erros de forma consistente. O cliente sempre sabe o que esperar, independentemente do endpoint chamado.

**Vantagens:**
- ✅ **Consistência:** Todas as respostas seguem o mesmo formato
- ✅ **Clareza:** O campo `success` indica imediatamente se a operação foi bem-sucedida
- ✅ **Detalhamento de Erros:** O array `error` permite retornar múltiplos erros de validação de forma estruturada
- ✅ **Facilita Integração:** Clientes podem ter uma lógica única de tratamento de resposta

**Desvantagens:**
- ❌ **Verbosidade:** Respostas são maiores que o necessário


**Conclusão:** O Result Pattern oferece consistência e facilita o consumo da API, e ideal para projetos corporativos.

## 7. Testes Unitários com xUnit, Moq e FluentAssertions

### Decisão
Implementei uma suíte de testes unitários usando **xUnit** como framework de testes, **Moq** para criação de mocks e **FluentAssertions** para asserções mais legíveis.

### Justificativa

Testes unitários são essenciais para garantir a qualidade e a manutenibilidade do código. A combinação dessas três bibliotecas oferece uma experiência de desenvolvimento de testes mais simples e produtiva.

**Cobertura de Testes:**
- **Serviços de Negócio:** Foram testadas as regras de negócio de cada módulo (cálculo de score, validações, etc.).
- **Isolamento:** Foram usados mocks para isolar as dependências (ex: `IMessagePublisher`, `I<Modulo>DbContext`).
- **Legibilidade:** Foi usado FluentAssertions para tornar as asserções mais legíveis e expressivas.


**Vantagens:**
- ✅ **Legibilidade:** FluentAssertions torna as asserções mais legíveis
- ✅ **Isolamento:** Mocks permitem testar unidades isoladamente
- ✅ **Rapidez:** Testes unitários são rápidos de executar
- ✅ **Documentação:** Testes servem como documentação do comportamento esperado

**Conclusão:** A suíte de testes garante a qualidade do código e facilita refatorações futuras.


## 8. Estrutura de Módulos

### Decisão
Cada módulo segue uma estrutura consistente inspirada na **Clean Architecture**:

```
Challenge.Credit.System.Module.<NomeModulo>/
├── Consumers/                   # Consumers de mensagens (quando aplicável)
├── Core/
│   ├── Application/
│   │   ├── DataTransferObjects/ # Data Transfer Objects
│   │   ├── Interfaces/          # Contratos de implementação
│   │   └── Services/            # Lógica de negócio
│   └── Domain/
│       ├── Entities/            # Entidades de domínio
│       └── ValueObjects
├── Infrastructure
│   └── Data                     # DbContext e configurações de banco
└── DependencyInjections.cs      # Configuração de DI
```

### Justificativa

Uma estrutura consistente facilita a navegação no código e a compreensão da responsabilidade de cada componente. Novos desenvolvedores podem rapidamente entender onde encontrar cada tipo de código.

**Vantagens:**
- ✅ **Manutenibilidade:** Fácil localizar e modificar código
- ✅ **Testabilidade:** Camadas internas são facilmente testáveis
- ✅ **Escalabilidade:** Estrutura suporta crescimento do projeto
- ✅ **Onboarding:** Novos desenvolvedores se adaptam rapidamente

**Conclusão:** A estrutura modular facilita a evolução e manutenção do projeto.


## 9. Docker e Docker Compose

### Decisão
Forneci um `docker-compose.yml` para facilitar a execução local da aplicação e do RabbitMQ.

### Justificativa

Docker Compose simplifica drasticamente a configuração do ambiente de desenvolvimento. Com um único comando (`docker-compose up`), o desenvolvedor pode iniciar todos os serviços necessários (RabbitMQ e a API) sem precisar instalar nada manualmente.

**Benefícios:**
- ✅ **Reprodutibilidade:** O ambiente é consistente entre diferentes máquinas
- ✅ **Simplicidade:** Não é necessário instalar RabbitMQ ou .NET localmente
- ✅ **Isolamento:** Os serviços rodam em contêineres isolados
- ✅ **Onboarding:** Novos desenvolvedores podem iniciar rapidamente

---

## Resumo das Decisões

| # | Decisão | Justificativa Principal | Trade-off |
|---|---------|-------------------------|-----------|
| 1 | Monolito Modular | Simplicidade operacional | Escalabilidade limitada |
| 2 | Outbox Pattern | Garantia de entrega de eventos | Latência de ~5s |
| 3 | RabbitMQ | Desacoplamento e resiliência | Complexidade adicional |
| 4 | Polly + DLQ | Resiliência a falhas transitórias | Complexidade de configuração |
| 5 | InMemory Database | Simplicidade de setup | Volatilidade de dados |
| 6 | Result Pattern | Consistência de respostas | Verbosidade |
| 7 | xUnit + Moq | Qualidade e manutenibilidade | Tempo de desenvolvimento |
| 8 | Estrutura | Manutenibilidade | Mais arquivos/estrutura |
| 9 | Docker Compose | Reprodutibilidade | Requer Docker instalado |

---

## Conclusão

As decisões arquiteturais tomadas neste projeto visam criar um sistema limpo, organizado, resiliente e de fácil manutenção. A arquitetura de Monolito Modular oferece um bom ponto de partida, permitindo evolução futura para microsserviços distribuídos se necessário. O uso de mensageria assíncrona, políticas de resiliência e testes unitários garantem a qualidade do sistema.

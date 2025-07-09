# 📞 AKS Contact List

Projeto desenvolvido para o **TechChallenge da FIAP**, com foco em demonstrar **estabilidade**, **escalabilidade** e **alta disponibilidade** por meio do **Azure Kubernetes Service (AKS)**, utilizando uma arquitetura de **microsserviços com mensageria**.

---

## 🎯 Objetivo

Criar uma aplicação de **lista telefônica**, onde cada funcionalidade está desacoplada e dividida em **serviços independentes**, executando em seus próprios **pods** no AKS. A comunicação entre os serviços ocorre de forma assíncrona via **RabbitMQ**, promovendo um sistema escalável e resiliente.

---

## 🛠️ Tecnologias Utilizadas

- **.NET 8 (C#)**
- **RabbitMQ** (mensageria)
- **Azure SQL Server**
- **Docker**
- **AKS (Azure Kubernetes Service)**
- **GitHub Actions** (CI/CD)

---

## 🧱 Arquitetura

A aplicação é composta por diversos **microsserviços**, cada um com uma responsabilidade única, que se comunicam por meio de **filas do RabbitMQ**. O deploy é automatizado com GitHub Actions e os containers são armazenados no **Docker Hub** e implantados no **AKS**.

> 🔍 Benefícios da arquitetura:
> - Independência de deploy e escalabilidade por serviço.
> - Alta disponibilidade com Kubernetes.
> - Resiliência com mensageria assíncrona.
> - Automação com pipelines de CI/CD.

---

## ⚙️ Serviços e Endpoints

| Serviço | Método | Rota | Descrição |
|--------|--------|------|-----------|
| **ContactCreateUpdateService** | POST | `/contatos` | Cria um novo contato via fila de criação. |
|  | PUT | `/editcontatos/{username}` | Atualiza dados de um contato. |
|  | GET | `/health` | Retorna status de saúde do serviço. |
| **ContactDeleteService** | DELETE | `/contatos/deleteID/{id}` | Deleta contato por ID. |
|  | GET | `/health` | Retorna status de saúde do serviço. |
| **ContactQueryService** | GET | `/contatos` | Retorna lista completa de contatos. |
|  | GET | `/contatos/por-ddd/{ddd}` | Retorna lista filtrada por DDD. |
|  | GET | `/health` | Retorna status de saúde do serviço. |
| **DB_Consumer** | - | - | Consome mensagens e persiste dados no banco via EF Core. |
|  | GET | `/health` | Retorna status de saúde do serviço. |

---

## 🐳 CI/CD com GitHub Actions

- Os pipelines estão configurados para:
  - **Buildar** os serviços com Docker.
  - **Publicar** as imagens no Docker Hub.
  - **Efetuar deploy automático** para o AKS.
  - As **credenciais** estão armazenadas de forma segura usando `GitHub Secrets`.
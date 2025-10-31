# 🚨 Dengue Alert (API + Dashboard Angular)

API para cálculo, consulta e visualização de alertas de dengue para a cidade de Belo Horizonte (ou a cidade configurada), garantindo que os dados exibidos no frontend sejam sempre baseados em Semanas Epidemiológicas (SE) completamente encerradas.

## 🛠️ Tecnologias

| Componente | Tecnologia | Versão Principal | Notas |
| :--- | :--- | :--- | :--- |
| **Backend (API)** | **ASP.NET Core / C#** | (Coloque sua versão, ex: .NET 7 ou 8) | Responsável pela lógica de alerta. |
| Repositório | (Ex: Entity Framework Core) | | |
| **Frontend (Dashboard)** | **Angular** | (Coloque sua versão) | Visualização dos dados em cards interativos. |
| Estilização | HTML/CSS customizado | | Design limpo e responsivo. |
| Banco de Dados | **(Seu Banco)** | | Persistência dos dados de alerta. |

## 💡 Descrição do Projeto

Este projeto foi desenvolvido com foco na **precisão epidemiológica** na exibição de dados. Ele resolve o problema comum de exibir informações parciais da semana atual, garantindo que o dashboard apresente apenas dados de **Semanas Epidemiológicas Completas**.

### Arquitetura e Estrutura

A aplicação é dividida em camadas seguindo as boas práticas (Controller, Service, Repository), com o Backend (.NET) focado em:

1.  **Cálculo da Semana Completa:** Um `EpidemiologicalWeekHelper` (ou lógica similar) determina qual é a última SE **encerrada** (geralmente no Sábado).
2.  **Lógica de Negócios:** O `AlertDengueService` gerencia a importação de dados da API pública (se aplicável) e a lógica para buscar as semanas corretas.
3.  **Exposição:** O `DengueController` expõe os dados para o Frontend.

O Frontend (Angular) consome esses dados e apresenta um componente interativo (`last-three`) que permite ao usuário carregar, atualizar e fechar a visualização dos alertas.

## 🔗 Endpoints Principais

| Endpoint | Método | Descrição |
| :--- | :--- | :--- |
| `/api/dengue/semana` | `GET` | Consulta dados de alerta para uma semana e ano específicos (`ew`, `ey`). |
| **`/api/dengue/ultimas-semanas/{count}`** | **`GET`** | **Retorna as últimas `N` Semanas Epidemiológicas COMPLETAS. (Usado no Frontend)** |
| `/api/dengue/importar` | `POST` | Dispara a importação e persistência dos dados dos últimos meses (via `AlertDengueService`). |

---

## ⚙️ Configuração e Execução

Para rodar a aplicação completa (API e Dashboard Angular) localmente:

### 1. Configuração do Banco de Dados

* Instale e configure o **(Seu Banco de Dados)**.
* Crie o banco de dados para o projeto, por exemplo, `dengue_db`.

**Configuração da Connection String:**
No arquivo `appsettings.json` da API, configure sua string de conexão:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=dengue_db;User=seu_user;Password=sua_senha;"
  }
}

Migrações (Para EF Core):Crie as tabelas do banco de dados executando as migrações a partir da pasta da API:Bashdotnet ef database update
2. Execução do Backend (API)Bash# Clone o repositório
git clone [https://www.youtube.com/watch?v=X49Wz3icO3E](https://www.youtube.com/watch?v=X49Wz3icO3E)
cd [Pasta da API, ex: DengueAlert.Api]

# Restaure e compile os pacotes
dotnet restore
dotnet build

# Execute a API
dotnet run
Acesse o Swagger para testar os endpoints em: https://localhost:{porta}/swagger3. Execução do Frontend (Angular)Bashcd [Pasta do Frontend, ex: DengueAlert.App]

# Instale as dependências
npm install

# Rode a aplicação Angular
ng serve --open
O Dashboard será aberto no seu navegador, geralmente em http://localhost:4200/.🔄 Importação Inicial de DadosPara carregar os dados históricos (casos notificados e estimados), realize uma requisição POST para o endpoint de importação da sua API:POST /api/dengue/importar
Isso irá iniciar o processo assíncrono de consumo da API externa e persistência no banco de dados.🌟 Detalhe do Componente FrontendO componente last-three implementa um padrão de carregamento sob demanda para melhor performance e usabilidade:Ação do UsuárioEstado do ComponentePágina CarregadaDados e cards ocultos. Botão exibe "Ver Dados".Clique em "Ver Dados"Conteúdo aparece, dispara o load() inicial. Botão muda para "Fechar Dados".Clique em "Atualizar Dados"Recarrega os dados do Backend sem ocultar o conteúdo.

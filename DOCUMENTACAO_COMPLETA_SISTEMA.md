# 📋 DOCUMENTAÇÃO COMPLETA - SISTEMA GERENTE

## 🎯 Visão Geral
O **Sistema Gerente** é uma aplicação web desenvolvida em ASP.NET Core MVC para gerenciamento completo de projetos, com sistema de autenticação, controle de acesso granular, gerenciamento de usuários, configuração de email e parâmetros do sistema.

---

## 🚀 FUNCIONALIDADES DO SISTEMA

### 1. 🔐 Sistema de Autenticação e Autorização
- **Login Seguro**: Autenticação com email e senha
- **Hash de Senhas**: Senhas criptografadas com SHA256
- **Redefinição de Senha**: Sistema completo de reset via email
- **Controle de Sessão**: Gerenciamento seguro de sessões
- **Logout Seguro**: Encerramento seguro de sessões

### 2. 👥 Gerenciamento de Usuários
- **Cadastro de Usuários**: Interface completa para criação de usuários
- **Edição de Perfis**: Modificação de dados e permissões
- **Status Ativo/Inativo**: Controle de status dos usuários
- **Associação de Perfis**: Vinculação com perfis de acesso
- **Email de Ativação**: Envio automático de credenciais
- **Listagem com Filtros**: Visualização organizada de usuários

### 3. 🛡️ Sistema de Perfis de Acesso
- **Perfis Padrão**: Administrador, Usuário, Gerente
- **Controle Granular**: Permissões específicas por funcionalidade
- **Acesso a Configurações**: Controle de acesso administrativo
- **Acesso a Usuários**: Gerenciamento de usuários
- **Acesso a Projetos**: Visualização e edição de projetos
- **Acesso a Relatórios**: Geração de relatórios
- **Acesso a Backlog**: Gerenciamento de backlog de arquitetura
- **Menu Dinâmico**: Interface adaptada ao perfil

### 4. 📧 Configuração de Email
- **Configuração SMTP**: Interface para configuração de servidor
- **Criptografia de Senhas**: Senhas de email criptografadas
- **Teste de Conexão**: Validação de configurações
- **Envio de Emails**: Sistema de envio automático
- **Suporte SSL/TLS**: Configurações de segurança

### 5. ⚙️ Parâmetros do Sistema
- **Cabeçalho Personalizável**: Título do sistema configurável
- **Versão do Sistema**: Controle de versão
- **Rodapé Personalizável**: Texto do rodapé configurável
- **Tema do Sistema**: Configuração de tema visual
- **Fuso Horário**: Configuração de timezone
- **Aplicação Automática**: Parâmetros aplicados em tempo real

### 6. 📋 Gerenciamento de Backlog
- **Cadastro de Tarefas**: Criação de itens do backlog
- **Edição de Tarefas**: Modificação de informações
- **Controle de Status**: Pendente, Em Andamento, Concluído
- **Definição de Prioridades**: Alta, Média, Baixa
- **Associação de Responsáveis**: Vinculação com usuários
- **Controle de Ambientes**: Desenvolvimento, Homologação, Produção
- **Data Estimada**: Controle de prazos

### 7. 📊 Relatórios e Estatísticas
- **Dashboard Principal**: Visão geral do sistema
- **Estatísticas de Usuários**: Contagem por perfil
- **Estatísticas de Tarefas**: Status e prioridades
- **Relatórios de Projetos**: Análise de backlog
- **Métricas do Sistema**: Parâmetros e configurações

### 8. 🎨 Interface Responsiva
- **Design Mobile-First**: Otimizado para dispositivos móveis
- **Sidebar Adaptativa**: Menu lateral responsivo
- **Formulários Responsivos**: Campos adaptados para touch
- **Breakpoints Múltiplos**: Desktop, tablet e mobile
- **Acessibilidade**: Navegação por teclado e screen readers
- **Tema Escuro**: Interface moderna e elegante

---

## 🛠️ ARQUITETURA TÉCNICA

### Tecnologias Utilizadas
- **Backend**: ASP.NET Core 8.0 MVC
- **Banco de Dados**: PostgreSQL 12+
- **Frontend**: HTML5, CSS3, JavaScript ES6+
- **Framework CSS**: Bootstrap 5.3
- **Ícones**: Bootstrap Icons
- **Fontes**: Inter (Google Fonts)
- **Email**: SMTP via configuração personalizada
- **Hash**: SHA256 para senhas

### Estrutura do Projeto
```
Gerente/
├── Controllers/                 # Controladores MVC
│   ├── BaseController.cs       # Controller base com parâmetros
│   ├── HomeController.cs       # Página inicial e logout
│   ├── LoginController.cs      # Autenticação e reset
│   ├── UsuarioController.cs    # Gerenciamento de usuários
│   ├── PerfilAcessoController.cs # Perfis de acesso
│   ├── ConfiguracaoEmailController.cs # Configuração de email
│   ├── ParametroSistemaController.cs # Parâmetros do sistema
│   ├── ParametroGanhoController.cs # Parâmetros de ganho
│   ├── ParametroAmbienteController.cs # Parâmetros de ambiente
│   ├── ProjetosController.cs   # Página de projetos
│   ├── RelatoriosController.cs # Página de relatórios
│   └── BacklogArquiteturaController.cs # Backlog de arquitetura
├── Models/                     # Modelos de dados
│   ├── Usuario.cs             # Modelo de usuário
│   ├── PerfilAcesso.cs        # Modelo de perfil
│   ├── ConfiguracaoEmail.cs   # Modelo de email
│   ├── ParametroSistema.cs    # Modelo de parâmetros
│   ├── PasswordResetToken.cs  # Modelo de token
│   ├── BacklogArquitetura.cs  # Modelo de backlog
│   ├── ParametroGanho.cs      # Modelo de ganho
│   ├── ParametroAmbiente.cs   # Modelo de ambiente
│   └── ErrorViewModel.cs      # Modelo de erro
├── Services/                   # Serviços de negócio
│   ├── AccessControlService.cs # Controle de acesso
│   ├── PasswordResetService.cs # Reset de senha
│   ├── EmailService.cs        # Serviço de email
│   └── UsuarioAtivacaoService.cs # Ativação de usuários
├── Filters/                    # Filtros de autorização
│   ├── AccessControlFilter.cs # Filtro de acesso
│   └── RequireSystemParametersAccessAttribute.cs # Filtro de parâmetros
├── Views/                      # Views Razor
│   ├── Home/                  # Views da página inicial
│   ├── Login/                 # Views de autenticação
│   ├── Usuario/               # Views de usuários
│   ├── PerfilAcesso/          # Views de perfis
│   ├── ConfiguracaoEmail/     # Views de email
│   ├── ParametroSistema/      # Views de parâmetros
│   ├── ParametroGanho/        # Views de ganho
│   ├── ParametroAmbiente/     # Views de ambiente
│   ├── Projetos/              # Views de projetos
│   ├── Relatorios/            # Views de relatórios
│   ├── BacklogArquitetura/    # Views de backlog
│   └── Shared/                # Views compartilhadas
├── wwwroot/                   # Arquivos estáticos
│   ├── css/                   # Estilos CSS
│   │   ├── site.css          # Estilos principais
│   │   ├── login.css         # Estilos de login
│   │   └── messages.css      # Sistema de mensagens
│   ├── js/                    # Scripts JavaScript
│   │   ├── site.js           # Scripts principais
│   │   ├── login.js          # Scripts de login
│   │   └── criarConta.js     # Scripts de criação de conta
│   └── lib/                   # Bibliotecas externas
├── Program.cs                 # Configuração da aplicação
├── appsettings.json          # Configurações
├── Gerente.csproj            # Arquivo do projeto
└── database_completo.sql     # Script completo do banco
```

---

## 🐧 INSTALAÇÃO NO UBUNTU

### Pré-requisitos
- Ubuntu 20.04 LTS ou superior
- .NET 8.0 SDK
- PostgreSQL 12+
- Nginx (opcional, para produção)
- Git

### 1. Atualização do Sistema
```bash
sudo apt update && sudo apt upgrade -y
```

### 2. Instalação do .NET 8.0 SDK
```bash
# Adicionar repositório Microsoft
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Instalar .NET 8.0 SDK
sudo apt update
sudo apt install -y apt-transport-https
sudo apt install -y dotnet-sdk-8.0

# Verificar instalação
dotnet --version
```

### 3. Instalação do PostgreSQL
```bash
# Instalar PostgreSQL
sudo apt install -y postgresql postgresql-contrib

# Iniciar e habilitar serviço
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Verificar status
sudo systemctl status postgresql
```

### 4. Configuração do PostgreSQL
```bash
# Acessar PostgreSQL como superusuário
sudo -u postgres psql

# Criar usuário e banco de dados
CREATE USER admin WITH PASSWORD 'admin123';
CREATE DATABASE "Projetos" OWNER admin;
GRANT ALL PRIVILEGES ON DATABASE "Projetos" TO admin;
\q
```

### 5. Download e Configuração do Projeto
```bash
# Criar diretório para aplicações
sudo mkdir -p /var/www/gerente
sudo chown $USER:$USER /var/www/gerente

# Clonar ou copiar o projeto
cd /var/www/gerente
# Copiar arquivos do projeto para este diretório

# Restaurar dependências
dotnet restore

# Compilar o projeto
dotnet build

# Executar script do banco de dados
psql -U admin -d Projetos -h localhost -f database_completo.sql
```

### 6. Configuração da Aplicação
```bash
# Editar arquivo de configuração
nano appsettings.json
```

Conteúdo do `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=Projetos;Username=admin;Password=admin123"
  }
}
```

### 7. Teste da Aplicação
```bash
# Executar aplicação em modo desenvolvimento
dotnet run

# Acessar no navegador
# http://localhost:5000
```

### 8. Configuração para Produção (Opcional)

#### Instalação do Nginx
```bash
sudo apt install -y nginx
sudo systemctl start nginx
sudo systemctl enable nginx
```

#### Configuração do Nginx
```bash
sudo nano /etc/nginx/sites-available/gerente
```

Conteúdo da configuração:
```nginx
server {
    listen 80;
    server_name seu-dominio.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

#### Ativar configuração
```bash
sudo ln -s /etc/nginx/sites-available/gerente /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

#### Configurar como serviço systemd
```bash
sudo nano /etc/systemd/system/gerente.service
```

Conteúdo do serviço:
```ini
[Unit]
Description=Sistema Gerente
After=network.target

[Service]
WorkingDirectory=/var/www/gerente
ExecStart=/usr/bin/dotnet /var/www/gerente/bin/Debug/net8.0/Gerente.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=gerente
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

#### Ativar serviço
```bash
sudo systemctl enable gerente.service
sudo systemctl start gerente.service
sudo systemctl status gerente.service
```

---

## 📊 ESTRUTURA DO BANCO DE DADOS

### Tabelas Principais

#### 1. `usuarios`
- **id**: Chave primária
- **nome**: Nome completo do usuário
- **email**: Email único do usuário
- **senha**: Senha criptografada (SHA256)
- **perfil_acesso_id**: Referência ao perfil
- **ativo**: Status ativo/inativo
- **data_criacao**: Data de criação
- **data_atualizacao**: Data de atualização

#### 2. `perfis_acesso`
- **id**: Chave primária
- **nome**: Nome do perfil
- **descricao**: Descrição do perfil
- **acesso_configuracoes**: Acesso a configurações
- **acesso_usuarios**: Acesso a usuários
- **acesso_projetos**: Acesso a projetos
- **acesso_relatorios**: Acesso a relatórios
- **acesso_backlog_arquitetura**: Acesso a backlog
- **acesso_parametros_sistema**: Acesso a parâmetros
- **ativo**: Status ativo/inativo

#### 3. `configuracoes_email`
- **id**: Chave primária
- **servidor_smtp**: Servidor SMTP
- **porta**: Porta do servidor
- **usuario**: Usuário do email
- **senha_criptografada**: Senha criptografada
- **usar_ssl**: Usar SSL/TLS
- **ativo**: Status ativo/inativo

#### 4. `parametros_sistema`
- **id**: Chave primária
- **chave**: Chave do parâmetro
- **valor**: Valor do parâmetro
- **descricao**: Descrição do parâmetro
- **ativo**: Status ativo/inativo

#### 5. `backlog_arquitetura`
- **id**: Chave primária
- **titulo**: Título da tarefa
- **descricao**: Descrição da tarefa
- **ambientes**: Ambientes (JSON)
- **prioridade**: Prioridade (Alta/Média/Baixa)
- **status**: Status (Pendente/Em Andamento/Concluído)
- **responsavel_id**: Referência ao usuário responsável
- **data_estimada**: Data estimada de conclusão

### Views Principais

#### 1. `vw_usuarios_com_perfil`
- Combina usuários com seus perfis de acesso
- Inclui todas as permissões do perfil

#### 2. `vw_estatisticas_sistema`
- Estatísticas gerais do sistema
- Contadores de usuários, tarefas, etc.

#### 3. `vw_backlog_com_responsavel`
- Backlog com informações do responsável
- Dados completos das tarefas

### Functions Principais

#### 1. `verificar_integridade_banco()`
- Verifica integridade das tabelas
- Retorna status de cada tabela

#### 2. `limpar_tokens_expirados()`
- Remove tokens de reset expirados
- Mantém banco limpo

#### 3. `estatisticas_usuarios_por_perfil()`
- Estatísticas de usuários por perfil
- Contadores de ativos/inativos

---

## 🔧 CONFIGURAÇÕES E PARÂMETROS

### Parâmetros do Sistema
- **cabecalho_sistema**: Título do sistema
- **versao_sistema**: Versão atual
- **nome_rodape**: Texto do rodapé
- **tema_sistema**: Tema visual
- **timezone_sistema**: Fuso horário

### Configurações de Email
- **Servidor SMTP**: Configurável
- **Porta**: Configurável (587, 465, etc.)
- **SSL/TLS**: Configurável
- **Autenticação**: Usuário e senha

### Configurações de Segurança
- **Hash de Senhas**: SHA256
- **Sessões**: Gerenciadas pelo ASP.NET Core
- **Tokens**: Expiração configurável
- **Criptografia**: AES para senhas de email

---

## 📱 INTERFACE E USABILIDADE

### Design Responsivo
- **Mobile-First**: Otimizado para dispositivos móveis
- **Breakpoints**: 576px, 768px, 992px, 1200px
- **Flexbox/Grid**: Layout moderno
- **Touch-Friendly**: Botões e campos otimizados

### Componentes de Interface
- **Sidebar**: Menu lateral responsivo
- **Cards**: Componentes de informação
- **Modais**: Diálogos e confirmações
- **Formulários**: Validação em tempo real
- **Tabelas**: Dados organizados
- **Alertas**: Sistema de mensagens

### Acessibilidade
- **Navegação por Teclado**: Suporte completo
- **Screen Readers**: Compatibilidade
- **Contraste**: Alto contraste
- **Foco**: Indicadores visuais

---

## 🔒 SEGURANÇA

### Autenticação
- **Hash Seguro**: SHA256 para senhas
- **Sessões Seguras**: Gerenciadas pelo framework
- **Logout Automático**: Expiração de sessão
- **Proteção CSRF**: Tokens de segurança

### Autorização
- **Controle Granular**: Permissões por funcionalidade
- **Filtros de Acesso**: Middleware de autorização
- **Validação de Sessão**: Verificação de autenticação
- **Proteção de Rotas**: Controle de acesso

### Dados
- **Criptografia**: Senhas e dados sensíveis
- **Validação**: Input validation
- **Sanitização**: Limpeza de dados
- **Backup**: Estratégia de backup

---

## 📈 MONITORAMENTO E LOGS

### Logs do Sistema
- **Logs de Acesso**: Tentativas de login
- **Logs de Erro**: Erros e exceções
- **Logs de Auditoria**: Ações dos usuários
- **Logs de Performance**: Métricas de performance

### Métricas
- **Usuários Ativos**: Contagem de usuários
- **Tarefas Pendentes**: Status do backlog
- **Performance**: Tempo de resposta
- **Erros**: Taxa de erro

---

## 🚀 DEPLOYMENT E MANUTENÇÃO

### Backup do Banco
```bash
# Backup completo
pg_dump -U admin -d Projetos > backup_gerente_$(date +%Y%m%d_%H%M%S).sql

# Backup incremental
pg_dump -U admin -d Projetos --data-only > backup_dados_$(date +%Y%m%d_%H%M%S).sql
```

### Atualização do Sistema
```bash
# Parar serviço
sudo systemctl stop gerente

# Backup
pg_dump -U admin -d Projetos > backup_pre_atualizacao.sql

# Atualizar código
cd /var/www/gerente
git pull origin main

# Restaurar dependências
dotnet restore
dotnet build

# Executar migrações (se houver)
psql -U admin -d Projetos -f nova_migracao.sql

# Reiniciar serviço
sudo systemctl start gerente
```

### Monitoramento
```bash
# Verificar status do serviço
sudo systemctl status gerente

# Verificar logs
sudo journalctl -u gerente -f

# Verificar uso de recursos
htop
df -h
```

---

## 🐛 TROUBLESHOOTING

### Problemas Comuns

#### 1. Erro de Conexão com Banco
```bash
# Verificar se PostgreSQL está rodando
sudo systemctl status postgresql

# Verificar conexão
psql -U admin -d Projetos -h localhost

# Verificar configurações
sudo nano /etc/postgresql/*/main/postgresql.conf
```

#### 2. Erro de Permissões
```bash
# Verificar permissões do diretório
ls -la /var/www/gerente

# Corrigir permissões
sudo chown -R www-data:www-data /var/www/gerente
sudo chmod -R 755 /var/www/gerente
```

#### 3. Erro de Porta
```bash
# Verificar portas em uso
sudo netstat -tlnp | grep :5000

# Matar processo se necessário
sudo kill -9 <PID>
```

#### 4. Erro de Email
```bash
# Verificar configurações SMTP
# Testar conexão manualmente
telnet smtp.gmail.com 587

# Verificar logs de email
tail -f /var/log/mail.log
```

---

## 📞 SUPORTE E CONTATO

### Informações de Contato
- **Email**: suporte@gerente.com
- **Documentação**: Este documento
- **Repositório**: GitHub do projeto

### Recursos Adicionais
- **Logs do Sistema**: `/var/log/gerente/`
- **Backups**: `/var/backups/gerente/`
- **Configurações**: `/var/www/gerente/appsettings.json`

---

## 📄 LICENÇA

Este sistema é desenvolvido para uso interno e comercial.
Todos os direitos reservados.

---

**Versão do Documento**: 1.0.0  
**Data de Atualização**: 2025  
**Sistema Gerente** - Documentação Completa 
-- Tabela: perfis_acesso
CREATE TABLE IF NOT EXISTS perfis_acesso (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    descricao VARCHAR(500),
    acesso_configuracoes BOOLEAN NOT NULL DEFAULT FALSE,
    acesso_usuarios BOOLEAN NOT NULL DEFAULT FALSE,
    acesso_projetos BOOLEAN NOT NULL DEFAULT FALSE,
    acesso_backlog_arquitetura BOOLEAN NOT NULL DEFAULT FALSE,
    acesso_relatorios BOOLEAN NOT NULL DEFAULT FALSE,
    acesso_parametros_sistema BOOLEAN NOT NULL DEFAULT FALSE,
    acesso_total BOOLEAN NOT NULL DEFAULT FALSE,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Adiciona constraint UNIQUE para nome
ALTER TABLE perfis_acesso ADD CONSTRAINT unique_nome_perfil UNIQUE (nome);

-- Tabela: usuarios
CREATE TABLE IF NOT EXISTS usuarios (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    senha VARCHAR(100) NOT NULL,
    perfil_acesso_id INTEGER REFERENCES perfis_acesso(id),
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: backlogs_arquitetura
CREATE TABLE IF NOT EXISTS backlogs_arquitetura (
    id SERIAL PRIMARY KEY,
    descricao_tarefa VARCHAR(500) NOT NULL,
    prioridade VARCHAR(50) NOT NULL,
    ganho VARCHAR(100) NOT NULL,
    usuario_id INTEGER NOT NULL REFERENCES usuarios(id),
    data_inicio DATE,
    data_fim DATE,
    progresso VARCHAR(100) NOT NULL DEFAULT 'Não Iniciado',
    ambientes VARCHAR(255),
    anotacoes VARCHAR(4000),
    anexos VARCHAR(1000),
    data_conclusao DATE,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_alteracao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: password_reset_tokens
CREATE TABLE IF NOT EXISTS password_reset_tokens (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES usuarios(id),
    token VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    used BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: parametros_sistema
CREATE TABLE IF NOT EXISTS parametros_sistema (
    id SERIAL PRIMARY KEY,
    cabecalho_sistema VARCHAR(255),
    versao_sistema VARCHAR(50),
    nome_rodape VARCHAR(255),
    cor_fundo_login VARCHAR(20),
    descricao_cabecalho_login VARCHAR(255),
    recaptcha_site_key VARCHAR(100),
    recaptcha_secret_key VARCHAR(100),
    recaptcha_enabled BOOLEAN DEFAULT FALSE,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: parametros_ambientes
CREATE TABLE IF NOT EXISTS parametros_ambientes (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: parametros_ganhos
CREATE TABLE IF NOT EXISTS parametros_ganhos (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: configuracoes_email
CREATE TABLE IF NOT EXISTS configuracoes_email (
    id SERIAL PRIMARY KEY,
    servidor_smtp VARCHAR(255) NOT NULL,
    porta INTEGER NOT NULL,
    email_remetente VARCHAR(255) NOT NULL,
    nome_remetente VARCHAR(255) NOT NULL,
    usuario_smtp VARCHAR(255) NOT NULL,
    senha_smtp VARCHAR(255) NOT NULL,
    security_mode VARCHAR(50),
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: anexos_backlog
CREATE TABLE IF NOT EXISTS anexos_backlog (
    id SERIAL PRIMARY KEY,
    backlog_id INTEGER NOT NULL REFERENCES backlogs_arquitetura(id),
    nome_arquivo VARCHAR(255) NOT NULL,
    nome_original VARCHAR(255) NOT NULL,
    caminho_arquivo VARCHAR(500) NOT NULL,
    tamanho_bytes BIGINT NOT NULL,
    tipo_mime VARCHAR(100) NOT NULL,
    usuario_id INTEGER NOT NULL REFERENCES usuarios(id),
    data_upload TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: comentarios_backlog
CREATE TABLE IF NOT EXISTS comentarios_backlog (
    id SERIAL PRIMARY KEY,
    backlog_id INTEGER NOT NULL REFERENCES backlogs_arquitetura(id),
    comentario VARCHAR(2000) NOT NULL,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    usuario_id INTEGER NOT NULL REFERENCES usuarios(id)
);

-- Seed para perfil admin
INSERT INTO perfis_acesso (nome, descricao, acesso_configuracoes, acesso_usuarios, acesso_projetos, acesso_backlog_arquitetura, acesso_relatorios, acesso_parametros_sistema, acesso_total, ativo, data_criacao, data_atualizacao)
VALUES ('Administrador', 'Perfil com acesso total', true, true, true, true, true, true, true, true, now(), now())
ON CONFLICT (nome) DO NOTHING;

-- Seed para usuário admin
INSERT INTO usuarios (nome, email, senha, perfil_acesso_id, ativo, data_criacao, data_atualizacao)
VALUES ('Administrador', 'admin@admin.com', 'Gwd0bNU4RzWTWkCvZWjYqUX9UdlZF7+Vs6QwYNvO28M=', (SELECT id FROM perfis_acesso WHERE nome = 'Administrador'), true, now(), now())
ON CONFLICT (email) DO NOTHING; 

-- Tabelas do Dashboard Operacional e Incidentes
CREATE TABLE IF NOT EXISTS ambientes (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    nome text NOT NULL,
    CONSTRAINT pk_ambientes PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS criticidades (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    nome text NOT NULL,
    cor text NOT NULL,
    peso integer NOT NULL,
    downtime boolean NOT NULL,
    descricao text NOT NULL,
    CONSTRAINT pk_criticidades PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS tiposincidente (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    nome text NOT NULL,
    descricao text NOT NULL,
    CONSTRAINT pk_tiposincidente PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS segmentos (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    nome text NOT NULL,
    ambienteid integer NOT NULL,
    CONSTRAINT pk_segmentos PRIMARY KEY (id),
    CONSTRAINT fk_segmentos_ambientes_ambienteid FOREIGN KEY (ambienteid) REFERENCES ambientes (id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS incidentes (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    datahorainicio timestamp with time zone NOT NULL,
    datahorafim timestamp with time zone,
    tipoincidenteid integer NOT NULL,
    ambienteid integer NOT NULL,
    segmentoid integer NOT NULL,
    criticidadeid integer NOT NULL,
    descricao text NOT NULL,
    acoestomadas text NOT NULL,
    duracaominutos integer,
    CONSTRAINT pk_incidentes PRIMARY KEY (id),
    CONSTRAINT fk_incidentes_ambientes_ambienteid FOREIGN KEY (ambienteid) REFERENCES ambientes (id) ON DELETE CASCADE,
    CONSTRAINT fk_incidentes_criticidades_criticidadeid FOREIGN KEY (criticidadeid) REFERENCES criticidades (id) ON DELETE CASCADE,
    CONSTRAINT fk_incidentes_segmentos_segmentoid FOREIGN KEY (segmentoid) REFERENCES segmentos (id) ON DELETE CASCADE,
    CONSTRAINT fk_incidentes_tiposincidente_tipoincidenteid FOREIGN KEY (tipoincidenteid) REFERENCES tiposincidente (id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS metas (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    ambienteid integer NOT NULL,
    segmentoid integer NOT NULL,
    peso integer NOT NULL,
    mttrmetahoras double precision NOT NULL,
    superacaomttr boolean NOT NULL,
    mtbfmetahoras double precision NOT NULL,
    superacaomtbf boolean NOT NULL,
    mtbfmetadias double precision NOT NULL,
    disponibilidademeta double precision NOT NULL,
    CONSTRAINT pk_metas PRIMARY KEY (id),
    CONSTRAINT fk_metas_ambientes_ambienteid FOREIGN KEY (ambienteid) REFERENCES ambientes (id) ON DELETE CASCADE,
    CONSTRAINT fk_metas_segmentos_segmentoid FOREIGN KEY (segmentoid) REFERENCES segmentos (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_incidentes_ambienteid ON incidentes (ambienteid);
CREATE INDEX IF NOT EXISTS ix_incidentes_criticidadeid ON incidentes (criticidadeid);
CREATE INDEX IF NOT EXISTS ix_incidentes_segmentoid ON incidentes (segmentoid);
CREATE INDEX IF NOT EXISTS ix_incidentes_tipoincidenteid ON incidentes (tipoincidenteid);
CREATE INDEX IF NOT EXISTS ix_metas_ambienteid ON metas (ambienteid);
CREATE INDEX IF NOT EXISTS ix_metas_segmentoid ON metas (segmentoid);
CREATE INDEX IF NOT EXISTS ix_segmentos_ambienteid ON segmentos (ambienteid);

-- Fim das tabelas do Dashboard Operacional e Incidentes 
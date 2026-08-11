-- =========================================================
-- Auth Service — schema
-- Banco: auth_db
-- =========================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ---------------------------------------------------------
-- Tabela: access_levels
-- Ex: "Admin", "Coach", "Viewer"
-- ---------------------------------------------------------
CREATE TABLE access_levels (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------
-- Tabela: users
-- Guarda credenciais + referência ao nível de acesso
-- ---------------------------------------------------------
CREATE TABLE users (
    id               UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email            VARCHAR(255) NOT NULL UNIQUE,
    password_hash    VARCHAR(255) NOT NULL,     -- hash bcrypt/argon2, nunca texto puro
    access_level_id  INT NOT NULL REFERENCES access_levels(id),
    is_active        BOOLEAN NOT NULL DEFAULT true,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_users_access_level_id ON users(access_level_id);

-- ---------------------------------------------------------
-- Seed inicial de níveis de acesso
-- ---------------------------------------------------------
INSERT INTO access_levels (name, description) VALUES
    ('Admin',  'Full Access'),
    ('Viewer', 'Only upload and streaming');

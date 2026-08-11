-- =========================================================
-- Video Service — schema
-- Banco: video_db
-- =========================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ---------------------------------------------------------
-- Tabela: categories
-- Ex: "Palestra", "Tutorial", "Treinamento WorldSkills"
-- ---------------------------------------------------------
CREATE TABLE categories (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------
-- Tabela: videos
-- Metadados + localização no MinIO (bucket + object key)
-- owner_id referencia o Users do Auth Service, mas como são
-- bancos separados não existe FK entre eles — só guardamos o id.
-- ---------------------------------------------------------
CREATE TABLE videos (
    id                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    owner_id          UUID NOT NULL,                 -- id do usuário no Auth Service (sem FK cross-db)
    category_id       INT REFERENCES categories(id),
    title             VARCHAR(200) NOT NULL,
    description       TEXT,

    -- Localização do arquivo original no MinIO
    minio_bucket      VARCHAR(100) NOT NULL,          -- ex: "videos-raw"
    minio_object_key  VARCHAR(500) NOT NULL,          -- ex: "2026/08/06/a1b2c3.mp4"

    -- Localização do thumbnail gerado pelo worker (nullable até processar)
    thumbnail_bucket      VARCHAR(100),
    thumbnail_object_key  VARCHAR(500),

    content_type      VARCHAR(100),                   -- ex: "video/mp4"
    size_bytes         BIGINT,
    duration_seconds    INT,

    status             VARCHAR(20) NOT NULL DEFAULT 'Uploaded',
        -- valores esperados: Uploaded | Processing | Ready | Failed

    created_at        TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at        TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT chk_videos_status CHECK (status IN ('Uploaded','Processing','Ready','Failed')),
    CONSTRAINT uq_videos_bucket_key UNIQUE (minio_bucket, minio_object_key)
);

CREATE INDEX idx_videos_owner_id   ON videos(owner_id);
CREATE INDEX idx_videos_category_id ON videos(category_id);
CREATE INDEX idx_videos_status      ON videos(status);

-- ---------------------------------------------------------
-- Tabela: video_chunks
-- Preparada para o futuro: quebrar o vídeo em pedaços
-- (ex: segmentos HLS, ou partes de upload multipart)
-- Cada linha aponta pra um objeto separado no MinIO.
-- ---------------------------------------------------------
CREATE TABLE video_chunks (
    id                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    video_id          UUID NOT NULL REFERENCES videos(id) ON DELETE CASCADE,
    chunk_index       INT NOT NULL,                   -- ordem do pedaço (0, 1, 2, ...)

    minio_bucket      VARCHAR(100) NOT NULL,
    minio_object_key  VARCHAR(500) NOT NULL,

    size_bytes         BIGINT,
    duration_seconds    NUMERIC(10,3),                -- duração do pedaço, pode ter casas decimais

    created_at        TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_video_chunks_video_index UNIQUE (video_id, chunk_index)
);

CREATE INDEX idx_video_chunks_video_id ON video_chunks(video_id);

-- ---------------------------------------------------------
-- Seed inicial de categorias
-- ---------------------------------------------------------
INSERT INTO categories (name, description) VALUES
    ('Tutorial',   'Conteúdo instrucional passo a passo'),
    ('Palestra',   'Apresentações e talks'),
    ('Treinamento','Material de treinamento e capacitação');

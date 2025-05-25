-- Create database schema for legal case management system

-- Create table for status (needed first for foreign keys)
CREATE TABLE IF NOT EXISTS estado (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  nombre VARCHAR(50) NOT NULL DEFAULT '0',
  descripcion VARCHAR(500) DEFAULT '0'
);

-- Create table for tags
CREATE TABLE IF NOT EXISTS etiquetas (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  nombre VARCHAR(50) NOT NULL
);

-- Create table for clients (needed before casos)
CREATE TABLE IF NOT EXISTS clientes (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  nombre VARCHAR(100) NOT NULL,
  apellido1 VARCHAR(100) NOT NULL,
  apellido2 VARCHAR(100),
  email1 VARCHAR(250) NOT NULL,
  email2 VARCHAR(250),
  telf1 VARCHAR(250) NOT NULL,
  telf2 VARCHAR(250),
  direccion VARCHAR(500) NOT NULL,
  fecha_contrato DATE NOT NULL
);

-- Create table for cases (depends on clientes and estado)
CREATE TABLE IF NOT EXISTS casos (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_cliente BIGINT NOT NULL,
  titulo VARCHAR(200) NOT NULL DEFAULT '',
  descripcion VARCHAR(1000) NOT NULL DEFAULT '',
  fecha_inicio DATE NOT NULL DEFAULT CURRENT_DATE,
  id_estado BIGINT NOT NULL DEFAULT 0,
  CONSTRAINT FK_casos_clientes FOREIGN KEY (id_cliente) REFERENCES clientes (id),
  CONSTRAINT FK_casos_estado FOREIGN KEY (id_estado) REFERENCES estado (id)
);

CREATE INDEX FK_casos_clientes ON casos (id_cliente);
CREATE INDEX FK_casos_estado ON casos (id_estado);

-- Create table for alerts (depends on casos)
CREATE TABLE IF NOT EXISTS alertas (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL,
  titulo VARCHAR(250) NOT NULL DEFAULT '0',
  descripcion VARCHAR(250) NOT NULL DEFAULT '0',
  fecha_alerta DATE NOT NULL,
  estado_alerta VARCHAR(50) NOT NULL DEFAULT '0',
  CONSTRAINT FK_alertas_casos FOREIGN KEY (id_caso) REFERENCES casos (id)
);

CREATE INDEX FK_alertas_casos ON alertas (id_caso);

-- Create table for case tags (depends on casos and estado)
CREATE TABLE IF NOT EXISTS casosetiquetas (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL,
  id_etiqueta BIGINT NOT NULL,
  CONSTRAINT FK_casosetiquetas_casos FOREIGN KEY (id_caso) REFERENCES casos (id),
  CONSTRAINT FK_casosetiquetas_estado FOREIGN KEY (id_etiqueta) REFERENCES estado (id)
);

CREATE INDEX FK_casosetiquetas_casos ON casosetiquetas (id_caso);
CREATE INDEX FK_casosetiquetas_estado ON casosetiquetas (id_etiqueta);

-- Create table for related cases (depends on casos)
CREATE TABLE IF NOT EXISTS casosrelacionados (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL DEFAULT 0,
  id_caso_relacionado BIGINT NOT NULL DEFAULT 0,
  CONSTRAINT FK_casosrelacionados_casos FOREIGN KEY (id_caso) REFERENCES casos (id),
  CONSTRAINT FK_casosrelacionados_casos_2 FOREIGN KEY (id_caso_relacionado) REFERENCES casos (id)
);

CREATE INDEX FK_casosrelacionados_casos ON casosrelacionados (id_caso);
CREATE INDEX FK_casosrelacionados_casos_2 ON casosrelacionados (id_caso_relacionado);

-- Create table for contacts (depends on casos)
CREATE TABLE IF NOT EXISTS contactos (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL DEFAULT 0,
  nombre VARCHAR(250) NOT NULL DEFAULT '0',
  tipo VARCHAR(100) NOT NULL DEFAULT '0',
  telefono VARCHAR(100) NOT NULL DEFAULT '0',
  email VARCHAR(100) DEFAULT '0',
  CONSTRAINT FK_contactos_casos FOREIGN KEY (id_caso) REFERENCES casos (id)
);

CREATE INDEX FK_contactos_casos ON contactos (id_caso);

-- Create table for documents (depends on casos)
CREATE TABLE IF NOT EXISTS documentos (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL,
  nombre VARCHAR(50) NOT NULL DEFAULT '',
  ruta VARCHAR(500) NOT NULL DEFAULT '',
  fecha_subid DATE NOT NULL DEFAULT CURRENT_DATE,
  CONSTRAINT FK_documentos_casos FOREIGN KEY (id_caso) REFERENCES casos (id)
);

CREATE INDEX FK_documentos_casos ON documentos (id_caso);

-- Create table for case files (depends on casos)
CREATE TABLE IF NOT EXISTS expedientes (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL DEFAULT 0,
  num_expediente VARCHAR(100) NOT NULL DEFAULT '0',
  juzgado VARCHAR(250) NOT NULL DEFAULT '0',
  jurisdiccion VARCHAR(250) NOT NULL DEFAULT '0',
  fecha_inicio DATE NOT NULL,
  observaciones VARCHAR(1000) NOT NULL DEFAULT '',
  CONSTRAINT FK_expedientes_casos FOREIGN KEY (id_caso) REFERENCES casos (id)
);

CREATE INDEX FK_expedientes_casos ON expedientes (id_caso);

-- Create table for status history (depends on casos and estado)
CREATE TABLE IF NOT EXISTS historial_estados (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL DEFAULT 0,
  id_estado BIGINT NOT NULL DEFAULT 0,
  fecha_cambio DATE NOT NULL,
  observaciones VARCHAR(1000) NOT NULL DEFAULT '0',
  CONSTRAINT FK_historial_estados_casos FOREIGN KEY (id_caso) REFERENCES casos (id),
  CONSTRAINT FK_historial_estados_estado FOREIGN KEY (id_estado) REFERENCES estado (id)
);

CREATE INDEX FK_historial_estados_casos ON historial_estados (id_caso);
CREATE INDEX FK_historial_estados_estado ON historial_estados (id_estado);

-- Create table for notifications (depends on casos)
CREATE TABLE IF NOT EXISTS notificaciones (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL DEFAULT 0,
  mensaje VARCHAR(250) NOT NULL DEFAULT '0',
  fecha_vencimiento DATE NOT NULL DEFAULT CURRENT_DATE,
  CONSTRAINT FK_notificaciones_casos FOREIGN KEY (id_caso) REFERENCES casos (id)
);

CREATE INDEX FK_notificaciones_casos ON notificaciones (id_caso);

-- Create table for recent cases (depends on casos)
CREATE TABLE IF NOT EXISTS reciente (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL,
  fecha_hora TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT FK__casos FOREIGN KEY (id_caso) REFERENCES casos (id)
);

CREATE INDEX FK__casos ON reciente (id_caso);

-- Create table for time records (depends on casos)
CREATE TABLE IF NOT EXISTS registrotiempos (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL DEFAULT 0,
  descripcion VARCHAR(500) DEFAULT '0',
  horas DECIMAL(5,2) NOT NULL,
  fecha_registro DATE NOT NULL,
  CONSTRAINT FK_registrotiempos_casos FOREIGN KEY (id_caso) REFERENCES casos (id)
);

CREATE INDEX FK_registrotiempos_casos ON registrotiempos (id_caso);

-- Create table for tasks (depends on casos)
CREATE TABLE IF NOT EXISTS tareas (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL DEFAULT 0,
  titulo VARCHAR(200) NOT NULL DEFAULT '0',
  descripcion VARCHAR(1000) DEFAULT '0',
  fecha_fin DATE NOT NULL,
  estado VARCHAR(50) NOT NULL DEFAULT '0',
  CONSTRAINT FK_tareas_casos FOREIGN KEY (id_caso) REFERENCES casos (id)
);

CREATE INDEX FK_tareas_casos ON tareas (id_caso);

-- Tabla para tipos de caso (si usas la versión normalizada)
CREATE TABLE tipos_caso (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  nombre VARCHAR(50) NOT NULL,
  abreviatura VARCHAR(2) NOT NULL
);

-- Añadir columnas a la tabla casos (si no existen)
ALTER TABLE casos ADD COLUMN IF NOT EXISTS id_tipo_caso BIGINT REFERENCES tipos_caso(id);
ALTER TABLE casos ADD COLUMN IF NOT EXISTS referencia VARCHAR(20) UNIQUE;

CREATE TABLE IF NOT EXISTS secuencia_referencia_caso (
    abreviatura VARCHAR(2) NOT NULL,
    anio VARCHAR(4) NOT NULL,
    ultimo_numero INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (abreviatura, anio)
);

-- Create table for event status
CREATE TABLE IF NOT EXISTS estados_eventos (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  nombre VARCHAR(50) NOT NULL
);

-- Create table for appointments/events
CREATE TABLE IF NOT EXISTS eventos_citas (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL,
  titulo VARCHAR(250) NOT NULL,
  descripcion VARCHAR(550),
  id_estado BIGINT NOT NULL,
  fecha DATE NOT NULL,
  CONSTRAINT FK_eventos_citas_casos FOREIGN KEY (id_caso) REFERENCES casos (id),
  CONSTRAINT FK_eventos_citas_estado FOREIGN KEY (id_estado) REFERENCES estado (id)
);

CREATE INDEX FK_eventos_citas_casos ON eventos_citas (id_caso);
CREATE INDEX FK_eventos_citas_estado ON eventos_citas (id_estado);

-- Create table for notes
CREATE TABLE IF NOT EXISTS notas (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id_caso BIGINT NOT NULL,
  nombre VARCHAR(300) NOT NULL,
  descripcion VARCHAR(1000) NOT NULL,
  CONSTRAINT FK_notas_casos FOREIGN KEY (id_caso) REFERENCES casos (id)
);

CREATE INDEX FK_notas_casos ON notas (id_caso);

-- Create table for document types
CREATE TABLE IF NOT EXISTS tipo_documentos (
  id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  nombre VARCHAR(100) NOT NULL
);

-- Modify documentos table to include tipo_documento
ALTER TABLE documentos ADD COLUMN IF NOT EXISTS tipo_documento BIGINT REFERENCES tipo_documentos(id);
CREATE INDEX FK_documentos_tipo_documentos ON documentos (tipo_documento); 

-- Insert initial status data
INSERT INTO estado (nombre, descripcion) VALUES
    ('Abierto', 'Caso en estado inicial'),
    ('En Proceso', 'Caso siendo gestionado'),
    ('Cerrado', 'Caso resuelto'),
    ('Pendiente', 'Esperando información'),
    ('Revisado', 'Revisado por supervisor');

-- Insert event status data
INSERT INTO estados_eventos (nombre) VALUES
    ('Programado'),
    ('Finalizado'),
    ('Cancelado');

-- Insert tags data
INSERT INTO etiquetas (nombre) VALUES
    ('Urgente'),
    ('Prioridad Alta'),
    ('Seguimiento'),
    ('Legal'),
    ('Interno'); 
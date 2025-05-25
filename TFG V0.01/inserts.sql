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
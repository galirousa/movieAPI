-- Script de inicialización de base de datos para Movie API
-- Este script crea las tablas necesarias para almacenar datos de películas

-- Crear la tabla de películas
CREATE TABLE IF NOT EXISTS movies (
    tmdb_id INTEGER UNIQUE NOT NULL PRIMARY KEY,
    title VARCHAR(500) NOT NULL,
    original_title VARCHAR(500),
    overview TEXT,
    release_date VARCHAR(50),
    poster_path VARCHAR(200),
    backdrop_path VARCHAR(200),
    adult BOOLEAN DEFAULT FALSE,
    genre_ids INTEGER[],
    original_language VARCHAR(10),
    popularity DECIMAL(10,3),
    vote_average DECIMAL(3,1),
    vote_count INTEGER,
    video BOOLEAN DEFAULT FALSE,
    last_updated TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

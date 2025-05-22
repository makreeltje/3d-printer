-- Initialize the database with default timezone
SET timezone = 'UTC';

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- The application will create the tables automatically via SQLAlchemy
-- This file is just for any initial setup or data if needed
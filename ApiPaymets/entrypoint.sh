#!/bin/sh
set -e

echo "Waiting for PostgreSQL to be ready..."
# Este loop espera até que a conexão com o banco de dados seja bem-sucedida.
# As variáveis (PGHOST, PGUSER, etc.) são padrões que o 'psql' entende.
until psql -h "$PGHOST" -U "$PGUSER" -d "$PGDATABASE" -c '\q'; do
  >&2 echo "Postgres is unavailable - sleeping"
  sleep 1
done

echo "PostgreSQL is up - executing schema script."
# Executa o nosso script SQL para garantir que as tabelas existam.
psql -h "$PGHOST" -U "$PGUSER" -d "$PGDATABASE" -f /app/init.sql

echo "Schema script executed. Starting application."
# Inicia a aplicação AOT principal.
exec "$@"
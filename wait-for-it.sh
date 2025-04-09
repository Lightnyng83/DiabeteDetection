#!/usr/bin/env bash
# Usage: ./wait-for-it.sh host:port -- command args
# Source: https://github.com/vishnubob/wait-for-it

HOSTPORT=$1
shift
COMMAND="$@"

until nc -z ${HOSTPORT%:*} ${HOSTPORT#*:}; do
  echo "Waiting for ${HOSTPORT}..."
  sleep 1
done

exec $COMMAND

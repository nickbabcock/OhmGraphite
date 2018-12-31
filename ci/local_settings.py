TAGDB = 'graphite.tags.redis.RedisTagDB'
TAGDB_REDIS_HOST = 'redis'
TAGDB_REDIS_PORT = 6379
TAGDB_REDIS_DB = 0

### FROM GRAPHITE DOCKER

import os

LOG_DIR = '/var/log/graphite'
SECRET_KEY = '$(date +%s | sha256sum | base64 | head -c 64)'

if (os.getenv("MEMCACHE_HOST") is not None):
    MEMCACHE_HOSTS = os.getenv("MEMCACHE_HOST").split(",")

if (os.getenv("DEFAULT_CACHE_DURATION") is not None):
    DEFAULT_CACHE_DURATION = int(os.getenv("CACHE_DURATION"))

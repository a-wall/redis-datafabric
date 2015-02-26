redis-datafabric
================

.NET client library for using Redis as a data fabric, combining pub/sub messaging with a data last value cache.

For the Redis instance you are using ensure that notifications have been configured:
```
CONFIG SET notify-keyspace-events Ks
```

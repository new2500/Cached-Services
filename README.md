# Cached-Services
Stored elements until a pre-defined timeout occurs and notifies upon removal.
DESCRIPTION:

Cached implements a cache in which you can place cache objects and specify a timeout value.
(For simplicity, I created a CacheExpiration enum for time specification, see ICacheSetting)

== FEATURES:

* Empty cached stores.
* Thread safety with a simple customize Semaphore design



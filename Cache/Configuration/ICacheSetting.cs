﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache.Configuration
{
    public interface ICacheSetting
    {
        int GetMinutes(CacheExpiration expiration);

        int GetMinutes(CacheSliding sliding);
    }
    public enum CacheSliding
    {
        VeryShort = 10,     // Ten Seconds
        Short = 60,         // One Minute
        Medium = 300,       // Five Minutes
        Long = 3600,        // One Hour
        VeryLong = 86400    // One Day
    }
    public enum CacheExpiration
    {
        VeryShort = 10,     // Ten Seconds
        Short = 60,         // One Minute
        Medium = 300,       // Five Minutes
        Long = 3600,        // One Hour
        VeryLong = 86400    // One Day
    }
}

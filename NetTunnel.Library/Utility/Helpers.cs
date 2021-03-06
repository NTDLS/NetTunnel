﻿using System;

namespace NetTunnel.Library.Utility
{
    public static class Helpers
    {
        public static object IsNull(object value, object defaultValue)
        {
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }
            return value;
        }

    }
}

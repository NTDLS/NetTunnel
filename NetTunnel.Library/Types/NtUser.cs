﻿namespace NetTunnel.EndPoint
{
    public class NtUser
    {
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }

        public NtUser(string username, string passwordHash)
        {
            Username = username.ToLower();
            PasswordHash = passwordHash.ToLower();
        }

        public NtUser Clone()
        {
            return new NtUser(Username, "");
        }
    }
}

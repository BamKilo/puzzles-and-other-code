﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FennecFox
{
    public class SecurityUtils
    {
        public static string md5(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }

            string password = s.ToString();
            return password;
        }
    }
}

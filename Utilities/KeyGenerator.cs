using System;
using System.Security.Cryptography;

public static class KeyGenerator
{
    public static byte[] GenerateKey()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] key = new byte[32]; 
            rng.GetBytes(key);
            return key;
        }
    }

    public static byte[] GenerateIV()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] iv = new byte[16]; 
            rng.GetBytes(iv);
            return iv;
        }
    }
}

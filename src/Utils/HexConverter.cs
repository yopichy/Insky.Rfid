using System;
using System.Text;

namespace Insky.Rfid.Utils;

public static class HexConverter
{
    public static string ConvertStringToHex(string data)
    {
        return SplitStringToHex(data);
    }

    public static string ConvertHexToString(string data)
    {
        var raw = new byte[data.Length / 2];
        for (var i = 0; i < raw.Length; i++)
        {
            raw[i] = Convert.ToByte(data.Substring(i * 2, 2), 16);
        }

        return Encoding.ASCII.GetString(raw);
    }

    public static byte[] HexStringToByteArray(string s)
    {
        s = s.Replace(" ", "");
        var buffer = new byte[s.Length / 2];
        for (var i = 0; i < s.Length; i += 2)
            buffer[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
        return buffer;
    }

    public static string ByteArrayToHexString(byte[] data)
    {
        var sb = new StringBuilder(data.Length * 3);
        foreach (var b in data)
            sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
        return sb.ToString().ToUpper();

    }

    private static string SplitStringToHex(string data)
    {
        var ba = Encoding.Default.GetBytes(data);
        byte[] nba;
        if (ba.Length % 2 == 1)
        {
            var newIndex = ba.Length + 1;
            nba = new byte[newIndex];
            ba.CopyTo(nba, 0);
            nba[newIndex - 1] = 0;
        }
        else
        {
            nba = ba;
        }

        var hexString = BitConverter.ToString(nba);
        hexString = hexString.Replace("-", "");

        return hexString;
    }
}
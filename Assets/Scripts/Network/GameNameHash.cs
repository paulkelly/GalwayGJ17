using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameNameHash
{
    public static string GetRoomCode(string roomGUID)
    {
        string name = IntToLetters(roomGUID.GetHashCode());
        if (name.Length > 4) name = name.Substring(0, 4);
        return name;
    }

    private static string IntToLetters(int value)
    {
        if (value < 0) value = -value;
        string result = string.Empty;
        while (--value >= 0)
        {
            result = (char)('A' + value % 26) + result;
            value /= 26;
        }
        return result;
    }
}
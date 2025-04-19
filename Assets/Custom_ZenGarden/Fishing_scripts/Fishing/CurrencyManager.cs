using UnityEngine;

public static class CurrencyManager
{
    private const string k_CurrencyKey = "PlayerCurrency";

    /// <summary>
    /// Get the player’s total saved currency.
    /// </summary>
    public static int GetCurrency()
    {
        return PlayerPrefs.GetInt(k_CurrencyKey, 0);
    }

    /// <summary>
    /// Add (or subtract) an amount and save.
    /// </summary>
    public static void ModifyCurrency(int delta)
    {
        int current = GetCurrency();
        current += delta;
        if (current < 0) current = 0;
        PlayerPrefs.SetInt(k_CurrencyKey, current);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Reset currency (e.g. for testing).
    /// </summary>
    public static void ResetCurrency()
    {
        PlayerPrefs.DeleteKey(k_CurrencyKey);
    }
}

using UnityEngine;

public static class GameConstants
{
    public const int STEAM_APPID = 1286380;

    public static readonly string COMPANY_NAME = "ReRolled";
    public static readonly string GAME_NAME = "GKSwitch";
    public const string TEXTLIST_PATH = "Texts/TextList";


    // Please, update this page with new cheatcodes:
    public enum CheatCodesList
    {
        FPS,            // showfps
        SIZE,           // showsize
        MEMORY,         // show memory
        TIME_5,         // Set Time Scale to 5
        SLOW,           // Set Time Scale to 0.1
        RESET,
        FRENCH,
        ENGLISH,
        Count
    }

/*    public enum SpecialCheatCodesList
    {
        GEMS_XXXX,      // Set amount of Real Currency = XXXX
        COINS_XXXX,     // Set amount of Virtual Currency = XXXX
        LEVEL_XX,     // Set level = XXXX
        Count
    }*/
}

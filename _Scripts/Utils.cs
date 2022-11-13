using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
namespace Superpow
{
    public class Utils
    {
        public static string GetLevelData(int world, int level)
        {
            return CPlayerPrefs.GetString("level_data_" + world + "_" + level);
        }

        public static void SetLevelData(int world, int level, string data)
        {
            CPlayerPrefs.SetString("level_data_" + world + "_" + level, data);
        }

        public static Vector2 ConvertToVector2(string position)
        {
            string[] arr = position.Split(',');
            return new Vector2(int.Parse(arr[0]), int.Parse(arr[1]));
        }

        public static string ConvertToString(Vector2 position)
        {
            return position.x + "," + position.y;
        }

        public static bool IsMakingLevel()
        {
            return MakeLevelController.instance != null;
        }

        public static int GetNumHint()
        {
            return CPlayerPrefs.GetInt("num_hint", 5);
        }

        public static void SetNumHint(int num)
        {
            CPlayerPrefs.SetInt("num_hint", num);
        }

        public static void IncreaseNumMoves(int world, int level)
        {
            int current = GetNumMoves(world, level);
            CPlayerPrefs.SetInt("num_move_" + world + "_" + level, current + 1);
        }

        public static int GetNumMoves(int world, int level)
        {
            return CPlayerPrefs.GetInt("num_move_" + world + "_" + level);
        }

        public static bool IsGiftReceived(int world, int level)
        {
            return CPlayerPrefs.GetBool("received_gift_" + world + "_" + level);
        }

        public static void ReceiveGift(int world, int level)
        {
            CPlayerPrefs.SetBool("received_gift_" + world + "_" + level, true);
        }
    }
}
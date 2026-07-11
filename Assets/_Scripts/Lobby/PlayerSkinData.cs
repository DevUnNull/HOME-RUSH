using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerSkinData : MonoBehaviour
{
    public static PlayerSkinData Instance { get; private set; }

    public Material[] colorMaterials;

    private Dictionary<PlayerRef, PlayerColor> playerSkinMap = new Dictionary<PlayerRef, PlayerColor>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Material GetMaterial(PlayerColor color)
    {
        int index = (int)color;
        if (index >= 0 && index < colorMaterials.Length)
        {
            return colorMaterials[index];
        }
        return null;
    }

    public void SetPlayerSkin(PlayerRef player, PlayerColor color)
    {
        if (playerSkinMap.ContainsKey(player))
        {
            playerSkinMap[player] = color;
        }
        else
        {
            playerSkinMap.Add(player, color);
        }
    }

    public PlayerColor GetPlayerSkin(PlayerRef player)
    {
        if (playerSkinMap.ContainsKey(player))
        {
            return playerSkinMap[player];
        }

        return PlayerColor.Black;
    }
}

public enum PlayerColor
{
    Black,
    Blue,
    Green,
    Gray,
    Purple,
    Red,
    White
}
using Connection;
using Gameplay;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UiController : NetworkBehaviour
{
    private class Temp
    {
        public Image hp;
        public Image ul;
        public GameObject block;
    }

    public static UiController Instance;
    public Dictionary<ulong, Image> charachtersHp;
    public Dictionary<ulong, Image> charachtersUl;
    int currentId;
    [SerializeField]
    List<GameObject> charachterUI;
    [SerializeField]
    List<Sprite> charachterIcons;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winText;
    private Dictionary<ulong, int> blocks;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        blocks = new Dictionary<ulong, int>();

        charachtersHp = new Dictionary<ulong, Image>();
        charachtersUl = new Dictionary<ulong, Image>();
        currentId = 0;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            GetClientDataServerRpc(NetworkManager.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetClientDataServerRpc(ulong id)
    {
        if (!charachtersHp.ContainsKey(id) && !charachtersUl.ContainsKey(id))
        {
            var stats = charachterUI[currentId].GetComponentsInChildren<Image>();
            int icon = NetworkManager.ConnectedClients[id].PlayerObject.GetComponent<Teammate>().charachter.Id;
            charachtersHp.Add(id, stats[1]);
            charachtersUl.Add(id, stats[2]);
            blocks.Add(id, currentId);
            charachterUI[currentId].GetComponent<Image>().sprite = charachterIcons[icon];
            charachterUI[currentId].SetActive(true);
            currentId++;
        }
        foreach (var i in NetworkManager.ConnectedClients)
        {
            AddCharachterHealthBarClientRpc(i.Value.PlayerObject.GetComponent<Teammate>().charachter.Id, i.Value.ClientId);
        }
    }

    [ClientRpc]
    private void AddCharachterHealthBarClientRpc(int s, ulong id)
    {
        if (!charachtersHp.ContainsKey(id) && !charachtersUl.ContainsKey(id))
        {
            var stats = charachterUI[currentId].GetComponentsInChildren<Image>();
            charachtersHp.Add(id, stats[1]);
            charachtersUl.Add(id, stats[2]);
            blocks.Add(id, currentId);
            charachterUI[currentId].GetComponent<Image>().sprite = charachterIcons[s];
            charachterUI[currentId].SetActive(true);
            currentId++;
        }
    }

    public void UpdateHp(ulong clientId, float hp)
    {
        charachtersHp[clientId].fillAmount = hp;
        if (hp <= 0)
        {
            var image = charachterUI[blocks[clientId]].GetComponent<Image>();
            image.color = Color.gray;
        }
    }

    public void UpdateUlta(ulong clientId, float ulta)
    {
        charachtersUl[clientId].fillAmount = ulta;
    }

    public void GameOver(Belonging winSide)
    {
        winPanel.SetActive(true);
        winText.text = winSide == Belonging.Red ? "Red Won" : "Blue Won";
    }
}
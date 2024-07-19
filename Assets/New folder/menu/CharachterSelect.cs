using Connection;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharachterSelect : MonoBehaviour
{
    [SerializeField]
    List<GameObject> charachters;
    [SerializeField]
    List<Charachter> charachtersSO;
    [SerializeField]
    List<Animator> animators;
    [SerializeField]
    List<Animator> UiAnimators;

    [SerializeField]
    Image health;

    [SerializeField]
    Image damage;

    [SerializeField]
    Image speed;

    int currentId;

    [SerializeField] private GameObject serverPanel;
    [SerializeField] private GameObject clientPanel;
    [SerializeField] private TMP_InputField maxPlayerInput;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private TMP_InputField addressInputServer;

    private void Update()
    {
        health.fillAmount = Mathf.Lerp(health.fillAmount, (float)charachtersSO[currentId].Health / 100, 10 * Time.deltaTime);
        damage.fillAmount = Mathf.Lerp(damage.fillAmount, (float)charachtersSO[currentId].Damage / 100, 10 * Time.deltaTime);
        speed.fillAmount = Mathf.Lerp(speed.fillAmount, (float)charachtersSO[currentId].Speed / 100, 10 * Time.deltaTime);
    }

    public void CharachterSelectButton(int id)
    {
        charachters[currentId].SetActive(false);
        charachters[id].SetActive(true);
        animators[id].SetInteger("gender", id / 2);
        if (UiAnimators[0].GetInteger("start") != 1)
            UiAnimators[0].SetInteger("start", 1);
        currentId = id;
        ConnectionHandler.Instance.UserPlayerPrefabId = id;
    }

    public void StartClientPanel()
    {
        addressInput.text = ConnectionHandler.Instance.Address;
        clientPanel.SetActive(!clientPanel.activeSelf);
        UiAnimators[0].SetInteger("start", clientPanel.activeSelf ? 2 : 0);
    }

    public void StartServerPanel()
    {
        addressInputServer.text = ConnectionHandler.Instance.Address;
        maxPlayerInput.text = ConnectionHandler.Instance.MaxNumberOfPlayers.ToString();
        serverPanel.SetActive(!serverPanel.activeSelf);
        UiAnimators[0].SetInteger("start", serverPanel.activeSelf ? 2 : 0);
    }

    public void ChangeAddress(string address)
    {
    }

    public void ChangeMaxNumberOfPlayers(string num)
    {
        if (int.TryParse(maxPlayerInput.text, out int numOfPlayers))
        {
            ConnectionHandler.Instance.MaxNumberOfPlayers = numOfPlayers;
        }
    }

    public void StartServerButton()
    {
        ConnectionHandler.Instance.Address = addressInputServer.text;
        UiAnimators[0].SetInteger("start", 2);
        charachters[currentId].SetActive(false);
        ConnectionHandler.Instance.StartServer();
        serverPanel.SetActive(false);
        clientPanel.SetActive(false);
    }

    public void StartClientButton()
    {
        ConnectionHandler.Instance.Address = addressInput.text;
        UiAnimators[0].SetInteger("start", 2);
        charachters[currentId].SetActive(false);
        ConnectionHandler.Instance.StartClient();
        clientPanel.SetActive(false);
        serverPanel.SetActive(false);
    }
}
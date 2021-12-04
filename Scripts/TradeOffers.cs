using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class TradeOffers : MonoBehaviour
{
    public Button[] tradeOfferButtons;
    [HideInInspector]
    public List<TradeInfo> tradeOffers;

    [HideInInspector]
    public TradeOfferInfo tradeOfferInfo;

    private int numTradeOffers;

    // instance
    public static TradeOffers instance;
    
    void Awake() { instance = this; }

    public void UpdateTradeOffers()
    {
        DisableAllTradeOfferButtons();

        ExecuteCloudScriptRequest getTradeOffersRequest = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetTradeIDs"
        };
        PlayFabClientAPI.ExecuteCloudScript(getTradeOffersRequest,
         result =>
         {
             string rawData = result.FunctionResult.ToString();
             tradeOfferInfo = JsonUtility.FromJson<TradeOfferInfo>(rawData);
             GetTradeInfo();
         },
         error => Debug.Log(error.ErrorMessage)
        );

    }
    void DisableAllTradeOfferButtons()
    {
        foreach (Button button in tradeOfferButtons)
            button.gameObject.SetActive(false);
    }

    void GetTradeInfo()
    {
        numTradeOffers = tradeOfferInfo.playerIds.Count;
        tradeOffers = new List<TradeInfo>();

        if (numTradeOffers == 0)
            UpdateTradeOffersUI();

        for(int i = 0; i < tradeOfferInfo.playerIds.Count; ++i)
        {
            GetTradeStatusRequest tradeStatusRequest = new GetTradeStatusRequest
            {
                OfferingPlayerId = tradeOfferInfo.playerIds[i],
                TradeId = tradeOfferInfo.tradeIds[i]
            };

            PlayFabClientAPI.GetTradeStatus(tradeStatusRequest,
                result =>
                {
                    tradeOffers.Add(result.Trade);

                    if (tradeOffers.Count == numTradeOffers)
                        UpdateTradeOffersUI();
                },
                error => Debug.Log(error.ErrorMessage)
                );
        }
    }
    void UpdateTradeOffersUI()
    {
        for(int i = 0; i < tradeOfferButtons.Length; ++i)
        {
            tradeOfferButtons[i].gameObject.SetActive(i < tradeOffers.Count);
            if (!tradeOfferButtons[i].gameObject.activeInHierarchy)
                continue;

            tradeOfferButtons[i].onClick.RemoveAllListeners();

            int tradeIndex = i;
            tradeOfferButtons[i].onClick.AddListener(() => OnTradeOfferButton(tradeIndex));
            tradeOfferButtons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tradeOfferInfo.playerDisplayNames[i];
        }
    }
    //called when click on a trade offer button
    public void OnTradeOfferButton(int tradeIndex)
    {
        ViewTradeWindow.instance.SetTradeWindow(tradeIndex);
    }


}

[System.Serializable]
public class TradeOfferInfo
{
    public List<string> playerIds;
    public List<string> playerDisplayNames;
    public List<string> tradeIds;
}

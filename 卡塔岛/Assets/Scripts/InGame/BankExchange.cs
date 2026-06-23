using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Bankexchange : MonoBehaviour
{
    [Header("交换面板")]
    [SerializeField] private GameObject exchangePanel;
    [SerializeField] private TextMeshProUGUI mainButtonLabel;
    [SerializeField] private List<ResourceButton> resourceButtons;

    [Header("接收面板")]
    [SerializeField] private GameObject receivePanel;
    [SerializeField] private TextMeshProUGUI receiveMainButtonLabel;
    [SerializeField] private List<ResourceButton> receiveResourceButtons;

    public Animator animator;

    private ResourceType? selectedGive;
    private ResourceType? selectedReceive;

    private static readonly ResourceType[] ResourceTypes = {
        ResourceType.Food, ResourceType.Housing,
        ResourceType.Medical, ResourceType.Legal, ResourceType.Credit
    };

    void Start()
    {
        for (int i = 0; i < resourceButtons.Count; i++)
        {
            int idx = i;
            if (resourceButtons[i].button != null)
                resourceButtons[i].button.onClick.AddListener(() => OnExchangeClicked(idx));
        }
        for (int i = 0; i < receiveResourceButtons.Count; i++)
        {
            int idx = i;
            if (receiveResourceButtons[i].button != null)
                receiveResourceButtons[i].button.onClick.AddListener(() => OnReceiveClicked(idx));
        }
    }

    public void beginexchange()
    {
        animator.SetBool("Start", true);
    }

    // ==================== 交换面板 ====================

    public void openimage()
    {
        exchangePanel?.SetActive(true);
        RefreshAmounts();
    }

    private void RefreshAmounts()
    {
        var player = GameDataManager.Instance?.playerData;
        if (player == null) return;

        for (int i = 0; i < resourceButtons.Count && i < ResourceTypes.Length; i++)
        {
            int amount = player.resources.Get(ResourceTypes[i]);
            if (resourceButtons[i].amountLabel != null)
                resourceButtons[i].amountLabel.text = amount.ToString();
        }
    }

    private void OnExchangeClicked(int index)
    {
        if (index < 0 || index >= ResourceTypes.Length) return;
        selectedGive = ResourceTypes[index];
        if (mainButtonLabel != null)
            mainButtonLabel.text = ResourceTypes[index].ToString();
        exchangePanel?.SetActive(false);
    }

    // ==================== 接收面板 ====================

    public void openReceivePanel()
    {
        receivePanel?.SetActive(true);
    }

    private void OnReceiveClicked(int index)
    {
        if (index < 0 || index >= ResourceTypes.Length) return;
        selectedReceive = ResourceTypes[index];
        if (receiveMainButtonLabel != null)
            receiveMainButtonLabel.text = ResourceTypes[index].ToString();
        receivePanel?.SetActive(false);
    }

    // ==================== 兑换 ====================

    /// <summary>
    /// 执行兑换：4个 give 资源 → 1 个 receive 资源
    /// </summary>
    public void DoExchange()
    {
        if (selectedGive == null || selectedReceive == null)
        {
            Debug.Log("请先选择要付出的资源和要获得的资源");
            return;
        }

        if (selectedGive == selectedReceive)
        {
            Debug.Log("不能兑换同一种资源");
            return;
        }

        var player = GameDataManager.Instance?.playerData;
        if (player == null) return;

        var cost = new ResourceCost(new ResourceAmount(selectedGive.Value, 4));

        if (!player.CanAfford(cost))
        {
            Debug.Log($"{selectedGive} 不足 4 个");
            return;
        }

        player.SpendResources(cost);
        player.AddResource(selectedReceive.Value, 1);
        Debug.Log($"兑换完成：4 {selectedGive} → 1 {selectedReceive}");
        RefreshAmounts();
    }

    public void CancelExchange()
    {
        selectedGive = null;
        selectedReceive = null;
    }

    // ==================== 通用 ====================

    public void stopexchange()
    {
        animator.SetBool("Start", false);
        exchangePanel?.SetActive(false);
        receivePanel?.SetActive(false);
    }

    [System.Serializable]
    public class ResourceButton
    {
        public Button button;
        public TextMeshProUGUI nameLabel;
        public TextMeshProUGUI amountLabel;
    }
}

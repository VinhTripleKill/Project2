using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HpBar : MonoBehaviour
{
    public static HpBar Instance;

    [Header("HP Settings")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int missDamage = 5;

    [Header("UI")]
    [SerializeField] private Image hpBarFill;
    [SerializeField] private TextMeshProUGUI hpText;

    private int currentHP;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();
    }

    public void TakeMissDamage()
    {
        TakeDamage(missDamage);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        UpdateHPUI();

        if (currentHP <= 0)
        {
            GameOver();
        }
    }

    void UpdateHPUI()
    {
        // cập nhật thanh máu
        float value = (float)currentHP / maxHP;
        hpBarFill.fillAmount = value;

        // cập nhật text
        hpText.text = $"{currentHP}/{maxHP}";
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");

        GameManager.Instance.TriggerGameOver();
    }
}
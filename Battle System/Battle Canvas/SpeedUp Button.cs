using BattleSystem.BattleControler;
using UnityEngine;

public class SpeedUpButton : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI buttonText;

    private int id = 0;

    private MainBattleManager _mainBattleManager;
    private void Awake()
    {
        _mainBattleManager = FindAnyObjectByType<MainBattleManager>();
        buttonText.text = ">";
    }
    public void OnButtonClick()
    {
        switch (id)
        {
            case 0:
                _mainBattleManager.SetWorldTime(2f);
                id = 1;
                buttonText.text = ">>";
                break;
            case 1:
                _mainBattleManager.SetWorldTime(3f);
                id = 2;
                buttonText.text = ">>>";
                break;
            case 2:
                _mainBattleManager.SetWorldTime(1f);
                id = 0;
                buttonText.text = ">";
                break;
        }
    }
}

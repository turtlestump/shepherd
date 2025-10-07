using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{

    public TMP_Text nameText;
    public TMP_Text levelText;
    public Slider HP;

    public void SetHUD(Herd herd, int index)
    {

        nameText.text = herd.names[index];
        levelText.text = "<rotate=\"10>Lv. " + herd.levels[index];
        HP.maxValue = herd.maxHP[index];
        HP.value = herd.currentHP[index];

    }

    public void SetHP(int hp)
    {

        HP.value = hp;

    }

}

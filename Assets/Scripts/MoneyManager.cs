using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;


    [SerializeField]
    private int money = 0;


    public int Money => money;



    private void Awake()
    {
        Instance = this;
    }



    public void AddMoney(int amount)
    {
        money += amount;

        Debug.Log(
            "Money: " + money
        );
    }
}
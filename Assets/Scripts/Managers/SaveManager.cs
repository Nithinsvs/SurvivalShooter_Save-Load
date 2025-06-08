using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;

public class Data
{
    public int score;
    public int health;

    public Vector3 playerPosition;
    public Vector3 playerRotation;
}

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;     //SINGLETON IMPLEMENTATION
    public static SaveManager Instance { get { return instance; } }

    public PlayerHealth playerHealth;
    private string path = null;
    private GameObject player;
    private string dataStore = null;
    [SerializeField] private bool enableEncryption;


    Data data = new Data();




    private void Awake()
    {
        if (instance == null && instance != this)
            instance = this;
        else
            Destroy(instance);

        path = Application.persistentDataPath + "/SaveManager";

        player = GameObject.FindGameObjectWithTag("Player");

        LoadGame();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    [MenuItem("JsonData/Delete", priority = 0)]
    public static void ClearData()
    {
        Debug.Log("Clearing data");
        if (File.Exists(Instance.path))
            File.Delete(Instance.path);
    }


    //LOADS PREVIOUSLY SAVED GAME WHEN GAME STARTS
    public void LoadGame()
    {
        if (!File.Exists(path))
        {
            SaveGame();
        }
        else
        {
            if (enableEncryption)
            {
                Debug.Log("Loading by decryption");
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = new FileStream(path, FileMode.Open);
                dataStore = bf.Deserialize(file).ToString();
            }
            else
            {
                dataStore = File.ReadAllText(path);
            }

            Debug.Log("Reading data");
            data = JsonUtility.FromJson<Data>(dataStore);
            ScoreManager.score = data.score;
            PlayerHealth.startingHealth = data.health;

            data.score = 0;

            player.transform.position = data.playerPosition;
            player.transform.eulerAngles = data.playerRotation;

        }
    }



    //SAVES THE GAME ON APPICATION CLOSED SCENARIO
    public void SaveGame()
    {
        Debug.Log("Saving a game");
        if (!File.Exists(path))
        {
            data.score = 0;
            data.health = 100;

            data.playerPosition = Vector3.zero;
            data.playerRotation = Vector3.zero;
        }
        else
        {
            data.score = ScoreManager.score;
            data.health = playerHealth.CurrentHealth;

            data.playerPosition = player.transform.position;
            data.playerRotation = player.transform.rotation.eulerAngles;
        }
        string dataStore = JsonUtility.ToJson(data);
        File.WriteAllText(path, dataStore);

        //ENCRYPTING USING BINARY FORMATTER
        if (enableEncryption)
        {
            Debug.Log("---Writing to file---");
            SaveBinaryFormatter(path, dataStore);
        }

        void SaveBinaryFormatter(string path, string content)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream file = File.Create(path);
            binaryFormatter.Serialize(file, content);
            file.Close();
            
            Debug.Log("---Wrote to file---");
        }

    }



    private void OnDisable()
    {
        SaveGame();
    }
}

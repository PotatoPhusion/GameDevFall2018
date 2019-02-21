using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class LoadSave {

    public static Car2DData[] LoadRacers(string difficulty) {
        string path = Application.dataPath + "/AI/" + difficulty;
        DirectoryInfo myDir = new DirectoryInfo(path);
        int numFiles = myDir.GetFiles("*.car").Length;
        if (numFiles == 0)
        {
            Debug.LogError("No racers found in " + path);
            return null;
        }
        Car2DData[] racers = new Car2DData[numFiles];
        for (int i = 0; i < numFiles; i++)
        {
            FileInfo[] files = myDir.GetFiles("*.car");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(files[i].FullName, FileMode.Open);
            racers[i] = (Car2DData)bf.Deserialize(file);
        }
        return racers;
    }

}

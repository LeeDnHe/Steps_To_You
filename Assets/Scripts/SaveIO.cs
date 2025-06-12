using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SaveIO
{
    public static Stat readStat(string path)
    {
        Stat stat = new Stat();
        
        if (File.Exists(path))
        {
            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stat.weight = reader.ReadSingle();
                    stat.bodyFat = reader.ReadSingle();
                    stat.muscleMass = reader.ReadSingle();

                    string plt = reader.ReadString();
                    stat.playTime = plt.Split(':').Select(int.Parse).ToArray();
                    
                    stat.friendship = reader.ReadInt32();
                }
            }
        }
        else
        {
            Debug.LogError("File not found: " + path);
        }
        
        return stat;
    }

    public static bool writeStat(Stat stat, string path)
    {
        using (FileStream stream = File.Open(path, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(stat.weight);
                writer.Write(stat.bodyFat);
                writer.Write(stat.muscleMass);

                string plt = stat.playTime[0] +  ":" + stat.playTime[1] + ":" + stat.playTime[2];
                writer.Write(plt);
                
                writer.Write(stat.friendship);
            }
        }
        
        return true;
    }
}

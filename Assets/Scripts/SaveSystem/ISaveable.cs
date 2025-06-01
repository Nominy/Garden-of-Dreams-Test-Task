using UnityEngine;

/// <summary>
/// Interface for objects that can be saved and loaded
/// </summary>
public interface ISaveable
{
    /// <summary>
    /// Get the unique ID for this saveable object
    /// </summary>
    string GetSaveId();
    
    /// <summary>
    /// Get the save data for this object
    /// </summary>
    object GetSaveData();
    
    /// <summary>
    /// Load data into this object
    /// </summary>
    void LoadSaveData(object data);
} 